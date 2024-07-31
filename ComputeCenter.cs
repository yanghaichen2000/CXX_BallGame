using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEditor.PlayerSettings;

public class ComputeCenter
{
    //
    const bool debugPrintReadbackTime = false;
    int debugReadbackFrame1 = 0;
    int debugReadbackFrame2 = 0;
    //

    public GameManager gameManager;

    public struct PlayerDatum
    {
        public Vector3 pos;
        public Int32 hpChange;
        public float3 hitMomentum;
        public float size;
        public UInt32 hittable;
        public float tmp1;
        public float tmp2;
        public float tmp3;
    }
    const int playerDatumSize = 48;

    public struct BulletDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public float speed;
        public float radius;
        public Int32 damage;
        public UInt32 bounces;
        public float expirationTime;
        public UInt32 tmp1;
    }
    const int bulletDatumSize = 48;

    public struct EnemyDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public Int32 hp;
        public float size;
        public float rotationY;
        public float radius;
        public float speed;
        public float maxSpeed;
        public Int32 weapon;
        public float lastShootTime;
        public float tmp3;
        public float tmp4;
    }
    const int enemyDatumSize = 64;

    public struct EnemyWeaponDatum
    {
        public float uniformRandomAngleBias;
        public float individualRandomAngleBias;
        public float shootInterval;
        public Int32 extraBulletsPerSide;
        public float angle;
        public float randomShootDelay;
        public float bulletSpeed;
        public float bulletRadius;
        public Int32 bulletDamage;
        public Int32 bulletBounces;
        public float bulletLifeSpan;
        public float tmp3;
    }
    const int enemyWeaponDatumSize = 48;

    const int maxPlayerBulletNum = 32768;
    const int maxEnemyBulletNum = 32768;
    const int maxNewBulletNum = 128;
    const int maxEnemyNum = 128;
    const int maxNewEnemyNum = 128;
    const int maxEnemyWeaponNum = 8;

    PlayerDatum[] playerData;
    ComputeBuffer playerDataCB;

    int currentResourceCBIndex;

    BulletDatum[] playerBulletData;
    ComputeBuffer[] playerBulletDataCB;
    ComputeBuffer sourcePlayerBulletDataCB;
    ComputeBuffer targetPlayerBulletDataCB;

    BulletDatum[] enemyBulletData;
    ComputeBuffer[] enemyBulletDataCB;
    ComputeBuffer sourceEnemyBulletDataCB;
    ComputeBuffer targetEnemyBulletDataCB;

    UInt32[] playerBulletNum;
    ComputeBuffer[] playerBulletNumCB;
    ComputeBuffer sourcePlayerBulletNumCB;
    ComputeBuffer targetPlayerBulletNumCB;

    UInt32[] enemyBulletNum;
    ComputeBuffer[] enemyBulletNumCB;
    ComputeBuffer sourceEnemyBulletNumCB;
    ComputeBuffer targetEnemyBulletNumCB;

    BulletDatum[] playerShootRequestData;
    ComputeBuffer playerShootRequestDataCB;
    int playerShootRequestNum;

    EnemyDatum[] sphereEnemyData;
    ComputeBuffer sphereEnemyDataCB;
    UInt32[] sphereEnemyNum;
    ComputeBuffer sphereEnemyNumCB;

    EnemyDatum[] cubeEnemyData;
    ComputeBuffer cubeEnemyDataCB;
    UInt32[] cubeEnemyNum;
    ComputeBuffer cubeEnemyNumCB;

    EnemyDatum[] createSphereEnemyRequestData;
    ComputeBuffer createSphereEnemyRequestDataCB;
    int createSphereEnemyRequestNum;

    EnemyDatum[] createCubeEnemyRequestData;
    ComputeBuffer createCubeEnemyRequestDataCB;
    int createCubeEnemyRequestNum;

    EnemyWeaponDatum[] enemyWeaponData;
    ComputeBuffer enemyWeaponDataCB;
    
    UInt32[] drawPlayerBulletArgs;
    ComputeBuffer drawPlayerBulletArgsCB;

    UInt32[] drawEnemyBulletArgs;
    ComputeBuffer drawEnemyBulletArgsCB;

    UInt32[] drawSphereEnemyArgs;
    ComputeBuffer drawSphereEnemyArgsCB;

    ComputeShader computeCenterCS;
    int playerShootKernel = -1;
    int updatePlayerBulletPositionKernel = -1;
    int cullPlayerBulletKernel = -1;
    int processPlayerBulletCollisionKernel = -1;
    int updateDrawPlayerBulletArgsKernel = -1;
    int createSphereEnemyKernel = -1;
    int cullSphereEnemyKernel = -1;
    int updateEnemyPositionKernel = -1;
    int processPlayerEnemyCollisionKernel = -1;
    int enemyShootKernel = -1;
    int updateEnemyBulletPositionKernel = -1;
    int cullEnemyBulletKernel = -1;
    int updateDrawEnemyBulletArgsKernel = -1;
    int processEnemyBulletCollisionKernel = -1;

    Mesh playerBulletMesh;
    Material playerBulletMaterial;

    Mesh enemyBulletMesh;
    Material enemyBulletMaterial;

    Mesh sphereEnemyMesh;
    Material sphereEnemyMaterial;


    public ComputeCenter(GameManager _gameManager) 
    {
        gameManager = _gameManager;

        playerData = new PlayerDatum[2];
        playerDataCB = new ComputeBuffer(2, playerDatumSize);

        currentResourceCBIndex = 0;

        playerBulletData = new BulletDatum[maxPlayerBulletNum];
        playerBulletDataCB = new ComputeBuffer[2];
        playerBulletDataCB[0] = new ComputeBuffer(maxPlayerBulletNum, bulletDatumSize);
        playerBulletDataCB[1] = new ComputeBuffer(maxPlayerBulletNum, bulletDatumSize);
        sourcePlayerBulletDataCB = playerBulletDataCB[currentResourceCBIndex];
        targetPlayerBulletDataCB = playerBulletDataCB[1 - currentResourceCBIndex];

        enemyBulletData = new BulletDatum[maxEnemyBulletNum];
        enemyBulletDataCB = new ComputeBuffer[2];
        enemyBulletDataCB[0] = new ComputeBuffer(maxEnemyBulletNum, bulletDatumSize);
        enemyBulletDataCB[1] = new ComputeBuffer(maxEnemyBulletNum, bulletDatumSize);
        sourceEnemyBulletDataCB = enemyBulletDataCB[currentResourceCBIndex];
        targetEnemyBulletDataCB = enemyBulletDataCB[1 - currentResourceCBIndex];

        playerBulletNum = new UInt32[1];
        playerBulletNumCB = new ComputeBuffer[2];
        playerBulletNumCB[0] = new ComputeBuffer(1, sizeof(UInt32));
        playerBulletNumCB[1] = new ComputeBuffer(1, sizeof(UInt32));
        sourcePlayerBulletNumCB = playerBulletNumCB[0];
        targetPlayerBulletNumCB = playerBulletNumCB[1];

        enemyBulletNum = new UInt32[1];
        enemyBulletNumCB = new ComputeBuffer[2];
        enemyBulletNumCB[0] = new ComputeBuffer(1, sizeof(UInt32));
        enemyBulletNumCB[1] = new ComputeBuffer(1, sizeof(UInt32));
        sourceEnemyBulletNumCB = enemyBulletNumCB[0];
        targetEnemyBulletNumCB = enemyBulletNumCB[1];

        playerShootRequestData = new BulletDatum[maxNewBulletNum];
        playerShootRequestDataCB = new ComputeBuffer(maxNewBulletNum, bulletDatumSize);
        playerShootRequestNum = 0;

        sphereEnemyData = new EnemyDatum[maxEnemyNum];
        sphereEnemyDataCB = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        sphereEnemyNum = new UInt32[1];
        sphereEnemyNumCB = new ComputeBuffer(1, sizeof(UInt32));

        createSphereEnemyRequestData = new EnemyDatum[maxNewEnemyNum];
        createSphereEnemyRequestDataCB = new ComputeBuffer(maxNewEnemyNum, enemyDatumSize);
        createSphereEnemyRequestNum = 0;

        cubeEnemyData = new EnemyDatum[maxEnemyNum];
        cubeEnemyDataCB = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        cubeEnemyNum = new UInt32[1];
        cubeEnemyNumCB = new ComputeBuffer(1, sizeof(UInt32));

        createCubeEnemyRequestData = new EnemyDatum[maxNewEnemyNum];
        createCubeEnemyRequestDataCB = new ComputeBuffer(maxNewEnemyNum, enemyDatumSize);
        createCubeEnemyRequestNum = 0;

        enemyWeaponData = new EnemyWeaponDatum[maxEnemyWeaponNum];
        enemyWeaponDataCB = new ComputeBuffer(maxEnemyWeaponNum, enemyWeaponDatumSize);

        drawPlayerBulletArgs = new UInt32[5];
        drawPlayerBulletArgsCB = new ComputeBuffer(1, 5 * sizeof(UInt32), ComputeBufferType.IndirectArguments);

        drawEnemyBulletArgs = new UInt32[5];
        drawEnemyBulletArgsCB = new ComputeBuffer(1, 5 * sizeof(UInt32), ComputeBufferType.IndirectArguments);

        drawSphereEnemyArgs = new UInt32[5];
        drawSphereEnemyArgsCB = new ComputeBuffer(1, 5 * sizeof(UInt32), ComputeBufferType.IndirectArguments);

        computeCenterCS = gameManager.computeCenterCS;
        playerShootKernel = computeCenterCS.FindKernel("PlayerShoot");
        updatePlayerBulletPositionKernel = computeCenterCS.FindKernel("UpdatePlayerBulletPosition");
        cullPlayerBulletKernel = computeCenterCS.FindKernel("CullPlayerBullet");
        processPlayerBulletCollisionKernel = computeCenterCS.FindKernel("ProcessPlayerBulletCollision");
        updateDrawPlayerBulletArgsKernel = computeCenterCS.FindKernel("UpdateDrawPlayerBulletArgs");
        createSphereEnemyKernel = computeCenterCS.FindKernel("CreateSphereEnemy");
        cullSphereEnemyKernel = computeCenterCS.FindKernel("CullSphereEnemy");
        updateEnemyPositionKernel = computeCenterCS.FindKernel("UpdateEnemyPosition");
        processPlayerEnemyCollisionKernel = computeCenterCS.FindKernel("ProcessPlayerEnemyCollision");
        enemyShootKernel = computeCenterCS.FindKernel("EnemyShoot");
        updateEnemyBulletPositionKernel = computeCenterCS.FindKernel("UpdateEnemyBulletPosition");
        cullEnemyBulletKernel = computeCenterCS.FindKernel("CullEnemyBullet");
        updateDrawEnemyBulletArgsKernel = computeCenterCS.FindKernel("UpdateDrawEnemyBulletArgs");
        processEnemyBulletCollisionKernel = computeCenterCS.FindKernel("ProcessEnemyBulletCollision");

        playerBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        playerBulletMaterial = Resources.Load<Material>("playerBullet");

        enemyBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        enemyBulletMaterial = Resources.Load<Material>("enemyBullet");

        sphereEnemyMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        sphereEnemyMaterial = Resources.Load<Material>("enemy");

        InitializeComputeBuffers();
        SetFrustumCullingGlobalConstant();
    }

    public void InitializeComputeBuffers()
    {
        playerBulletDataCB[0].SetData(playerBulletData);
        playerBulletDataCB[1].SetData(playerBulletData);

        enemyBulletDataCB[0].SetData(enemyBulletData);
        enemyBulletDataCB[1].SetData(enemyBulletData);

        playerBulletNum[0] = 0;
        playerBulletNumCB[0].SetData(playerBulletNum);
        playerBulletNumCB[1].SetData(playerBulletNum);

        enemyBulletNum[0] = 0;
        enemyBulletNumCB[0].SetData(enemyBulletNum);
        enemyBulletNumCB[1].SetData(enemyBulletNum);

        drawPlayerBulletArgs[0] = playerBulletMesh.GetIndexCount(0);
        drawPlayerBulletArgs[1] = 0;
        drawPlayerBulletArgs[2] = playerBulletMesh.GetIndexStart(0);
        drawPlayerBulletArgs[3] = playerBulletMesh.GetBaseVertex(0);
        drawPlayerBulletArgsCB.SetData(drawPlayerBulletArgs);

        drawEnemyBulletArgs[0] = enemyBulletMesh.GetIndexCount(0);
        drawEnemyBulletArgs[1] = 0;
        drawEnemyBulletArgs[2] = enemyBulletMesh.GetIndexStart(0);
        drawEnemyBulletArgs[3] = enemyBulletMesh.GetBaseVertex(0);
        drawEnemyBulletArgsCB.SetData(drawEnemyBulletArgs);

        drawSphereEnemyArgs[0] = sphereEnemyMesh.GetIndexCount(0);
        drawSphereEnemyArgs[1] = 0;
        drawSphereEnemyArgs[2] = sphereEnemyMesh.GetIndexStart(0);
        drawSphereEnemyArgs[3] = sphereEnemyMesh.GetBaseVertex(0);
        drawSphereEnemyArgsCB.SetData(drawSphereEnemyArgs);

        sphereEnemyNum[0] = 0;
        sphereEnemyNumCB.SetData(sphereEnemyNum);
        cubeEnemyNum[0] = 0;
        cubeEnemyNumCB.SetData(cubeEnemyNum);

        InitializeEnemyWeapon();
    }

    public void TickGPU()
    {
        UpdateComputeGlobalConstant();
        UpdatePlayerComputeBuffer();

        ExecutePlayerShootRequest();
        ExecuteCreateEnemyRequest();
        EnemyShoot();

        ProcessPlayerBulletCollision();
        ProcessEnemyBulletCollision();
        ProcessPlayerEnemyCollision();

        UpdatePlayerBulletPosition();
        UpdateEnemyBulletPosition();
        UpdateEnemyPosition();

        CullPlayerBullet();
        CullEnemyBullet();
        CullEnemy();

        ClearPlayerShootRequest();
        ClearCreateEnemyRequest();

        SwapBulletDataBuffer();

        DrawPlayerBullet();
        DrawEnemyBullet();
        DrawEnemy();

        SendGPUReadbackRequest();

        if (true)
        {
            
        }
    }

    public void SetFrustumCullingGlobalConstant()
    {
        Camera camera = Camera.main;
        Plane plane = new Plane(Vector3.up, new Vector3(0, 0.5f, 0));

        Vector3[] screenCorners = new Vector3[4];
        screenCorners[0] = new Vector3(0, 0, 0);
        screenCorners[1] = new Vector3(Screen.width, 0, 0);
        screenCorners[2] = new Vector3(0, Screen.height, 0);
        screenCorners[3] = new Vector3(Screen.width, Screen.height, 0);

        Vector3[] intersectionList = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            Ray ray = camera.ScreenPointToRay(screenCorners[i]);
            plane.Raycast(ray, out float enter);
            intersectionList[i] = ray.GetPoint(enter);
            Debug.Log(intersectionList[i]);
        }

        float zMin = intersectionList[0].z;
        float zMax = intersectionList[2].z;
        float xAtZMin = intersectionList[1].x;
        float xAtZMax = intersectionList[3].x;
        float lerpCoefficient = 1.0f / (zMax - zMin);

        computeCenterCS.SetFloat("viewFrustrumZMin", zMin);
        computeCenterCS.SetFloat("viewFrustrumZMax", zMax);
        computeCenterCS.SetFloat("viewFrustrumXAtZMin", xAtZMin);
        computeCenterCS.SetFloat("viewFrustrumXAtZMax", xAtZMax);
        computeCenterCS.SetFloat("frustrumCullingLerpCoefficient", lerpCoefficient);
    }

    public void ProcessEnemyBulletCollision()
    {
        int kernel = processEnemyBulletCollisionKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxEnemyBulletNum, 64), 1, 1);
    }

    public void CullEnemyBullet()
    {
        int kernel = cullEnemyBulletKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "culledEnemyBulletData", targetEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "culledEnemyBulletNum", targetEnemyBulletNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxEnemyBulletNum, 64), 1, 1);
    }

    public void UpdateEnemyBulletPosition()
    {
        int kernel = updateEnemyBulletPositionKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxEnemyBulletNum, 64), 1, 1);
    }

    public void EnemyShoot()
    {
        int kernel = enemyShootKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "enemyWeaponData", enemyWeaponDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 128), 1, 1);
    }

    public void InitializeEnemyWeapon()
    {
        enemyWeaponData[0] = new EnemyWeaponDatum
        {
            shootInterval = -1.0f,
        };

        enemyWeaponData[1] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 20.0f,
            individualRandomAngleBias = 5.0f,
            shootInterval = 2.0f,
            extraBulletsPerSide = 1,
            angle = 90.0f,
            randomShootDelay = 0.2f,
            bulletSpeed = 3.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 10.0f
        };

        enemyWeaponData[2] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 1.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.1f,
            extraBulletsPerSide = 0,
            angle = 0.0f,
            randomShootDelay = 0.0f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 10.0f
        };

        enemyWeaponDataCB.SetData(enemyWeaponData);
    }

    public void UpdatePlayerComputeBuffer()
    {
        playerData[0] = new PlayerDatum
        {
            pos = GameManager.player1.GetPos(),
            hpChange = 0,
            hitMomentum = new float3(0.0f, 0.0f, 0.0f),
            size = 1.0f,
            hittable = GameManager.player1.hittable ? (uint)1 : 0
        };
        playerData[1] = new PlayerDatum
        {
            pos = GameManager.player2.GetPos(),
            hpChange = 0,
            hitMomentum = new float3(0.0f, 0.0f, 0.0f),
            size = 1.0f,
            hittable = GameManager.player2.hittable ? (uint)1 : 0
        };
        playerDataCB.SetData(playerData);
    }

    public void SendGPUReadbackRequest()
    {
        if (debugPrintReadbackTime) { Debug.Log(String.Format("frame {0} readback started: {1}", debugReadbackFrame1++, GameManager.gameTime)); }
        AsyncGPUReadback.Request(playerDataCB, dataRequest =>
        {
            var readbackPlayerData = dataRequest.GetData<PlayerDatum>();
            if (debugPrintReadbackTime) { Debug.Log(String.Format("frame {0} readback completed: {1}", debugReadbackFrame2++, GameManager.gameTime)); }
            OnGPUReadBackCompleted(readbackPlayerData);
        });
    }

    public void OnGPUReadBackCompleted(NativeArray<PlayerDatum> readbackPlayerData)
    {
        GameManager.player1.OnProcessReadbackData(readbackPlayerData[0]);
        GameManager.player2.OnProcessReadbackData(readbackPlayerData[1]);
    }

    public void ResetCulledBulletNum()
    {
        playerBulletNum[0] = 0;
        targetPlayerBulletNumCB.SetData(playerBulletNum);
        enemyBulletNum[0] = 0;
        targetEnemyBulletNumCB.SetData(enemyBulletNum);
    }

    public void DrawPlayerBullet()
    {
        Shader.SetGlobalBuffer("playerBulletData", sourcePlayerBulletDataCB);

        int kernel = updateDrawPlayerBulletArgsKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "drawPlayerBulletArgs", drawPlayerBulletArgsCB);
        computeCenterCS.Dispatch(kernel, 1, 1, 1);

        Graphics.DrawMeshInstancedIndirect(
            playerBulletMesh,
            0,
            playerBulletMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawPlayerBulletArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.Off
        );
    }

    public void DrawEnemyBullet()
    {
        Shader.SetGlobalBuffer("enemyBulletData", sourceEnemyBulletDataCB);

        int kernel = updateDrawEnemyBulletArgsKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "drawEnemyBulletArgs", drawEnemyBulletArgsCB);
        computeCenterCS.Dispatch(kernel, 1, 1, 1);

        Graphics.DrawMeshInstancedIndirect(
            enemyBulletMesh,
            0,
            enemyBulletMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawEnemyBulletArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.Off
        );
    }


    public void DrawEnemy()
    {
        Shader.SetGlobalBuffer("sphereEnemyData", sphereEnemyDataCB);

        Graphics.DrawMeshInstancedIndirect(
            sphereEnemyMesh,
            0,
            sphereEnemyMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawSphereEnemyArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.On
        );
    }

    public void ProcessPlayerBulletCollision()
    {
        int kernel = processPlayerBulletCollisionKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sphereEnemyNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    public void ProcessPlayerEnemyCollision()
    {
        int kernel = processPlayerEnemyCollisionKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    // 这里的cull包含三个方面
    // 1. 剩余弹射次数
    // 2. 存在时间
    // 3. 位置是否在视锥体内
    // 把所有没有被剔除的bullet存放在culledPlayerBulletData里面用于draw indirect
    public void CullPlayerBullet()
    {
        int kernel = cullPlayerBulletKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "culledPlayerBulletData", targetPlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "culledPlayerBulletNum", targetPlayerBulletNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    // 在剔除敌人的同时更新draw indirect参数
    // 注意，这样做要求所有线程必须在同一个线程组
    public void CullEnemy()
    {
        int kernel = cullSphereEnemyKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "drawSphereEnemyArgs", drawSphereEnemyArgsCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void SwapBulletDataBuffer()
    {
        currentResourceCBIndex = 1 - currentResourceCBIndex;

        sourcePlayerBulletDataCB = playerBulletDataCB[currentResourceCBIndex];
        targetPlayerBulletDataCB = playerBulletDataCB[1 - currentResourceCBIndex];
        sourcePlayerBulletNumCB = playerBulletNumCB[currentResourceCBIndex];
        targetPlayerBulletNumCB = playerBulletNumCB[1 - currentResourceCBIndex];

        sourceEnemyBulletDataCB = enemyBulletDataCB[currentResourceCBIndex];
        targetEnemyBulletDataCB = enemyBulletDataCB[1 - currentResourceCBIndex];
        sourceEnemyBulletNumCB = enemyBulletNumCB[currentResourceCBIndex];
        targetEnemyBulletNumCB = enemyBulletNumCB[1 - currentResourceCBIndex];

        ResetCulledBulletNum();
    }

    public void UpdatePlayerBulletPosition()
    {
        int kernel = updatePlayerBulletPositionKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }
    public void UpdateEnemyPosition()
    {
        int kernel = updateEnemyPositionKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sphereEnemyNumCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void UpdateComputeGlobalConstant()
    {
        computeCenterCS.SetInt("playerShootRequestNum", playerShootRequestNum);
        computeCenterCS.SetInt("createSphereEnemyRequestNum", createSphereEnemyRequestNum);
        computeCenterCS.SetFloat("deltaTime", GameManager.deltaTime);
        computeCenterCS.SetFloat("gameTime", GameManager.gameTime);

        Vector3 pPos = GameManager.player1.GetPos();
        computeCenterCS.SetFloats("player1Pos", pPos.x, pPos.y, pPos.z);
        pPos = GameManager.player2.GetPos();
        computeCenterCS.SetFloats("player2Pos", pPos.x, pPos.y, pPos.z);
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

        sphereEnemyNum[0] = (uint)sphereIndex;
        cubeEnemyNum[0] = (uint)cubeIndex;

        sphereEnemyDataCB.SetData(sphereEnemyData);
        cubeEnemyDataCB.SetData(cubeEnemyData);
    }

    public void AppendPlayerShootRequest(Vector3 _pos, Vector3 _dir, float _speed, float _radius, int _damage, int _bounces, float _lifeSpan)
    {
        playerShootRequestData[playerShootRequestNum] = new BulletDatum()
        {
            pos = _pos,
            dir = _dir,
            speed = _speed,
            radius = _radius,
            damage = _damage,
            bounces = (uint)_bounces,
            expirationTime = GameManager.gameTime + _lifeSpan,
        };
        playerShootRequestNum++;
    }

    public void AppendCreateSphereEnemyRequest(Vector3 _pos, float _size, float _radius, float _speed, int _hp, int _weapon)
    {
        createSphereEnemyRequestData[createSphereEnemyRequestNum] = new EnemyDatum()
        {
            pos = _pos,
            size = _size,
            radius = _radius,
            speed = _speed,
            hp = _hp,
            maxSpeed = _speed,
            weapon = _weapon,
            lastShootTime = -99999.0f
        };
        createSphereEnemyRequestNum++;
    }

    public void ExecutePlayerShootRequest()
    {
        Debug.Assert(playerShootRequestNum <= maxNewBulletNum);
        if (playerShootRequestNum == 0) return;

        playerShootRequestDataCB.SetData(playerShootRequestData);

        int kernel = playerShootKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerShootRequestData", playerShootRequestDataCB);
        computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(playerShootRequestNum, 128), 1, 1);
    }

    public void ExecuteCreateEnemyRequest()
    {
        Debug.Assert(createSphereEnemyRequestNum <= maxNewEnemyNum);
        Debug.Assert(createCubeEnemyRequestNum <= maxNewEnemyNum);

        if (createSphereEnemyRequestNum > 0)
        {
            createSphereEnemyRequestDataCB.SetData(createSphereEnemyRequestData);

            int kernel = createSphereEnemyKernel;
            computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sphereEnemyDataCB);
            computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sphereEnemyNumCB);
            computeCenterCS.SetBuffer(kernel, "createSphereEnemyRequestData", createSphereEnemyRequestDataCB);
            computeCenterCS.Dispatch(kernel, GameUtils.GetComputeGroupNum(createSphereEnemyRequestNum, 128), 1, 1);
        }
    }

    public void ClearPlayerShootRequest()
    {
        playerShootRequestNum = 0;
    }

    public void ClearCreateEnemyRequest()
    {
        createSphereEnemyRequestNum = 0;
        createCubeEnemyRequestNum = 0;
    }

}
