﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace DCL
{
    /// <summary>
    /// AvatarMeshCombinerHelper uses the AvatarMeshCombiner utility class to combine many skinned renderers
    /// into a single one.
    ///
    /// This class will recycle the same gameObject and renderer each time it is called,
    /// and binds the AvatarMeshCombiner output to a proper well configured SkinnedMeshRenderer. 
    /// </summary>
    public class AvatarMeshCombinerHelper : IDisposable
    {
        private static bool VERBOSE = false;
        private static ILogger logger = new Logger(Debug.unityLogger.logHandler) { filterLogType = VERBOSE ? LogType.Log : LogType.Warning };

        public GameObject container { get; private set; }
        public SkinnedMeshRenderer renderer { get; private set; }

        public AvatarMeshCombinerHelper (GameObject container = null)
        {
            this.container = container;
        }

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
        public bool Combine(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderersToCombine, Material materialAsset)
        {
            Assert.IsTrue(bonesContainer != null, "bonesContainer should never be null!");
            Assert.IsTrue(renderersToCombine != null, "renderersToCombine should never be null!");
            Assert.IsTrue(materialAsset != null, "materialAsset should never be null!");

            SkinnedMeshRenderer[] renderers = renderersToCombine;

            // Sanitize renderers list
            renderers = renderers.Where( (x) => x != null && x.enabled && x.sharedMesh != null ).ToArray();

            if ( renderers.Length == 0 )
                return false;

            bool success = CombineInternal(
                bonesContainer,
                renderers,
                materialAsset);

            // Disable original renderers
            for ( int i = 0; i < renderers.Length; i++ )
            {
                renderers[i].enabled = false;
            }

            return success;
        }

        private bool CombineInternal(SkinnedMeshRenderer bonesContainer, SkinnedMeshRenderer[] renderers, Material materialAsset)
        {
            Assert.IsTrue(bonesContainer != null, "bonesContainer should never be null!");
            Assert.IsTrue(bonesContainer.sharedMesh != null, "bonesContainer should never be null!");
            Assert.IsTrue(bonesContainer.sharedMesh.bindposes != null, "bonesContainer bindPoses should never be null!");
            Assert.IsTrue(bonesContainer.bones != null, "bonesContainer bones should never be null!");
            Assert.IsTrue(renderers != null, "renderers should never be null!");
            Assert.IsTrue(materialAsset != null, "materialAsset should never be null!");

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
            renderer.quality = SkinQuality.Bone4;
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