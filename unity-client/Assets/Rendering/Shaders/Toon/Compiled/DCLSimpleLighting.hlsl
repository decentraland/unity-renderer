#ifndef DCL_SIMPLE_FRAGMENT_INCLUDED
#define DCL_SIMPLE_FRAGMENT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../../Includes/DCLConstants.hlsl"

half4 DCL_SimpleFragmentPBR(InputData inputData, SurfaceData surfaceData)
{
#ifdef _SPECULARHIGHLIGHTS_OFF
    bool specularHighlightsOff = true;
#else
    bool specularHighlightsOff = false;
#endif

    BRDFData brdfData;

    // NOTE: can modify alpha
    InitializeBRDFData(surfaceData.albedo, 0, 0, 0, surfaceData.alpha, brdfData);

    BRDFData brdfDataClearCoat = (BRDFData)0;
#if defined(_CLEARCOAT) || defined(_CLEARCOATMAP)
    // base brdfData is modified here, rely on the compiler to eliminate dead computation by InitializeBRDFData()
    InitializeBRDFDataClearCoat(surfaceData.clearCoatMask, surfaceData.clearCoatSmoothness, brdfData, brdfDataClearCoat);
#endif

    // To ensure backward compatibility we have to avoid using shadowMask input, as it is not present in older shaders
#if defined(SHADOWS_SHADOWMASK) && defined(LIGHTMAP_ON)
    half4 shadowMask = inputData.shadowMask;
#elif !defined (LIGHTMAP_ON)
    half4 shadowMask = unity_ProbesOcclusion;
#else
    half4 shadowMask = half4(1, 1, 1, 1);
#endif

    #if defined(_SCREEN_SPACE_OCCLUSION)
        AmbientOcclusionFactor aoFactor = GetScreenSpaceAmbientOcclusion(inputData.normalizedScreenSpaceUV);
        surfaceData.occlusion = min(surfaceData.occlusion, aoFactor.indirectAmbientOcclusion * DCL_CUSTOM_AO_TOON_FACTOR);
        surfaceData.occlusion = max(surfaceData.occlusion, aoFactor.directAmbientOcclusion);
    #endif
    
    half3 color = surfaceData.albedo * surfaceData.occlusion;

#ifdef _ADDITIONAL_LIGHTS_VERTEX
    color += inputData.vertexLighting * brdfData.diffuse;
#endif
    
    color += surfaceData.emission * DCL_EMISSION_MULTIPLIER_TOON;

    return half4(color, surfaceData.alpha);
}

half4 DCL_SimpleFragmentPBR(InputData inputData, half3 albedo, half metallic, half3 specular,
    half smoothness, half occlusion, half3 emission, half alpha)
{
    SurfaceData s;
    s.albedo              = albedo;
    s.metallic            = metallic;
    s.specular            = specular;
    s.smoothness          = smoothness;
    s.occlusion           = occlusion;
    s.emission            = emission;
    s.alpha               = alpha;
    s.clearCoatMask       = 0.0;
    s.clearCoatSmoothness = 1.0;
    return DCL_SimpleFragmentPBR(inputData, s);
}


#endif
