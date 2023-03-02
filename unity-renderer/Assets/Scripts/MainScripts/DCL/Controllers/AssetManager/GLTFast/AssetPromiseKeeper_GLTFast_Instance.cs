//[assembly: InternalsVisibleTo("AssetPromiseKeeper_AssetBundleModelTests")]

namespace DCL
{
    public class AssetPromiseKeeper_GLTFast_Instance : AssetPromiseKeeper<Asset_GLTFast_Instance, AssetLibrary_GLTFast_Instance, AssetPromise_GLTFast_Instance>
    {
        private static AssetPromiseKeeper_GLTFast_Instance instance;

        public static AssetPromiseKeeper_GLTFast_Instance i
        {
            get
            {
                return instance ??= new AssetPromiseKeeper_GLTFast_Instance();
            }
        }

        public AssetPromiseKeeper_GLTFast_Instance() : base(new AssetLibrary_GLTFast_Instance()) { }

        protected override void OnSilentForget(AssetPromise_GLTFast_Instance promise)
        {
            promise.asset.Hide();
        }
    }
}
