using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class AvatarMeshCombinerHelper : IDisposable
    {
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = LogType.Warning };

        public GameObject container { get; private set; }
        public SkinnedMeshRenderer renderer { get; private set; }

        public AvatarMeshCombinerHelper (GameObject container = null)
        {
            this.container = container;
        }

        public void Combine(Transform root, SkinnedMeshRenderer bonesContainer)
        {
            if ( renderer != null )
                renderer.enabled = false;

            float time = Time.realtimeSinceStartup;

            SkinnedMeshRenderer[] renderers = root.GetComponentsInChildren<SkinnedMeshRenderer>();

            Material materialAsset = Resources.Load<Material>("OptimizedToonMaterial");

            renderers = renderers.Where((r) => !r.transform.parent.gameObject.name.Contains("Mask")).ToArray();

            bool combineSuccess = CombineInternal(
                bonesContainer,
                renderers,
                materialAsset);

            float totalTime = Time.realtimeSinceStartup - time;
            logger.Log("AvatarMeshCombiner.Combine time = " + totalTime);

            if ( !combineSuccess )
            {
                if ( renderer != null )
                    renderer.enabled = true;

                return;
            }

            for ( int i = 0; i < renderers.Length; i++ )
            {
                renderers[i].enabled = false;
            }

            renderer.enabled = true;

            container.transform.SetParent( root, true );
        }

        internal bool CombineInternal(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderers, Material materialAsset)
        {
            //
            // Reset bones to put character in T pose. Renderers are going to be baked later.
            // This is a workaround, it had to be done because renderers original matrices don't match the T pose.
            // We need wearables in T pose to properly combine the avatar mesh. 
            //
            AvatarMeshCombinerUtils.ResetBones(bonesContainer);

            //
            // Get combined layers. Layers are groups of renderers that have a id -> tex mapping.
            //
            // This id is going to get written to uv channels so the material can use up to 12 textures
            // in a single draw call.
            //
            // Layers are divided accounting for the 12 textures limit and transparency/opaque limit.
            //
            var layers = CombineLayerUtils.Slice( renderers );

            if ( layers == null )
            {
                logger.LogError("AvatarMeshCombiner", "Combine failed!");
                return false;
            }

            // Prepare mesh data for the combine operation
            AvatarMeshCombinerOutput output = AvatarMeshCombiner.CombineSkinnedMeshes(
                bonesContainer.sharedMesh.bindposes,
                layers,
                materialAsset);

            Transform rootBone = bonesContainer.rootBone;

            if ( container == null )
                this.container = new GameObject("CombinedAvatar");

            if ( renderer == null )
                renderer = container.AddComponent<SkinnedMeshRenderer>();

            UnloadAssets();

            container.layer = bonesContainer.gameObject.layer;
            renderer.sharedMesh = output.mesh;
            renderer.bones = bonesContainer.bones;
            renderer.rootBone = rootBone;
            renderer.sharedMaterials = output.materials;
            renderer.quality = SkinQuality.Bone1;
            renderer.updateWhenOffscreen = false;
            renderer.skinnedMotionVectors = false;
            renderer.enabled = true;

            logger.Log("AvatarMeshCombiner", "Finish combining avatar. Click here to focus on GameObject.", container);
            return true;
        }

        private void UnloadAssets()
        {
            if (renderer == null)
                return;

            if (renderer.sharedMesh != null)
                Object.Destroy(renderer.sharedMesh);

            if ( renderer.sharedMaterials != null)
            {
                foreach ( var material in renderer.sharedMaterials )
                {
                    Object.Destroy(material);
                }
            }
        }

        public void Dispose()
        {
            UnloadAssets();
            Object.Destroy(container);
        }
    }
}