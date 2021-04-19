namespace DCL
{
    /// <summary>
    /// An AssetLibrary has a collection of Assets, and it handles caching and storing of "master assets".
    /// 
    /// If we have a pool that is useful for the AssetType specified, this class should act as a wrapper
    /// for said pool.
    /// </summary>
    /// <typeparam name="AssetType">The Asset type to be handled by the library</typeparam>
    public abstract class AssetLibrary<AssetType>
        where AssetType : Asset
    {

        public abstract bool Add(AssetType asset);

        public abstract AssetType Get(object id);

        public abstract void Release(AssetType asset);

        public abstract bool Contains(object id);

        public abstract bool Contains(AssetType asset);

        public abstract void Cleanup();
    }
}
