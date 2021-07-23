using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class CombineMeshDataUtils
    {
        public static void ComputeBoneWeights( this CombineMeshData data, List<CombineLayer> layers )
        {
            int layersCount = layers.Count;

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;

                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];

                    // Bone Weights
                    var sharedMesh = renderer.sharedMesh;
                    var meshBoneWeights = sharedMesh.boneWeights;
                    data.boneWeights.AddRange(meshBoneWeights);
                }
            }
        }

        public static void FlattenMaterials(this CombineMeshData data, List<CombineLayer> layers, Material materialAsset)
        {
            int layersCount = layers.Count;

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;

                Material newMaterial = Object.Instantiate(materialAsset);

                CullMode cullMode = layer.cullMode;
                bool isOpaque = layer.isOpaque;

                if ( isOpaque )
                {
                    newMaterial.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.One);
                    newMaterial.SetInt(ShaderUtils.DstBlend, (int)BlendMode.Zero);
                    newMaterial.SetInt(ShaderUtils.ZWrite, 1);
                    newMaterial.SetInt(ShaderUtils.Surface, 0);
                }
                else
                {
                    newMaterial.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.SrcAlpha);
                    newMaterial.SetInt(ShaderUtils.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    newMaterial.SetInt(ShaderUtils.ZWrite, 0);
                    newMaterial.SetInt(ShaderUtils.Surface, 1);
                }

                newMaterial.SetInt(ShaderUtils.Cull, (int)cullMode);

                data.materials.Add( newMaterial );

                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];

                    // Bone Weights
                    var sharedMesh = renderer.sharedMesh;
                    int vertexCount = sharedMesh.vertexCount;

                    // Texture IDs
                    Material mat = renderer.sharedMaterial;

                    Texture2D baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                    Texture2D emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);
                    float cutoff = mat.GetFloat(ShaderUtils.Cutoff);

                    int baseMapId = baseMap != null ? layer.textureToId[baseMap] : -1;
                    int emissionMapId = emissionMap != null ? layer.textureToId[emissionMap] : -1;

                    data.texturePointers.AddRange(Enumerable.Repeat(new Vector3(baseMapId, emissionMapId, cutoff), vertexCount));

                    if ( baseMapId != -1 )
                    {
                        string targetMap = $"_AvatarMap{(baseMapId + 1)}";
                        newMaterial.SetTexture(targetMap, baseMap);
                        //logger.Log($"(opaque) Setting map {targetMap} to {baseMap}");
                    }

                    if ( emissionMapId != -1 )
                    {
                        string targetMap = $"_AvatarMap{(emissionMapId + 1)}";
                        newMaterial.SetTexture(targetMap, emissionMap);
                        //logger.Log($"(emission) Setting map {targetMap} to {baseMap}");
                    }

                    // Base Colors
                    Color baseColor = mat.GetColor(ShaderUtils.BaseColor);
                    data.colors.AddRange(Enumerable.Repeat(baseColor, vertexCount));

                    // Emission Colors
                    Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                    Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);
                    data.emissionColors.AddRange(Enumerable.Repeat(emissionColorV4, vertexCount));
                }

                SRPBatchingHelper.OptimizeMaterial(newMaterial);
            }
        }

        public static void ComputeSubMeshes(this CombineMeshData data, List<CombineLayer> layers)
        {
            int layersCount = layers.Count;
            int subMeshIndexOffset = 0;

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;
                int layerRenderersCount = layerRenderers.Count;

                int subMeshVertexCount = 0;
                int subMeshIndexCount = 0;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];

                    int vertexCount = renderer.sharedMesh.vertexCount;
                    int indexCount = (int)renderer.sharedMesh.GetIndexCount(0);

                    subMeshVertexCount += vertexCount;
                    subMeshIndexCount += indexCount;
                }

                var subMesh = new SubMeshDescriptor(subMeshIndexOffset, subMeshIndexCount);
                subMesh.vertexCount = subMeshVertexCount;
                data.subMeshes.Add(subMesh);

                subMeshIndexOffset += subMeshIndexCount;
            }
        }

        public static void ComputeCombineInstancesData(this CombineMeshData data, List<CombineLayer> layers )
        {
            int layersCount = layers.Count;

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;
                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];

                    Transform meshTransform = renderer.transform;
                    Transform prevParent = meshTransform.parent;
                    meshTransform.SetParent(null, true);

                    data.combineInstances.Add( new CombineInstance()
                    {
                        subMeshIndex = 0,
                        mesh = renderer.sharedMesh,
                        transform = meshTransform.localToWorldMatrix
                    });

                    meshTransform.SetParent( prevParent );
                }
            }
        }
    }
}