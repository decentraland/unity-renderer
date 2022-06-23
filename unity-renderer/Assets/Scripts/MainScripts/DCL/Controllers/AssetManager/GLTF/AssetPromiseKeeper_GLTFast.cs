namespace DCL
{
    public class AssetPromiseKeeper_GLTFast : AssetPromiseKeeper<Asset_GLTFast, AssetLibrary_GLTFast, AssetPromise_GLTFast>
    {
        private static AssetPromiseKeeper_GLTFast instance;
        public static AssetPromiseKeeper_GLTFast i { get { return instance ??= new AssetPromiseKeeper_GLTFast(); } }
        
        public AssetPromiseKeeper_GLTFast() : base(new AssetLibrary_GLTFast()) { }

        protected override void OnSilentForget(AssetPromise_GLTFast promise)
        {
            promise.OnSilentForget();
        }
    }
}