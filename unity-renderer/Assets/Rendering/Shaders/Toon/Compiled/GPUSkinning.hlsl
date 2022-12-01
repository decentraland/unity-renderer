#ifndef DCL_GPU_SKINNING_INCLUDED
#define DCL_GPU_SKINNING_INCLUDED

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

void ApplyGPUSkinning(float3 positionOS, float3 normalOS, out float3 outPositionOS, out float3 outNormalOS, float4 boneWeights01, float4 boneWeights23)
{
    const float4x4 boneMatrix0_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights01.x]), _BindPoses[boneWeights01.x]);
    const float4x4 boneMatrix1_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights01.z]), _BindPoses[boneWeights01.z]);
    const float4x4 boneMatrix2_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights23.x]), _BindPoses[boneWeights23.x]);
    const float4x4 boneMatrix3_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights23.z]), _BindPoses[boneWeights23.z]);

    // Skin with 4 weights per vertex
    const float4x4 skinningMatrixOS =
        mul(boneMatrix0_OS, boneWeights01.y) + mul(boneMatrix1_OS, boneWeights01.w)
        + mul(boneMatrix2_OS, boneWeights23.y) + mul(boneMatrix3_OS, boneWeights23.w);

    const float4 finalVertexPositionOS = mul(skinningMatrixOS, float4(positionOS.xyz, 1.0));
    const float3x3 transposedFinalPose = transpose(inverse(skinningMatrixOS));
    const float3 normalOSInversed = normalize(mul(transposedFinalPose, normalOS));

    outPositionOS = finalVertexPositionOS.xyz;
    outNormalOS = mul(normalOSInversed, _WorldInverse);
}

void ApplyGPUSkinning(float3 positionOS, out float3 outPositionOS, float4 boneWeights01, float4 boneWeights23)
{
    const float4x4 boneMatrix0_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights01.x]), _BindPoses[boneWeights01.x]);
    const float4x4 boneMatrix1_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights01.z]), _BindPoses[boneWeights01.z]);
    const float4x4 boneMatrix2_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights23.x]), _BindPoses[boneWeights23.x]);
    const float4x4 boneMatrix3_OS =
        mul(mul(_WorldInverse, _Matrices[boneWeights23.z]), _BindPoses[boneWeights23.z]);

    // Skin with 4 weights per vertex
    const float4x4 skinningMatrixOS =
        mul(boneMatrix0_OS, boneWeights01.y) + mul(boneMatrix1_OS, boneWeights01.w)
        + mul(boneMatrix2_OS, boneWeights23.y) + mul(boneMatrix3_OS, boneWeights23.w);

    const float4 finalVertexPositionOS = mul(skinningMatrixOS, float4(positionOS.xyz, 1.0));

    outPositionOS = finalVertexPositionOS.xyz;
}
#endif
