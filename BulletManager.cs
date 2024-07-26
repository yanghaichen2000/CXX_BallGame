using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.PlayerSettings;

public class BulletManager
{
    public InstancePool<Bullet> bulletPool = new InstancePool<Bullet>();
    public HashSet<Bullet> bullets = new HashSet<Bullet>();
    public Stack<Bullet> bulletRecycleBin = new Stack<Bullet>();

    /////////////////////////// cs test ///////////////////////////

    public GameManager gameManager;

    public struct BulletDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public float speed;
        public float radius;
        public float damage;
        public Int32 bounces;
        public float expirationTime;
        public Int32 valid;
    }
    const int bulletDatumSize = 48;

    public struct EnemyDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public float hp;
        public float size;
        public Int32 valid;
        public Int32 tmp1;
        public Int32 tmp2;
        public Int32 tmp3;
    }
    const int enemyDatumSize = 48;

    const int maxPlayerBulletNum = 16384;
    const int maxNewPlayerBulletNum = 128;
    const int maxEnemyNum = 128;

    public GameObject[] playerBulletArray = new GameObject[maxPlayerBulletNum];

    BulletDatum[] playerBulletData;
    ComputeBuffer playerBulletDataCB;
    Int32[] playerBulletStack;
    ComputeBuffer playerBulletStackCB;

    BulletDatum[] playerShootRequestData;
    ComputeBuffer playerShootRequestDataCB;
    int playerShootRequestNum;

    EnemyDatum[] sphereEnemyData;
    ComputeBuffer sphereEnemyDataCB;
    int sphereEnemyNum;

    ComputeShader playerBulletCS;
    int playerShootKernel;
    int updatePlayerBulletPositionKernel;

    public BulletManager(GameManager _gameManager) 
    {
        gameManager = _gameManager;

        playerBulletData = new BulletDatum[maxPlayerBulletNum];
        playerBulletDataCB = new ComputeBuffer(maxPlayerBulletNum, bulletDatumSize);
        playerBulletStack = new Int32[maxPlayerBulletNum + 1];
        playerBulletStackCB = new ComputeBuffer(maxPlayerBulletNum + 1, sizeof(Int32));

        playerShootRequestData = new BulletDatum[maxNewPlayerBulletNum];
        playerShootRequestDataCB = new ComputeBuffer(maxNewPlayerBulletNum, bulletDatumSize);
        playerShootRequestNum = 0;

        sphereEnemyData = new EnemyDatum[maxEnemyNum];
        sphereEnemyDataCB = new ComputeBuffer(maxEnemyNum, enemyDatumSize);

        playerBulletCS = gameManager.playerBulletCS;
        playerShootKernel = playerBulletCS.FindKernel("PlayerShoot");
        updatePlayerBulletPositionKernel = playerBulletCS.FindKernel("UpdatePlayerBulletPosition");

        InitializeComputeBuffers();
        CreatePlayerBullets();
    }

    public void InitializeComputeBuffers()
    {
        for (int i = 0; i < maxPlayerBulletNum; i++)
        {
            playerBulletData[i] = new BulletDatum()
            {
                pos = GameManager.bulletPoolRecyclePosition,
                dir = new Vector3(1.0f, 0.0f, 0.0f),
                valid = 0
            };
            playerBulletStack[i] = maxPlayerBulletNum - i - 1;
        }
        playerBulletStack[maxPlayerBulletNum] = maxPlayerBulletNum - 1;

        playerBulletDataCB.SetData(playerBulletData);
        playerBulletStackCB.SetData(playerBulletStack);
    }

    public void CreatePlayerBullets()
    {
        for (int id = 0; id < maxPlayerBulletNum; id++)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = String.Format("bullet{0}", id);
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (Collider collider in colliders) collider.enabled = false;
            obj.transform.SetParent(GameManager.basicTransform);
            Renderer renderer = obj.GetComponent<Renderer>();
            renderer.material = Resources.Load<Material>("bullet");

            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            mpb.SetInt("_ObjectID", id);
            renderer.SetPropertyBlock(mpb);
        }
    }

    public void TickAllBulletsGPU()
    {
        SetComputeGlobalConstants();

        UpdateEnemyDataGPU();

        ExecutePlayerShootRequest();

        UpdatePlayerBulletPosition();

        SetGlobalBufferForRendering();

        ClearPlayerShootRequest();
    }

    public void UpdatePlayerBulletPosition()
    {
        playerBulletCS.SetBuffer(updatePlayerBulletPositionKernel, "playerBulletData", playerBulletDataCB);
        playerBulletCS.Dispatch(updatePlayerBulletPositionKernel,
            BallGameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    public void SetComputeGlobalConstants()
    {
        playerBulletCS.SetInt("maxPlayerBulletNum", maxPlayerBulletNum);
        playerBulletCS.SetInt("playerShootRequestNum", playerShootRequestNum);
        playerBulletCS.SetFloat("deltaTime", GameManager.deltaTime);
    }

    public void UpdateEnemyDataGPU()
    {
        int sphereIndex = 0;
        foreach (Enemy enemy in GameManager.enemyLegion.enemies)
        {
            if (enemy.GetEnemyType() == "Sphere")
            {
                SphereEnemy e = (SphereEnemy)enemy;
                sphereEnemyData[sphereIndex] = new EnemyDatum()
                {
                    pos = e.pos,
                    hp = e.hp,
                    size = e.radius * 2.0f,
                    valid = 1
                };
                sphereIndex++;
            }
        }

        sphereEnemyNum = sphereIndex;

        while (sphereIndex < maxEnemyNum)
        {
            sphereEnemyData[sphereIndex] = new EnemyDatum()
            {
                valid = 0
            };
            sphereIndex++;
        }
    }

    public void AppendPlayerShootRequest(Vector3 _pos, Vector3 _dir, float _speed, float _radius, float _damage, int bounces, float lifeSpan)
    {
        playerShootRequestData[playerShootRequestNum] = new BulletDatum()
        {
            pos = _pos,
            dir = _dir,
            speed = _speed,
            radius = _radius,
            damage = _damage,
            bounces = 5,
            expirationTime = GameManager.gameTime + lifeSpan,
            valid = 1
        };
        playerShootRequestNum++;
    }

    public void ClearPlayerShootRequest()
    {
        playerShootRequestNum = 0;
    }

    public void ExecutePlayerShootRequest()
    {
        Debug.Assert(playerShootRequestNum <= maxNewPlayerBulletNum);
        if (playerShootRequestNum == 0) return;

        playerShootRequestDataCB.SetData(playerShootRequestData);

        playerBulletCS.SetBuffer(playerShootKernel, "playerBulletData", playerBulletDataCB);
        playerBulletCS.SetBuffer(playerShootKernel, "playerBulletStack", playerBulletStackCB);
        playerBulletCS.SetBuffer(playerShootKernel, "playerShootRequestData", playerShootRequestDataCB);

        playerBulletCS.Dispatch(playerShootKernel, 
            BallGameUtils.GetComputeGroupNum(playerShootRequestNum, 128), 1, 1);
    }

    public void SetGlobalBufferForRendering()
    {
        Shader.SetGlobalBuffer("playerBulletData", playerBulletDataCB);
    }




    /////////////////////////// cs test end ///////////////////////////


    public void TickAllBullets()
    {
        using (new BallGameUtils.Profiler("CheckBulletDeath")) { CheckBulletDeath(); }
        using (new BallGameUtils.Profiler("MoveBulletsGPU")) { GameManager.gameManagerGPU.MovePlayerBullets(); }
        using (new BallGameUtils.Profiler("MoveBullets")) { MoveBulletsOld(); }
        using (new BallGameUtils.Profiler("RecycleDeadBullets")) { RecycleDeadBullets(); }
    }

    public void CheckBulletDeath()
    {
        foreach (Bullet bullet in bullets)
        {
            if ((GameManager.currentTime - bullet.createdTime).TotalSeconds > GameManager.bulletLifeSpan)
            {
                bulletRecycleBin.Push(bullet);
            }
        }
    }

    public void MoveBulletsOld()
    {
        foreach (Bullet bullet in bullets)
        {
            bullet.pos += bullet.speed * bullet.dir * GameManager.deltaTime;
            bullet.obj.transform.localPosition = bullet.pos;
        }
    }

    public void RecycleDeadBullets()
    {
        while (bulletRecycleBin.TryPop(out var bullet))
        {
            bullet.MoveToSomeplace();
            bullets.Remove(bullet);
            bulletPool.Return(bullet);
        }
    }

    public void ShootOneBullet(Vector3 _pos, Vector3 _dir, float _speed, float _radius, float _damage, int bounces = 5, float lifeSpan = 12.0f)
    {
        //var bullet = bulletPool.Get();
        //bullet.Initialize(GameManager.currentTime, _pos, _dir, _speed, _radius, _damage);
        //bullets.Add(bullet);

        AppendPlayerShootRequest(_pos, _dir, _speed, _radius, _damage, bounces, lifeSpan);
    }

}
