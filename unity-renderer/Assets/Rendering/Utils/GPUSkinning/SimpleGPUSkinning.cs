using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleGPUSkinning
{
    public Transform[] bones;
    public Renderer meshRenderer;
    public Material skinningMaterial;

    private Matrix4x4[] boneMatrices;
    private static readonly int BONE_MATRICES = Shader.PropertyToID("_Matrices");
    private static readonly int BIND_POSES = Shader.PropertyToID("_BindPoses");
    private static readonly int RENDERER_WORLD_INVERSE = Shader.PropertyToID("_WorldInverse");

    private static HashSet<Mesh> processedBindPoses = new HashSet<Mesh>();

    /// <summary>
    /// This must be done once per SkinnedMeshRenderer before animating.
    /// </summary>
    /// <param name="skr"></param>
    private static void ConfigureBindPoses(SkinnedMeshRenderer skr)
    {
        if ( processedBindPoses.Contains(skr.sharedMesh))
            return;

        processedBindPoses.Add( skr.sharedMesh );

        int vertexCount = skr.sharedMesh.vertexCount;
        Vector4[] bone01data = new Vector4[vertexCount];
        Vector4[] bone23data = new Vector4[vertexCount];

        Debug.Log($"Configuring bind poses for bones... vertex count: {vertexCount}");

        var boneWeights = skr.sharedMesh.boneWeights;

        for ( int i = 0; i < vertexCount; i ++ )
        {
            BoneWeight boneWeight = boneWeights[i];
            bone01data[i].x = boneWeight.boneIndex0;
            bone01data[i].y = boneWeight.weight0;
            bone01data[i].z = boneWeight.boneIndex1;
            bone01data[i].w = boneWeight.weight1;

            bone23data[i].x = boneWeight.boneIndex2;
            bone23data[i].y = boneWeight.weight2;
            bone23data[i].z = boneWeight.boneIndex3;
            bone23data[i].w = boneWeight.weight3;
        }

        skr.sharedMesh.tangents = bone01data;
        skr.sharedMesh.SetUVs(1, bone23data);
    }

    public SimpleGPUSkinning (SkinnedMeshRenderer skr, Material gpuSkinningMaterial)
    {
        ConfigureBindPoses(skr);

        boneMatrices = new Matrix4x4[skr.bones.Length];

        GameObject go = skr.gameObject;

        go.AddComponent<MeshFilter>().sharedMesh = skr.sharedMesh;

        meshRenderer = go.AddComponent<MeshRenderer>();

        this.skinningMaterial = new Material(gpuSkinningMaterial);
        skinningMaterial.SetMatrixArray(BIND_POSES, skr.sharedMesh.bindposes.ToArray());

        meshRenderer.sharedMaterial = skinningMaterial;
        bones = skr.bones;

        Object.Destroy(skr);
    }

    public void Update()
    {
        int bonesLength = bones.Length;

        for (int i = 0; i < bonesLength; i++)
        {
            var bone = bones[i];
            boneMatrices[i] = bone.localToWorldMatrix;
        }

        skinningMaterial.SetMatrix(RENDERER_WORLD_INVERSE, meshRenderer.transform.worldToLocalMatrix);
        skinningMaterial.SetMatrixArray(BONE_MATRICES, boneMatrices.ToArray());
    }
}