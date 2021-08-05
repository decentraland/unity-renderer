using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AssetPromiseKeeper_AssetBundleModelTests")]

namespace DCL
{
    public class AssetPromiseKeeper_AB_GameObject : AssetPromiseKeeper<Asset_AB_GameObject, AssetLibrary_AB_GameObject, AssetPromise_AB_GameObject>
    {
        private static AssetPromiseKeeper_AB_GameObject instance;
        public static AssetPromiseKeeper_AB_GameObject i { get { return instance ??= new AssetPromiseKeeper_AB_GameObject(); } }

        public AssetPromiseKeeper_AB_GameObject() : base(new AssetLibrary_AB_GameObject()) { }
        protected override void OnSilentForget(AssetPromise_AB_GameObject promise) { promise.asset.Hide(); }

    }

}