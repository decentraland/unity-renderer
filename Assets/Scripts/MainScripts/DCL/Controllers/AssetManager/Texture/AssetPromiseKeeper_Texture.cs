namespace DCL
{
    public class AssetLibrary_Texture : AssetLibrary<Asset_Texture>
    {
        public override void Add(Asset_Texture asset)
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

        public override bool Contains(Asset_Texture asset)
        {
            throw new System.NotImplementedException();
        }

        public override Asset_Texture Get(object id)
        {
            throw new System.NotImplementedException();
        }

        public override void Release(Asset_Texture asset)
        {
            throw new System.NotImplementedException();
        }
    }
    public class AssetPromiseKeeper_Texture : AssetPromiseKeeper<Asset_Texture, AssetLibrary_Texture, AssetPromise_Texture>
    {
        public AssetPromiseKeeper_Texture(AssetLibrary_Texture library) : base(library)
        {
        }
    }
}
