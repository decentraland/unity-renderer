using DCL;
using DCL.Helpers;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

public class AssetPK_GLTF_Tests : TestsBase
{
    [UnityTest]
    public IEnumerator LoadAndUnloadSingleFrame()
    {
        yield return base.InitScene();

        var library = new AssetLibrary_GLTF();
        var keeper = new AssetPromiseKeeper_GLTF(library);

        string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
        AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF loadedAsset = null;

        prom.OnSuccessEvent +=
            (x) =>
            {
                loadedAsset = x;
            };

        keeper.Keep(prom);
        yield return prom;

        keeper.Forget(prom);
        keeper.Keep(prom);

        yield return prom;

        Assert.IsTrue(prom.asset.container != null);
    }

    [UnityTest]
    public IEnumerator IsPooledAndSetupCorrectlyAfterLoad()
    {
        yield return base.InitScene();

        var library = new AssetLibrary_GLTF();
        var keeper = new AssetPromiseKeeper_GLTF(library);

        string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
        AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF loadedAsset = null;


        prom.OnSuccessEvent +=
            (x) =>
            {
                loadedAsset = x;
            }
        ;

        Vector3 initialPos = Vector3.one;
        Quaternion initialRot = Quaternion.LookRotation(Vector3.right, Vector3.up);
        Vector3 initialScale = Vector3.one * 2;

        prom.settings.initialLocalPosition = initialPos;
        prom.settings.initialLocalRotation = initialRot;
        prom.settings.initialLocalScale = initialScale;

        keeper.Keep(prom);

        yield return prom;

        Assert.IsTrue(PoolManager.i.ContainsPool(loadedAsset.id), "Not in pool after loaded!");

        Pool pool = PoolManager.i.GetPool(loadedAsset.id);

        Assert.AreEqual(0, pool.inactiveCount, "incorrect inactive objects in pool");
        Assert.AreEqual(1, pool.activeCount, "incorrect active objects in pool");
        Assert.IsTrue(pool.original != loadedAsset.container, "In pool, the original gameObject must NOT be the loaded asset!");

        //NOTE(Brian): If the following asserts fail, check that ApplySettings_LoadStart() is called from AssetPromise_GLTF.AddToLibrary() when the clone is made.
        Assert.AreEqual(initialPos.ToString(), loadedAsset.container.transform.localPosition.ToString(), "initial position not set correctly!");
        Assert.AreEqual(initialRot.ToString(), loadedAsset.container.transform.localRotation.ToString(), "initial rotation not set correctly!");
        Assert.AreEqual(initialScale.ToString(), loadedAsset.container.transform.localScale.ToString(), "initial scale not set correctly!");

        Assert.IsTrue(loadedAsset != null);
        Assert.IsTrue(library.Contains(loadedAsset));
        Assert.AreEqual(1, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator LoadAndUnload()
    {
        yield return base.InitScene();

        var library = new AssetLibrary_GLTF();
        var keeper = new AssetPromiseKeeper_GLTF(library);

        string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
        AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF loadedAsset = null;


        prom.OnSuccessEvent +=
            (x) =>
            {
                loadedAsset = x;
            };

        keeper.Keep(prom);

        Assert.IsTrue(prom.state == AssetPromiseState.LOADING);

        yield return prom;

        Assert.IsTrue(loadedAsset != null);
        //Assert.IsTrue(loadedAsset.isLoaded);
        Assert.IsTrue(library.Contains(loadedAsset));
        Assert.AreEqual(1, library.masterAssets.Count);

        keeper.Forget(prom);

        yield return prom;

        Assert.IsTrue(prom.state == AssetPromiseState.IDLE_AND_EMPTY);

        yield return MemoryManager.i.CleanupPoolsIfNeeded(forceCleanup: true);

        Assert.IsTrue(!library.Contains(loadedAsset.id));
        Assert.AreEqual(0, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator UnloadWhileLoading()
    {
        yield return base.InitScene();

        var library = new AssetLibrary_GLTF();
        var keeper = new AssetPromiseKeeper_GLTF(library);

        string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";
        AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF asset = null;
        prom.OnSuccessEvent += (x) => { asset = x; };

        keeper.Keep(prom);

        yield return new WaitForSeconds(0.5f);

        keeper.Forget(prom);

        Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

        yield return new WaitForSeconds(1.5f);

        yield return MemoryManager.i.CleanupPoolsIfNeeded(forceCleanup: true);

        Assert.IsTrue(asset == null);
        Assert.IsTrue(!library.Contains(asset));
        Assert.AreEqual(0, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator LoadMany()
    {
        yield return InitScene();

        var library = new AssetLibrary_GLTF();
        var keeper = new AssetPromiseKeeper_GLTF(library);

        string url = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb";

        string id = "1";
        AssetPromise_GLTF prom = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF asset = null;
        prom.OnSuccessEvent += (x) => { asset = x; };

        AssetPromise_GLTF prom2 = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF asset2 = null;
        prom2.OnSuccessEvent += (x) => { asset2 = x; };

        AssetPromise_GLTF prom3 = new AssetPromise_GLTF(scene.contentProvider, url);
        Asset_GLTF asset3 = null;
        prom3.OnSuccessEvent += (x) => { asset3 = x; };

        keeper.Keep(prom);
        keeper.Keep(prom2);
        keeper.Keep(prom3);

        Assert.AreEqual(3, keeper.waitingPromisesCount);

        yield return prom;
        yield return new WaitForSeconds(2.0f);

        Assert.IsTrue(asset != null);
        Assert.IsTrue(asset2 != null);
        Assert.IsTrue(asset3 != null);

        Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
        Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
        Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

        Assert.IsTrue(asset2.id == asset.id);
        Assert.IsTrue(asset3.id == asset.id);
        Assert.IsTrue(asset2.id == asset3.id);

        Assert.IsTrue(asset != asset2);
        Assert.IsTrue(asset != asset3);
        Assert.IsTrue(asset2 != asset3);

        Assert.IsTrue(library.Contains(asset));
        Assert.AreEqual(1, library.masterAssets.Count);
    }


}
