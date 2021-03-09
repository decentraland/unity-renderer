namespace DCL
{

    public class AssetLibrary_GLTF : AssetLibrary_Poolable<Asset_GLTF>
    {
        public AssetLibrary_GLTF() : base(new PoolInstantiator_GLTF())
        {
        }
    }
}
