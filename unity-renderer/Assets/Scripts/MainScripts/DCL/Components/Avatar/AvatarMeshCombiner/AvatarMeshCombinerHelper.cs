using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    /// <summary>
    /// 
    /// </summary>
    public class AvatarMeshCombinerHelper : IDisposable
    {
        private static bool VERBOSE = true;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public GameObject container { get; private set; }
        public SkinnedMeshRenderer renderer { get; private set; }

        public AvatarMeshCombinerHelper (GameObject container = null)
        {
            this.container = container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bonesContainer"></param>
        /// <param name="renderersToCombine"></param>
        /// <param name="materialAsset"></param>
        /// <returns></returns>
        public bool Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderersToCombine, Material materialAsset)
        {
            float time = Time.realtimeSinceStartup;

            SkinnedMeshRenderer[] renderers = renderersToCombine;

            // Sanitize renderers list
            renderers = renderers.Where( (x) => x != null && x.enabled && x.sharedMesh != null ).ToArray();

            if ( renderers.Length == 0 )
                return false;

            bool success = CombineInternal(
                bonesContainer,
                renderers,
                materialAsset);

            float totalTime = Time.realtimeSinceStartup - time;
            logger.Log("AvatarMeshCombiner.Combine time = " + totalTime);

            // Disable original renderers
            for ( int i = 0; i < renderers.Length; i++ )
            {
                renderers[i].enabled = false;
            }

            return success;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bonesContainer"></param>
        /// <param name="renderers"></param>
        /// <param name="materialAsset"></param>
        /// <returns></returns>
        internal bool CombineInternal(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderers, Material materialAsset)
        {
            AvatarMeshCombiner.Output output = AvatarMeshCombiner.CombineSkinnedMeshes(
                bonesContainer.sharedMesh.bindposes,
                bonesContainer.bones,
                renderers,
                materialAsset);

            if ( !output.isValid )
            {
                logger.LogError("AvatarMeshCombiner", "Combine failed!");
                return false;
            }

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

        /// <summary>
        /// 
        /// </summary>
        private void UnloadAssets()
        {
            if (renderer == null)
                return;

            if (renderer.sharedMesh != null)
            {
                Object.Destroy(renderer.sharedMesh);
            }

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