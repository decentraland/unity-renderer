#include "GPUSkinning.hlsl"

#ifndef SG_SHADOW_PASS_INCLUDED
#define SG_SHADOW_PASS_INCLUDED

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
    // TODO(Brian): ApplyGPUSkinning sets the final world space normal and this ends up
    // stored to normalOS. The BuildVaryings method recalculates the normal world position
    // assuming the normalOS is still in object space, so we have to force it to world space
    // again here. We should improve this code in the future to avoid this workaround.
    output.normalWS = input.normalOS;
    
    PackedVaryings packedOutput = (PackedVaryings)0;
    packedOutput = PackVaryings(output);
    return packedOutput;
}

half4 frag(PackedVaryings packedInput) : SV_TARGET
{
    Varyings unpacked = UnpackVaryings(packedInput);
    UNITY_SETUP_INSTANCE_ID(unpacked);
    SurfaceDescription surfaceDescription = BuildSurfaceDescription(unpacked);

    #if _ALPHATEST_ON
    clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
    #endif

    return 0;
}

#endif
