using System;

namespace DCL
{
    public class AssetPromiseKeeper_DCLTexture : AssetPromiseKeeper<Asset_DCLTexture, AssetLibrary_RefCounted<Asset_DCLTexture>, AssetPromise_DCLTexture>
    {
        private static AssetPromiseKeeper_DCLTexture instance;
        public static AssetPromiseKeeper_DCLTexture i { get { return instance ??= new AssetPromiseKeeper_DCLTexture(); } }
        
        public AssetPromiseKeeper_DCLTexture() : base(new AssetLibrary_RefCounted<Asset_DCLTexture>()) { }
    }
}