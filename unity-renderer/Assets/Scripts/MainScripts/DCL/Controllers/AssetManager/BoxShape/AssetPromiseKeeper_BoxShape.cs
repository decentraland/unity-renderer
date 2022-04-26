using System;

namespace DCL
{
    public class AssetPromiseKeeper_BoxShape : AssetPromiseKeeper<Asset_BoxShape, AssetLibrary_RefCounted<Asset_BoxShape>, AssetPromise_BoxShape>
    {
        private static AssetPromiseKeeper_BoxShape instance;
        public static AssetPromiseKeeper_BoxShape i { get { return instance ??= new AssetPromiseKeeper_BoxShape(); } }
        
        public AssetPromiseKeeper_BoxShape() : base(new AssetLibrary_RefCounted<Asset_BoxShape>()) { }
        public AssetPromiseKeeper_BoxShape(AssetLibrary_RefCounted<Asset_BoxShape> library) : base(library) { }
    }
}