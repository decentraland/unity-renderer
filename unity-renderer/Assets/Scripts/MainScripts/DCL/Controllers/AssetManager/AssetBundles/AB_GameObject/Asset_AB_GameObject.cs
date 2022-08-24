using System.Collections.Generic;
using DCL.Configuration;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using UnityEngine;

namespace DCL
{
    public class Asset_AB_GameObject : Asset_WithPoolableContainer
    {
        private const string CONTAINER_GO_NAME = "AB Container";
    
        internal AssetPromise_AB ownerPromise;

        public override GameObject container { get; set; }
        public Dictionary<Mesh, int> meshToTriangleCount = new Dictionary<Mesh, int>();
        public HashSet<Renderer> renderers = new HashSet<Renderer>();
        public HashSet<Mesh> meshes = new HashSet<Mesh>();
        public HashSet<Material> materials = new HashSet<Material>();
        public HashSet<Texture> textures = new HashSet<Texture>();
        public HashSet<AnimationClip> animationClips = new HashSet<AnimationClip>();
        public int totalTriangleCount = 0;
        public long animationClipSize = 0;
        public long meshDataSize = 0;

        public Asset_AB_GameObject()
        {
            container = new GameObject(CONTAINER_GO_NAME);
            // Hide gameobject until it's been correctly processed, otherwise it flashes at 0,0,0
            container.transform.position = EnvironmentSettings.MORDOR;
        }

        public override object Clone()
        {
            Asset_AB_GameObject result = this.MemberwiseClone() as Asset_AB_GameObject;
            result.meshes = new HashSet<Mesh>(meshes);
            result.meshToTriangleCount = new Dictionary<Mesh, int>(meshToTriangleCount);
            result.renderers = new HashSet<Renderer>(renderers);
            result.materials = new HashSet<Material>(materials);
            result.textures = new HashSet<Texture>(textures);
            result.animationClips = new HashSet<AnimationClip>(animationClips);
            return result;
        }

        public override void Cleanup()
        {
            AssetPromiseKeeper_AB.i.Forget(ownerPromise);
            Object.Destroy(container);
        }

        public void Show(System.Action OnFinish)
        {
            OnFinish?.Invoke();
        }

        public void Hide()
        {
            container.transform.parent = null;
            container.transform.position = EnvironmentSettings.MORDOR;
        }
        public void SetTextures(HashSet<Texture> texturesHashSet)
        {
            textures = texturesHashSet;

            for (int i = 0; i < textures.Count; i++)
            {
                PerformanceAnalytics.ABTextureTracker.Track();
            }
        }
    }
}