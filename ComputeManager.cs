using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class ComputeManager
{
    //
    const bool debugPrintReadbackTime = true;
    int debugReadbackFrame1 = 0;
    int debugReadbackFrame2 = 0;
    //

    public GameManager gameManager;

    public struct PlayerDatum
    {
        public Vector3 pos;
        public int hpChange;
        public int3 hitImpulse;
        public float size;
        public uint hittable;
        public int hitByEnemy;
        public Vector3 velocity;
        public Vector3 tmp;
    }
    const int playerDatumSize = 64;

    public struct PlayerSkillDatum
    {
        public int player1Skill0;
        public int player1Skill1;
        public int player2Skill0;
        public int player2Skill1;
        public int sharedSkill0;
        public int sharedSkill1;
        public int player2Skill0HPRestoration;
        public Vector3 player1Skill1AimingPointPosition;
        public int player2Skill0HitEnemy;
        public float tmp2;
    }
    const int playerSkillDatumSize = 48;

    // packedInfo:
    // 0 player
    // 1 affectedByPlayer0Skill1
    public struct BulletDatum
    {
        public Vector3 pos;
        public Vector3 dir;
        public float speed;
        public float radius;
        public int damage;
        public uint bounces;
        public float expirationTime;
        public float impulse;
        public float virtualY;
        public uint packedInfo;
        public float renderingBiasY;
        public uint color;
    }
    const int bulletDatumSize = 64;

    public struct EnemyDatum
    {
        public Vector3 pos;
        public Vector3 velocity;
        public int maxHP;
        public int hp;
        public float size;
        public float radius;
        public int3 hitImpulse;
        public int weapon;
        public float lastShootTime;
        public float originalM;
        public float m;
        public float acceleration;
        public float frictionalDeceleration;
        public float maxSpeed;
        public uint baseColor;
        public float lastHitByPlayer2Skill0Time;
        public float createdTime;
        public int knockedOutByBoss;
    }
    const int enemyDatumSize = 96;

    public struct EnemyCollisionCacheDatum
    {
        public Vector3 deltaPos;
        public Vector3 deltaVelocity;
    }
    const int enemyCollisionCacheDatumSize = 24;

    public struct EnemyWeaponDatum
    {
        public float uniformRandomAngleBias;
        public float individualRandomAngleBias;
        public float shootInterval;
        public int extraBulletsPerSide;
        public float angle;
        public float randomShootDelay;
        public float bulletSpeed;
        public float bulletRadius;
        public int bulletDamage;
        public int bulletBounces;
        public float bulletLifeSpan;
        public float bulletImpulse;
        public float virtualYRange;
        public float tmp1;
        public float tmp2;
        public float tmp3;
    }
    const int enemyWeaponDatumSize = 64;

    public struct AvailablePositionDatum
    {
        public Vector3 pos1;
        public Vector3 pos2;
        public int num;
        public int tmp;
    }
    const int availablePositionDatumSize = 32;

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
    const int bulletGridDatumSize = 128;

    public struct EnemyGridDatum
    {
        public int size;
        public int3 index;
    }
    const int enemyGridDatumSize = 16;

    public struct BulletRenderingGridDatum
    {
        public int size;
        public Vector3 pos1;
        public Vector3 pos2;
        public Vector3 pos3;
        public Vector3 pos4;
        public Vector3 color1;
        public Vector3 color2;
        public Vector3 color3;
        public Vector3 color4;
    }
    const int bulletRenderingGridDatumSize = 100;

    public struct BossDatum
    {
        public Vector3 pos;
        public int hpChange;
        public int3 hitImpulse;
        public float tmp;
        public Vector3 velocity;
        public float tmp1;
        public Vector4 tmp2;
    }
    const int bossDatumSize = 64;

    // bullet grid覆盖范围：[-32.0, 32.0] x [-16.0, 16.0]
    // 每个grid大小为0.2，要求子弹半径不能大于0.1
    const int bulletGridLengthX = 320;
    const int bulletGridLengthZ = 160;
    const float bulletGridXMin = -32.0f;
    const float bulletGridZMin = -16.0f;
    const float bulletGridSize = 0.2f;
    const float bulletGridSizeInv = 1.0f / bulletGridSize;

    // enemy grid覆盖范围：[-32.0, 32.0] x [-16.0, 16.0]
    // grid大小为1
    const int enemyGridLengthX = 64;
    const int enemyGridLengthZ = 32;
    const int enemyGridLength = enemyGridLengthX * enemyGridLengthZ;
    const float enemyGridXMin = -32.0f;
    const float enemyGridZMin = -16.0f;
    const float enemyGridSize = 1.0f;
    const float enemyGridSizeInv = 1.0f / enemyGridSize;

    public const int maxPlayerBulletNum = 131072;
    public const int maxEnemyBulletNum = 131072; // 两种子弹的最大数量必须相等，适配GeneratePlaneLightingTexture
    public const int maxNewBulletNum = 2048;
    public const int maxEnemyNum = 4096;
    public const int maxDeployingEnemyNum = 2048;
    public const int maxNewEnemyNum = 2048;
    public const int maxEnemyWeaponNum = 8;

    PlayerDatum[] playerData;
    ComputeBuffer playerDataCB;

    BossDatum[] bossData;
    ComputeBuffer bossDataCB;

    public PlayerSkillDatum[] playerSkillData;
    ComputeBuffer playerSkillDataCB;

    int currentResourceCBIndex;

    public bool knockOutAllEnemyRequest;

    BulletDatum[] playerBulletData;
    ComputeBuffer[] playerBulletDataCB;
    ComputeBuffer sourcePlayerBulletDataCB;
    ComputeBuffer targetPlayerBulletDataCB;

    BulletDatum[] enemyBulletData;
    ComputeBuffer[] enemyBulletDataCB;
    ComputeBuffer sourceEnemyBulletDataCB;
    ComputeBuffer targetEnemyBulletDataCB;

    uint[] playerBulletNum;
    ComputeBuffer[] playerBulletNumCB;
    ComputeBuffer sourcePlayerBulletNumCB;
    ComputeBuffer targetPlayerBulletNumCB;

    uint[] enemyBulletNum;
    ComputeBuffer[] enemyBulletNumCB;
    ComputeBuffer sourceEnemyBulletNumCB;
    ComputeBuffer targetEnemyBulletNumCB;

    BulletDatum[] playerShootRequestData;
    ComputeBuffer playerShootRequestDataCB;
    int playerShootRequestNum;

    BulletDatum[] bossShootRequestData;
    ComputeBuffer bossShootRequestDataCB;
    int bossShootRequestNum;

    BulletGridDatum[] bulletGridData;
    ComputeBuffer playerBulletGridDataCB;
    ComputeBuffer enemyBulletGridDataCB;

    EnemyGridDatum[] enemyGridData;
    ComputeBuffer enemyGridDataCB;

    BulletRenderingGridDatum[] bulletRenderingGridData;
    ComputeBuffer[] bulletRenderingGridDataCB;

    EnemyDatum[] sphereEnemyData;
    ComputeBuffer[] sphereEnemyDataCB;
    ComputeBuffer sourceSphereEnemyDataCB;
    ComputeBuffer targetSphereEnemyDataCB;

    ComputeBuffer enemyCollisionCacheDataCB;

    uint[] sphereEnemyNum;
    ComputeBuffer[] sphereEnemyNumCB;
    ComputeBuffer sourceSphereEnemyNumCB;
    ComputeBuffer targetSphereEnemyNumCB;

    EnemyDatum[] deployingSphereEnemyData;
    ComputeBuffer[] deployingSphereEnemyDataCB;
    ComputeBuffer sourceDeployingSphereEnemyDataCB;
    ComputeBuffer targetDeployingSphereEnemyDataCB;

    int[] deployingSphereEnemyNum;
    ComputeBuffer[] deployingSphereEnemyNumCB;
    ComputeBuffer sourceDeployingSphereEnemyNumCB;
    ComputeBuffer targetDeployingSphereEnemyNumCB;

    EnemyDatum[] cubeEnemyData;
    ComputeBuffer cubeEnemyDataCB;
    uint[] cubeEnemyNum;
    ComputeBuffer cubeEnemyNumCB;

    EnemyDatum[] createSphereEnemyRequestData;
    ComputeBuffer createSphereEnemyRequestDataCB;
    int createSphereEnemyRequestNum;

    EnemyDatum[] createCubeEnemyRequestData;
    ComputeBuffer createCubeEnemyRequestDataCB;
    int createCubeEnemyRequestNum;

    EnemyWeaponDatum[] enemyWeaponData;
    ComputeBuffer enemyWeaponDataCB;
    
    uint[] drawPlayerBulletArgs;
    ComputeBuffer drawPlayerBulletArgsCB;

    uint[] drawEnemyBulletArgs;
    ComputeBuffer drawEnemyBulletArgsCB;

    uint[] drawSphereEnemyArgs;
    ComputeBuffer drawSphereEnemyArgsCB;

    uint[] drawDeployingSphereEnemyArgs;
    ComputeBuffer drawDeployingSphereEnemyArgsCB;

    AvailablePositionDatum[] availablePositionData;
    ComputeBuffer availablePositionDataCB;

    int[] deadEnemyNum;
    ComputeBuffer deadEnemyNumCB;

    ComputeShader computeManagerCS;
    int playerShootKernel = -1;
    int bossShootKernel = -1;
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
    int skillTransferBulletTypeKernel = -1;
    int skillGetAvailablePositionKernel = -1;
    int updateDeployingEnemyKernel = -1;
    int resetBulletRenderingGridKernel = -1;
    int resolveBulletRenderingGrid1x1Kernel = -1;
    int resolveBulletRenderingGrid2x2Kernel = -1;
    int resolveBulletRenderingGrid4x4Kernel = -1;
    int resolveEnemyCollision1Kernel = -1;
    int resolveEnemyCollision2Kernel = -1;
    int applyEnemyGravityKernel = -1;
    int resetPlaneLightingTextureKernel = -1;
    int generatePlaneLightingTextureKernel = -1;
    int resolvePlaneLightingTextureKernel = -1;
    int gaussianBlurUKernel = -1;
    int gaussianBlurVKernel = -1;
    int processPlayerBulletBossCollisionKernel = -1;
    int knockOutAllEnemyKernel = -1;
    int processBossEnemyCollisionKernel = -1;
    int physicallyBasedBlurKernel = -1;
    int copyTextureKernel = -1;
    int copyTextureAndReverseYKernel = -1;
    int dftStepUKernel = -1;
    int dftStepVKernel = -1;
    int idftStepUKernel = -1;
    int idftStepVKernel = -1;
    int planeLightingFrequencyDomainMultiplyKernel = -1;
    int initializePlaneLightingTemporalConvolutionKernelKernel = -1;
    int padPlaneLightingTextureKernel = -1;
    int clearEnemyGridKernel = -1;
    int buildEnemyGridKernel = -1;


    Mesh playerBulletMesh;
    Material playerBulletMaterial;

    Mesh enemyBulletMesh;
    Material enemyBulletMaterial;

    Mesh sphereEnemyMesh;
    Material sphereEnemyMaterial;
    Material deployingSphereEnemyMaterial;

    const int planeLightingTextureWidth = 256;
    const int planeLightingTextureHeight = 256;
    const int fftTextureSize = 256; // 要求planeLightingTexture长宽一样
    RenderTexture planeLightingTexture;
    RenderTexture planeLightingTextureIn;
    RenderTexture planeLightingTextureInImag;
    RenderTexture planeLightingTextureTmp;
    RenderTexture planeLightingTextureTmpImag;
    RenderTexture planeLightingTextureOut;
    RenderTexture planeLightingTextureOutImag;
    RenderTexture planeLightingConvolutionKernel;
    RenderTexture planeLightingConvolutionKernelImag;


    public ComputeManager(GameManager _gameManager) 
    {
        gameManager = _gameManager;

        playerData = new PlayerDatum[2];
        playerDataCB = new ComputeBuffer(2, playerDatumSize);

        bossData = new BossDatum[1];
        bossDataCB = new ComputeBuffer(1, bossDatumSize);

        playerSkillData = new PlayerSkillDatum[1];
        playerSkillDataCB = new ComputeBuffer(1, playerSkillDatumSize);

        currentResourceCBIndex = 0;

        knockOutAllEnemyRequest = false;

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

        playerBulletNum = new uint[1];
        playerBulletNumCB = new ComputeBuffer[2];
        playerBulletNumCB[0] = new ComputeBuffer(1, sizeof(uint));
        playerBulletNumCB[1] = new ComputeBuffer(1, sizeof(uint));
        sourcePlayerBulletNumCB = playerBulletNumCB[0];
        targetPlayerBulletNumCB = playerBulletNumCB[1];

        enemyBulletNum = new uint[1];
        enemyBulletNumCB = new ComputeBuffer[2];
        enemyBulletNumCB[0] = new ComputeBuffer(1, sizeof(uint));
        enemyBulletNumCB[1] = new ComputeBuffer(1, sizeof(uint));
        sourceEnemyBulletNumCB = enemyBulletNumCB[0];
        targetEnemyBulletNumCB = enemyBulletNumCB[1];

        playerShootRequestData = new BulletDatum[maxNewBulletNum];
        playerShootRequestDataCB = new ComputeBuffer(maxNewBulletNum, bulletDatumSize);
        playerShootRequestNum = 0;

        bossShootRequestData = new BulletDatum[maxNewBulletNum];
        bossShootRequestDataCB = new ComputeBuffer(maxNewBulletNum, bulletDatumSize);
        bossShootRequestNum = 0;

        bulletGridData = new BulletGridDatum[bulletGridLengthX * bulletGridLengthZ];
        playerBulletGridDataCB = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletGridDatumSize);
        enemyBulletGridDataCB = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletGridDatumSize);

        enemyGridData = new EnemyGridDatum[enemyGridLengthX * enemyGridLengthZ];
        enemyGridDataCB = new ComputeBuffer(enemyGridLengthX * enemyGridLengthZ, enemyGridDatumSize);

        bulletRenderingGridData = new BulletRenderingGridDatum[bulletGridLengthX * bulletGridLengthZ];
        bulletRenderingGridDataCB = new ComputeBuffer[3];
        bulletRenderingGridDataCB[0] = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletRenderingGridDatumSize);
        bulletRenderingGridDataCB[1] = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletRenderingGridDatumSize);
        bulletRenderingGridDataCB[2] = new ComputeBuffer(bulletGridLengthX * bulletGridLengthZ, bulletRenderingGridDatumSize);

        sphereEnemyData = new EnemyDatum[maxEnemyNum];
        sphereEnemyDataCB = new ComputeBuffer[2];
        sphereEnemyDataCB[0] = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        sphereEnemyDataCB[1] = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        sourceSphereEnemyDataCB = sphereEnemyDataCB[0];
        targetSphereEnemyDataCB = sphereEnemyDataCB[1];

        sphereEnemyNum = new uint[1];
        sphereEnemyNumCB = new ComputeBuffer[2];
        sphereEnemyNumCB[0] = new ComputeBuffer(1, sizeof(uint));
        sphereEnemyNumCB[1] = new ComputeBuffer(1, sizeof(uint));
        sourceSphereEnemyNumCB = sphereEnemyNumCB[0];
        targetSphereEnemyNumCB = sphereEnemyNumCB[1];

        enemyCollisionCacheDataCB = new ComputeBuffer(maxEnemyNum, enemyCollisionCacheDatumSize);

        deployingSphereEnemyData = new EnemyDatum[maxDeployingEnemyNum];
        deployingSphereEnemyDataCB = new ComputeBuffer[2];
        deployingSphereEnemyDataCB[0] = new ComputeBuffer(maxDeployingEnemyNum, enemyDatumSize);
        deployingSphereEnemyDataCB[1] = new ComputeBuffer(maxDeployingEnemyNum, enemyDatumSize);
        sourceDeployingSphereEnemyDataCB = deployingSphereEnemyDataCB[0];
        targetDeployingSphereEnemyDataCB = deployingSphereEnemyDataCB[1];

        deployingSphereEnemyNum = new int[1];
        deployingSphereEnemyNumCB = new ComputeBuffer[2];
        deployingSphereEnemyNumCB[0] = new ComputeBuffer(1, sizeof(int));
        deployingSphereEnemyNumCB[1] = new ComputeBuffer(1, sizeof(int));
        sourceDeployingSphereEnemyNumCB = deployingSphereEnemyNumCB[0];
        targetDeployingSphereEnemyNumCB = deployingSphereEnemyNumCB[1];

        createSphereEnemyRequestData = new EnemyDatum[maxNewEnemyNum];
        createSphereEnemyRequestDataCB = new ComputeBuffer(maxNewEnemyNum, enemyDatumSize);
        createSphereEnemyRequestNum = 0;

        cubeEnemyData = new EnemyDatum[maxEnemyNum];
        cubeEnemyDataCB = new ComputeBuffer(maxEnemyNum, enemyDatumSize);
        cubeEnemyNum = new uint[1];
        cubeEnemyNumCB = new ComputeBuffer(1, sizeof(uint));

        createCubeEnemyRequestData = new EnemyDatum[maxNewEnemyNum];
        createCubeEnemyRequestDataCB = new ComputeBuffer(maxNewEnemyNum, enemyDatumSize);
        createCubeEnemyRequestNum = 0;

        enemyWeaponData = new EnemyWeaponDatum[maxEnemyWeaponNum];
        enemyWeaponDataCB = new ComputeBuffer(maxEnemyWeaponNum, enemyWeaponDatumSize);

        drawPlayerBulletArgs = new uint[5];
        drawPlayerBulletArgsCB = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        drawEnemyBulletArgs = new uint[5];
        drawEnemyBulletArgsCB = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        drawSphereEnemyArgs = new uint[5];
        drawSphereEnemyArgsCB = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        drawDeployingSphereEnemyArgs = new uint[5];
        drawDeployingSphereEnemyArgsCB = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        availablePositionData = new AvailablePositionDatum[1];
        availablePositionDataCB = new ComputeBuffer(1, availablePositionDatumSize);

        deadEnemyNum = new int[1];
        deadEnemyNumCB = new ComputeBuffer(1, sizeof(int));

        computeManagerCS = gameManager.computeManagerCS;
        playerShootKernel = computeManagerCS.FindKernel("PlayerShoot");
        bossShootKernel = computeManagerCS.FindKernel("BossShoot");
        updatePlayerBulletPositionKernel = computeManagerCS.FindKernel("UpdatePlayerBulletPosition");
        cullPlayerBulletKernel = computeManagerCS.FindKernel("CullPlayerBullet");
        processPlayerBulletCollisionKernel = computeManagerCS.FindKernel("ProcessPlayerBulletCollision");
        updateDrawPlayerBulletArgsKernel = computeManagerCS.FindKernel("UpdateDrawPlayerBulletArgs");
        createSphereEnemyKernel = computeManagerCS.FindKernel("CreateSphereEnemy");
        cullSphereEnemyKernel = computeManagerCS.FindKernel("CullSphereEnemy");
        updateEnemyVelocityAndPositionKernel = computeManagerCS.FindKernel("UpdateEnemyVelocityAndPosition");
        processPlayerEnemyCollisionKernel = computeManagerCS.FindKernel("ProcessPlayerEnemyCollision");
        enemyShootKernel = computeManagerCS.FindKernel("EnemyShoot");
        updateEnemyBulletVelocityAndPositionKernel = computeManagerCS.FindKernel("UpdateEnemyBulletVelocityAndPosition");
        cullEnemyBulletKernel = computeManagerCS.FindKernel("CullEnemyBullet");
        updateDrawEnemyBulletArgsKernel = computeManagerCS.FindKernel("UpdateDrawEnemyBulletArgs");
        processEnemyBulletCollisionKernel = computeManagerCS.FindKernel("ProcessEnemyBulletCollision");
        buildPlayerBulletGridKernel = computeManagerCS.FindKernel("BuildPlayerBulletGrid");
        buildEnemyBulletGridKernel = computeManagerCS.FindKernel("BuildEnemyBulletGrid");
        resetBulletGridKernel = computeManagerCS.FindKernel("ResetBulletGrid");
        processBulletBulletCollisionKernel = computeManagerCS.FindKernel("ProcessBulletBulletCollision");
        updateDrawEnemyArgsKernel = computeManagerCS.FindKernel("UpdateDrawEnemyArgs");
        skillTransferBulletTypeKernel = computeManagerCS.FindKernel("SkillTransferBulletType");
        skillGetAvailablePositionKernel = computeManagerCS.FindKernel("SkillGetAvailablePosition");
        updateDeployingEnemyKernel = computeManagerCS.FindKernel("UpdateDeployingEnemy");
        resetBulletRenderingGridKernel = computeManagerCS.FindKernel("ResetBulletRenderingGrid");
        resolveBulletRenderingGrid1x1Kernel = computeManagerCS.FindKernel("ResolveBulletRenderingGrid1x1");
        resolveBulletRenderingGrid2x2Kernel = computeManagerCS.FindKernel("ResolveBulletRenderingGrid2x2");
        resolveBulletRenderingGrid4x4Kernel = computeManagerCS.FindKernel("ResolveBulletRenderingGrid4x4");
        resolveEnemyCollision1Kernel = computeManagerCS.FindKernel("ResolveEnemyCollision1");
        resolveEnemyCollision2Kernel = computeManagerCS.FindKernel("ResolveEnemyCollision2");
        applyEnemyGravityKernel = computeManagerCS.FindKernel("ApplyEnemyGravity");
        resetPlaneLightingTextureKernel = computeManagerCS.FindKernel("ResetPlaneLightingTexture");
        generatePlaneLightingTextureKernel = computeManagerCS.FindKernel("GeneratePlaneLightingTexture");
        resolvePlaneLightingTextureKernel = computeManagerCS.FindKernel("ResolvePlaneLightingTexture");
        gaussianBlurUKernel = computeManagerCS.FindKernel("GaussianBlurU");
        gaussianBlurVKernel = computeManagerCS.FindKernel("GaussianBlurV");
        processPlayerBulletBossCollisionKernel = computeManagerCS.FindKernel("ProcessPlayerBulletBossCollision");
        knockOutAllEnemyKernel = computeManagerCS.FindKernel("KnockOutAllEnemy");
        processBossEnemyCollisionKernel = computeManagerCS.FindKernel("ProcessBossEnemyCollision");
        physicallyBasedBlurKernel = computeManagerCS.FindKernel("PhysicallyBasedBlur");
        copyTextureKernel = computeManagerCS.FindKernel("CopyTexture");
        copyTextureAndReverseYKernel = computeManagerCS.FindKernel("CopyTextureAndReverseY");
        dftStepUKernel = computeManagerCS.FindKernel("DFTStepU");
        dftStepVKernel = computeManagerCS.FindKernel("DFTStepV");
        idftStepUKernel = computeManagerCS.FindKernel("IDFTStepU");
        idftStepVKernel = computeManagerCS.FindKernel("IDFTStepV");
        planeLightingFrequencyDomainMultiplyKernel = computeManagerCS.FindKernel("PlaneLightingFrequencyDomainMultiply");
        initializePlaneLightingTemporalConvolutionKernelKernel = computeManagerCS.FindKernel("InitializePlaneLightingTemporalConvolutionKernel");
        padPlaneLightingTextureKernel = computeManagerCS.FindKernel("PadPlaneLightingTexture");
        clearEnemyGridKernel = computeManagerCS.FindKernel("ClearEnemyGrid");
        buildEnemyGridKernel = computeManagerCS.FindKernel("BuildEnemyGrid");

        //playerBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        playerBulletMesh = Resources.Load<GameObject>("bulletMesh").GetComponent<MeshFilter>().sharedMesh;
        playerBulletMaterial = Resources.Load<Material>("playerBullet");

        //enemyBulletMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        enemyBulletMesh = Resources.Load<GameObject>("bulletMesh").GetComponent<MeshFilter>().sharedMesh;
        enemyBulletMaterial = Resources.Load<Material>("enemyBullet");

        sphereEnemyMesh = GameObject.Find("Player1").GetComponent<MeshFilter>().mesh;
        sphereEnemyMaterial = Resources.Load<Material>("Enemy");
        deployingSphereEnemyMaterial = Resources.Load<Material>("DeployingEnemy");

        planeLightingTexture = new RenderTexture(planeLightingTextureWidth, planeLightingTextureHeight, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTexture.enableRandomWrite = true;
        planeLightingTexture.Create();

        planeLightingTextureIn = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTextureIn.enableRandomWrite = true;
        planeLightingTextureIn.Create();

        planeLightingTextureInImag = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTextureInImag.enableRandomWrite = true;
        planeLightingTextureInImag.Create();

        ClearPlaneLightingTexture(planeLightingTextureInImag);
        ClearPlaneLightingTexture(planeLightingTextureIn);

        planeLightingTextureTmp = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTextureTmp.enableRandomWrite = true;
        planeLightingTextureTmp.Create();

        planeLightingTextureTmpImag = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTextureTmpImag.enableRandomWrite = true;
        planeLightingTextureTmpImag.Create();

        planeLightingTextureOut = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTextureOut.enableRandomWrite = true;
        planeLightingTextureOut.Create();

        planeLightingTextureOutImag = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingTextureOutImag.enableRandomWrite = true;
        planeLightingTextureOutImag.Create();

        planeLightingConvolutionKernel = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingConvolutionKernel.enableRandomWrite = true;
        planeLightingConvolutionKernel.Create();

        planeLightingConvolutionKernelImag = new RenderTexture(fftTextureSize, fftTextureSize, 0, RenderTextureFormat.ARGBFloat);
        planeLightingConvolutionKernelImag.enableRandomWrite = true;
        planeLightingConvolutionKernelImag.Create();

        InitializeComputeBuffers();
        SetGlobalConstant();
        PrecomputePlaneLightingConvolutionKernel();
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
        playerSkillDataCB.SetData(playerSkillData);

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

        drawDeployingSphereEnemyArgs[0] = sphereEnemyMesh.GetIndexCount(0);
        drawDeployingSphereEnemyArgs[1] = 0;
        drawDeployingSphereEnemyArgs[2] = sphereEnemyMesh.GetIndexStart(0);
        drawDeployingSphereEnemyArgs[3] = sphereEnemyMesh.GetBaseVertex(0);
        drawDeployingSphereEnemyArgsCB.SetData(drawDeployingSphereEnemyArgs);

        sphereEnemyNum[0] = 0;
        sphereEnemyNumCB[0].SetData(sphereEnemyNum);
        sphereEnemyNumCB[1].SetData(sphereEnemyNum);
        cubeEnemyNum[0] = 0;
        cubeEnemyNumCB.SetData(cubeEnemyNum);

        deployingSphereEnemyNum[0] = 0;
        deployingSphereEnemyNumCB[0].SetData(deployingSphereEnemyNum);
        deployingSphereEnemyNumCB[1].SetData(deployingSphereEnemyNum);

        InitializeEnemyWeapon();
    }

    public void Release()
    {
        planeLightingTexture.Release();
        planeLightingTextureIn.Release();
        planeLightingTextureInImag.Release();
        planeLightingTextureTmp.Release();
        planeLightingTextureTmpImag.Release();
        planeLightingTextureOut.Release();
        planeLightingTextureOutImag.Release();
        planeLightingConvolutionKernel.Release();
        planeLightingConvolutionKernelImag.Release();

        playerDataCB.Release();
        bossDataCB.Release();
        playerSkillDataCB.Release();
        playerBulletDataCB[0].Release();
        playerBulletDataCB[1].Release();
        playerBulletNumCB[0].Release();
        playerBulletNumCB[1].Release();
        enemyBulletDataCB[0].Release();
        enemyBulletDataCB[1].Release();
        enemyBulletNumCB[0].Release();
        enemyBulletNumCB[1].Release();
        playerShootRequestDataCB.Release();
        bossShootRequestDataCB.Release();

        playerBulletGridDataCB.Release();
        enemyBulletGridDataCB.Release();
        enemyGridDataCB.Release();
        bulletRenderingGridDataCB[0].Release();
        bulletRenderingGridDataCB[1].Release();
        sphereEnemyDataCB[0].Release();
        sphereEnemyDataCB[1].Release();
        enemyCollisionCacheDataCB.Release();
        sphereEnemyNumCB[0].Release();
        sphereEnemyNumCB[1].Release();
        deployingSphereEnemyDataCB[0].Release();
        deployingSphereEnemyDataCB[1].Release();
        deployingSphereEnemyNumCB[0].Release();
        deployingSphereEnemyNumCB[1].Release();
        cubeEnemyDataCB.Release(); 
        cubeEnemyNumCB.Release();
        createSphereEnemyRequestDataCB.Release();
        createCubeEnemyRequestDataCB.Release();
        enemyWeaponDataCB.Release();
        drawPlayerBulletArgsCB.Release();
        drawEnemyBulletArgsCB.Release();
        drawSphereEnemyArgsCB.Release();
        drawDeployingSphereEnemyArgsCB.Release();
        availablePositionDataCB.Release();
        deadEnemyNumCB.Release();
    }


    public void UpdateGPU()
    {
        using (new GUtils.PFL("UpdateComputeGlobalConstant")) { UpdateComputeGlobalConstant(); }
        using (new GUtils.PFL("UpdatePlayerComputeBuffer")) { UpdatePlayerComputeBuffer(); }
        using (new GUtils.PFL("UpdateBossComputeBuffer")) { UpdateBossComputeBuffer(); }
        using (new GUtils.PFL("UpdatePlayerSkillComputeBuffer")) { UpdatePlayerSkillComputeBuffer(); }

        using (new GUtils.PFL("ExecutePlayerShootRequest")) { ExecutePlayerShootRequest(); }
        using (new GUtils.PFL("ExecuteBossShootRequest")) { ExecuteBossShootRequest(); }
        using (new GUtils.PFL("ExecuteCreateEnemyRequest")) { ExecuteCreateEnemyRequest(); }
        using (new GUtils.PFL("ExecuteKnockOutAllEnemyRequest")) { ExecuteKnockOutAllEnemyRequest(); }
        using (new GUtils.PFL("UpdateDeployingEnemy")) { UpdateDeployingEnemy(); }
        using (new GUtils.PFL("EnemyShoot")) { EnemyShoot(); }

        using (new GUtils.PFL("BuildBulletCollisionGrid")) { BuildBulletCollisionGrid(); }

        using (new GUtils.PFL("ProcessPlayerBulletCollision")) { ProcessPlayerBulletCollision(); }
        using (new GUtils.PFL("ProcessPlayerBulletBossCollision")) { ProcessPlayerBulletBossCollision(); }
        using (new GUtils.PFL("ProcessEnemyBulletCollision")) { ProcessEnemyBulletCollision(); }
        using (new GUtils.PFL("ProcessPlayerEnemyCollision")) { ProcessPlayerEnemyCollision(); } // 这个必须要放在敌人和子弹碰撞之后
        using (new GUtils.PFL("ProcessBossEnemyCollision")) { ProcessBossEnemyCollision(); }
        using (new GUtils.PFL("ProcessBulletBulletCollision")) { ProcessBulletBulletCollision(); }

        using (new GUtils.PFL("UpdatePlayerBulletPosition")) { UpdatePlayerBulletPosition(); }
        using (new GUtils.PFL("UpdateEnemyBulletVelocityAndPosition")) { UpdateEnemyBulletVelocityAndPosition(); }
        using (new GUtils.PFL("UpdateEnemyVelocityAndPosition")) { UpdateEnemyVelocityAndPosition(); }

        using (new GUtils.PFL("CullPlayerBullet")) { CullPlayerBullet(); }
        using (new GUtils.PFL("CullEnemyBullet")) { CullEnemyBullet(); }
        using (new GUtils.PFL("CullEnemy")) { CullEnemy(); }

        using (new GUtils.PFL("ClearPlayerShootRequest")) { ClearShootRequest(); }
        using (new GUtils.PFL("ClearCreateEnemyRequest")) { ClearCreateEnemyRequest(); }

        using (new GUtils.PFL("SwapBulletDataBuffer")) { SwapAndResetDataBuffer(); }

        using (new GUtils.PFL("SkillTransferBulletType")) { SkillTransferBulletType(); }
        using (new GUtils.PFL("SkillGetAvailablePosition")) { SkillGetAvailablePosition(); }

        using (new GUtils.PFL("BuildBulletRenderingGrid")) { BuildBulletRenderingGrid(); }
        using (new GUtils.PFL("GeneratePlaneLightingTexture")) { GeneratePlaneLightingTexture(); }

        using (new GUtils.PFL("UpdateGlobalBufferForRendering")) { UpdateGlobalBufferForRendering(); }
        using (new GUtils.PFL("DrawPlayerBullet")) { DrawPlayerBullet(); }
        using (new GUtils.PFL("DrawEnemyBullet")) { DrawEnemyBullet(); }
        using (new GUtils.PFL("DrawEnemy")) { DrawEnemy(); }

        using (new GUtils.PFL("SendReadbackRequest")) { SendReadbackRequest(); }
        using (new GUtils.PFL("SendDebugReadbackRequest")) { SendDebugReadbackRequest(); }

        if (true)
        {
            
        }
    }

    public void UpdateEnemyGrid()
    {
        int kernel = clearEnemyGridKernel;
        computeManagerCS.SetBuffer(kernel, "enemyGridData", enemyGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(enemyGridLengthX, 32), 1, GUtils.GetComputeGroupNum(enemyGridLengthZ, 32));

        kernel = buildEnemyGridKernel;
        computeManagerCS.SetBuffer(kernel, "enemyGridData", enemyGridDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 256), 1, 1);
    }

    public void PrecomputePlaneLightingConvolutionKernel()
    {
        // Get temporal kernel
        int kernel = initializePlaneLightingTemporalConvolutionKernelKernel;
        computeManagerCS.SetTexture(kernel, "planeLightingConvolutionKernel", planeLightingConvolutionKernel);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);

        // DFTU
        kernel = dftStepUKernel;
        int fftButterflySize = 1;
        if (fftButterflySize < fftTextureSize)
        {
            computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingConvolutionKernel);
            computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureInImag);
            computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
            computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
            computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 16), 1);
            SwapFFTTexture();
            fftButterflySize <<= 1;

            while (fftButterflySize < fftTextureSize)
            {
                computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureTmp);
                computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureTmpImag);
                computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
                computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
                computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
                computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 16), 1);
                SwapFFTTexture();
                fftButterflySize <<= 1;
            }
        }

        // DFTV
        kernel = dftStepVKernel;
        fftButterflySize = 1;
        while (fftButterflySize < fftTextureSize)
        {
            computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureTmp);
            computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureTmpImag);
            computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
            computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
            computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 16), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);
            SwapFFTTexture();
            fftButterflySize <<= 1;
        }

        // Copy the result
        kernel = copyTextureKernel;
        computeManagerCS.SetTexture(kernel, "textureFrom", planeLightingTextureTmp);
        computeManagerCS.SetTexture(kernel, "textureTo", planeLightingConvolutionKernel);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);
        computeManagerCS.SetTexture(kernel, "textureFrom", planeLightingTextureTmpImag);
        computeManagerCS.SetTexture(kernel, "textureTo", planeLightingConvolutionKernelImag);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);
    }

    public void PlaneLightingTextureBlurFFT()
    {
        // padding
        int kernel = padPlaneLightingTextureKernel;
        computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureIn);
        computeManagerCS.SetTexture(kernel, "planeLightingTexture", planeLightingTexture);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 32), GUtils.GetComputeGroupNum(planeLightingTextureHeight, 32), 1);

        // DFTU
        kernel = dftStepUKernel;
        int fftButterflySize = 1;
        if (fftButterflySize < fftTextureSize)
        {
            computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureIn);
            computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureInImag);
            computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
            computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
            computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 16), 1);
            SwapFFTTexture();
            fftButterflySize <<= 1;

            while (fftButterflySize < fftTextureSize)
            {
                computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureTmp);
                computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureTmpImag);
                computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
                computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
                computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
                computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 16), 1);
                SwapFFTTexture();
                fftButterflySize <<= 1;
            }
        }

        // DFTV
        kernel = dftStepVKernel;
        fftButterflySize = 1;
        while (fftButterflySize < fftTextureSize)
        {
            computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureTmp);
            computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureTmpImag);
            computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
            computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
            computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 16), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);
            SwapFFTTexture();
            fftButterflySize <<= 1;
        }

        // Multiply in frequency domain
        
        kernel = planeLightingFrequencyDomainMultiplyKernel;
        computeManagerCS.SetTexture(kernel, "planeLightingConvolutionKernel", planeLightingConvolutionKernel);
        computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureTmp);
        computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureTmpImag);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);
        

        // IDFTU
        kernel = idftStepUKernel;
        fftButterflySize = 1;
        while (fftButterflySize < fftTextureSize)
        {
            computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureTmp);
            computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureTmpImag);
            computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
            computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
            computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 32), GUtils.GetComputeGroupNum(fftTextureSize, 16), 1);
            SwapFFTTexture();
            fftButterflySize <<= 1;
        }

        // IDFTV
        kernel = idftStepVKernel;
        fftButterflySize = 1;
        while (fftButterflySize < fftTextureSize)
        {
            computeManagerCS.SetTexture(kernel, "fftInReal", planeLightingTextureTmp);
            computeManagerCS.SetTexture(kernel, "fftInImag", planeLightingTextureTmpImag);
            computeManagerCS.SetTexture(kernel, "fftOutReal", planeLightingTextureOut);
            computeManagerCS.SetTexture(kernel, "fftOutImag", planeLightingTextureOutImag);
            computeManagerCS.SetInt("fftButterflySize", fftButterflySize);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(fftTextureSize, 16), GUtils.GetComputeGroupNum(fftTextureSize, 32), 1);
            SwapFFTTexture();
            fftButterflySize <<= 1;
        }

        // Copy the result
        kernel = copyTextureKernel;
        computeManagerCS.SetTexture(kernel, "textureFrom", planeLightingTextureTmp);
        computeManagerCS.SetTexture(kernel, "textureTo", planeLightingTexture);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 32), GUtils.GetComputeGroupNum(planeLightingTextureHeight, 32), 1);
    }

    public void SwapFFTTexture()
    {
        RenderTexture tmp;

        tmp = planeLightingTextureOut;
        planeLightingTextureOut = planeLightingTextureTmp;
        planeLightingTextureTmp = tmp;

        tmp = planeLightingTextureOutImag;
        planeLightingTextureOutImag = planeLightingTextureTmpImag;
        planeLightingTextureTmpImag = tmp;
    }

    public void ClearPlaneLightingTexture(RenderTexture tex)
    {
        int kernel = resetPlaneLightingTextureKernel;
        computeManagerCS.SetTexture(kernel, "planeLightingTexture", tex);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 32), 1, GUtils.GetComputeGroupNum(planeLightingTextureHeight, 32));
    }

    public void SkillGetAvailablePosition()
    {
        availablePositionData[0].num = 0;
        availablePositionDataCB.SetData(availablePositionData);

        int kernel = skillGetAvailablePositionKernel;
        computeManagerCS.SetBuffer(kernel, "availablePositionData", availablePositionDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.Dispatch(kernel, 16, 1, 16); // 这个改了之后需要同步改compute shader
    }

    public void SkillTransferBulletType()
    {
        int state = GameManager.playerSkillManager.skills["SharedSkill0"].GetState();
        if (state == 3 || state == 4)
        {
            int kernel = skillTransferBulletTypeKernel;
            computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
            computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
            computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
            computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);

            enemyBulletNum[0] = 0;
            sourceEnemyBulletNumCB.SetData(enemyBulletNum);
        }
    }

    public void ProcessBulletBulletCollision()
    {
        int kernel = processBulletBulletCollisionKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX, 8), 1, GUtils.GetComputeGroupNum(bulletGridLengthZ, 8));
    }

    public void BuildBulletCollisionGrid()
    {
        int kernel = resetBulletGridKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX * bulletGridLengthZ, 256), 1, 1);

        kernel = buildPlayerBulletGridKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);

        kernel = buildEnemyBulletGridKernel;
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void BuildBulletRenderingGrid()
    {
        /*
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
        */

        int kernel = resetBulletRenderingGridKernel;
        computeManagerCS.SetBuffer(kernel, "bulletRenderingGridData1x1", bulletRenderingGridDataCB[0]);
        computeManagerCS.SetBuffer(kernel, "bulletRenderingGridData2x2", bulletRenderingGridDataCB[1]);
        computeManagerCS.SetBuffer(kernel, "bulletRenderingGridData4x4", bulletRenderingGridDataCB[2]);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX * bulletGridLengthZ, 256), 1, 1);

        kernel = resolveBulletRenderingGrid1x1Kernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletGridData", playerBulletGridDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletGridData", enemyBulletGridDataCB);
        computeManagerCS.SetBuffer(kernel, "bulletRenderingGridData1x1", bulletRenderingGridDataCB[0]);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "enemyGridData", enemyGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX, 8), 1, GUtils.GetComputeGroupNum(bulletGridLengthZ, 8));

        kernel = resolveBulletRenderingGrid2x2Kernel;
        computeManagerCS.SetBuffer(kernel, "bulletRenderingGridData1x1", bulletRenderingGridDataCB[0]);
        computeManagerCS.SetBuffer(kernel, "bulletRenderingGridData2x2", bulletRenderingGridDataCB[1]);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX, 8), 1, GUtils.GetComputeGroupNum(bulletGridLengthZ, 8));

        /*
        kernel = resolveBulletRenderingGrid4x4Kernel;
        computeCenterCS.SetBuffer(kernel, "bulletRenderingGridData2x2", bulletRenderingGridDataCB[1]);
        computeCenterCS.SetBuffer(kernel, "bulletRenderingGridData4x4", bulletRenderingGridDataCB[2]);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bulletGridLengthX, 8), 1, GUtils.GetComputeGroupNum(bulletGridLengthZ, 8));
        */
    }

    public void GeneratePlaneLightingTexture()
    {
        ClearPlaneLightingTexture(planeLightingTexture);

        int kernel = generatePlaneLightingTextureKernel;
        computeManagerCS.SetTexture(kernel, "planeLightingTexture", planeLightingTexture);
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);

        kernel = resolvePlaneLightingTextureKernel;
        computeManagerCS.SetTexture(kernel, "planeLightingTexture", planeLightingTexture);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 16), 1, GUtils.GetComputeGroupNum(planeLightingTextureHeight, 16));

        PlaneLightingTextureBlurFFT();

        // physically based blur
        /*
        kernel = physicallyBasedBlurKernel;
        computeCenterCS.SetTexture(kernel, "planeLightingTexture", planeLightingTexture);
        computeCenterCS.SetTexture(kernel, "planeLightingTextureTmp", planeLightingTextureTmp);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 32), GUtils.GetComputeGroupNum(planeLightingTextureHeight, 32), 1);

        kernel = copyTextureKernel;
        computeCenterCS.SetTexture(kernel, "textureFrom", planeLightingTextureTmp);
        computeCenterCS.SetTexture(kernel, "textureTo", planeLightingTexture);
        computeCenterCS.Dispatch(kernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 32), GUtils.GetComputeGroupNum(planeLightingTextureHeight, 32), 1);
        */

        // gaussian blur
        /*
        computeCenterCS.SetTexture(gaussianBlurUKernel, "planeLightingTexture", planeLightingTexture);
        computeCenterCS.SetTexture(gaussianBlurUKernel, "planeLightingTextureTmp", planeLightingTextureTmp);
        
        computeCenterCS.SetTexture(gaussianBlurVKernel, "planeLightingTexture", planeLightingTexture);
        computeCenterCS.SetTexture(gaussianBlurVKernel, "planeLightingTextureTmp", planeLightingTextureTmp);

        for (int i = 0; i < 10; i++)
        {
            computeCenterCS.Dispatch(gaussianBlurUKernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 160), GUtils.GetComputeGroupNum(planeLightingTextureHeight, 1), 1);
            computeCenterCS.Dispatch(gaussianBlurVKernel, GUtils.GetComputeGroupNum(planeLightingTextureWidth, 1), GUtils.GetComputeGroupNum(planeLightingTextureHeight, 150), 1);
        }
        */
    }

    public void SetGlobalConstant()
    {
        // gravity
        computeManagerCS.SetFloat("gravity", 9.8f);

        // plane
        computeManagerCS.SetFloat("planeXMin", -20.0f);
        computeManagerCS.SetFloat("planeXMax", 20.0f);
        computeManagerCS.SetFloat("planeZMin", -15.0f);
        computeManagerCS.SetFloat("planeZMax", 15.0f);

        // plane lighting
        computeManagerCS.SetFloat("planeLightingTextureWidth", planeLightingTextureWidth);
        computeManagerCS.SetFloat("planeLightingTextureHeight", planeLightingTextureHeight);
        computeManagerCS.SetFloat("planeLightingTexturePixelSizeX", 40.0f / planeLightingTextureWidth);
        computeManagerCS.SetFloat("planeLightingTexturePixelSizeY", 30.0f / planeLightingTextureHeight);
        computeManagerCS.SetFloats("planeSizeInv", 1.0f / 40.0f, 1.0f, 1.0f / 30.0f);
        computeManagerCS.SetInt("fftTextureSize", fftTextureSize);
        Shader.SetGlobalFloat("planeLightingTextureIntensity", 0.0f);

        // plane lighting gaussian blur precomputed weights
        float k = gameManager.planeLightingGaussianBlurCoeff;
        float[] weights = new float[7 * 4];
        float weightSum = 0.0f;
        for (int i = 0; i < 7; i++)
        {
            float weight = Mathf.Exp(-k * (i - 3) * (i - 3));
            weights[i * 4] = weight;
            weightSum += weight;
        }
        for (int i = 0; i < 7; i++)
        {
            weights[i * 4] /= weightSum;
        }
        computeManagerCS.SetFloats("planeLightingGaussianBlurWeight", weights);

        // screen
        computeManagerCS.SetFloat("screenWidth", (float)Screen.width);
        computeManagerCS.SetFloat("screenHeight", (float)Screen.height);
        Shader.SetGlobalFloat("screenWidth", (float)Screen.width);
        Shader.SetGlobalFloat("screenHeight", (float)Screen.height);

        // enemy movement
        computeManagerCS.SetFloat("enemySpacingAcceleration", 0.2f);
        computeManagerCS.SetFloat("enemyCollisionVelocityRestitution", 0.5f);

        // bullet grid
        computeManagerCS.SetInt("bulletGridLengthX", bulletGridLengthX);
        computeManagerCS.SetInt("bulletGridLengthZ", bulletGridLengthZ);
        computeManagerCS.SetVector("bulletGridBottomLeftPos", new Vector3(-32.0f, 0.5f, -16.0f));
        computeManagerCS.SetFloat("bulletGridSize", bulletGridSize);
        computeManagerCS.SetFloat("bulletGridSizeInv", bulletGridSizeInv);
        Shader.SetGlobalInt("bulletGridLengthX", bulletGridLengthX);
        Shader.SetGlobalInt("bulletGridLengthZ", bulletGridLengthZ);
        Shader.SetGlobalVector("bulletGridBottomLeftPos", new Vector3(-32.0f, 0.5f, -16.0f));
        Shader.SetGlobalFloat("bulletGridSize", bulletGridSize);
        Shader.SetGlobalFloat("bulletGridSizeInv", bulletGridSizeInv);
        Shader.SetGlobalVector("bulletGridBottomLeftCellCenterPos", new Vector3(-32.0f + 0.5f * bulletGridSize, 0.5f, -16.0f + 0.5f * bulletGridSize));

        // enemy grid
        computeManagerCS.SetInt("enemyGridLengthX", enemyGridLengthX);
        computeManagerCS.SetInt("enemyGridLengthZ", enemyGridLengthZ);
        computeManagerCS.SetInt("enemyGridLength", enemyGridLength);
        computeManagerCS.SetVector("enemyGridBottomLeftPos", new Vector3(-32.0f, 0.5f, -16.0f));
        computeManagerCS.SetFloat("enemyGridSize", enemyGridSize);
        computeManagerCS.SetFloat("enemyGridSizeInv", enemyGridSizeInv);

        // bullet color
        Shader.SetGlobalVector("player1BulletColor", gameManager.player1BulletColor);
        Shader.SetGlobalVector("player2BulletColor", gameManager.player2BulletColor);
        computeManagerCS.SetInt("packedPlayer1BulletColor", (int)GUtils.SRGBColorToLinearUInt(gameManager.player1BulletColor));
        computeManagerCS.SetInt("packedPlayer2BulletColor", (int)GUtils.SRGBColorToLinearUInt(gameManager.player2BulletColor));

        // lighting
        Vector3 lightDir = GameObject.Find("Directional Light").GetComponent<Transform>().forward;
        Shader.SetGlobalVector("bulletLightDir", -lightDir);
        Shader.SetGlobalFloat("bulletLightIntensity", gameManager.bulletDirectionalLightIntensity);
        Shader.SetGlobalFloat("bulletEmissionIntensity", gameManager.bulletEmissionIntensity);

        // frustum culling
        SetFrustumCullingGlobalConstant();

        // player 2 skill 0
        float player2Skill0V0 = 2.5f;
        float player2Skill0TMax = player2Skill0V0 * 0.2f;
        computeManagerCS.SetFloat("player2Skill0TMax", player2Skill0TMax);
        Shader.SetGlobalFloat("player2Skill0TMax", player2Skill0TMax);
        Shader.SetGlobalFloat("player2Skill0V0", player2Skill0V0);
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
        }

        float zMin = intersectionList[0].z;
        float zMax = intersectionList[2].z;
        float xAtZMin = intersectionList[1].x;
        float xAtZMax = intersectionList[3].x;
        float lerpCoefficient = 1.0f / (zMax - zMin);

        computeManagerCS.SetFloat("viewFrustrumZMin", zMin);
        computeManagerCS.SetFloat("viewFrustrumZMax", zMax);
        computeManagerCS.SetFloat("viewFrustrumXAtZMin", xAtZMin);
        computeManagerCS.SetFloat("viewFrustrumXAtZMax", xAtZMax);
        computeManagerCS.SetFloat("viewFrustrumCullingLerpCoefficient", lerpCoefficient);
    }

    public void ProcessEnemyBulletCollision()
    {
        int kernel = processEnemyBulletCollisionKernel;
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeManagerCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void CullEnemyBullet()
    {
        int kernel = cullEnemyBulletKernel;
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "culledEnemyBulletData", targetEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "culledEnemyBulletNum", targetEnemyBulletNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void UpdateEnemyBulletVelocityAndPosition()
    {
        int kernel = updateEnemyBulletVelocityAndPositionKernel;
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyBulletNum, 256), 1, 1);
    }

    public void EnemyShoot()
    {
        int kernel = enemyShootKernel;
        computeManagerCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeManagerCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "enemyWeaponData", enemyWeaponDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void InitializeEnemyWeapon()
    {
        float constantVirtualYRange = 0.06f;

        enemyWeaponData[0] = new EnemyWeaponDatum
        {
            shootInterval = -1.0f,
        };

        // 精准慢速
        enemyWeaponData[1] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 10.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 3.0f,
            extraBulletsPerSide = 0,
            angle = 90.0f,
            randomShootDelay = 5.0f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 精准中速
        enemyWeaponData[2] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 2.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.6f,
            extraBulletsPerSide = 0,
            angle = 0.0f,
            randomShootDelay = 0.1f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 中速散弹
        enemyWeaponData[3] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 2.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.8f,
            extraBulletsPerSide = 2,
            angle = 5.0f,
            randomShootDelay = 0.1f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 精准高速
        enemyWeaponData[4] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 4.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.15f,
            extraBulletsPerSide = 0,
            angle = 0.0f,
            randomShootDelay = 0.03f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 高速散弹
        enemyWeaponData[5] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 2.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.17f,
            extraBulletsPerSide = 3,
            angle = 8.0f,
            randomShootDelay = 0.03f,
            bulletSpeed = 6.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 可怕散弹
        enemyWeaponData[6] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 2.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.17f,
            extraBulletsPerSide = 4,
            angle = 8.0f,
            randomShootDelay = 0.03f,
            bulletSpeed = 8.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
            bulletImpulse = 0.1f,
            virtualYRange = constantVirtualYRange,
        };

        // 高速直线
        enemyWeaponData[7] = new EnemyWeaponDatum
        {
            uniformRandomAngleBias = 2.0f,
            individualRandomAngleBias = 0.0f,
            shootInterval = 0.08f,
            extraBulletsPerSide = 1,
            angle = 3.0f,
            randomShootDelay = 0.0f,
            bulletSpeed = 12.0f,
            bulletRadius = 0.07f,
            bulletDamage = 1,
            bulletBounces = 2,
            bulletLifeSpan = 20.0f,
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
            velocity = GameManager.player1.body.velocity,
        };
        playerData[1] = new PlayerDatum
        {
            pos = GameManager.player2.GetPos(),
            hpChange = 0,
            hitImpulse = new int3(0, 0, 0),
            size = 1.0f,
            hittable = GameManager.player2.hittable ? (uint)1 : 0,
            hitByEnemy = 0,
            velocity = GameManager.player2.body.velocity,
        };
        playerDataCB.SetData(playerData);
    }

    public void UpdateBossComputeBuffer()
    {
        bossData[0] = new BossDatum
        {
            pos = GameManager.boss.obj.transform.localPosition,
            hpChange = 0,
            hitImpulse = new int3(0, 0, 0),
            velocity = GameManager.boss.body.velocity,
        };
        bossDataCB.SetData(bossData);
    }

    public void UpdatePlayerSkillComputeBuffer()
    {
        // 技能状态数据已经在Skill.UpdateComputeBufferData()中更新

        playerSkillData[0].player2Skill0HPRestoration = 0;
        playerSkillData[0].player2Skill0HitEnemy = 0;
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

        AsyncGPUReadback.Request(bossDataCB, dataRequest =>
        {
            var readbackBossData = dataRequest.GetData<BossDatum>();
            GameManager.boss.OnProcessBossReadbackData(readbackBossData[0]);
        });

        AsyncGPUReadback.Request(playerSkillDataCB, dataRequest =>
        {
            var readbackPlayerSkillData = dataRequest.GetData<PlayerSkillDatum>();
            GameManager.player1.OnProcessPlayerSkillReadbackData(readbackPlayerSkillData[0]);
            GameManager.player2.OnProcessPlayerSkillReadbackData(readbackPlayerSkillData[0]);
            if (readbackPlayerSkillData[0].player2Skill0HitEnemy > 0)
            {
                GameManager.cameraMotionManager.ShakeByXYDisplacement();
            }
        });

        AsyncGPUReadback.Request(availablePositionDataCB, dataRequest =>
        {
            var availablePositionData = dataRequest.GetData<AvailablePositionDatum>();
            Player2Skill1.availablePosition1 = availablePositionData[0].pos1;
            Player2Skill1.availablePosition2 = availablePositionData[0].pos2;
            Player2Skill1.canTeleport = availablePositionData[0].num >= 2;
        });

        AsyncGPUReadback.Request(deadEnemyNumCB, dataRequest =>
        {
            var deadEnemyNumData = dataRequest.GetData<int>();
            GameManager.player1.exp += deadEnemyNumData[0];
            GameManager.player2.exp += deadEnemyNumData[0];
        });

        AsyncGPUReadback.Request(sourceSphereEnemyNumCB, dataRequest =>
        {
            var sphereEnemyNum = dataRequest.GetData<int>();
            GameManager.level.currentEnemyNum = sphereEnemyNum[0];
            GameManager.boss.state4enemyNum = sphereEnemyNum[0];
        });

        AsyncGPUReadback.Request(sourceDeployingSphereEnemyNumCB, dataRequest =>
        {
            var deployingSphereEnemyNum = dataRequest.GetData<int>();
            GameManager.level.currentDeployingEnemyNum = deployingSphereEnemyNum[0];
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
        int kernel = updateDrawPlayerBulletArgsKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "drawPlayerBulletArgs", drawPlayerBulletArgsCB);
        computeManagerCS.Dispatch(kernel, 1, 1, 1);

        Graphics.DrawMeshInstancedIndirect(
            playerBulletMesh,
            0,
            playerBulletMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawPlayerBulletArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.Off
        );
    }

    public void UpdateGlobalBufferForRendering()
    {
        Shader.SetGlobalBuffer("playerBulletData", sourcePlayerBulletDataCB);
        Shader.SetGlobalBuffer("enemyBulletData", sourceEnemyBulletDataCB);
        Shader.SetGlobalBuffer("playerSkillData", playerSkillDataCB);
        
        Shader.SetGlobalBuffer("playerBulletGridData", playerBulletGridDataCB);
        Shader.SetGlobalBuffer("enemyBulletGridData", enemyBulletGridDataCB);

        Shader.SetGlobalBuffer("bulletRenderingGridData1x1", bulletRenderingGridDataCB[0]);
        Shader.SetGlobalBuffer("bulletRenderingGridData2x2", bulletRenderingGridDataCB[1]);
        Shader.SetGlobalBuffer("bulletRenderingGridData4x4", bulletRenderingGridDataCB[2]);

        Shader.SetGlobalFloat("gameTime", GameManager.gameTime);

        Shader.SetGlobalFloat("bulletLightIntensity", gameManager.bulletDirectionalLightIntensity);
        Shader.SetGlobalFloat("bulletEmissionIntensity", gameManager.bulletEmissionIntensity);
        Shader.SetGlobalFloat("bulletLightingOnEnemyIntensity", gameManager.bulletLightingOnEnemyIntensity);

        Shader.SetGlobalTexture("planeLightingTexture", planeLightingTexture);
        Shader.SetGlobalFloat("planeLightingTextureIntensity", gameManager.planeLightingTextureIntensity);
    }

    public void DrawEnemyBullet()
    {
        int kernel = updateDrawEnemyBulletArgsKernel;
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "drawEnemyBulletArgs", drawEnemyBulletArgsCB);
        computeManagerCS.Dispatch(kernel, 1, 1, 1);

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
        int kernel = updateDrawEnemyArgsKernel;
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "deployingSphereEnemyNum", sourceDeployingSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "drawSphereEnemyArgs", drawSphereEnemyArgsCB);
        computeManagerCS.SetBuffer(kernel, "drawDeployingSphereEnemyArgs", drawDeployingSphereEnemyArgsCB);
        computeManagerCS.Dispatch(kernel, 1, 1, 1);

        Shader.SetGlobalBuffer("sphereEnemyData", sourceSphereEnemyDataCB);
        Shader.SetGlobalBuffer("deployingSphereEnemyData", sourceDeployingSphereEnemyDataCB);

        Graphics.DrawMeshInstancedIndirect(
            sphereEnemyMesh,
            0,
            sphereEnemyMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawSphereEnemyArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.On
        );

        Graphics.DrawMeshInstancedIndirect(
            sphereEnemyMesh,
            0,
            deployingSphereEnemyMaterial,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)),
            drawDeployingSphereEnemyArgsCB,
            castShadows: UnityEngine.Rendering.ShadowCastingMode.On
        );
    }

    public void ProcessPlayerBulletCollision()
    {
        UpdateEnemyGrid();

        int kernel = processPlayerBulletCollisionKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "enemyGridData", enemyGridDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);
    }

    public void ProcessPlayerBulletBossCollision()
    {
        int kernel = processPlayerBulletBossCollisionKernel;
        computeManagerCS.SetBuffer(kernel, "bossData", bossDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);
    }

    public void ProcessPlayerEnemyCollision()
    {
        int kernel = processPlayerEnemyCollisionKernel;
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void ProcessBossEnemyCollision()
    {
        int kernel = processBossEnemyCollisionKernel;
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "bossData", bossDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    // 这里的cull包含三个方面
    // 1. 剩余弹射次数
    // 2. 存在时间
    // 3. 位置是否在视锥体内
    // 把所有没有被剔除的bullet存放在culledPlayerBulletData里面用于draw indirect
    public void CullPlayerBullet()
    {
        int kernel = cullPlayerBulletKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "culledPlayerBulletData", targetPlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "culledPlayerBulletNum", targetPlayerBulletNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);
    }

    public void CullEnemy()
    {
        deadEnemyNum[0] = 0;
        deadEnemyNumCB.SetData(deadEnemyNum);

        int kernel = cullSphereEnemyKernel;
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "culledSphereEnemyData", targetSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "culledSphereEnemyNum", targetSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "drawSphereEnemyArgs", drawSphereEnemyArgsCB);
        computeManagerCS.SetBuffer(kernel, "deadEnemyNum", deadEnemyNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
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
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxPlayerBulletNum, 256), 1, 1);
    }
    public void UpdateEnemyVelocityAndPosition()
    {
        int kernel = updateEnemyVelocityAndPositionKernel;
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeManagerCS.SetBuffer(kernel, "playerSkillData", playerSkillDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);

        computeManagerCS.SetBuffer(resolveEnemyCollision1Kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(resolveEnemyCollision1Kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(resolveEnemyCollision1Kernel, "enemyCollisionCacheData", enemyCollisionCacheDataCB);
        computeManagerCS.SetBuffer(resolveEnemyCollision1Kernel, "enemyGridData", enemyGridDataCB);

        computeManagerCS.SetBuffer(resolveEnemyCollision2Kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(resolveEnemyCollision2Kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(resolveEnemyCollision2Kernel, "enemyCollisionCacheData", enemyCollisionCacheDataCB);
        
        for (int i = 0; i < 8; i++)
        {
            UpdateEnemyGrid();

            if (i == 0) computeManagerCS.SetFloat("resolveEnemyCollision2VelocityCoeff", 1.0f);
            else computeManagerCS.SetFloat("resolveEnemyCollision2VelocityCoeff", 0.3f);
            computeManagerCS.Dispatch(resolveEnemyCollision1Kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
            computeManagerCS.Dispatch(resolveEnemyCollision2Kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
        }
        
        kernel = applyEnemyGravityKernel;
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "playerData", playerDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
    }

    public void UpdateComputeGlobalConstant()
    {
        computeManagerCS.SetInt("playerShootRequestNum", playerShootRequestNum);
        computeManagerCS.SetInt("bossShootRequestNum", bossShootRequestNum);
        computeManagerCS.SetInt("createSphereEnemyRequestNum", createSphereEnemyRequestNum);
        computeManagerCS.SetFloat("deltaTime", GameManager.deltaTime);
        computeManagerCS.SetFloat("gameTime", GameManager.gameTime);

        Vector3 pPos = GameManager.player1.GetPos();
        computeManagerCS.SetFloats("player1Pos", pPos.x, pPos.y, pPos.z);
        pPos = GameManager.player2.GetPos();
        computeManagerCS.SetFloats("player2Pos", pPos.x, pPos.y, pPos.z);
    }

    public void AppendPlayerShootRequest(Vector3 _pos, Vector3 _dir, float _speed, float _radius, int _damage, int _bounces, float _lifeSpan, float _impulse, float _virtualY, int _player, float _renderingBiasY, uint _color, bool _affectedByPlayer1Skill1)
    {
        if (playerShootRequestNum >= maxNewBulletNum)
        {
            GUtils.LogWithCD("AppendPlayerShootRequest() playerShootRequestNum >= maxNewBulletNum");
            return;
        }

        uint _packedInfo = 0;
        _packedInfo |= (_player == 0 ? 0u : 1u) << 0;
        _packedInfo |= (_affectedByPlayer1Skill1 ? 1u : 0u) << 1;

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
            packedInfo = _packedInfo,
            renderingBiasY = _renderingBiasY,
            color = _color,
        };
        playerShootRequestNum++;
    }

    public void AppendBossShootRequest(Vector3 _pos, Vector3 _dir, float _speed, float _radius, int _damage, int _bounces, float _lifeSpan, float _impulse, float _virtualY, float _renderingBiasY, uint _color)
    {
        if (bossShootRequestNum >= maxNewBulletNum)
        {
            GUtils.LogWithCD("AppendBossShootRequest() bossShootRequestNum >= maxNewBulletNum");
            return;
        }

        bossShootRequestData[bossShootRequestNum] = new BulletDatum()
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
            packedInfo = 0,
            renderingBiasY = _renderingBiasY,
            color = _color,
        };
        bossShootRequestNum++;
    }

    public void AppendCreateSphereEnemyRequest(Vector3 _pos, Vector3 _velocity, int _maxHP, int _hp, float _size, float _radius, int3 _hitImpulse, int _weapon, float _lastShootTime, float _originalM, float _m, float _acceleration, float _frictionalDeceleration, float maxSpeed, uint _baseColor, float extraDelay)
    {
        createSphereEnemyRequestData[createSphereEnemyRequestNum] = new EnemyDatum()
        {
            pos = _pos,
            velocity = _velocity,
            maxHP = _maxHP,
            hp = _hp,
            size = _size,
            radius = _radius,
            hitImpulse = _hitImpulse,
            weapon = _weapon,
            lastShootTime = _lastShootTime,
            originalM = _originalM,
            m = _m,
            acceleration = _acceleration,
            frictionalDeceleration = _frictionalDeceleration,
            maxSpeed = maxSpeed,
            baseColor = _baseColor,
            lastHitByPlayer2Skill0Time = -99999.0f,
            createdTime = GameManager.gameTime + 3.0f + extraDelay,
            knockedOutByBoss = 0,
        };
        createSphereEnemyRequestNum++;
    }

    public void ExecutePlayerShootRequest()
    {
        Debug.Assert(playerShootRequestNum <= maxNewBulletNum);
        if (playerShootRequestNum == 0) return;

        playerShootRequestDataCB.SetData(playerShootRequestData);

        int kernel = playerShootKernel;
        computeManagerCS.SetBuffer(kernel, "playerBulletData", sourcePlayerBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "playerBulletNum", sourcePlayerBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "playerShootRequestData", playerShootRequestDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(playerShootRequestNum, 256), 1, 1);
    }

    public void ExecuteBossShootRequest()
    {
        Debug.Assert(bossShootRequestNum <= maxNewBulletNum);
        if (bossShootRequestNum == 0) return;

        bossShootRequestDataCB.SetData(bossShootRequestData);

        int kernel = bossShootKernel;
        computeManagerCS.SetBuffer(kernel, "enemyBulletData", sourceEnemyBulletDataCB);
        computeManagerCS.SetBuffer(kernel, "enemyBulletNum", sourceEnemyBulletNumCB);
        computeManagerCS.SetBuffer(kernel, "bossShootRequestData", bossShootRequestDataCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(bossShootRequestNum, 256), 1, 1);
    }

public void ExecuteCreateEnemyRequest()
    {
        Debug.Assert(createSphereEnemyRequestNum <= maxNewEnemyNum);

        if (createSphereEnemyRequestNum > 0)
        {
            createSphereEnemyRequestDataCB.SetData(createSphereEnemyRequestData);

            int kernel = createSphereEnemyKernel;
            computeManagerCS.SetBuffer(kernel, "deployingSphereEnemyData", sourceDeployingSphereEnemyDataCB);
            computeManagerCS.SetBuffer(kernel, "deployingSphereEnemyNum", sourceDeployingSphereEnemyNumCB);
            computeManagerCS.SetBuffer(kernel, "createSphereEnemyRequestData", createSphereEnemyRequestDataCB);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(createSphereEnemyRequestNum, 128), 1, 1);
        }
    }

    public void UpdateDeployingEnemy()
    {
        int kernel = updateDeployingEnemyKernel;
        computeManagerCS.SetBuffer(kernel, "deployingSphereEnemyData", sourceDeployingSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "deployingSphereEnemyNum", sourceDeployingSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "culledDeployingSphereEnemyData", targetDeployingSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "culledDeployingSphereEnemyNum", targetDeployingSphereEnemyNumCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
        computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
        computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxDeployingEnemyNum, 128), 1, 1);

        ComputeBuffer tmp = sourceDeployingSphereEnemyDataCB;
        sourceDeployingSphereEnemyDataCB = targetDeployingSphereEnemyDataCB;
        targetDeployingSphereEnemyDataCB = tmp;
        tmp = sourceDeployingSphereEnemyNumCB;
        sourceDeployingSphereEnemyNumCB = targetDeployingSphereEnemyNumCB;
        targetDeployingSphereEnemyNumCB = tmp;

        deployingSphereEnemyNum[0] = 0;
        targetDeployingSphereEnemyNumCB.SetData(deployingSphereEnemyNum);
    }

    public void ExecuteKnockOutAllEnemyRequest()
    {
        if (knockOutAllEnemyRequest)
        {
            knockOutAllEnemyRequest = false;

            int kernel = knockOutAllEnemyKernel;
            computeManagerCS.SetBuffer(kernel, "sphereEnemyData", sourceSphereEnemyDataCB);
            computeManagerCS.SetBuffer(kernel, "sphereEnemyNum", sourceSphereEnemyNumCB);
            computeManagerCS.Dispatch(kernel, GUtils.GetComputeGroupNum(maxEnemyNum, 128), 1, 1);
        }
    }

    public void ClearShootRequest()
    {
        playerShootRequestNum = 0;
        bossShootRequestNum = 0;
    }

    public void ClearCreateEnemyRequest()
    {
        createSphereEnemyRequestNum = 0;
        createCubeEnemyRequestNum = 0;
    }
}
