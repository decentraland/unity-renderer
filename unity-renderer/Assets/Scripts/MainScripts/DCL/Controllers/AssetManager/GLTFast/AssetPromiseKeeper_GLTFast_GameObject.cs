﻿//[assembly: InternalsVisibleTo("AssetPromiseKeeper_AssetBundleModelTests")]

namespace DCL
{
    public class AssetPromiseKeeper_GLTFast_GameObject : AssetPromiseKeeper<Asset_GLTFast_Instance, AssetLibrary_GLTFast_GameObject, AssetPromise_GLTFast_Instance>
    {
        private static AssetPromiseKeeper_GLTFast_GameObject instance;
        public static AssetPromiseKeeper_GLTFast_GameObject i { get { return instance ??= new AssetPromiseKeeper_GLTFast_GameObject(); } }

        public AssetPromiseKeeper_GLTFast_GameObject() : base(new AssetLibrary_GLTFast_GameObject()) { }
        protected override void OnSilentForget(AssetPromise_GLTFast_Instance promise) { promise.asset.Hide(); }
    }

}