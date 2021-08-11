namespace DCL
{
    public class AssetPromiseKeeper_AB : AssetPromiseKeeper<Asset_AB, AssetLibrary_AB, AssetPromise_AB>
    {
        private static AssetPromiseKeeper_AB instance;
        public static AssetPromiseKeeper_AB i { get { return instance ??= new AssetPromiseKeeper_AB(); } }

        public AssetPromiseKeeper_AB() : base(new AssetLibrary_AB()) { }
    }
}