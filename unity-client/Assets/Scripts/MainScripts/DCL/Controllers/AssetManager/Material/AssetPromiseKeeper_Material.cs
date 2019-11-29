namespace DCL
{
    public class AssetLibrary_Material : AssetLibrary<Asset_Material>
    {
        public override bool Add(Asset_Material asset)
        {
            throw new System.NotImplementedException();
        }

        public override void Cleanup()
        {
            throw new System.NotImplementedException();
        }

        public override bool Contains(object id)
        {
            throw new System.NotImplementedException();
        }

        public override bool Contains(Asset_Material asset)
        {
            throw new System.NotImplementedException();
        }

        public override Asset_Material Get(object id)
        {
            throw new System.NotImplementedException();
        }

        public override void Release(Asset_Material asset)
        {
            throw new System.NotImplementedException();
        }
    }

    public class AssetPromiseKeeper_Material : AssetPromiseKeeper<Asset_Material, AssetLibrary_Material, AssetPromise_Material>
    {
        public AssetPromiseKeeper_Material(AssetLibrary_Material library) : base(library)
        {
        }
    }
}
