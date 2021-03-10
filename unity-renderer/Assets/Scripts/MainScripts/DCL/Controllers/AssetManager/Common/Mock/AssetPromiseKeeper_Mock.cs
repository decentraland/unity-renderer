namespace DCL
{
    public class Asset_Mock : Asset
    {
        public override void Cleanup()
        {
        }
    }

    public class AssetPromiseKeeper_Mock : AssetPromiseKeeper<Asset_Mock, AssetLibrary_Mock, AssetPromise_Mock>
    {
        public AssetPromiseKeeper_Mock(AssetLibrary_Mock library) : base(library)
        {
        }

        protected override void OnSilentForget(AssetPromise_Mock promise)
        {
            base.OnSilentForget(promise);
        }
    }
}
