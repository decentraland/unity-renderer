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
    public static class AvatarMeshCombinerUtils
    {
        /// <summary>
        /// Determines if the given renderer is going to be enqueued at the opaque section of the rendering pipeline.
        /// </summary>
        /// <param name="renderer">Renderer to be checked.</param>
        /// <returns>True if its opaque</returns>
        internal static bool IsOpaque(Renderer renderer)
        {
            Material firstMat = renderer.sharedMaterials[0];

            if (firstMat == null)
                return true;

            if (firstMat.HasProperty(ShaderUtils.ZWrite) &&
                (int) firstMat.GetFloat(ShaderUtils.ZWrite) == 0)
            {
                return false;
            }

            return true;
        }
    }

    public static class AvatarMeshCombiner
    {
        public class CombineLayer
        {
            public bool isOpaque;
            public List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();
            public Dictionary<Texture2D, int> idMap = new Dictionary<Texture2D, int>();
        }

        public static ILogger logger = new Logger(Debug.unityLogger.logHandler);

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
            ResetBones(bonesContainer, bindPosesContainer);

            //
            // Get combined layers
            //
            var layers = Slice( renderers, filterFunction );

            if ( layers == null )
            {
                logger.Log("Combine failure! 2");
                return null;
            }

            //
            // Start combining meshes
            //
            Mesh finalMesh = new Mesh();

            List<CombineInstance> combineInstances = new List<CombineInstance>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<Vector2> texturePointers = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<Vector4> emissionColors = new List<Vector4>();

            bool RendererIsInvalid(Func<Renderer, bool> func, SkinnedMeshRenderer skinnedMeshRenderer)
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

            Material mainMaterial =  Resources.Load<Material>("OptimizedToonMaterial");
            List<Material> materials = new List<Material>();

            for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                CombineLayer layer = layers[layerIndex];
                var layerRenderers = layer.renderers.ToArray();

                Material newMaterial = UnityEngine.Object.Instantiate(mainMaterial);
                materials.Add( newMaterial );

                for (int i = 0; i < layerRenderers.Length; i++)
                {
                    var renderer = layerRenderers[i];

                    if (RendererIsInvalid(filterFunction, renderer))
                        continue;

                    Mesh mesh = new Mesh();
                    renderer.BakeMesh(mesh, true);

                    // Bone Weights
                    var meshBoneWeights = renderer.sharedMesh.boneWeights;
                    int elementsCount = meshBoneWeights.Length;
                    boneWeights.AddRange(meshBoneWeights);

                    // Texture IDs
                    Material mat = renderer.sharedMaterial;

                    Texture2D baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                    Texture2D emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);

                    int id1 = baseMap != null ? layer.idMap[baseMap] : -1;
                    int id2 = emissionMap != null ? layer.idMap[emissionMap] : -1;

                    texturePointers.AddRange(Enumerable.Repeat(new Vector2(id1, id2), elementsCount));

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
                    colors.AddRange(Enumerable.Repeat(baseColor, elementsCount));

                    // Emission Colors
                    Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                    Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);
                    emissionColors.AddRange(Enumerable.Repeat(emissionColorV4, elementsCount));

                    Transform meshTransform = renderer.transform;
                    meshTransform.SetParent(null, true);
                    Transform prevParent = meshTransform.parent;

                    combineInstances.Add( new CombineInstance()
                    {
                        subMeshIndex = layerIndex,
                        mesh = mesh,
                        transform = meshTransform.localToWorldMatrix
                    });

                    meshTransform.SetParent( prevParent );
                    renderer.enabled = false;
                }
            }

            // Vector3 lastPos = transform.position;
            // transform.position = Vector3.zero;
            //
            // transform.position = lastPos;

            finalMesh.CombineMeshes(combineInstances.ToArray(), true, true);

            for ( int i = 0; i < combineInstances.Count; i++ )
            {
                UnityEngine.Object.Destroy(combineInstances[i].mesh);
            }

            var poses = bindPosesContainer.sharedMesh.bindposes;
            finalMesh.bindposes = poses;

            finalMesh.boneWeights = boneWeights.ToArray();
            finalMesh.SetUVs(3, emissionColors);
            finalMesh.SetUVs(2, texturePointers);
            finalMesh.SetColors(colors);
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
            newSkinnedMeshRenderer.sharedMaterials = materials.ToArray();
            newSkinnedMeshRenderer.quality = SkinQuality.Bone1;
            newSkinnedMeshRenderer.updateWhenOffscreen = false;
            newSkinnedMeshRenderer.skinnedMotionVectors = false;

            //result.transform.parent = null;

            return result;
        }

        private static void ResetBones(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer bindPosesContainer)
        {
            var bindPoses = bindPosesContainer.sharedMesh.bindposes;
            var tmpBones = bonesContainer.bones;

            for ( int i = 0 ; i < tmpBones.Length; i++ )
            {
                Matrix4x4 bindPose = bindPoses[i].inverse;
                tmpBones[i].position = bindPose.MultiplyPoint3x4(Vector3.zero);
                tmpBones[i].rotation = bindPose.rotation;

                Vector3 bindPoseScale = bindPose.lossyScale;
                Vector3 boneScale = tmpBones[i].lossyScale;

                tmpBones[i].localScale = new Vector3(bindPoseScale.x / boneScale.x,
                    bindPoseScale.y / boneScale.y,
                    bindPoseScale.z / boneScale.z);
            }
        }

        private static List<CombineLayer> Slice(SkinnedMeshRenderer[] renderers, System.Func<Renderer, bool> filterFunction)
        {
            logger.Log("Slice Start!");
            List<CombineLayer> result = new List<CombineLayer>();

            CombineLayer currentLayer = new CombineLayer();
            result.Add(currentLayer);
            int textureId = 0;

            bool CanAddToMap(Texture2D tex)
            {
                return tex != null && !currentLayer.idMap.ContainsKey(tex);
            }

            void AddToMap(Texture2D tex)
            {
                if ( !CanAddToMap(tex) )
                    return;

                currentLayer.idMap.Add(tex, textureId);
                textureId++;
            }

            // Group renderers on opaque and transparent materials
            var surfaceGroups = renderers.GroupBy( AvatarMeshCombinerUtils.IsOpaque );

            foreach ( var group in surfaceGroups )
            {
                var groupRenderers = group.ToArray();
                currentLayer.isOpaque = group.Key;

                foreach ( var r in groupRenderers )
                {
                    if ( !filterFunction(r) || !r.enabled || r.sharedMesh == null )
                    {
                        logger.Log($"Filtering out renderer: {r.transform.parent.name}");
                        continue;
                    }

                    currentLayer.renderers.Add(r);
                    var mats = r.sharedMaterials;

                    for ( int i = 0; i < mats.Length; i++ )
                    {
                        var mat = mats[i];

                        if (mat == null)
                            continue;

                        var baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                        var emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);

                        AddToMap(baseMap);
                        AddToMap(emissionMap);

                        if ( textureId >= 12 )
                        {
                            textureId = 0;
                            currentLayer = new CombineLayer();
                            result.Add(currentLayer);
                        }
                    }
                }
            }

            // No valid materials were found
            if ( result.Count == 1 && textureId == 0 )
            {
                logger.Log("Slice End Fail!");
                return null;
            }

            int layInd = 0;

            foreach ( var layer in result )
            {
                string rendererNames = layer.renderers
                    .Select( (x) => $"{x.transform.parent.name}" )
                    .Aggregate( (i, j) => i + "\n" + j);

                logger.Log($"Layer index: {layInd} ... renderer count: {layer.renderers.Count} ... textures found: {layer.idMap.Count}\nrenderers: {rendererNames}");
            }

            logger.Log("Slice End Success!");
            return result;
        }
    }
}