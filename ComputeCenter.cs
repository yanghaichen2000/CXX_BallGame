using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using static ComputeCenter;

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
        public int3 hitImpulse;
        public float size;
        public UInt32 hittable;
        public float tmp1;
        public Int32 hitByEnemy;
        public float tmp3;
    }
    const int playerDatumSize = 48;

    public struct PlayerSkillDatum
    {
        public Int32 player1Skill0;
        public Int32 player1Skill1;
        public Int32 player2Skill0;
        public Int32 player2Skill1;
        public Int32 sharedSkill0;
        public Int32 sharedSkill1;
        public Int32 player2Skill0HPRestoration;
        public Int32 tmp2;
    }
    const int playerSkillDatumSize = 32;

    public struct BulletDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public float speed;
        public float radius;
        public Int32 damage;
        public UInt32 bounces;
        public float expirationTime;
        public float impulse;
        public float virtualY;
        public Int32 player;
        public float renderingBiasY;
        public float tmp2;
    }
    const int bulletDatumSize = 64;

    public struct EnemyDatum
    {
        public Vector3 pos;
        public Vector3 velocity;
        public Int32 hp;
        public float size;
        public float radius;
        public int3 hitImpulse;
        public Int32 weapon;
        public float lastShootTime;
        public float originalM;
        public float m;
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
        public float bulletImpulse;
        public float virtualYRange;
        public float tmp1;
        public float tmp2;
        public float tmp3;
    }
    const int enemyWeaponDatumSize = 64;

    /* (hlsl)
    struct BulletGridDatum
    {
        int size;
        float tmp1;
        float tmp2;
        float tmp3;
        int bulletIndexList[12];
    };
    */
    public struct BulletGridDatum
    {
        public int size;
        public float tmp1;
        public float tmp2;
        public float tmp3;
        public Vector4 tmp4;
        public Vector4 tmp5;
        public Vector4 tmp6;
    }
    const int bulletGridDatumSIze = 128;

    // 目前grid覆盖范围：[-32.0, 32.0] x [-16.0, 16.0]
    // 每个grid大小为0.2，要求子弹半径不能大于0.1
    const int bulletGridLengthX = 320;
    const int bulletGridLengthZ = 160;
    const float bulletGridXMin = -32.0f;
    const float bulletGridZMin = -16.0f;
    const float bulletGridSize = 0.2f;
    const float bulletGridSizeInv = 1.0f / bulletGridSize;

    const int maxPlayerBulletNum = 131072;
    const int maxEnemyBulletNum = 131072;
    const int maxNewBulletNum = 2048;
    const int maxEnemyNum = 1024;
    const int maxNewEnemyNum = 128;
    const int maxEnemyWeaponNum = 8;

    PlayerDatum[] playerData;
    ComputeBuffer playerDataCB;

    public PlayerSkillDatum[] playerSkillData;
    ComputeBuffer playerSkillDataCB;

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

    BulletGridDatum[] bulletGridData;
    ComputeBuffer playerBulletGridDataCB;
    ComputeBuffer enemyBulletGridDataCB;

    EnemyDatum[] sphereEnemyData;
    ComputeBuffer[] sphereEnemyDataCB;
    ComputeBuffer sourceSphereEnemyDataCB;
    ComputeBuffer targetSphereEnemyDataCB;

    UInt32[] sphereEnemyNum;
    ComputeBuffer[] sphereEnemyNumCB;
    ComputeBuffer sourceSphereEnemyNumCB;
    ComputeBuffer targetSphereEnemyNumCB;

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
    int updateEnemyVelocityAndPositionKernel = -1;
    int processPlayerEnemyCollisionKernel = -1;
    int enemyShootKernel = -1;
    int updateEnemyBulletVelocityAndPositionKernel = -1;
    int cullEnemyBulletKernel = -1;
    int updateDrawEnemyBulletArgsKernel = -1;
    int processEnemyBulletCollisionKernel = -1;
    int buildPlayerBulletGridKernel = -1;
    int buildEnemyBulletGridKernel = -1;
    int resetBulletGridKernel = -1;
    int processBulletBulletCollisionKernel = -1;
    int updateDrawEnemyArgsKernel = -1;

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

        playerSkillData = new PlayerSkillDatum[1];
        playerSkillDataCB = new ComputeBuffer(1, playerSkillDatumSize);

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

        bulletGridData = new BulletGridDatum[bulletGridLengthX * bulletGridLengthZ];
        playerBulletGridDataCB = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletGridDatumSIze);
        enemyBulletGridDataCB = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletGridDatumSIze);

        sphereEnemyData = new EnemyDatum[maxEnemyNum];
        sphereEnemyDataCB = new ComputeBuffer[2];
        sphereEnemyDataCB[0] = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        sphereEnemyDataCB[1] = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        sourceSphereEnemyDataCB = sphereEnemyDataCB[0];
        targetSphereEnemyDataCB = sphereEnemyDataCB[1];

        sphereEnemyNum = new UInt32[1];
        sphereEnemyNumCB = new ComputeBuffer[2];
        sphereEnemyNumCB[0] = new ComputeBuffer(1, sizeof(UInt32));
        sphereEnemyNumCB[1] = new ComputeBuffer(1, sizeof(UInt32));
        sourceSphereEnemyNumCB = sphereEnemyNumCB[0];
        targetSphereEnemyNumCB = sphereEnemyNumCB[1];

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
        updateEnemyVelocityAndPositionKernel = computeCenterCS.FindKernel("UpdateEnemyVelocityAndPosition");
        processPlayerEnemyCollisionKernel = computeCenterCS.FindKernel("ProcessPlayerEnemyCollision");
        enemyShootKernel = computeCenterCS.FindKernel("EnemyShoot");
        updateEnemyBulletVelocityAndPositionKernel = computeCenterCS.FindKernel("UpdateEnemyBulletVelocityAndPosition");
        cullEnemyBulletKernel = computeCenterCS.FindKernel("CullEnemyBullet");
        updateDrawEnemyBulletArgsKernel = computeCenterCS.FindKernel("UpdateDrawEnemyBulletArgs");
        processEnemyBulletCollisionKernel = computeCenterCS.FindKernel("ProcessEnemyBulletCollision");
        buildPlayerBulletGridKernel = computeCenterCS.FindKernel("BuildPlayerBulletGrid");
        buildEnemyBulletGridKernel = computeCenterCS.FindKernel("BuildEnemyBulletGrid");
        resetBulletGridKernel = computeCenterCS.FindKernel("ResetBulletGrid");
        processBulletBulletCollisionKernel = computeCenterCS.FindKernel("ProcessBulletBulletCollision");
        updateDrawEnemyArgsKernel = computeCenterCS.FindKernel("UpdateDrawEnemyArgs");

        //playerBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        playerBulletMesh = Resources.Load<GameObject>("bulletMesh").GetComponent<MeshFilter>().sharedMesh;
        playerBulletMaterial = Resources.Load<Material>("playerBullet");

        //enemyBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        enemyBulletMesh = Resources.Load<GameObject>("bulletMesh").GetComponent<MeshFilter>().sharedMesh;
        enemyBulletMaterial = Resources.Load<Material>("enemyBullet");

        sphereEnemyMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        sphereEnemyMaterial = Resources.Load<Material>("enemy1");

        InitializeComputeBuffers();
        SetGlobalConstant();
    }

    public void InitializeComputeBuffers()
    {
        playerBulletDataCB[0].SetData(playerBulletData);
        playerBulletDataCB[1].SetData(playerBulletData);

        playerSkillData[0].player1Skill0 = 0;
        playerSkillData[0].player1Skill1 = 0;
        playerSkillData[0].player2Skill0 = 0;
        playerSkillData[0].player2Skill1 = 0;
        playerSkillData[0].sharedSkill0 = 0;
        playerSkillData[0].sharedSkill1 = 0;

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
        sphereEnemyNumCB[0].SetData(sphereEnemyNum);
        sphereEnemyNumCB[1].SetData(sphereEnemyNum);
        cubeEnemyNum[0] = 0;
        cubeEnemyNumCB.SetData(cubeEnemyNum);

        InitializeEnemyWeapon();
    }

    public void TickGPU()
    {
        using (new GUtils.PFL("UpdateComputeGlobalConstant")) { UpdateComputeGlobalConstant(); }
        using (new GUtils.PFL("UpdatePlayerComputeBuffer")) { UpdatePlayerComputeBuffer(); }
        using (new GUtils.PFL("UpdatePlayerSkillComputeBuffer")) { UpdatePlayerSkillComputeBuffer(); }

        using (new GUtils.PFL("ExecutePlayerShootRequest")) { ExecutePlayerShootRequest(); }
        using (new GUtils.PFL("ExecuteCreateEnemyRequest")) { ExecuteCreateEnemyRequest(); }
        using (new GUtils.PFL("EnemyShoot")) { EnemyShoot(); }

        using (new GUtils.PFL("BuildBulletGrid")) { BuildBulletGrid(); }

        using (new GUtils.PFL("ProcessPlayerBulletCollision")) { ProcessPlayerBulletCollision(); }
        using (new GUtils.PFL("ProcessEnemyBulletCollision")) { ProcessEnemyBulletCollision(); }
        using (new GUtils.PFL("ProcessPlayerEnemyCollision")) { ProcessPlayerEnemyCollision(); } // 这个必须要放在敌人和子弹碰撞之后
        using (new GUtils.PFL("ProcessBulletBulletCollision")) { ProcessBulletBulletCollision(); }

        using (new GUtils.PFL("UpdatePlayerBulletPosition")) { UpdatePlayerBulletPosition(); }
        using (new GUtils.PFL("UpdateEnemyBulletVelocityAndPosition")) { UpdateEnemyBulletVelocityAndPosition(); }
        using (new GUtils.PFL("UpdateEnemyVelocityAndPosition")) { UpdateEnemyVelocityAndPosition(); } // 线程同步还没做好，可能出问题

        using (new GUtils.PFL("CullPlayerBullet")) { CullPlayerBullet(); }
        using (new GUtils.PFL("CullEnemyBullet")) { CullEnemyBullet(); }
        using (new GUtils.PFL("CullEnemy")) { CullEnemy(); }

        using (new GUtils.PFL("ClearPlayerShootRequest")) { ClearPlayerShootRequest(); }
        using (new GUtils.PFL("ClearCreateEnemyRequest")) { ClearCreateEnemyRequest(); }

        using (new GUtils.PFL("SwapBulletDataBuffer")) { SwapAndResetDataBuffer(); }

        using (new GUtils.PFL("DrawPlayerBullet")) { DrawPlayerBullet(); }
        using (new GUtils.PFL("DrawEnemyBullet")) { DrawEnemyBullet(); }
        using (new GUtils.PFL("DrawEnemy")) { DrawEnemy(); }

        using (new GUtils.PFL("SendReadbackRequest")) { SendReadbackRequest(); }
        using (new GUtils.PFL("SendDebugReadbackRequest")) { SendDebugReadbackRequest(); }

        if (true)
        {
            
        }
    }

    public void ProcessBulletBulletCollision()
    {
        int kernel = processBulletBulletCollisionKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX, 8), 1, GUtils.GetComputeGroupNum(bulletGridLengthZ, 8));
    }

    public void BuildBulletGrid()
    {
        int kernel = resetBulletGridKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX * bulletGridLengthZ, 256), 1, 1);

        kernel = buildPlayerBulletGridKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);

        kernel = buildEnemyBulletGridKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void SetGlobalConstant()
    {
        // common
        computeCenterCS.SetFloat("gravity", 9.8f);
        computeCenterCS.SetFloat("planeXMin", -20.0f);
        computeCenterCS.SetFloat("planeXMax", 20.0f);
        computeCenterCS.SetFloat("planeZMin", -15.0f);
        computeCenterCS.SetFloat("planeZMax", 15.0f);

        // enemy movement
        computeCenterCS.SetFloat("enemyAcceleration", 2.0f);
        computeCenterCS.SetFloat("enemyMaxSpeed", 1.0f);
        computeCenterCS.SetFloat("enemyFrictionalDeceleration", 2.0f);
        computeCenterCS.SetFloat("enemySpacingAcceleration", 0.2f);
        computeCenterCS.SetFloat("enemyCollisionVelocityRestitution", 0.5f);

        // bullet grid
        computeCenterCS.SetInt("bulletGridLengthX", bulletGridLengthX);
        computeCenterCS.SetInt("bulletGridLengthZ", bulletGridLengthZ);
        computeCenterCS.SetVector("bulletGridBottomLeftPos", new Vector3(-32.0f, 0.5f, -16.0f));
        computeCenterCS.SetFloat("bulletGridSize", bulletGridSize);
        computeCenterCS.SetFloat("bulletGridSizeInv", bulletGridSizeInv);

        // player bullet color
        Shader.SetGlobalVector("player1BulletColor", gameManager.player1BulletColor);
        Shader.SetGlobalVector("player2BulletColor", gameManager.player2BulletColor);

        // frustum culling
        SetFrustumCullingGlobalConstant();
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
        computeCenterCS.SetFloat("viewFrustrumCullingLerpCoefficient", lerpCoefficient);
    }

    public void ProcessEnemyBulletCollision()
    {
        int kernel = processEnemyBulletCollisionKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeCenterCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void CullEnemyBullet()
    {
        int kernel = cullEnemyBulletKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "culledEnemyBulletData", targetEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "culledEnemyBulletNum", targetEnemyBulletNumCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void UpdateEnemyBulletVelocityAndPosition()
    {
        int kernel = updateEnemyBulletVelocityAndPositionKernel;
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void EnemyShoot()
    {
        int kernel = enemyShootKernel;
        computeCenterCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "enemyWeaponData", enemyWeaponDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void InitializeEnemyWeapon()
    {
        float constantVirtualYRange = 0.06f;

        enemyWeaponData[0] = new EnemyWeaponDatum
        {
            shootInterval = -1.0f,
        };

        // 随机方向慢速
        enemyWeaponData[1] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 20.0f,
            individualRandomAngleBias = 5.0f,
            shootInterval = 5.0f,
            extraBulletsPerSide = 1,
            angle = 90.0f,
            randomShootDelay = 5.0f,
            bulletSpeed = 1.5f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 10.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 快速
        enemyWeaponData[2] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 1.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.8f,
            extraBulletsPerSide = 0,
            angle = 0.0f,
            randomShootDelay = 0.0f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 12.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 一般散弹
        enemyWeaponData[3] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 1.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.12f,
            extraBulletsPerSide = 2,
            angle = 10.0f,
            randomShootDelay = 0.3f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 12.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 离谱散弹
        enemyWeaponData[4] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 1.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.1f,
            extraBulletsPerSide = 10,
            angle = 3.0f,
            randomShootDelay = 0.0f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 12.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 全屏
        enemyWeaponData[5] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 1.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.1f,
            extraBulletsPerSide = 60,
            angle = 3.0f,
            randomShootDelay = 0.0f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 12.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        enemyWeaponDataCB.SetData(enemyWeaponData);
    }

    public void UpdatePlayerComputeBuffer()
    {
        playerData[0] = new PlayerDatum
        {
            pos = GameManager.player1.GetPos(),
            hpChange = 0,
            hitImpulse = new int3(0, 0, 0),
            size = 1.0f,
            hittable = GameManager.player1.hittable ? (uint)1 : 0,
            hitByEnemy = 0,
        };
        playerData[1] = new PlayerDatum
        {
            pos = GameManager.player2.GetPos(),
            hpChange = 0,
            hitImpulse = new int3(0, 0, 0),
            size = 1.0f,
            hittable = GameManager.player2.hittable ? (uint)1 : 0,
            hitByEnemy = 0
        };
        playerDataCB.SetData(playerData);
    }

    public void UpdatePlayerSkillComputeBuffer()
    {
        // 技能状态数据已经在Skill.UpdateComputeBufferData()中更新

        playerSkillData[0].player2Skill0HPRestoration = 0;
        playerSkillDataCB.SetData(playerSkillData);
    }

    public void SendReadbackRequest()
    {
        if (debugPrintReadbackTime) { Debug.Log(String.Format("frame {0} readback started: {1}", debugReadbackFrame1++, GameManager.gameTime)); }
        
        AsyncGPUReadback.Request(playerDataCB, dataRequest =>
        {
            var readbackPlayerData = dataRequest.GetData<PlayerDatum>();
            GameManager.player1.OnProcessPlayerReadbackData(readbackPlayerData[0]);
            GameManager.player2.OnProcessPlayerReadbackData(readbackPlayerData[1]);
        });

        AsyncGPUReadback.Request(playerSkillDataCB, dataRequest =>
        {
            var readbackPlayerData = dataRequest.GetData<PlayerSkillDatum>();
            GameManager.player1.OnProcessPlayerSkillReadbackData(readbackPlayerData[0]);
            GameManager.player2.OnProcessPlayerSkillReadbackData(readbackPlayerData[0]);
        });
    }

    public void SendDebugReadbackRequest()
    {
        AsyncGPUReadback.Request(sourceSphereEnemyNumCB, dataRequest =>
        {
            var readbackData = dataRequest.GetData<int>();
            GameManager.uiManager.enemyNum.text = string.Format("enemyNum = {0}", readbackData[0]);
        });

        AsyncGPUReadback.Request(sourceEnemyBulletNumCB, dataRequest =>
        {
            var readbackData = dataRequest.GetData<int>();
            GameManager.uiManager.enemyBulletNum.text = string.Format("enemyBulletNum = {0}", readbackData[0]);
        });

        AsyncGPUReadback.Request(sourcePlayerBulletNumCB, dataRequest =>
        {
            var readbackData = dataRequest.GetData<int>();
            GameManager.uiManager.playerBulletNum.text = string.Format("playerBulletNum = {0}", readbackData[0]);
        });
    }

    public void ResetCulledInstanceNum()
    {
        playerBulletNum[0] = 0;
        targetPlayerBulletNumCB.SetData(playerBulletNum);
        enemyBulletNum[0] = 0;
        targetEnemyBulletNumCB.SetData(enemyBulletNum);
        sphereEnemyNum[0] = 0;
        targetSphereEnemyNumCB.SetData(sphereEnemyNum);
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
        Shader.SetGlobalBuffer("sphereEnemyData", sourceSphereEnemyDataCB);

        int kernel = updateDrawEnemyArgsKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "drawSphereEnemyArgs", drawSphereEnemyArgsCB);
        computeCenterCS.Dispatch(kernel, 1, 1, 1);

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
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 64), 1, 1);
    }

    public void ProcessPlayerEnemyCollision()
    {
        int kernel = processPlayerEnemyCollisionKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
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
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);
    }

    public void CullEnemy()
    {
        int kernel = cullSphereEnemyKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "culledSphereEnemyData", targetSphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "culledSphereEnemyNum", targetSphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "drawSphereEnemyArgs", drawSphereEnemyArgsCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void SwapAndResetDataBuffer()
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

        sourceSphereEnemyDataCB = sphereEnemyDataCB[currentResourceCBIndex];
        targetSphereEnemyDataCB = sphereEnemyDataCB[1 - currentResourceCBIndex];
        sourceSphereEnemyNumCB = sphereEnemyNumCB[currentResourceCBIndex];
        targetSphereEnemyNumCB = sphereEnemyNumCB[1 - currentResourceCBIndex];

        ResetCulledInstanceNum();
    }

    public void UpdatePlayerBulletPosition()
    {
        int kernel = updatePlayerBulletPositionKernel;
        computeCenterCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeCenterCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);
    }
    public void UpdateEnemyVelocityAndPosition()
    {
        int kernel = updateEnemyVelocityAndPositionKernel;
        computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeCenterCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
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

    public void AppendPlayerShootRequest(Vector3 _pos, Vector3 _dir, float _speed, float _radius, int _damage, int _bounces, float _lifeSpan, float _impulse, float _virtualY, int _player, float renderingBiasY)
    {
        if (playerShootRequestNum >= maxNewBulletNum)
        {
            GUtils.LogWithCD("AppendPlayerShootRequest() playerShootRequestNum >= maxNewBulletNum");
            return;
        }

        playerShootRequestData[playerShootRequestNum] = new BulletDatum()
        {
            pos = _pos,
            dir = _dir,
            speed = _speed,
            radius = _radius,
            damage = _damage,
            bounces = (uint)_bounces,
            expirationTime = GameManager.gameTime + _lifeSpan,
            impulse = _impulse,
            virtualY = _virtualY,
            player = _player,
            renderingBiasY = renderingBiasY,
        };
        playerShootRequestNum++;
    }

    public void AppendCreateSphereEnemyRequest(Vector3 _pos, Vector3 _velocity, int _hp, float _size, float _radius, int3 _hitImpulse, int _weapon, float _lastShootTime, float _originalM, float _m)
    {
        createSphereEnemyRequestData[createSphereEnemyRequestNum] = new EnemyDatum()
        {
            pos = _pos,
            velocity = _velocity,
            hp = _hp,
            size = _size,
            radius = _radius,
            hitImpulse = _hitImpulse,
            weapon = _weapon,
            lastShootTime = _lastShootTime,
            originalM = _originalM,
            m = _m
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
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(playerShootRequestNum, 256), 1, 1);
    }

    public void ExecuteCreateEnemyRequest()
    {
        Debug.Assert(createSphereEnemyRequestNum <= maxNewEnemyNum);

        if (createSphereEnemyRequestNum > 0)
        {
            createSphereEnemyRequestDataCB.SetData(createSphereEnemyRequestData);

            int kernel = createSphereEnemyKernel;
            computeCenterCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
            computeCenterCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
            computeCenterCS.SetBuffer(kernel, "createSphereEnemyRequestData", createSphereEnemyRequestDataCB);
            computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(createSphereEnemyRequestNum, 128), 1, 1);
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
