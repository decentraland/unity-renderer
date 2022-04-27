using System;

namespace DCL
{
    public class AssetPromiseKeeper_TextureResource : AssetPromiseKeeper<Asset_TextureResource, AssetLibrary_RefCounted<Asset_TextureResource>, AssetPromise_TextureResource>
    {
        private static AssetPromiseKeeper_TextureResource instance;
        public static AssetPromiseKeeper_TextureResource i { get { return instance ??= new AssetPromiseKeeper_TextureResource(); } }
        
        public AssetPromiseKeeper_TextureResource() : base(new AssetLibrary_RefCounted<Asset_TextureResource>()) { }
    }
}