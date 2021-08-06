namespace DCL
{
    public class AssetPromiseKeeper_Texture : AssetPromiseKeeper<Asset_Texture, AssetLibrary_Texture, AssetPromise_Texture>
    {
        private static AssetPromiseKeeper_Texture instance;
        public static AssetPromiseKeeper_Texture i { get { return instance ??= new AssetPromiseKeeper_Texture(); } }

        public AssetPromiseKeeper_Texture() : base(new AssetLibrary_Texture()) { }
    }
}