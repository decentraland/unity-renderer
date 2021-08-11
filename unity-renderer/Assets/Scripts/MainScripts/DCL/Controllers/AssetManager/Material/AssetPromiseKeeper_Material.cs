using System;

namespace DCL
{
    public class AssetLibrary_Material : AssetLibrary<Asset_Material>
    {
        public override bool Add(Asset_Material asset) { throw new NotImplementedException(); }

        public override void Cleanup() { throw new NotImplementedException(); }

        public override bool Contains(object id) { throw new NotImplementedException(); }

        public override bool Contains(Asset_Material asset) { throw new NotImplementedException(); }

        public override Asset_Material Get(object id) { throw new NotImplementedException(); }

        public override void Release(Asset_Material asset) { throw new NotImplementedException(); }
    }

    public class AssetPromiseKeeper_Material : AssetPromiseKeeper<Asset_Material, AssetLibrary_Material, AssetPromise_Material>
    {
        private static AssetPromiseKeeper_Material instance;
        public static AssetPromiseKeeper_Material i { get { return instance ??= new AssetPromiseKeeper_Material(); } }
        
        public AssetPromiseKeeper_Material() : base(new AssetLibrary_Material()) { }
        public AssetPromiseKeeper_Material(AssetLibrary_Material library) : base(library) { }
    }
}