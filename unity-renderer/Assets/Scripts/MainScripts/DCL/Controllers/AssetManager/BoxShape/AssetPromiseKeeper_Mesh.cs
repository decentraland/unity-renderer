using System;

namespace DCL
{
    public class AssetPromiseKeeper_Mesh : AssetPromiseKeeper<Asset_Mesh, AssetLibrary_RefCounted<Asset_Mesh>, AssetPromise_Mesh>
    {
        private static AssetPromiseKeeper_Mesh instance;
        public static AssetPromiseKeeper_Mesh i { get { return instance ??= new AssetPromiseKeeper_Mesh(); } }
        
        public AssetPromiseKeeper_Mesh() : base(new AssetLibrary_RefCounted<Asset_Mesh>()) { }
        public AssetPromiseKeeper_Mesh(AssetLibrary_RefCounted<Asset_Mesh> library) : base(library) { }
    }
}