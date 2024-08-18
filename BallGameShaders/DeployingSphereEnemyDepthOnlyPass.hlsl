#ifndef UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED
#define UNIVERSAL_DEPTH_ONLY_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#if defined(LOD_FADE_CROSSFADE)
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
#endif

struct Attributes
{
    float4 position     : POSITION;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    #if defined(_ALPHATEST_ON)
        float2 uv       : TEXCOORD0;
    #endif
    float4 positionCS   : SV_POSITION;
    
    uint customInstanceId : TEXCOORD10;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

#include "Assets/Scripts/ShaderCommon.hlsl"
    
Varyings DepthOnlyVertex(Attributes input, uint instanceID : SV_InstanceID)
{
    Varyings output = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    #if defined(_ALPHATEST_ON)
        output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    #endif
    output.positionCS = GetDeployingEnemyVertexPositionCS(input.position.xyz, instanceID);
    output.customInstanceId = instanceID;
    return output;
}

half DepthOnlyFragment(Varyings input) : SV_TARGET
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    #if defined(_ALPHATEST_ON)
        Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
    #endif

    #if defined(LOD_FADE_CROSSFADE)
        LODFadeCrossFade(input.positionCS);
    #endif
    
    EnemyDatum enemy = deployingSphereEnemyData[input.customInstanceId];
    
    if (enemy.createdTime - gameTime >= 3.0f)
    {
        discard;
    }
    else
    {
        float2 uv = GetNormalizedScreenSpaceUV(input.positionCS);
        float dither = GetDither8x8(uv);
        float rate = (enemy.createdTime - gameTime) * 0.33333f;
            float desiredAlpha = lerp(1.0f, 0.2f, pow(rate, 0.2f) - 0.4f);
        if (dither > desiredAlpha)
            discard;
    }
        
    return input.positionCS.z;
}
#endif
