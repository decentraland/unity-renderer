using System;
using System.Linq;
using GPUSkinning;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace DCL
{
    public interface IAvatarMeshCombinerHelper : IDisposable
    {
        public bool useCullOpaqueHeuristic { get; set; }
        public bool prepareMeshForGpuSkinning { get; set; }
        public bool uploadMeshToGpu { get; set; }
        public bool enableCombinedMesh { get; set; }

        public GameObject container { get; }
        public SkinnedMeshRenderer renderer { get; }

        public bool Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderersToCombine, bool keepPose = true);
        public bool Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderersToCombine, Material materialAsset, bool keepPose = true);
    }
    
    /// <summary>
    /// AvatarMeshCombinerHelper uses the AvatarMeshCombiner utility class to combine many skinned renderers
    /// into a single one.
    ///
    /// This class will recycle the same gameObject and renderer each time it is called,
    /// and binds the AvatarMeshCombiner output to a proper well configured SkinnedMeshRenderer. 
    /// </summary>
    public class AvatarMeshCombinerHelper : IAvatarMeshCombinerHelper
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public GameObject container { get; private set; }
        public SkinnedMeshRenderer renderer { get; private set; }

        public bool useCullOpaqueHeuristic { get; set; } = false;
        public bool prepareMeshForGpuSkinning { get; set; } = false;
        public bool uploadMeshToGpu { get; set; } = true;
        public bool enableCombinedMesh { get; set; } = true;

        private AvatarMeshCombiner.Output? lastOutput;

        public AvatarMeshCombinerHelper (GameObject container = null) 
        { 
            this.container = container;
        }

        /// <summary>
        /// Combine will use AvatarMeshCombiner to generate a combined avatar mesh.
        /// After the avatar is combined, it will generate a new GameObject with a SkinnedMeshRenderer that contains
        /// the generated mesh. This GameObject and Renderer will be preserved over any number of Combine() calls.
        ///
        /// Uses a predefined Material ready for the Avatar
        /// 
        /// For more details on the combining check out AvatarMeshCombiner.CombineSkinnedMeshes.
        /// </summary>
        /// <param name="bonesContainer">A SkinnedMeshRenderer that must contain the bones and bindposes that will be used by the combined avatar.</param>
        /// <param name="renderersToCombine">A list of avatar parts to be combined</param>
        public bool Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderersToCombine, bool keepPose = true) { return Combine(bonesContainer, renderersToCombine, Resources.Load<Material>("Avatar Material"), keepPose); }

        /// <summary>
        /// Combine will use AvatarMeshCombiner to generate a combined avatar mesh.
        /// After the avatar is combined, it will generate a new GameObject with a SkinnedMeshRenderer that contains
        /// the generated mesh. This GameObject and Renderer will be preserved over any number of Combine() calls.
        /// 
        /// For more details on the combining check out AvatarMeshCombiner.CombineSkinnedMeshes.
        /// </summary>
        /// <param name="bonesContainer">A SkinnedMeshRenderer that must contain the bones and bindposes that will be used by the combined avatar.</param>
        /// <param name="renderersToCombine">A list of avatar parts to be combined</param>
        /// <param name="materialAsset">A material asset that will serve as the base of the combine result. A new materialAsset will be created for each combined sub-mesh.</param>
        /// <returns>true if succeeded, false if not</returns>
        public bool Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderersToCombine, Material materialAsset, bool keepPose)
        {
            Assert.IsTrue(bonesContainer != null, "bonesContainer should never be null!");
            Assert.IsTrue(renderersToCombine != null, "renderersToCombine should never be null!");
            Assert.IsTrue(materialAsset != null, "materialAsset should never be null!");
            Debug.Log("FA");
            SkinnedMeshRenderer[] renderers = renderersToCombine;
            Debug.Log("FB");

            // Sanitize renderers list
            renderers = renderers.Where( (x) => x != null && x.sharedMesh != null ).ToArray();
            Debug.Log("FC");

            if ( renderers.Length == 0 )
                return false;
            Debug.Log("FD");

            bool success = CombineInternal(
                bonesContainer,
                renderers,
                materialAsset,
                keepPose);
            Debug.Log("FE");

            // Disable original renderers
            for ( int i = 0; i < renderers.Length; i++ )
            {
                renderers[i].enabled = false;
            }
            Debug.Log("FF");

            return success;
        }

        private bool CombineInternal(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderers, Material materialAsset, bool keepPose)
        {
            Debug.Log("GA");

            Assert.IsTrue(bonesContainer != null, "bonesContainer should never be null!");
            Assert.IsTrue(bonesContainer.sharedMesh != null, "bonesContainer should never be null!");
            Assert.IsTrue(bonesContainer.sharedMesh.bindposes != null, "bonesContainer bindPoses should never be null!");
            Assert.IsTrue(bonesContainer.bones != null, "bonesContainer bones should never be null!");
            Assert.IsTrue(renderers != null, "renderers should never be null!");
            Assert.IsTrue(materialAsset != null, "materialAsset should never be null!");
            Debug.Log("GB");

            CombineLayerUtils.ENABLE_CULL_OPAQUE_HEURISTIC = useCullOpaqueHeuristic;
            Debug.Log("GC");

            AvatarMeshCombiner.Output output = AvatarMeshCombiner.CombineSkinnedMeshes(
                bonesContainer.sharedMesh.bindposes,
                bonesContainer.bones,
                renderers,
                materialAsset,
                keepPose);
            Debug.Log("GD");

            if ( !output.isValid )
            {
                logger.LogError("AvatarMeshCombiner", "Combine failed!");
                return false;
            }
            Debug.Log("GF");

            Transform rootBone = bonesContainer.rootBone;
            Debug.Log("GG");

            if ( container == null )
                this.container = new GameObject("CombinedAvatar");

            if ( renderer == null )
                renderer = container.AddComponent<SkinnedMeshRenderer>();
            Debug.Log("GH");

            UnloadAssets();
            Debug.Log("GI");

            lastOutput = output;

            container.layer = bonesContainer.gameObject.layer;
            Debug.Log("GJ");

            renderer.sharedMesh = output.mesh;
            Debug.Log("GK");

            renderer.bones = bonesContainer.bones;
            Debug.Log("GL");

            renderer.rootBone = rootBone;
            Debug.Log("GM");

            renderer.sharedMaterials = output.materials;
            Debug.Log("GN");

            renderer.quality = SkinQuality.Bone4;
            Debug.Log("GO");

            renderer.updateWhenOffscreen = false;
            Debug.Log("GP");

            renderer.skinnedMotionVectors = false;
            Debug.Log("GQ");

            renderer.enabled = enableCombinedMesh;
            Debug.Log("GR");

            if (prepareMeshForGpuSkinning)
                GPUSkinningUtils.EncodeBindPosesIntoMesh(renderer);
            Debug.Log("GS");

            if (uploadMeshToGpu)
                output.mesh.UploadMeshData(true);
            Debug.Log("GT");

            logger.Log("AvatarMeshCombiner", "Finish combining avatar. Click here to focus on GameObject.", container);
            return true;
        }

        private void UnloadAssets()
        {
            if (!lastOutput.HasValue)
                return;

            if (lastOutput.Value.mesh != null)
                Object.Destroy(lastOutput.Value.mesh);

            if (lastOutput.Value.materials != null)
            {
                foreach ( var material in lastOutput.Value.materials )
                {
                    Object.Destroy(material);
                }
            }
        }

        /// <summary>
        /// Disposes the created mesh, materials, GameObject and Renderer.
        /// </summary>
        public void Dispose()
        {
            UnloadAssets();
            Object.Destroy(container);
        }
    }
}