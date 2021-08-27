#ifndef DCL_GPU_SKINNING_INCLUDED
#define DCL_GPU_SKINNING_INCLUDED
#include <HLSLSupport.cginc>

CBUFFER_START(UnityPerMaterial)
float4x4 _WorldInverse;
float4x4 _Matrices[100];
float4x4 _BindPoses[100];
CBUFFER_END

float4x4 inverse(float4x4 input)
{
    #define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))

    float4x4 cofactors = float4x4(
        minor(_22_23_24, _32_33_34, _42_43_44),
        -minor(_21_23_24, _31_33_34, _41_43_44),
        minor(_21_22_24, _31_32_34, _41_42_44),
        -minor(_21_22_23, _31_32_33, _41_42_43),

        -minor(_12_13_14, _32_33_34, _42_43_44),
        minor(_11_13_14, _31_33_34, _41_43_44),
        -minor(_11_12_14, _31_32_34, _41_42_44),
        minor(_11_12_13, _31_32_33, _41_42_43),

        minor(_12_13_14, _22_23_24, _42_43_44),
        -minor(_11_13_14, _21_23_24, _41_43_44),
        minor(_11_12_14, _21_22_24, _41_42_44),
        -minor(_11_12_13, _21_22_23, _41_42_43),

        -minor(_12_13_14, _22_23_24, _32_33_34),
        minor(_11_13_14, _21_23_24, _31_33_34),
        -minor(_11_12_14, _21_22_24, _31_32_34),
        minor(_11_12_13, _21_22_23, _31_32_33)
    );
    #undef minor
    return transpose(cofactors) / determinant(input);
}

void ApplyGPUSkinning(inout Attributes input, float4 boneWeights01, float4 boneWeights23)
{
    
    float4x4 localBoneMatrix0_weigthed = mul(mul(_Matrices[boneWeights01.x], _BindPoses[boneWeights01.x]), boneWeights01.y);
    float4x4 localBoneMatrix1_weigthed = mul(mul(_Matrices[boneWeights01.z], _BindPoses[boneWeights01.z]), boneWeights01.w);
    float4x4 localBoneMatrix2_weighted = mul(mul(_Matrices[boneWeights23.x], _BindPoses[boneWeights23.x]), boneWeights23.y);
    float4x4 localBoneMatrix3_weighted = mul(mul(_Matrices[boneWeights23.z], _BindPoses[boneWeights23.z]), boneWeights23.w);

    float4x4 finalPose = localBoneMatrix0_weigthed +
        localBoneMatrix1_weigthed +
        localBoneMatrix2_weighted +
        localBoneMatrix3_weighted;

    // Skin with 4 weights per vertex
    float4 pos =
        mul(localBoneMatrix0_weigthed, float4(input.positionOS, 1))
        +
        mul(localBoneMatrix1_weigthed, float4(input.positionOS, 1))
        +
        mul(localBoneMatrix2_weighted, float4(input.positionOS, 1))
        +
        mul(localBoneMatrix3_weighted, float4(input.positionOS, 1));

    // Substract the renderer position, as we want to only have bone positions into account.
    // If we do not do this, when moving the outer parents the world position will be computed twice.
    // (one for the bones, one for the renderer).
    input.positionOS = mul(_WorldInverse, pos).xyz; //There's probably a way to reuse `finalPose` but I cannot find it   
    input.normalOS = mul(transpose(inverse(finalPose)), input.normalOS);
}
#endif
