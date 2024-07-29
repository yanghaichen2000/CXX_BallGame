using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.PlayerSettings;

public class BulletManager
{
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
        public Int32 tmp1;
    }
    const int bulletDatumSize = 48;

    public struct EnemyDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public float hp;
        public float size;
        public float rotationY;
        public Int32 tmp1;
        public Int32 tmp2;
        public Int32 tmp3;
    }
    const int enemyDatumSize = 48;

    const int maxPlayerBulletNum = 16384;
    const int maxNewPlayerBulletNum = 128;
    const int maxEnemyNum = 128;

    BulletDatum[] playerBulletData;
    ComputeBuffer[] playerBulletDataCB;
    ComputeBuffer sourcePlayerBulletDataCB;
    ComputeBuffer targetPlayerBulletDataCB;
    int currentResourceCBIndex;

    Int32[] playerBulletNum;
    ComputeBuffer[] playerBulletNumCB;
    ComputeBuffer sourcePlayerBulletNumCB;
    ComputeBuffer targetPlayerBulletNumCB;

    BulletDatum[] playerShootRequestData;
    ComputeBuffer playerShootRequestDataCB;
    int playerShootRequestNum;

    EnemyDatum[] sphereEnemyData;
    ComputeBuffer sphereEnemyDataCB;
    int sphereEnemyNum;

    EnemyDatum[] cubeEnemyData;
    ComputeBuffer cubeEnemyDataCB;
    int cubeEnemyNum;

    UInt32[] drawPlayerBulletArgs;
    ComputeBuffer drawPlayerBulletArgsCB;

    ComputeShader playerBulletCS;
    int playerShootKernel = -1;
    int updatePlayerBulletPositionKernel = -1;
    int cullPlayerBulletKernel = -1;
    int processPlayerBulletCollisionKernel = -1;
    int updateDrawPlayerBulletArgsKernel = -1;

    Mesh playerBulletMesh;
    Material playerBulletMaterial;


    public BulletManager(GameManager _gameManager) 
    {
        gameManager = _gameManager;

        playerBulletData = new BulletDatum[maxPlayerBulletNum];
        playerBulletDataCB = new ComputeBuffer[2];
        playerBulletDataCB[0] = new ComputeBuffer(maxPlayerBulletNum, bulletDatumSize);
        playerBulletDataCB[1] = new ComputeBuffer(maxPlayerBulletNum, bulletDatumSize);
        currentResourceCBIndex = 0;
        sourcePlayerBulletDataCB = playerBulletDataCB[currentResourceCBIndex];
        targetPlayerBulletDataCB = playerBulletDataCB[1 - currentResourceCBIndex];

        playerBulletNum = new Int32[1];
        playerBulletNumCB = new ComputeBuffer[2];
        playerBulletNumCB[0] = new ComputeBuffer(1, sizeof(Int32));
        playerBulletNumCB[1] = new ComputeBuffer(1, sizeof(Int32));
        sourcePlayerBulletNumCB = playerBulletNumCB[0];
        targetPlayerBulletNumCB = playerBulletNumCB[1];

        playerShootRequestData = new BulletDatum[maxNewPlayerBulletNum];
        playerShootRequestDataCB = new ComputeBuffer(maxNewPlayerBulletNum, bulletDatumSize);
        playerShootRequestNum = 0;

        sphereEnemyData = new EnemyDatum[maxEnemyNum];
        sphereEnemyDataCB = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        cubeEnemyData = new EnemyDatum[maxEnemyNum];
        cubeEnemyDataCB = new ComputeBuffer(maxEnemyNum, enemyDatumSize);

        drawPlayerBulletArgs = new UInt32[5];
        drawPlayerBulletArgsCB = new ComputeBuffer(1, 5 * sizeof(UInt32), ComputeBufferType.IndirectArguments);

        playerBulletCS = gameManager.playerBulletCS;
        playerShootKernel = playerBulletCS.FindKernel("PlayerShoot");
        updatePlayerBulletPositionKernel = playerBulletCS.FindKernel("UpdatePlayerBulletPosition");
        cullPlayerBulletKernel = playerBulletCS.FindKernel("CullPlayerBullet");
        processPlayerBulletCollisionKernel = playerBulletCS.FindKernel("ProcessPlayerBulletCollision");
        updateDrawPlayerBulletArgsKernel = playerBulletCS.FindKernel("UpdateDrawPlayerBulletArgs");

        playerBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        playerBulletMaterial = Resources.Load<Material>("bullet");

        InitializeComputeBuffers();
    }

    public void InitializeComputeBuffers()
    {
        for (int i = 0; i < maxPlayerBulletNum; i++)
        {
            playerBulletData[i] = new BulletDatum()
            {
                pos = GameManager.bulletPoolRecyclePosition,
                dir = new Vector3(1.0f, 0.0f, 0.0f),
                tmp1 = 0
            };
        }
        playerBulletDataCB[0].SetData(playerBulletData);
        playerBulletDataCB[1].SetData(playerBulletData);

        playerBulletNum[0] = 0;
        playerBulletNumCB[0].SetData(playerBulletNum);
        playerBulletNumCB[1].SetData(playerBulletNum);

        drawPlayerBulletArgs[0] = playerBulletMesh.GetIndexCount(0);
        drawPlayerBulletArgs[1] = 0;
        drawPlayerBulletArgs[2] = playerBulletMesh.GetIndexStart(0);
        drawPlayerBulletArgs[3] = playerBulletMesh.GetBaseVertex(0);
        drawPlayerBulletArgsCB.SetData(drawPlayerBulletArgs);
    }

    public void TickAllBulletsGPU()
    {
        //UpdateEnemyData();

        SetComputeGlobalConstant();

        ExecutePlayerShootRequest();

        ProcessPlayerBulletCollision();

        UpdatePlayerBulletPosition();

        ResetCulledBulletNum();

        CullPlayerBullet();

        ClearPlayerShootRequest();

        SwapBulletDataBuffer();

        SetGlobalBufferForRendering();

        DrawBullets();
    }

    public void ResetCulledBulletNum()
    {
        playerBulletNum[0] = 0;
        targetPlayerBulletNumCB.SetData(playerBulletNum);
    }

    public void DrawBullets()
    {
        int kernel = updateDrawPlayerBulletArgsKernel;
        playerBulletCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        playerBulletCS.SetBuffer(kernel, "drawPlayerBulletArgs", drawPlayerBulletArgsCB);
        playerBulletCS.Dispatch(kernel, 1, 1, 1);

        Graphics.DrawMeshInstancedIndirect(
            playerBulletMesh,
            0,
            playerBulletMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawPlayerBulletArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.On
        );
    }

    public void ProcessPlayerBulletCollision()
    {
        int kernel = processPlayerBulletCollisionKernel;
        playerBulletCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        playerBulletCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        playerBulletCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
        playerBulletCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    // 这里的cull包含三个方面
    // 1. 剩余弹射次数
    // 2. 存在时间
    // 3. 位置是否在视锥体内
    // 把所有没有被剔除的bullet存放在culledPlayerBulletData里面用于draw indirect
    public void CullPlayerBullet()
    {
        int kernel = cullPlayerBulletKernel;
        playerBulletCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        playerBulletCS.SetBuffer(kernel, "culledPlayerBulletData", targetPlayerBulletDataCB);
        playerBulletCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        playerBulletCS.SetBuffer(kernel, "culledPlayerBulletNum", targetPlayerBulletNumCB);
        playerBulletCS.SetBuffer(kernel, "drawPlayerBulletArgs", drawPlayerBulletArgsCB);
        playerBulletCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    public void SwapBulletDataBuffer()
    {
        currentResourceCBIndex = 1 - currentResourceCBIndex;
        sourcePlayerBulletDataCB = playerBulletDataCB[currentResourceCBIndex];
        targetPlayerBulletDataCB = playerBulletDataCB[1 - currentResourceCBIndex];
        sourcePlayerBulletNumCB = playerBulletNumCB[currentResourceCBIndex];
        targetPlayerBulletNumCB = playerBulletNumCB[1 - currentResourceCBIndex];
    }

    public void UpdatePlayerBulletPosition()
    {
        int kernel = updatePlayerBulletPositionKernel;
        playerBulletCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        playerBulletCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        playerBulletCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    public void SetComputeGlobalConstant()
    {
        playerBulletCS.SetInt("maxPlayerBulletNum", maxPlayerBulletNum);
        playerBulletCS.SetInt("playerShootRequestNum", playerShootRequestNum);
        playerBulletCS.SetFloat("deltaTime", GameManager.deltaTime);
        playerBulletCS.SetFloat("gameTime", GameManager.gameTime);

        playerBulletCS.SetInt("sphereEnemyNum", sphereEnemyNum);
    }

    public void UpdateEnemyData()
    {
        int sphereIndex = 0;
        int cubeIndex = 0;
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
                };
                sphereIndex++;
            }
            else if (enemy.GetEnemyType() == "Cube")
            {
                CubeEnemy e = (CubeEnemy)enemy;
                cubeEnemyData[cubeIndex] = new EnemyDatum()
                {
                    pos = e.pos,
                    dir = e.dir,
                    hp = e.hp,
                    size = e.size,
                    rotationY = e.rotationY
                };
                cubeIndex++;
            }
        }

        sphereEnemyNum = sphereIndex;
        cubeEnemyNum = cubeIndex;

        sphereEnemyDataCB.SetData(sphereEnemyData);
        cubeEnemyDataCB.SetData(cubeEnemyData);
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
            tmp1 = 1
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

        int kernel = playerShootKernel;
        playerBulletCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        playerBulletCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        playerBulletCS.SetBuffer(kernel, "playerShootRequestData", playerShootRequestDataCB);

        playerBulletCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(playerShootRequestNum, 128), 1, 1);
    }

    public void SetGlobalBufferForRendering()
    {
        Shader.SetGlobalBuffer("playerBulletData", sourcePlayerBulletDataCB);
    }

    public void ShootOneBullet(Vector3 _pos, Vector3 _dir, float _speed, float _radius, float _damage, int bounces = 5, float lifeSpan = 6.0f)
    {
        AppendPlayerShootRequest(_pos, _dir, _speed, _radius, _damage, bounces, lifeSpan);
    }
}
