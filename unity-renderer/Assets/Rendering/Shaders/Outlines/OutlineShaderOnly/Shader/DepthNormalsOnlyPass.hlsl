#include "GPUSkinning.hlsl"
#ifndef SG_DEPTH_ONLY_PASS_INCLUDED
#define SG_DEPTH_ONLY_PASS_INCLUDED

PackedVaryings vert(Attributes input)
{
    Varyings output = (Varyings)0;
    #ifdef _GPU_SKINNING
    ApplyGPUSkinning(input, input.tangentOS, input.uv1);
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
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(unpacked);

    SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
    SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

    #if _AlphaClip
    clip(surfaceDescription.Alpha - surfaceDescription.AlphaClipThreshold);
    #endif

    return float4(PackNormalOctRectEncode(TransformWorldToViewDir(unpacked.normalWS, true)), 0.0, 0.0);
}

#endif
