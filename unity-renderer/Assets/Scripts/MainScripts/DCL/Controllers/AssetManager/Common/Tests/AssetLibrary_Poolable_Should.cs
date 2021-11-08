using DCL;
using NUnit.Framework;

namespace AssetPromiseKeeper_Tests
{
    public class AssetLibrary_Poolable_Should
    {
        [Test]
        public void AB_GLTF_ShouldNotCollideInPool()
        {
            const string assetId = "TheRealPravus";

            AssetLibrary_GLTF libraryGltf = new AssetLibrary_GLTF();
            AssetLibrary_AB_GameObject libraryAbGameObject = new AssetLibrary_AB_GameObject();

            Asset_GLTF assetGltf = new Asset_GLTF();
            Asset_AB_GameObject assetAbGameObject = new Asset_AB_GameObject();

            assetGltf.id = assetId;
            assetAbGameObject.id = assetId;

            libraryGltf.Add(assetGltf);
            libraryAbGameObject.Add(assetAbGameObject);

            var gltfPoolId = libraryGltf.AssetIdToPoolId(assetGltf.id);
            var abPoolId = libraryAbGameObject.AssetIdToPoolId(assetAbGameObject.id);

            Assert.IsTrue(PoolManager.i.ContainsPool(gltfPoolId));
            Assert.IsTrue(PoolManager.i.ContainsPool(abPoolId));

            Pool gltfPool = PoolManager.i.GetPool(gltfPoolId);
            Pool abPool = PoolManager.i.GetPool(abPoolId);

            Assert.AreNotSame(gltfPool, abPool);

            assetGltf.Cleanup();
            assetAbGameObject.Cleanup();
            PoolManager.i.Dispose();
        }
    }
}