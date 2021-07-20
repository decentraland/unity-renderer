using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;


// Recipe:

// - Re-arrange boneWeights manually
// - Re-arrange normals manually
//     - It seems that normals are scaled by the matrix when using CombineMeshes.
//       This is wrong and and shouldn't happen, so we have to arrange them manually.

// - Call BakeMesh with useScale to TRUE

// - Doesn't matter if we have an animation or not, BUT IT HAS TO BE BAKED BEFORE THE ANIMATION STARTS
//      - If animation already was played, we re-position all bones according to original mesh bindposes matrix
//        matrix has to be inversed and multiplied by 0 to get position. rotation and scale can be extracted directly
//        using Matrix4x4 methods.

// - Set bounds using combined mesh bounds

// - Root bone has to be correctly aligned, or the mesh is not going to be baked correctly

namespace DCL
{
    internal class CombineLayer
    {
        public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
        public Dictionary<Texture2D, int> idMap = new Dictionary<Texture2D, int>();
    }

    internal class CombineMeshData
    {
        public List<CombineInstance> combineInstances = new List<CombineInstance>();
        public List<BoneWeight> boneWeights = new List<BoneWeight>();
        public List<Vector2> texturePointers = new List<Vector2>();
        public List<Color> colors = new List<Color>();
        public List<Vector4> emissionColors = new List<Vector4>();
        public List<Material> materials = new List<Material>();
    }

    public static class AvatarMeshCombiner
    {
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler);

        public static GameObject Combine(SkinnedMeshRenderer bonesContainer, Transform root, System.Func<Renderer, bool> filterFunction = null)
        {
            logger.logEnabled = true;

            //
            // Find first enabled SkinnedMeshRenderer with valid sharedMesh
            //
            SkinnedMeshRenderer bindPosesContainer = null;
            var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(false);

            for ( int i = 0; i < renderers.Length; i++ )
            {
                if ( renderers[i].sharedMesh == null || !renderers[i].enabled )
                    continue;

                bindPosesContainer = renderers[i];
                break;
            }

            if (bindPosesContainer == null)
            {
                logger.Log("Combine failure!");
                return null;
            }

            //
            // Reset bones
            //
            AvatarMeshCombinerUtils.ResetBones(bonesContainer, bindPosesContainer);

            //
            // Get combined layers
            //
            var layers = AvatarMeshCombinerUtils.Slice( renderers, filterFunction );

            if ( layers == null )
            {
                logger.Log("Combine failure! 2");
                return null;
            }

            //
            // Start combining meshes
            //
            CombineMeshData data = PrepareCombineData(layers);

            Mesh finalMesh = new Mesh();
            finalMesh.CombineMeshes(data.combineInstances.ToArray(), true, true);

            var layerRenderers = layers.SelectMany( (x) => x.renderers ).ToArray();

            for ( int i = 0; i < layerRenderers.Length; i++ )
            {
                layerRenderers[i].enabled = false;
            }

            for ( int i = 0; i < data.combineInstances.Count; i++ )
            {
                UnityEngine.Object.Destroy(data.combineInstances[i].mesh);
            }

            var poses = bindPosesContainer.sharedMesh.bindposes;
            finalMesh.bindposes = poses;

            finalMesh.boneWeights = data.boneWeights.ToArray();
            finalMesh.SetUVs(3, data.emissionColors);
            finalMesh.SetUVs(2, data.texturePointers);
            finalMesh.SetColors(data.colors);
            finalMesh.Optimize();

            finalMesh.UploadMeshData(true);

            var bounds = finalMesh.bounds;
            Vector3 newCenter = bounds.center;
            newCenter.Scale(new Vector3(1, 0, 1));
            bounds.center = newCenter;

            GameObject result = new GameObject("Combined Avatar");
            result.layer = root.gameObject.layer;
            result.transform.parent = null;
            result.transform.position = Vector3.zero;

            var newSkinnedMeshRenderer = result.AddComponent<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.sharedMesh = finalMesh;
            newSkinnedMeshRenderer.bones = bonesContainer.bones;
            newSkinnedMeshRenderer.rootBone = bonesContainer.rootBone;
            newSkinnedMeshRenderer.localBounds = bonesContainer.localBounds;
            newSkinnedMeshRenderer.sharedMaterials = data.materials.ToArray();
            newSkinnedMeshRenderer.quality = SkinQuality.Bone1;
            newSkinnedMeshRenderer.updateWhenOffscreen = false;
            newSkinnedMeshRenderer.skinnedMotionVectors = false;

            return result;
        }

        private static bool RendererIsInvalid(Func<Renderer, bool> func, SkinnedMeshRenderer skinnedMeshRenderer)
        {
            if ( skinnedMeshRenderer.sharedMesh == null || !skinnedMeshRenderer.enabled )
                return true;

            bool canMerge = true;

            if ( func != null )
                canMerge = func.Invoke(skinnedMeshRenderer);

            if ( !canMerge )
            {
                skinnedMeshRenderer.enabled = false;
                return true;
            }

            return false;
        }

        internal static CombineMeshData PrepareCombineData(List<CombineLayer> layers, System.Func<Renderer, bool> filterFunction = null)
        {
            CombineMeshData result = new CombineMeshData();

            Material mainMaterial = Resources.Load<Material>("OptimizedToonMaterial");
            int layersCount = layers.Count;

            for (int layerIndex = 0; layerIndex < layersCount; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers;

                Material newMaterial = UnityEngine.Object.Instantiate(mainMaterial);
                result.materials.Add( newMaterial );

                int layerRenderersCount = layerRenderers.Count;

                for (int i = 0; i < layerRenderersCount; i++)
                {
                    var renderer = layerRenderers[i];

                    if (RendererIsInvalid(filterFunction, renderer))
                        continue;

                    Mesh mesh = new Mesh();
                    renderer.BakeMesh(mesh, true);

                    // Bone Weights
                    var sharedMesh = renderer.sharedMesh;
                    var meshBoneWeights = sharedMesh.boneWeights;
                    int vertexCount = sharedMesh.vertexCount;

                    result.boneWeights.AddRange(meshBoneWeights);

                    // Texture IDs
                    Material mat = renderer.sharedMaterial;

                    Texture2D baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                    Texture2D emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);

                    int id1 = baseMap != null ? layer.idMap[baseMap] : -1;
                    int id2 = emissionMap != null ? layer.idMap[emissionMap] : -1;

                    result.texturePointers.AddRange(Enumerable.Repeat(new Vector2(id1, id2), vertexCount));

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
                    result.colors.AddRange(Enumerable.Repeat(baseColor, vertexCount));

                    // Emission Colors
                    Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                    Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);
                    result.emissionColors.AddRange(Enumerable.Repeat(emissionColorV4, vertexCount));

                    Transform meshTransform = renderer.transform;
                    meshTransform.SetParent(null, true);
                    Transform prevParent = meshTransform.parent;

                    result.combineInstances.Add( new CombineInstance()
                    {
                        subMeshIndex = layerIndex,
                        mesh = mesh,
                        transform = meshTransform.localToWorldMatrix
                    });

                    meshTransform.SetParent( prevParent );
                }
            }

            return result;
        }
    }
}