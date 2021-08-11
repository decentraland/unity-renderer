namespace DCL
{
    public class AssetPromiseKeeper_Gif : AssetPromiseKeeper<Asset_Gif, AssetLibrary_RefCounted<Asset_Gif>, AssetPromise_Gif>
    {
        private static AssetPromiseKeeper_Gif instance;
        public static AssetPromiseKeeper_Gif i { get { return instance ??= new AssetPromiseKeeper_Gif(); } }

        public AssetPromiseKeeper_Gif() : base(new AssetLibrary_RefCounted<Asset_Gif>()) { }
    }
}