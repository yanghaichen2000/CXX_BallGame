#ifndef UNIVERSAL_FORWARD_LIT_PASS_INCLUDED
#define UNIVERSAL_FORWARD_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

// GLES2 has limited amount of interpolators
#if defined(_PARALLAXMAP) && !defined(SHADER_API_GLES)
#define REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR
#endif

#if (defined(_NORMALMAP) || (defined(_PARALLAXMAP) && !defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR))) || defined(_DETAIL)
#define REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR
#endif

// keep this file in sync with LitGBufferPass.hlsl

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 staticLightmapUV   : TEXCOORD1;
    float2 dynamicLightmapUV  : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    float3 positionWS               : TEXCOORD1;
#endif

    float3 normalWS                 : TEXCOORD2;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    half4 tangentWS                : TEXCOORD3;    // xyz: tangent, w: sign
#endif

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    half4 fogFactorAndVertexLight   : TEXCOORD5; // x: fogFactor, yzw: vertex light
#else
    half  fogFactor                 : TEXCOORD5;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    float4 shadowCoord              : TEXCOORD6;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS                : TEXCOORD7;
#endif

    DECLARE_LIGHTMAP_OR_SH(staticLightmapUV, vertexSH, 8);
#ifdef DYNAMICLIGHTMAP_ON
    float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
#endif

    float4 positionCS               : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO

    uint customInstanceId : TEXCOORD10;
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    inputData.positionWS = input.positionWS;
#endif

    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
#if defined(_NORMALMAP) || defined(_DETAIL)
    float sgn = input.tangentWS.w;      // should be either +1 or -1
    float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);

    #if defined(_NORMALMAP)
    inputData.tangentToWorld = tangentToWorld;
    #endif
    inputData.normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
#else
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    inputData.viewDirectionWS = viewDirWS;

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    inputData.shadowCoord = input.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
    inputData.shadowCoord = TransformWorldToShadowCoord(inputData.positionWS);
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactorAndVertexLight.x);
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
#else
    inputData.fogCoord = InitializeInputDataFog(float4(input.positionWS, 1.0), input.fogFactor);
#endif

#if defined(DYNAMICLIGHTMAP_ON)
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
#else
    inputData.bakedGI = SAMPLE_GI(input.staticLightmapUV, input.vertexSH, inputData.normalWS);
#endif

    inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
    inputData.shadowMask = SAMPLE_SHADOWMASK(input.staticLightmapUV);

    #if defined(DEBUG_DISPLAY)
    #if defined(DYNAMICLIGHTMAP_ON)
    inputData.dynamicLightmapUV = input.dynamicLightmapUV;
    #endif
    #if defined(LIGHTMAP_ON)
    inputData.staticLightmapUV = input.staticLightmapUV;
    #else
    inputData.vertexSH = input.vertexSH;
    #endif
    #endif
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

#include "Assets/Scripts/ShaderCommon.hlsl"

VertexPositionInputs GetVertexPositionInputsNew(float3 positionOS, uint instanceID)
{
    VertexPositionInputs input;
    EnemyDatum datum = sphereEnemyData[instanceID];
    input.positionWS = positionOS * datum.size + datum.pos;
    
    float t = gameTime - datum.lastHitByPlayer2Skill0Time; // timeSinceHitByPlayer2Skill0
    if (t < player2Skill0TMax)
    {
        input.positionWS += player2Skill0V0 * t - 5.0f * t * t;
    }
    
        input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);
 
    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;
 
    return input;
}

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input, uint instanceID : SV_InstanceID)
{
    Varyings output = (Varyings) 0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    
    output.customInstanceId = instanceID;
    
    VertexPositionInputs vertexInput = GetVertexPositionInputsNew(input.positionOS.xyz, instanceID);

    // normalWS and tangentWS already normalize.
    // this is required to avoid skewing the direction during interpolation
    // also required for per-vertex lighting and SH evaluation
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);

    half fogFactor = 0;
#if !defined(_FOG_FRAGMENT)
    fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
#endif

    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    // already normalized from normal transform to WS.
    output.normalWS = normalInput.normalWS;
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR) || defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    real sign = input.tangentOS.w * GetOddNegativeScale();
    half4 tangentWS = half4(normalInput.tangentWS.xyz, sign);
#endif
#if defined(REQUIRES_WORLD_SPACE_TANGENT_INTERPOLATOR)
    output.tangentWS = tangentWS;
#endif

#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(tangentWS, output.normalWS, viewDirWS);
    output.viewDirTS = viewDirTS;
#endif

    OUTPUT_LIGHTMAP_UV(input.staticLightmapUV, unity_LightmapST, output.staticLightmapUV);
#ifdef DYNAMICLIGHTMAP_ON
    output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#endif
    OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
#ifdef _ADDITIONAL_LIGHTS_VERTEX
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
#else
    output.fogFactor = fogFactor;
#endif

#if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
    output.positionWS = vertexInput.positionWS;
#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;

    return output;
}

/*
struct InputData 
{
    float3 positionWS;
    float3 normalWS;
}
*/

// Used in Standard (Physically Based) shader
void LitPassFragment(
    Varyings input
    , out half4 outColor : SV_Target0
#ifdef _WRITE_RENDERING_LAYERS
    , out float4 outRenderingLayers : SV_Target1
#endif
)
{
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

#if defined(_PARALLAXMAP)
#if defined(REQUIRES_TANGENT_SPACE_VIEW_DIR_INTERPOLATOR)
    half3 viewDirTS = input.viewDirTS;
#else
    half3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
    half3 viewDirTS = GetViewDirectionTangentSpace(input.tangentWS, input.normalWS, viewDirWS);
#endif
    ApplyPerPixelDisplacement(viewDirTS, input.uv);
#endif

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uv, surfaceData);

#ifdef LOD_FADE_CROSSFADE
    LODFadeCrossFade(input.positionCS);
#endif

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);
    SETUP_DEBUG_TEXTURE_DATA(inputData, input.uv, _BaseMap);

#ifdef _DBUFFER
    ApplyDecalToSurfaceData(input.positionCS, surfaceData, inputData);
#endif

    
    EnemyDatum enemy = sphereEnemyData[input.customInstanceId];
    
    // surface data
    float3 enemyColor = PackeduintColorToFloat3(enemy.baseColor);
    float enemyHP = max(enemy.hp, 0.0f);
    float enemyCondition = 1.0f - enemyHP / enemy.maxHP;
    enemyCondition = pow(enemyCondition, 2.0f);
    surfaceData.albedo = lerp(enemyColor, float3(0.4f, 0.4f, 0.4f), enemyCondition);
    if (enemy.hp <= 0)
    {
        half2 uv = input.uv;
        half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
        surfaceData.albedo = lerp(surfaceData.albedo, texColor, 0.4f);
    }
    surfaceData.smoothness = lerp(surfaceData.smoothness, 0.0f, enemyCondition);
    
    // basic
    half4 color = UniversalFragmentPBR(inputData, surfaceData);
    
    // bullet
    float3 inputRadiance = float3(0.0f, 0.0f, 0.0f);
    int anchorX;
    int anchorZ;
    GetBulletGridXZFromPos(enemy.pos - float3(0.1f, 0.0f, 0.1f), anchorX, anchorZ);
    
    
    // 1x1 cell
    for (int x = anchorX - 3; x <= anchorX + 4; x++)
    {
        for (int z = anchorZ - 3; z <= anchorZ + 4; z++)
        {
            if (x < 0 || x >= bulletGridLengthX || z < 0 || z >= bulletGridLengthZ) continue;
            if (x >= anchorX - 1 && x <= anchorX + 2 && z >= anchorZ - 1 && z <= anchorZ + 2) continue;
            
            float3 cellRelativePos = float3(float(x - anchorX), 0.0f, float(z - anchorZ));
            if (dot(cellRelativePos, inputData.normalWS) < -0.05)
                continue;
            
            BulletRenderingGridDatum datum = bulletRenderingGridData1x1[z * bulletGridLengthX + x];
            for (int i = 0; i < datum.size; i++)
            {
                float3 dir = datum.pos[i] - inputData.positionWS;
                float distance = length(dir);
                dir = normalize(dir);
                float cosine = saturate(dot(dir, normalize(inputData.normalWS)));
                float distanceFade = 1.0f / (1.0f + distance);
                distanceFade = distanceFade * distanceFade;
                float3 bulletColor = datum.color[0];
                inputRadiance += bulletColor * distanceFade * cosine;
            }
        }
    }
    
    
    // 2x2 cell
    for (int x = anchorX - 7; x <= anchorX + 7; x += 2)
    {
        for (int z = anchorZ - 7; z <= anchorZ + 7; z += 2)
        {
            if (x < 0 || x >= bulletGridLengthX || z < 0 || z >= bulletGridLengthZ)
                continue;
            if (x >= anchorX - 3 && x <= anchorX + 4 && z >= anchorZ - 3 && z <= anchorZ + 4)
                continue;
            
            float3 cellRelativePos = float3(float(x - anchorX), 0.0f, float(z - anchorZ));
            if (dot(cellRelativePos, inputData.normalWS) < -0.05)
                continue;
            
            BulletRenderingGridDatum datum = bulletRenderingGridData2x2[z * bulletGridLengthX + x];
            for (int i = 0; i < datum.size; i++)
            {
                float3 dir = datum.pos[i] - inputData.positionWS;
                float distance = length(dir);
                dir = normalize(dir);
                float cosine = saturate(dot(dir, normalize(inputData.normalWS)));
                float distanceFade = 1.0f / (1.0f + distance);
                distanceFade = distanceFade * distanceFade;
                float3 bulletColor = datum.color[0];
                inputRadiance += bulletColor * distanceFade * cosine;
            }
        }
    }
    
    /*
    // 4x4 cell
    for (int x = anchorX - 11; x <= anchorX + 9; x += 4)
    {
        for (int z = anchorZ - 11; z <= anchorZ + 9; z += 4)
        {
            if (x < 0 || x >= bulletGridLengthX || z < 0 || z >= bulletGridLengthZ)
                continue;
            if (x >= anchorX - 7 && x <= anchorX + 8 && z >= anchorZ - 7 && z <= anchorZ + 8)
                continue;
            
            BulletRenderingGridDatum datum = bulletRenderingGridData4x4[z * bulletGridLengthX + x];
            for (int i = 0; i < datum.size; i++)
            {
                float3 dir = datum.pos[i] - inputData.positionWS;
                float distance = length(dir);
                dir = normalize(dir);
                float cosine = saturate(dot(dir, normalize(inputData.normalWS)));
                float distanceFade = 1.0f / (1.0f + distance);
                distanceFade = distanceFade * distanceFade;
                float3 bulletColor = datum.color[0];
                inputRadiance += bulletColor * distanceFade * cosine;
            }
        }
    }
    */
    
    // diffuse lighting from bullets
    color.rgb += inputRadiance * surfaceData.albedo;
    
    
    color.rgb = MixFog(color.rgb, inputData.fogCoord);
    color.a = OutputAlpha(color.a, IsSurfaceTypeTransparent(_Surface));
    
    outColor = color;

#ifdef _WRITE_RENDERING_LAYERS
    uint renderingLayers = GetMeshRenderingLayer();
    outRenderingLayers = float4(EncodeMeshRenderingLayer(renderingLayers), 0, 0, 0);
#endif
}

#endif
