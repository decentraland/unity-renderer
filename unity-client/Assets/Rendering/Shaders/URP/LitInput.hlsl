#ifndef LIGHTWEIGHT_LIT_INPUT_INCLUDED
#define LIGHTWEIGHT_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;
float _CullYPlane;
half _FadeThickness;
half _FadeDirection;
int _BaseMapUVs;
int _NormalMapUVs;
int _MetallicMapUVs;
int _EmissiveMapUVs;

TEXTURE2D(_OcclusionMap);       SAMPLER(sampler_OcclusionMap);
TEXTURE2D(_MetallicGlossMap);   SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
TEXTURE2D(_AlphaTexture);       SAMPLER(sampler_AlphaTexture);
CBUFFER_END

#ifdef _SPECULAR_SETUP
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv)
#else
    #define SAMPLE_METALLICSPECULAR(uv) SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv)
#endif

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss;

#ifdef _METALLICSPECGLOSSMAP
    specGloss = SAMPLE_METALLICSPECULAR(uv);

    //GLTF Provides Metallic in B and Roughness in G
    specGloss.a = 1.0 - specGloss.g; //Conversion to GLTF and from RoughnessToSmoothness
    specGloss.rgb = specGloss.bbb; //Conversion to GLTF

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a *= _Smoothness;
    #endif
    specGloss.rgb *= _Metallic.rrr;
#else // _METALLICSPECGLOSSMAP
    #if _SPECULAR_SETUP
        specGloss.rgb = _SpecColor.rgb;
    #else
        specGloss.rgb = _Metallic.rrr;
    #endif

    #ifdef _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        specGloss.a = albedoAlpha * _Smoothness;
    #else
        specGloss.a = _Smoothness;
    #endif
#endif

    return specGloss;
}

half SampleOcclusion(float2 uv)
{
#ifdef _OCCLUSIONMAP
// TODO: Controls things like these by exposing SHADER_QUALITY levels (low, medium, high)
#if defined(SHADER_API_GLES)
    return SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
#else
    half occ = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).g;
    return LerpWhiteTo(occ, _OcclusionStrength);
#endif
#else
    return 1.0;
#endif
}

inline void InitializeStandardLitSurfaceData(float2 uvAlbedo, float2 uvNormal, float2 uvMetallic, float2 uvEmissive, out SurfaceData outSurfaceData)
{
    half4 albedoAlpha = SampleAlbedoAlpha(uvAlbedo, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    half4 alphaTexture = SampleAlbedoAlpha(uvAlbedo, TEXTURE2D_ARGS(_AlphaTexture, sampler_AlphaTexture));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, _Cutoff) * alphaTexture.r;

    half4 specGloss = SampleMetallicSpecGloss(uvMetallic, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;

#if _SPECULAR_SETUP
    outSurfaceData.metallic = 1.0h;
    outSurfaceData.specular = specGloss.rgb;
#else
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0h, 0.0h, 0.0h);
#endif

    outSurfaceData.smoothness = specGloss.a;
    outSurfaceData.normalTS = SampleNormal(uvNormal, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);

    if (outSurfaceData.normalTS.x > -.004 && outSurfaceData.normalTS.x < .004)
        outSurfaceData.normalTS.x = 0;

    if (outSurfaceData.normalTS.y > -.004 && outSurfaceData.normalTS.y < .004)
        outSurfaceData.normalTS.y = 0;

    outSurfaceData.occlusion = SampleOcclusion(uvAlbedo); //Uses Albedo UVs due to TEXCOORDS amount limit
    outSurfaceData.emission = SampleEmission(uvEmissive, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
}

inline void InitializeStandardLitSurfaceData(float2 uv, out SurfaceData outSurfaceData)
{
    InitializeStandardLitSurfaceData(uv, uv, uv, uv, outSurfaceData);
}

#endif // LIGHTWEIGHT_INPUT_SURFACE_PBR_INCLUDED
