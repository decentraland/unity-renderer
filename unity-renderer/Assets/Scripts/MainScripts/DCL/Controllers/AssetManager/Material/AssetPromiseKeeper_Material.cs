namespace DCL
{
    public class AssetPromiseKeeper_Material : AssetPromiseKeeper<Asset_Material, AssetLibrary_RefCounted<Asset_Material>, AssetPromise_Material>
    {
        private static AssetPromiseKeeper_Material instance;
        public static AssetPromiseKeeper_Material i { get { return instance ??= new AssetPromiseKeeper_Material(); } }
        public AssetPromiseKeeper_Material() : base(new AssetLibrary_RefCounted<Asset_Material>()) { }
    }
}