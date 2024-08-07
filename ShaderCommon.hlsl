#ifndef BALL_GAME_SHADER_COMMON
#define BALL_GAME_SHADER_COMMON

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
	int player;
    float renderingBiasY;
	float tmp2;
};

StructuredBuffer<BulletDatum> playerBulletData;
StructuredBuffer<BulletDatum> enemyBulletData;

float3 player1BulletColor;
float3 player2BulletColor;
float3 enemyBulletColor;

float3 bulletLightDir;
float bulletLightIntensity;

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
    return baseColor * bulletLightIntensity * saturate(dot(bulletLightDir, normal)) + float3(0.2f, 0.2f, 0.2f);
}

float3 BulletBlinnPhongShading(float3 baseColor, float3 normal)
{
    float d = saturate(dot(bulletLightDir, normal));
    float d2 = d * d;
    float d4 = d2 * d2;
    return float3(0.2f, 0.2f, 0.2f) + lerp(
        baseColor * bulletLightIntensity * d,
        baseColor * bulletLightIntensity * d4 * 5.0f,
        0.2f);
}

#endif