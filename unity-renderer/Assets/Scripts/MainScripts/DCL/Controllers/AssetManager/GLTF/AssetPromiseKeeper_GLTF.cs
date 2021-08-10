namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        private static AssetPromiseKeeper_GLTF instance;
        public static AssetPromiseKeeper_GLTF i { get { return instance ??= new AssetPromiseKeeper_GLTF(); } }
        
        public AssetPromiseKeeper_GLTF() : base(new AssetLibrary_GLTF()) { }
    }
}