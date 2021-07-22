using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class CombineMeshDataUtils
    {
        public static void ComputeVertexAttributesData (this CombineMeshData data, List<CombineLayer> layers, Material materialAsset)
        {
            int layersCount = layers.Count;

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;

                Material newMaterial = UnityEngine.Object.Instantiate(materialAsset);
                data.materials.Add( newMaterial );

                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];

                    // Bone Weights
                    var sharedMesh = renderer.sharedMesh;
                    var meshBoneWeights = sharedMesh.boneWeights;
                    int vertexCount = sharedMesh.vertexCount;

                    data.boneWeights.AddRange(meshBoneWeights);

                    // Texture IDs
                    Material mat = renderer.sharedMaterial;

                    Texture2D baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                    Texture2D emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);

                    int id1 = baseMap != null ? layer.idMap[baseMap] : -1;
                    int id2 = emissionMap != null ? layer.idMap[emissionMap] : -1;

                    data.texturePointers.AddRange(Enumerable.Repeat(new Vector2(id1, id2), vertexCount));

                    if ( id1 != -1 )
                    {
                        string targetMap = $"_AvatarMap{(id1 + 1)}";
                        newMaterial.SetTexture(targetMap, baseMap);
                        //logger.Log($"(opaque) Setting map {targetMap} to {baseMap}");
                    }

                    if ( id2 != -1 )
                    {
                        string targetMap = $"_AvatarMap{(id2 + 1)}";
                        newMaterial.SetTexture(targetMap, emissionMap);
                        //logger.Log($"(emission) Setting map {targetMap} to {baseMap}");
                    }

                    newMaterial.SetInt(ShaderUtils.ZWrite, mat.GetInt(ShaderUtils.ZWrite));
                    newMaterial.SetInt(ShaderUtils.SrcBlend, mat.GetInt(ShaderUtils.SrcBlend));
                    newMaterial.SetInt(ShaderUtils.DstBlend, mat.GetInt(ShaderUtils.DstBlend));

                    // Base Colors
                    Color baseColor = mat.GetColor(ShaderUtils.BaseColor);
                    data.colors.AddRange(Enumerable.Repeat(baseColor, vertexCount));

                    // Emission Colors
                    Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                    Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);
                    data.emissionColors.AddRange(Enumerable.Repeat(emissionColorV4, vertexCount));
                }
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