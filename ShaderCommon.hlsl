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
float3 player1BulletColor;
float3 player2BulletColor;

StructuredBuffer<BulletDatum> enemyBulletData;

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

#endif