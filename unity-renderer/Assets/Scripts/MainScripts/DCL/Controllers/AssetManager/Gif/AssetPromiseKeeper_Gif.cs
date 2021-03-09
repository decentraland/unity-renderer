namespace DCL
{
    public class AssetPromiseKeeper_Gif : AssetPromiseKeeper<Asset_Gif, AssetLibrary_RefCounted<Asset_Gif>, AssetPromise_Gif>
    {
        public AssetPromiseKeeper_Gif() : base(new AssetLibrary_RefCounted<Asset_Gif>())
        {
        }
    }
}