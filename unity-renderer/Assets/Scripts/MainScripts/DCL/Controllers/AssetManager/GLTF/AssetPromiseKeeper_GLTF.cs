using UnityGLTF;

namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        private static AssetPromiseKeeper_GLTF instance;
        public static AssetPromiseKeeper_GLTF i { get { return instance ??= new AssetPromiseKeeper_GLTF(); } }

        public GLTFThrottlingCounter throttlingCounter = new GLTFThrottlingCounter();
        public AssetPromiseKeeper_GLTF() : base(new AssetLibrary_GLTF()) { }

        protected override void OnSilentForget(AssetPromise_GLTF promise)
        {
            promise.OnSilentForget();
        }
    }
}