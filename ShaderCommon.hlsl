#ifndef BALL_GAME_SHADER_COMMON
#define BALL_GAME_SHADER_COMMON

struct PlayerSkillDatum
{
    int player1Skill0;
    int player1Skill1;
    int player2Skill0;
    int player2Skill1;
    int sharedSkill0;
    int sharedSkill1;
    int player2Skill0HPRestoration;
    int tmp2;
};

struct BulletDatum
{
    float3 pos;
    float3 dir;
    float speed;
    float radius;
    int damage;
    uint bounces;
    float expirationTime;
    float impulse;
    float virtualY;
    uint packedInfo;
    float renderingBiasY;
    uint color;
};

struct EnemyDatum
{
    float3 pos;
    float3 velocity;
    int maxHP;
    int hp;
    float size;
    float radius;
    int3 hitImpulse;
    int weapon;
    float lastShootTime;
    float originalM;
    float m;
    float acceleration;
    float frictionalDeceleration;
    float maxSpeed;
    uint baseColor;
    float lastHitByPlayer2Skill0Time;
    float createdTime;
    float tmp;
};

#define BULLET_GRID_CAPACITY 28
struct BulletGridDatum
{
    int size;
    float tmp1;
    float tmp2;
    float tmp3;
    int bulletIndexList[BULLET_GRID_CAPACITY];
};

struct BulletRenderingGridDatum
{
    int size;
    float3 pos[4];
    float3 color[4];
};

StructuredBuffer<EnemyDatum> sphereEnemyData;
StructuredBuffer<EnemyDatum> deployingSphereEnemyData;

StructuredBuffer<BulletDatum> playerBulletData;
StructuredBuffer<BulletDatum> enemyBulletData;

StructuredBuffer<BulletGridDatum> playerBulletGridData;
StructuredBuffer<BulletGridDatum> enemyBulletGridData;

StructuredBuffer<BulletRenderingGridDatum> bulletRenderingGridData1x1;
StructuredBuffer<BulletRenderingGridDatum> bulletRenderingGridData2x2;
StructuredBuffer<BulletRenderingGridDatum> bulletRenderingGridData4x4;

StructuredBuffer<PlayerSkillDatum> playerSkillData;

float3 player1BulletColor;
float3 player2BulletColor;
float3 enemyBulletColor;

float3 bulletLightDir;
float bulletLightIntensity;
float bulletEmissionIntensity;
float bulletLightingOnEnemyIntensity;

float gameTime;

float sharedSkill0LastTriggeredTime;
float sharedSkill0CdStartTime;

float player2Skill0TMax;
float player2Skill0V0;

float screenWidth;
float screenHeight;

int bulletGridLengthX;
int bulletGridLengthZ;
float3 bulletGridBottomLeftPos;
float bulletGridSize;
float bulletGridSizeInv;
float3 bulletGridBottomLeftCellCenterPos;

float planeLightingTextureIntensity;
sampler2D planeLightingTexture;

inline int GetBulletGridIndexFromPos(float3 pos)
{
    int3 xyz = floor((pos - bulletGridBottomLeftPos + 0.00001f) * bulletGridSizeInv);
    xyz = clamp(xyz, int3(0, 0, 0), int3(bulletGridLengthX - 1, 0, bulletGridLengthZ - 1));
    return xyz.z * bulletGridLengthX + xyz.x;
}

inline void GetBulletGridXZFromPos(float3 pos, inout int x, inout int z)
{
    int3 xyz = floor((pos - bulletGridBottomLeftPos + 0.00001f) * bulletGridSizeInv);
    xyz = clamp(xyz, int3(0, 0, 0), int3(bulletGridLengthX - 1, 0, bulletGridLengthZ - 1));
    x = xyz.x;
    z = xyz.z;
}

inline int GetBulletGridIndexFromXZ(int x, int z)
{
    return z * bulletGridLengthX + x;
}

inline float3 GetCellPosFromXZ(int x, int z)
{
    return bulletGridBottomLeftCellCenterPos + float3(x, 0.0f, z) * bulletGridSize;
}

VertexPositionInputs GetPlayerBulletVertexPositionInputs(float3 positionOS, uint instanceID)
{
    VertexPositionInputs input;
    BulletDatum datum = playerBulletData[instanceID];
    input.positionWS = positionOS * datum.radius * 2 + datum.pos + float3(0.0f, datum.renderingBiasY, 0.0f);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);
 
    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;
 
    return input;
}

float4 GetPlayerBulletVertexPositionCS(float3 positionOS, uint instanceID)
{
    BulletDatum datum = playerBulletData[instanceID];
    float3 positionWS = positionOS * datum.radius * 2 + datum.pos + float3(0.0f, datum.renderingBiasY, 0.0f);
    return TransformWorldToHClip(positionWS);
}

float4 GetEnemyVertexPositionCS(float3 positionOS, uint instanceID)
{
    EnemyDatum datum = sphereEnemyData[instanceID];
    float3 positionWS = positionOS * datum.size + datum.pos;
    return TransformWorldToHClip(positionWS);
}

VertexPositionInputs GetEnemyBulletVertexPositionInputs(float3 positionOS, uint instanceID)
{
    VertexPositionInputs input;
    BulletDatum datum = enemyBulletData[instanceID];
    input.positionWS = positionOS * datum.radius * 2 + datum.pos;
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);
 
    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;
 
    return input;
}

float4 GetEnemyBulletVertexPositionCS(float3 positionOS, uint instanceID)
{
    BulletDatum datum = enemyBulletData[instanceID];
    float3 positionWS = positionOS * datum.radius * 2 + datum.pos + float3(0.0f, datum.renderingBiasY, 0.0f);
    return TransformWorldToHClip(positionWS);
}

float3 BulletDiffuseShading(float3 baseColor, float3 normal)
{
    return baseColor * bulletLightIntensity * saturate(dot(bulletLightDir, normal)) +
            bulletEmissionIntensity * baseColor;
}

float3 BulletBlinnPhongShading(float3 baseColor, float3 normal)
{
    float d = saturate(dot(bulletLightDir, normal));
    float d2 = d * d;
    float d4 = d2 * d2;
    return float3(0.1f, 0.1f, 0.1f) * baseColor + lerp(
        baseColor * bulletLightIntensity * d,
        baseColor * bulletLightIntensity * d4 * 5.0f,
        0.1f);
}

// (hue, saturation, value)
float3 HSVToRGB(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float3 PackeduintColorToFloat3(uint color)
{
    float3 ret;
    ret.r = (color >> 24 & 255) / 255.0f;
    ret.g = (color >> 16 & 255) / 255.0f;
    ret.b = (color >> 8 & 255) / 255.0f;
    
    return ret;
}

float GetDither4x4(float2 uv, int scale = 1)
{
    static const float4x4 ditherMatrix = float4x4(
        0.0f, 8.0f, 2.0f, 10.0f,
        12.0f, 4.0f, 14.0f, 6.0f,
        3.0f, 11.0f, 1.0f, 9.0f,
        15.0f, 7.0f, 13.0f, 5.0f
    );
    
    return ditherMatrix[((int) (uv.x * screenWidth - 0.5f) / scale) % 4][((int) (uv.y * screenHeight - 0.5f) / scale) % 4] / 16.0f;
}

float GetDither8x8(float2 uv, int scale = 1)
{
    static const float ditherMatrix[64] =
    {
        0.0f, 32.0f, 8.0f, 40.0f, 2.0f, 34.0f, 10.0f, 42.0f,
        48.0f, 16.0f, 56.0f, 24.0f, 50.0f, 18.0f, 58.0f, 26.0f,
        12.0f, 44.0f, 4.0f, 36.0f, 14.0f, 46.0f, 6.0f, 38.0f,
        60.0f, 28.0f, 52.0f, 20.0f, 62.0f, 30.0f, 54.0f, 22.0f,
        3.0f, 35.0f, 11.0f, 43.0f, 1.0f, 33.0f, 9.0f, 41.0f,
        51.0f, 19.0f, 50.0f, 27.0f, 49.0f, 17.0f, 57.0f, 25.0f,
        15.0f, 47.0f, 7.0f, 39.0f, 13.0f, 45.0f, 5.0f, 37.0f,
        63.0f, 31.0f, 55.0f, 23.0f, 61.0f, 29.0f, 53.0f, 21.0f
    };
    
    return ditherMatrix[((int) (uv.x * screenWidth - 0.5f) / scale) % 8 * 8 + ((int) (uv.y * screenHeight - 0.5f) / scale) % 8] / 64.0f;
}

#endif