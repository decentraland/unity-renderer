using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

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

        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = LogType.Warning };


        public static List<BoneWeight> ComputeBoneWeights( List<CombineLayer> layers )
        {
            List<BoneWeight> result = new List<BoneWeight>();
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
                    result.AddRange(meshBoneWeights);
                }
            }

            return result;
        }

        public static FlattenedMaterialsData FlattenMaterials(List<CombineLayer> layers, Material materialAsset)
        {
            var result = new FlattenedMaterialsData();
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
                    newMaterial.SetInt(ShaderUtils.Surface, 0);
                    newMaterial.SetFloat(ShaderUtils.ZWrite, 1);
                    newMaterial.EnableKeyword("_ALPHATEST_ON");
                    newMaterial.DisableKeyword("_ALPHABLEND_ON");
                    newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    newMaterial.SetOverrideTag("RenderType", "TransparentCutout");
                }
                else
                {
                    newMaterial.SetInt(ShaderUtils.SrcBlend, (int)BlendMode.SrcAlpha);
                    newMaterial.SetInt(ShaderUtils.DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    newMaterial.SetInt(ShaderUtils.Surface, 1);
                    newMaterial.SetFloat(ShaderUtils.ZWrite, 0);
                    newMaterial.DisableKeyword("_ALPHATEST_ON");
                    newMaterial.EnableKeyword("_ALPHABLEND_ON");
                    newMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    newMaterial.SetOverrideTag("RenderType", "Transparent");
                }

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

                    result.texturePointers.AddRange(Enumerable.Repeat(new Vector3(baseMapId, emissionMapId, cutoff), vertexCount));

                    if ( baseMapId != -1 )
                    {
                        int targetMap = AVATAR_MAP_ID_PROPERTIES[baseMapId];
                        newMaterial.SetTexture(targetMap, baseMap);
                    }

                    if ( emissionMapId != -1 )
                    {
                        int targetMap = AVATAR_MAP_ID_PROPERTIES[emissionMapId];
                        newMaterial.SetTexture(targetMap, emissionMap);
                    }

                    // Base Colors
                    Color baseColor = mat.GetColor(ShaderUtils.BaseColor);
                    result.colors.AddRange(Enumerable.Repeat(baseColor, vertexCount));

                    // Emission Colors
                    Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                    Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);
                    result.emissionColors.AddRange(Enumerable.Repeat(emissionColorV4, vertexCount));
                }

                SRPBatchingHelper.OptimizeMaterial(newMaterial);
            }

            return result;
        }

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

        public static List<CombineInstance> ComputeCombineInstancesData(List<CombineLayer> layers )
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
                        subMeshIndex = 0,
                        mesh = renderer.sharedMesh,
                        transform = meshTransform.localToWorldMatrix
                    });

                    meshTransform.SetParent( prevParent );
                }
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="renderer"></param>
        internal static void ResetBones(SkinnedMeshRenderer renderer)
        {
            var bindPoses = renderer.sharedMesh.bindposes;
            var bones = renderer.bones;

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
            DrawDebugSkeleton(renderer);
#endif
        }

        internal static void DrawDebugSkeleton(SkinnedMeshRenderer renderer)
        {
            var bones = renderer.bones;

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