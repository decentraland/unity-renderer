#include "GPUSkinning.hlsl"

#ifndef SG_DEPTH_ONLY_PASS_INCLUDED
#define SG_DEPTH_ONLY_PASS_INCLUDED

PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    #ifdef _GPU_SKINNING 
    float3 gpuSkinnedPositionOS;
    float3 gpuSkinnedNormalOS;
    ApplyGPUSkinning(input.positionOS, input.normalOS, gpuSkinnedPositionOS, gpuSkinnedNormalOS, input.tangentOS, input.uv1);
    input.positionOS = gpuSkinnedPositionOS;
    input.normalOS = gpuSkinnedNormalOS;
    #endif
    output = BuildVaryings(input);
    PackedVaryings packedOutput = (PackedVaryings)0;
    packedOutput = PackVaryings(output);
    return packedOutput;
}

half4 frag(PackedVaryings packedInput) : SV_TARGET
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

    #if _ALPHATEST_ON
    clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
    #endif

    return 0;
}

#endif
