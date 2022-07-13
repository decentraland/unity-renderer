using System;

namespace DCL
{
    public class AssetPromiseKeeper_Font : AssetPromiseKeeper<Asset_Font, AssetLibrary_RefCounted<Asset_Font>, AssetPromise_Font>
    {
        private static AssetPromiseKeeper_Font instance;
        public static AssetPromiseKeeper_Font i { get { return instance ??= new AssetPromiseKeeper_Font(); } }
        
        public AssetPromiseKeeper_Font() : base(new AssetLibrary_RefCounted<Asset_Font>()) { }
        public AssetPromiseKeeper_Font(AssetLibrary_RefCounted<Asset_Font> library) : base(library) { }
    }
}