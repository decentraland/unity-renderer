using System;
using System.Collections.Generic;
using DCL.Helpers;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class AvatarMeshCombinerUtils
    {
        internal const string AVATAR_MAP_PROPERTY_NAME = "_AvatarMap";

        internal static readonly int[] AVATAR_MAP_ID_PROPERTIES =
        {
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "1"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "2"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "3"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "4"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "5"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "6"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "7"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "8"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "9"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "10"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "11"),
            Shader.PropertyToID(AVATAR_MAP_PROPERTY_NAME + "12")
        };

        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        /// <summary>
        /// This method iterates over all the renderers contained in the given CombineLayer list, and
        /// outputs an array of all the bones per vertexes
        /// 
        /// This is needed because Mesh.CombineMeshes don't calculate boneWeights correctly.
        /// When using Mesh.CombineMeshes, the boneWeights returned correspond to indexes of skeleton copies,
        /// not the same skeleton.
        /// </summary>
        /// <param name="layers">A CombineLayer list. You can generate this array using CombineLayerUtils.Slice().</param>
        /// <returns>A list of Bones per vertex that share the same skeleton.</returns>
        public static NativeArray<byte> CombineBonesPerVertex(List<CombineLayer> layers)
        {
            int layersCount = layers.Count;
            int totalVertexes = 0;

            List<NativeArray<byte>> bonesPerVertexList = new List<NativeArray<byte>>();

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;
                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var bonesPerVertex = layerRenderers[i].sharedMesh.GetBonesPerVertex();
                    
                    bonesPerVertexList.Add(bonesPerVertex);
                    totalVertexes += bonesPerVertex.Length;
                }
            }

            NativeArray<byte> finalBpV = new NativeArray<byte>(totalVertexes, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            int indexOffset = 0;
            int bonesPerVertexListCount = bonesPerVertexList.Count;

            for (int i = 0; i < bonesPerVertexListCount; i++)
            {
                var narray = bonesPerVertexList[i];
                int narrayLength = narray.Length;
                NativeArray<byte>.Copy(narray, 0, finalBpV, indexOffset, narrayLength);
                indexOffset += narrayLength;
                narray.Dispose();
            }

            return finalBpV;
        }
        
        /// <summary>
        /// This method iterates over all the renderers contained in the given CombineLayer list, and
        /// outputs an array of all the bone weights
        /// 
        /// This is needed because Mesh.CombineMeshes don't calculate boneWeights correctly.
        /// When using Mesh.CombineMeshes, the boneWeights returned correspond to indexes of skeleton copies,
        /// not the same skeleton.
        /// </summary>
        /// <param name="layers">A CombineLayer list. You can generate this array using CombineLayerUtils.Slice().</param>
        /// <returns>A list of bone weights that share the same skeleton.</returns>
        public static NativeArray<BoneWeight1> CombineBonesWeights(List<CombineLayer> layers)
        {
            int layersCount = layers.Count;
            int totalBones = 0;

            List<NativeArray<BoneWeight1>> boneWeightArrays = new List<NativeArray<BoneWeight1>>(10);

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;
                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var boneWeights = layerRenderers[i].sharedMesh.GetAllBoneWeights();
                    boneWeightArrays.Add(boneWeights);
                    totalBones += boneWeights.Length;
                }
            }

            NativeArray<BoneWeight1> finalBones = new NativeArray<BoneWeight1>(totalBones, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

            int indexOffset = 0;
            var finalBonesCount = boneWeightArrays.Count;

            for (int i = 0; i < finalBonesCount; i++)
            {
                var narray = boneWeightArrays[i];
                int narrayLength = narray.Length;
                NativeArray<BoneWeight1>.Copy(narray, 0, finalBones, indexOffset, narrayLength);
                indexOffset += narrayLength;
                narray.Dispose();
            }

            return finalBones;
        }


        /// <summary>
        /// FlattenMaterials take a CombineLayer list and returns a FlattenedMaterialsData object.
        /// 
        /// This object can be used to construct a combined mesh that has uniform data encoded in uv attributes.
        /// This type of encoding can be used to greatly reduce draw calls for seemingly unrelated objects.
        ///
        /// The returned object also contains a single material per CombineLayer.
        /// </summary>
        /// <param name="layers">A CombineLayer list. You can generate this array using CombineLayerUtils.Slice().</param>
        /// <param name="materialAsset">Material asset that will be cloned to generate the returned materials.</param>
        /// <returns>A FlattenedMaterialsData object. This object can be used to construct a combined mesh that has uniform data encoded in UV attributes.</returns>
        public static FlattenedMaterialsData FlattenMaterials(List<CombineLayer> layers, Material materialAsset)
        {
            int layersCount = layers.Count;

            int finalVertexCount = 0;
            int currentVertexCount = 0;
            
            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;
                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];
                    finalVertexCount += renderer.sharedMesh.vertexCount;
                }
            }

            var result = new FlattenedMaterialsData(finalVertexCount);

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;

                Material newMaterial = new Material(materialAsset);

                CullMode cullMode = layer.cullMode;
                bool isOpaque = layer.isOpaque;

                if ( isOpaque )
                    MaterialUtils.SetOpaque(newMaterial);
                else
                    MaterialUtils.SetTransparent(newMaterial);

                newMaterial.SetInt(ShaderUtils.Cull, (int)cullMode);

                result.materials.Add( newMaterial );

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

                    bool baseMapIdIsValid = baseMap != null && layer.textureToId.ContainsKey(baseMap);
                    bool emissionMapIdIsValid = emissionMap != null && layer.textureToId.ContainsKey(emissionMap);

                    int baseMapId = baseMapIdIsValid ? layer.textureToId[baseMap] : -1;
                    int emissionMapId = emissionMapIdIsValid ? layer.textureToId[emissionMap] : -1;

                    if ( baseMapId != -1 )
                    {
                        if ( baseMapId < AVATAR_MAP_ID_PROPERTIES.Length )
                        {
                            int targetMap = AVATAR_MAP_ID_PROPERTIES[baseMapId];
                            newMaterial.SetTexture(targetMap, baseMap);
                        }
                        else
                        {
                            if (VERBOSE)
                                logger.Log(LogType.Error, "FlattenMaterials", $"Base Map ID out of bounds! {baseMapId}");
                        }
                    }

                    if ( emissionMapId != -1 )
                    {
                        if ( baseMapId < AVATAR_MAP_ID_PROPERTIES.Length )
                        {
                            int targetMap = AVATAR_MAP_ID_PROPERTIES[emissionMapId];
                            newMaterial.SetTexture(targetMap, emissionMap);
                        }
                        else
                        {
                            if (VERBOSE)
                                logger.Log(LogType.Error, "FlattenMaterials", $"Emission Map ID out of bounds! {emissionMapId}");
                        }
                    }

                    Vector4 baseColor = mat.GetVector(ShaderUtils.BaseColor);
                    Vector4 emissionColor = mat.GetVector(ShaderUtils.EmissionColor);
                    Vector3 texturePointerData = new Vector3(baseMapId, emissionMapId, cutoff);

                    for ( int ai = 0; ai < vertexCount; ai++ )
                    {
                        result.texturePointers[currentVertexCount] = texturePointerData;
                        result.colors[currentVertexCount] = baseColor;
                        result.emissionColors[currentVertexCount] = emissionColor;
                        currentVertexCount++;
                    }

                    if (VERBOSE)
                        logger.Log($"Layer {i} - vertexCount: {vertexCount} - texturePointers: ({baseMapId}, {emissionMapId}, {cutoff}) - emissionColor: {emissionColor} - baseColor: {baseColor}");

                }

                SRPBatchingHelper.OptimizeMaterial(newMaterial);

            }   

            return result;
        }

        /// <summary>
        /// ComputeSubMeshes iterates over the given CombineLayer list, and returns a list that can be used to map a
        /// sub-mesh for each CombineLayer object. A CombineLayer object can group more than a single mesh.
        ///
        /// Note that this had to be done because Mesh.CombineMeshes lack the option of controlling the sub-mesh
        /// output. Currently the only options are to combine everything in a single sub-mesh, or generate a single sub-mesh
        /// per combined mesh. The CombineLayer approach may need to combine specific meshes to a single sub-mesh
        /// (because they all can have the same material, if they share the same render state -- i.e. transparency or cull mode).
        /// </summary>
        /// <param name="layers">A CombineLayer list. You can generate this array using CombineLayerUtils.Slice().</param>
        /// <returns>A SubMeshDescriptor list that can be used later to set the sub-meshes of the final combined mesh
        /// in a way that each sub-mesh corresponds with its own layer.</returns>
        public static List<SubMeshDescriptor> ComputeSubMeshes(List<CombineLayer> layers)
        {
            List<SubMeshDescriptor> result = new List<SubMeshDescriptor>();
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
                result.Add(subMesh);

                subMeshIndexOffset += subMeshIndexCount;
            }

            return result;
        }

        /// <summary>
        /// ComputeCombineInstancesData returns a CombineInstance list that can be used to combine all the meshes
        /// specified by the given CombineLayer list. This is done via Mesh.CombineMeshes() Unity method.
        /// </summary>
        /// <param name="layers">A CombineLayer list. You can generate this array using CombineLayerUtils.Slice()</param>
        /// <returns>CombineInstance list usable by Mesh.CombineMeshes()</returns>
        public static List<CombineInstance> ComputeCombineInstancesData(List<CombineLayer> layers)
        {
            var result = new List<CombineInstance>();
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

                    result.Add( new CombineInstance()
                    {
                        subMeshIndex = 0, // this means the source sub-mesh, not destination
                        mesh = renderer.sharedMesh,
                        transform = meshTransform.localToWorldMatrix
                    });

                    meshTransform.SetParent( prevParent );
                }
            }

            return result;
        }

        public static Mesh CombineMeshesWithLayers( List<CombineInstance> combineInstancesData, List<CombineLayer> layers )
        {
            Mesh result = new Mesh();
            // Is important to use the layerRenderers to combine (i.e. no the original renderers)
            // Layer renderers are in a specific order that must be abided, or the combining will be broken.
            List<SkinnedMeshRenderer> layerRenderers = new List<SkinnedMeshRenderer>();

            for (int i = 0; i < layers.Count; i++)
            {
                layerRenderers.AddRange( layers[i].renderers );
            }

            using (var bakedInstances = new BakedCombineInstances())
            {
                bakedInstances.Bake(combineInstancesData, layerRenderers);
                result.CombineMeshes(combineInstancesData.ToArray(), true, true);
            }

            return result;
        }

        /// <summary>
        /// ResetBones will reset the given SkinnedMeshRenderer bones to the original bindposes position.
        /// 
        /// This is done without taking into account the rootBone. We need to do it this way because the meshes must be
        /// combined posing to match the raw bindposes matrix.
        ///
        /// If the combined mesh don't match the bindposes matrices, the resulting skinning will not work.
        ///
        /// For this reason, this method doesn't resemble the original method that unity uses to reset the skeleton found here:
        /// https://github.com/Unity-Technologies/UnityCsReference/blob/61f92bd79ae862c4465d35270f9d1d57befd1761/Editor/Mono/Inspector/Avatar/AvatarSetupTool.cs#L890
        /// </summary>
        internal static void ResetBones(Matrix4x4[] bindPoses, Transform[] bones)
        {
            for ( int i = 0 ; i < bones.Length; i++ )
            {
                Transform bone = bones[i];
                Matrix4x4 bindPose = bindPoses[i].inverse;
                bone.position = bindPose.MultiplyPoint3x4(Vector3.zero);
                bone.rotation = bindPose.rotation;

                Vector3 bindPoseScale = bindPose.lossyScale;
                Vector3 boneScale = bone.lossyScale;

                bone.localScale = new Vector3(bindPoseScale.x / boneScale.x,
                    bindPoseScale.y / boneScale.y,
                    bindPoseScale.z / boneScale.z);
            }

#if UNITY_EDITOR
            DrawDebugSkeleton(bones);
#endif
        }

        internal static void DrawDebugSkeleton(Transform[] bones)
        {
            for ( int i = 0 ; i < bones.Length; i++ )
            {
                Transform bone = bones[i];
                Debug.DrawLine(bone.position, bone.position + bone.forward, Color.cyan, 60);

                foreach ( Transform child in bone )
                {
                    Debug.DrawLine(bone.position, child.position, Color.green, 60);
                }
            }
        }
    }
}