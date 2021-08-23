#ifndef DCL_GPU_SKINNING_INCLUDED
#define DCL_GPU_SKINNING_INCLUDED

CBUFFER_START(UnityPerMaterial)
float4x4 _WorldInverse;
float4x4 _Matrices[100];
float4x4 _BindPoses[100];
CBUFFER_END

float3 GetSkinnedPos(float3 vertex, float4 boneWeights01, float4 boneWeights23)
{
    // Skin with 4 weights per vertex
    float4 pos =
        mul(mul(_Matrices[boneWeights01.x], _BindPoses[boneWeights01.x]), float4(vertex,1)) * boneWeights01.y
        +
        mul(mul(_Matrices[boneWeights01.z], _BindPoses[boneWeights01.z]), float4(vertex,1)) * boneWeights01.w
        +
        mul(mul(_Matrices[boneWeights23.x], _BindPoses[boneWeights23.x]), float4(vertex,1)) * boneWeights23.y
        +
        mul(mul(_Matrices[boneWeights23.z], _BindPoses[boneWeights23.z]), float4(vertex,1)) * boneWeights23.w;

    // Substract the renderer position, as we want to only have bone positions into account.
    // If we do not do this, when moving the outer parents the world position will be computed twice.
    // (one for the bones, one for the renderer).
    return mul(_WorldInverse, pos).xyz;
}
#endif