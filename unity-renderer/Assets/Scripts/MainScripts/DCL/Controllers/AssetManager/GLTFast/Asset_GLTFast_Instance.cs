using System;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class Asset_GLTFast_Instance : Asset_WithPoolableContainer
    {
        internal AssetPromise_GLTFast_Loader ownerPromise;
        public sealed override GameObject container { get; set; }

        public Asset_GLTFast_Instance()
        {
            container = new GameObject("GLTFast Container")
            {
                transform = { position = EnvironmentSettings.MORDOR }
            };
        }

        public Renderer[] ExtractRenderers() => container.GetComponentsInChildren<Renderer>();

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

            var tris = 0;
            foreach (Renderer renderer in renderers)
            {
                switch (renderer)
                {
                    case SkinnedMeshRenderer skinnedMeshRenderer:
                        Mesh skinnedMesh = skinnedMeshRenderer.sharedMesh;
                        meshes.Add(skinnedMesh);
                        tris += skinnedMesh.triangles.Length;
                        break;
                    case MeshRenderer:
                        Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                        meshes.Add(mesh);
                        tris += mesh.triangles.Length;
                        break;
                }

                foreach (Material material in renderer.sharedMaterials) { materials.Add(material); }
            }

            for (int i = 0; i < ownerPromise.asset.GltfImport.TextureCount; i++)
                textures.Add(ownerPromise.asset.GltfImport.GetTexture(i));

            HashSet<AnimationClip> animations = new HashSet<AnimationClip>();

            var animationClips = ownerPromise.asset.GltfImport.GetAnimationClips();

            if (animationClips != null)
            {
                foreach (AnimationClip clip in animationClips)
                {
                    animations.Add(clip);
                }
            }

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
                totalTriangleCount = tris,
                animationClipSize = 0,
                meshDataSize = 0
            };
        }
    }
}
