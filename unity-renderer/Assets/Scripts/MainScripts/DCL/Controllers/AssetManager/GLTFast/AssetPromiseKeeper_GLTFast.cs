namespace DCL
{
    public class AssetPromiseKeeper_GLTFast : AssetPromiseKeeper<Asset_GLTFast_Loader, AssetLibrary_GLTFast, AssetPromise_GLTFast_Loader>
    {
        private static AssetPromiseKeeper_GLTFast instance;

        public static AssetPromiseKeeper_GLTFast i
        {
            get
            {
                return instance ??= new AssetPromiseKeeper_GLTFast();
            }
        }

        public AssetPromiseKeeper_GLTFast() : base(new AssetLibrary_GLTFast()) { }
    }
}
