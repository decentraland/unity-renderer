using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public static class AvatarMeshCombiner_TextureArray
    {
        private static TextureArrayHelper textureArrayHelper = new TextureArrayHelper();
        public static ILogger logger = new Logger(Debug.unityLogger.logHandler);

        private static List<GameObject> combined = new List<GameObject>();

        public static GameObject CombineAll()
        {
            List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();

            int previewLayer = UnityEngine.LayerMask.NameToLayer("CharacterPreview");

            foreach ( GameObject c in combined )
            {
                if ( c != null && c.layer != previewLayer)
                {
                    renderers.AddRange( c.GetComponentsInChildren<SkinnedMeshRenderer>(false) );
                }
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
            List<Transform> allBones = new List<Transform>();

            Material mainMaterial =  Resources.Load<Material>("OptimizedToonMaterial_TextureArray");
            List<Material> materials = new List<Material>();

            Material newMaterial = UnityEngine.Object.Instantiate(mainMaterial);
            materials.Add( newMaterial );

            for (int i = 0; i < renderers.Count; i++)
            {
                var renderer = renderers[i];

                if ( renderer.sharedMesh == null || !renderer.enabled )
                    continue;

                Mesh mesh = renderer.sharedMesh;
                //renderer.BakeMesh(mesh, true);

                // Bone Weights
                // var meshBoneWeights = renderer.sharedMesh.boneWeights;
                //int elementsCount = mesh.vertexCount;
                // boneWeights.AddRange(meshBoneWeights);

                allBones.AddRange( renderer.bones );

                // Texture IDs
                // Material mat = renderer.sharedMaterial;
                //
                // Texture2D baseMap = (Texture2D)mat.GetTexture(ShaderUtils.BaseMap);
                // Texture2D emissionMap = (Texture2D)mat.GetTexture(ShaderUtils.EmissionMap);
                //
                // int id1 = textureArrayHelper.AddTexture(baseMap);
                // int id2 = textureArrayHelper.AddTexture(emissionMap);

                var tmpTexList = new List<Vector2>();
                mesh.GetUVs(4, tmpTexList);
                texturePointers.AddRange(tmpTexList);

                // Base Colors
                //Color baseColor = mat.GetColor(ShaderUtils.BaseColor);
                colors.AddRange(mesh.colors);

                // Emission Colors
                //Color emissionColor = mat.GetColor(ShaderUtils.EmissionColor);
                //Vector4 emissionColorV4 = new Vector4(emissionColor.r, emissionColor.g, emissionColor.b, emissionColor.a);
                var tmpList = new List<Vector4>();
                mesh.GetUVs(3, tmpList);
                emissionColors.AddRange(tmpList);

                Transform meshTransform = renderer.transform;
                meshTransform.SetParent(null, true);
                Transform prevParent = meshTransform.parent;

                combineInstances.Add( new CombineInstance()
                {
                    mesh = mesh,
                    transform = meshTransform.localToWorldMatrix
                });

                meshTransform.SetParent( prevParent );
                renderer.enabled = false;
            }

            // Vector3 lastPos = transform.position;
            // transform.position = Vector3.zero;
            //
            // transform.position = lastPos;
            finalMesh.indexFormat = IndexFormat.UInt32;
            finalMesh.CombineMeshes(combineInstances.ToArray(), true, false);

            for ( int i = 0; i < combineInstances.Count; i++ )
            {
                UnityEngine.Object.Destroy(combineInstances[i].mesh);
            }

            //var poses = bindPosesContainer.sharedMesh.bindposes;
            //finalMesh.bindposes = poses;

            //finalMesh.boneWeights = boneWeights.ToArray();
            finalMesh.SetUVs(3, emissionColors);
            finalMesh.SetUVs(4, texturePointers);
            finalMesh.SetColors(colors);
            finalMesh.Optimize();

            finalMesh.UploadMeshData(true);

            var bounds = finalMesh.bounds;
            Vector3 newCenter = bounds.center;
            newCenter.Scale(new Vector3(1, 0, 1));
            bounds.center = newCenter;

            GameObject result = new GameObject("Combined Avatar Group");
            result.layer = renderers[0].gameObject.layer;
            result.transform.parent = null;
            result.transform.position = Vector3.zero;

            var newSkinnedMeshRenderer = result.AddComponent<SkinnedMeshRenderer>();
            newSkinnedMeshRenderer.sharedMesh = finalMesh;
            newSkinnedMeshRenderer.bones = allBones.ToArray();
            newSkinnedMeshRenderer.rootBone = null;
            //newSkinnedMeshRenderer.localBounds = bonesContainer.localBounds;
            newSkinnedMeshRenderer.sharedMaterials = materials.ToArray();
            newSkinnedMeshRenderer.quality = SkinQuality.Bone1;
            newSkinnedMeshRenderer.updateWhenOffscreen = true;
            newSkinnedMeshRenderer.skinnedMotionVectors = false;

            return result;
        }

        private static Coroutine combineAllListener = null;

        private static IEnumerator ListenForCombineKey()
        {
            while (true)
            {
                if ( Input.GetKeyDown(KeyCode.J))
                {
                    Debug.Log("Combining all!!");
                    CombineAll();
                }

                yield return null;
            }
        }

        public static GameObject Combine(SkinnedMeshRenderer bonesContainer, Transform root, System.Func<Renderer, bool> filterFunction = null)
        {
            if ( combineAllListener == null )
            {
                combineAllListener = CoroutineStarter.Start(ListenForCombineKey());
            }

            logger.logEnabled = true;

            //
            // Find first enabled SkinnedMeshRenderer with valid sharedMesh
            //
            SkinnedMeshRenderer bindPosesContainer = null;

            var renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(false);
            for ( int i = 0;
                i < renderers.Length;
                i++ )
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

            Material mainMaterial =  Resources.Load<Material>("OptimizedToonMaterial_TextureArray");
            List<Material> materials = new List<Material>();

            //for (int layerIndex = 0; layerIndex < layers.Count; layerIndex++)
            {
                //AvatarMeshCombiner.CombineLayer layer = layers[layerIndex];

                Material newMaterial = UnityEngine.Object.Instantiate(mainMaterial);
                materials.Add( newMaterial );

                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];

                    if (RendererIsInvalid(filterFunction, renderer))
                    {
                        continue;
                    }

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

                    int id1 = textureArrayHelper.AddTexture(baseMap);
                    int id2 = textureArrayHelper.AddTexture(emissionMap);

                    texturePointers.AddRange(Enumerable.Repeat(new Vector2(id1, id2), elementsCount));

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
            finalMesh.SetUVs(4, texturePointers);
            finalMesh.SetColors(colors);
            finalMesh.Optimize();
            finalMesh.UploadMeshData(false);

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
            combined.Add(result);
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
    }
}