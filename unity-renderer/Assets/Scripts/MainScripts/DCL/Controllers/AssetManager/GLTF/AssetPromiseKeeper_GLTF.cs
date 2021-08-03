namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        public AssetPromiseKeeper_GLTF() : base(new AssetLibrary_GLTF()) { }
    }
}