namespace DCL
{
    public class AssetPromiseKeeper_GLTF : AssetPromiseKeeper<Asset_GLTF, AssetLibrary_GLTF, AssetPromise_GLTF>
    {
        public AssetPromiseKeeper_GLTF(AssetLibrary_GLTF library) : base(library)
        {
        }

        protected override void OnSilentForget(AssetPromise_GLTF promise)
        {
            promise.asset.Hide();
        }
    }
}
