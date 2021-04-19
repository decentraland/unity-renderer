namespace DCL
{
    public class AssetLibrary_Texture : AssetLibrary_RefCounted<Asset_Texture>
    {
        public override void Release(Asset_Texture asset)
        {
            if (asset.dependencyAsset != null)
                base.Release(asset.dependencyAsset);

            base.Release(asset);
        }
    }
}