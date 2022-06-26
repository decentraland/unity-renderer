using System;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_GLTFast_GameObject : Asset_WithPoolableContainer
    {
        internal AssetPromise_GLTFast ownerPromise;
        public sealed override GameObject container { get; set; }

        public Asset_GLTFast_GameObject()
        {
            container = new GameObject("GLTFast Container")
            {
                transform = { position = EnvironmentSettings.MORDOR }
            };
        }

        public Renderer[] ExtractRenderers()
        {
            return container.GetComponentsInChildren<Renderer>();
        }

        public override void Cleanup()
        {
            AssetPromiseKeeper_GLTFast.i.Forget(ownerPromise);
            Object.Destroy(container);
        }

        public void Hide()
        {
            container.SetActive(false);
            container.transform.parent = null;
            container.transform.position = EnvironmentSettings.MORDOR;
        }

        public void Show(Action success)
        {
            container.SetActive(true);
            success?.Invoke();
        }
        public Rendereable ToRendereable()
        {
            Renderer[] renderers = container.gameObject.GetComponentsInChildren<Renderer>();
            HashSet<Mesh> meshes = new HashSet<Mesh>();
            HashSet<Material> materials = new HashSet<Material>();
            HashSet<Texture> textures = new HashSet<Texture>();

            foreach (Renderer renderer in renderers)
            {
                // extract meshes
                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    meshes.Add(skinnedMeshRenderer.sharedMesh);
                } else if (renderer is MeshRenderer)
                {
                    meshes.Add(renderer.GetComponent<MeshFilter>().sharedMesh);
                }
                
                // extract materials
                foreach (Material material in renderer.sharedMaterials)
                {
                    materials.Add(material);
                }
            }

            // Warning! (Kinerius) We are getting the original textures and animations, this may cause bugs!
            for (int i = 0; i < ownerPromise.asset.gltfImport.textureCount; i++)
            {
                textures.Add(ownerPromise.asset.gltfImport.GetTexture(i));
            }
            
            HashSet<AnimationClip> animations = new HashSet<AnimationClip>();

            var animationClips = ownerPromise.asset.gltfImport.GetAnimationClips();

            if (animationClips != null)
            {
                foreach (AnimationClip clip in animationClips)
                {
                    animations.Add(clip);
                }
            }
            // End of Warning!

            return new Rendereable
            {
                container = container,
                meshes = meshes,
                renderers = new HashSet<Renderer>(renderers),
                materials = materials,
                textures = textures,
                meshToTriangleCount = new Dictionary<Mesh, int>(),
                animationClips = animations,

                // TODO: Fill me!
                totalTriangleCount = 0,
                animationClipSize = 0,
                meshDataSize = 0
            };
        }
    }
}