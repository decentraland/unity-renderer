namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        public AssetPromiseKeeper_GLTF() : base(new AssetLibrary_GLTF())
        {
        }

        protected override void OnSilentForget(AssetPromise_GLTF promise)
        {
            promise.asset.Hide();
        }
    }
}
