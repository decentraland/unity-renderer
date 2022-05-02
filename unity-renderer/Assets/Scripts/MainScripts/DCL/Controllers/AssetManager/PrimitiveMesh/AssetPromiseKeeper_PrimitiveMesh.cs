using System;

namespace DCL
{
    public class AssetPromiseKeeper_PrimitiveMesh : AssetPromiseKeeper<Asset_PrimitiveMesh, AssetLibrary_RefCounted<Asset_PrimitiveMesh>, AssetPromise_PrimitiveMesh>
    {
        private static AssetPromiseKeeper_PrimitiveMesh instance;
        public static AssetPromiseKeeper_PrimitiveMesh i { get { return instance ??= new AssetPromiseKeeper_PrimitiveMesh(); } }
        
        public AssetPromiseKeeper_PrimitiveMesh() : base(new AssetLibrary_RefCounted<Asset_PrimitiveMesh>()) { }
    }
}