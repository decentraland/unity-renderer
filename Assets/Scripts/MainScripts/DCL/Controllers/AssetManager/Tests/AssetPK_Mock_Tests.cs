using DCL;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;
public class AssetPK_Mock_Tests
{
    [UnityTest]
    public IEnumerator PromiseLoadCallback()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        AssetPromise_Mock prom = new AssetPromise_Mock();

        bool successCalled = false;

        prom.idGenerator = "1";

        System.Action<Asset_Mock> testEvent = (x) =>
        {
            successCalled = true;
        };

        prom.OnSuccessEvent += testEvent;

        prom.SetLibrary_Test(library);
        prom.Load_Test();

        yield return prom;

        Assert.IsTrue(successCalled == true, "Success callback wasn't called!");

        successCalled = false;
        prom.Load_Test();
        Assert.IsTrue(successCalled == false, "Success callback was called when it shouldn't and the event didn't clear!");

        successCalled = false;
        prom.OnSuccessEvent += testEvent;
        prom.Load_Test();

        Assert.IsTrue(successCalled == false, "Success callback was called when it shouldn't!");
    }

    [UnityTest]
    public IEnumerator PromiseUnloadWhenCompleted()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        AssetPromise_Mock prom = new AssetPromise_Mock();

        bool failCalled = false;

        System.Action<Asset_Mock> testEvent = (x) =>
        {
            failCalled = true;
        };

        prom.idGenerator = "1";
        prom.OnFailEvent += testEvent;

        prom.SetLibrary_Test(library);
        prom.Load_Test();

        yield return prom;

        Assert.IsTrue(prom.state == AssetPromiseState.FINISHED);

        //NOTE(Brian): Test that nothing should happen if called multiple times
        prom.Unload_Test();

        Assert.IsTrue(!failCalled, "Fail callback was called when it shouldn't!");
        Assert.IsTrue(prom.state == AssetPromiseState.IDLE_AND_EMPTY, "Status should be NOT_GIVEN");

        failCalled = false;
        prom.OnFailEvent += testEvent;

        prom.Unload_Test();
        Assert.IsTrue(!failCalled, "Fail callback was called when it shouldn't!");
        Assert.IsTrue(prom.state == AssetPromiseState.IDLE_AND_EMPTY, "Status should be NOT_GIVEN");

        Assert.IsTrue(prom.GetAsset_Test() == null, "Asset should be null when promise is cancelled!");
    }

    [UnityTest]
    public IEnumerator PromiseUnloadWhenWaiting()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        AssetPromise_Mock prom = new AssetPromise_Mock();
        bool failCalled = false;

        System.Action<Asset_Mock> testEvent = (x) =>
        {
            failCalled = true;
        };

        prom.idGenerator = "1";
        prom.OnFailEvent += testEvent;

        prom.SetLibrary_Test(library);
        prom.Load_Test();

        Assert.IsTrue(prom.state == AssetPromiseState.LOADING);
        Assert.IsTrue(prom.GetAsset_Test() != null, "Asset shouldn't be null when loading!");

        //NOTE(Brian): Test that nothing should happen if called multiple times
        prom.Unload_Test();

        Assert.IsTrue(failCalled, "Fail callback was not called when it should!");

        failCalled = false;
        prom.Unload_Test();
        Assert.IsTrue(!failCalled, "Fail callback was called when it shouldn't and fail event didn't clear!");

        failCalled = false;
        prom.OnFailEvent += testEvent;

        prom.Unload_Test();
        Assert.IsTrue(!failCalled, "Fail callback was called twice when it shouldn't!");

        //NOTE(Brian): Test that nothing should happen if called multiple times
        prom.Unload_Test();
        prom.Unload_Test();
        prom.Unload_Test();

        Assert.IsTrue(prom.GetAsset_Test() == null, "Asset should be null when promise is cancelled!");
        yield break;
    }

    [UnityTest]
    public IEnumerator LoadAndUnload()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        AssetPromise_Mock prom = new AssetPromise_Mock();
        Asset_Mock loadedAsset = null;

        prom.idGenerator = "1";
        prom.OnSuccessEvent +=
            (x) =>
            {
                loadedAsset = x;
            }
        ;



        keeper.Keep(prom);

        Assert.AreEqual(AssetPromiseState.LOADING, prom.state);

        yield return prom;

        Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);

        Assert.IsTrue(loadedAsset != null);
        Assert.IsTrue(library.Contains(loadedAsset));

        Assert.AreEqual(1, library.masterAssets.Count);

        keeper.Forget(prom);

        yield return prom;

        Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

        Assert.IsTrue(!library.Contains(loadedAsset.id));
        Assert.AreEqual(0, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator UnloadWhileLoading()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        AssetPromise_Mock prom = new AssetPromise_Mock();
        Asset_Mock asset = null;
        prom.idGenerator = "1";
        prom.OnSuccessEvent += (x) => { asset = x; };

        keeper.Keep(prom);

        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(AssetPromiseState.LOADING, prom.state);

        keeper.Forget(prom);

        Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);

        yield return new WaitForSeconds(1.5f);

        Assert.IsTrue(asset == null);
        Assert.IsTrue(!library.Contains(asset));
        Assert.AreEqual(0, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator ForgetMasterPromise()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        string id = "1";
        AssetPromise_Mock prom = new AssetPromise_Mock();
        prom.idGenerator = id;

        Asset_Mock asset = null;
        bool masterSuccessCalled = false;
        bool masterFailCalled = false;

        prom.OnSuccessEvent += (x) => { asset = x; masterSuccessCalled = true; };
        prom.OnFailEvent += (x) => { masterFailCalled = true; };

        AssetPromise_Mock_Alt_Loading_Approach prom2 = new AssetPromise_Mock_Alt_Loading_Approach();
        prom2.idGenerator = id;
        Asset_Mock asset2 = null;
        prom2.OnSuccessEvent += (x) => { asset2 = x; };

        AssetPromise_Mock prom3 = new AssetPromise_Mock();
        prom3.idGenerator = id;
        Asset_Mock asset3 = null;
        prom3.OnSuccessEvent += (x) => { asset3 = x; };

        keeper.Keep(prom);
        keeper.Keep(prom2);
        keeper.Keep(prom3);

        Assert.AreEqual(3, keeper.waitingPromisesCount);

        yield return new WaitForSeconds(prom.loadTime * 0.5f);

        keeper.Forget(prom);

        yield return prom;

        Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
        Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
        Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

        Assert.IsTrue(!masterSuccessCalled, "Success event called when it shouldn't!");
        Assert.IsTrue(!masterFailCalled, "Fail event called when it shouldn't!");

        Assert.IsTrue(asset == null);
        Assert.IsTrue(prom.GetAsset_Test() != null);

        Assert.IsTrue(asset2 != null);
        Assert.IsTrue(asset3 != null);

        Assert.IsTrue(asset2.id == asset3.id);
        Assert.IsTrue(asset2 != asset3);

        Assert.AreEqual(1, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator LoadMany()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        string id = "1";
        AssetPromise_Mock prom = new AssetPromise_Mock();
        prom.idGenerator = id;
        Asset_Mock asset = null;
        prom.OnSuccessEvent += (x) => { asset = x; };

        AssetPromise_Mock_Alt_Loading_Approach prom2 = new AssetPromise_Mock_Alt_Loading_Approach();
        prom2.idGenerator = id;
        Asset_Mock asset2 = null;
        prom2.OnSuccessEvent += (x) => { asset2 = x; };

        AssetPromise_Mock prom3 = new AssetPromise_Mock();
        prom3.idGenerator = id;
        Asset_Mock asset3 = null;
        prom3.OnSuccessEvent += (x) => { asset3 = x; };

        keeper.Keep(prom);
        keeper.Keep(prom2);
        keeper.Keep(prom3);

        Assert.AreEqual(3, keeper.waitingPromisesCount);

        yield return new WaitForSeconds(prom.loadTime);

        Assert.AreEqual(AssetPromiseState.FINISHED, prom.state);
        Assert.AreEqual(AssetPromiseState.FINISHED, prom2.state);
        Assert.AreEqual(AssetPromiseState.FINISHED, prom3.state);

        Assert.IsTrue(asset != null);
        Assert.IsTrue(asset2 != null);
        Assert.IsTrue(asset3 != null);

        Assert.IsTrue(asset2.id == asset.id);
        Assert.IsTrue(asset3.id == asset.id);
        Assert.IsTrue(asset2.id == asset3.id);

        Assert.IsTrue(asset != asset2);
        Assert.IsTrue(asset != asset3);
        Assert.IsTrue(asset2 != asset3);

        Assert.IsTrue(library.Contains(asset));
        Assert.AreEqual(1, library.masterAssets.Count);
    }

    [UnityTest]
    public IEnumerator LoadAndUnloadSingleFrame()
    {
        var library = new AssetLibrary_Mock();
        var keeper = new AssetPromiseKeeper_Mock(library);

        AssetPromise_Mock prom = new AssetPromise_Mock();
        Asset_Mock loadedAsset = null;
        object id = null;

        prom.idGenerator = "1";
        prom.OnSuccessEvent +=
            (x) =>
            {
                loadedAsset = x;
                id = loadedAsset.id;
            };



        keeper.Keep(prom);
        keeper.Forget(prom);
        keeper.Keep(prom);
        keeper.Forget(prom);

        keeper.Keep(prom);
        Assert.AreEqual(1, keeper.waitingPromisesCount);
        keeper.Keep(prom);
        Assert.AreEqual(1, keeper.waitingPromisesCount);
        keeper.Forget(prom);
        Assert.AreEqual(0, keeper.waitingPromisesCount);
        keeper.Forget(prom);
        Assert.AreEqual(0, keeper.waitingPromisesCount);
        keeper.Forget(prom);
        Assert.AreEqual(0, keeper.waitingPromisesCount);

        Assert.AreEqual(AssetPromiseState.IDLE_AND_EMPTY, prom.state);
        Assert.AreEqual(0, keeper.waitingPromisesCount);
        Assert.IsTrue(loadedAsset == null);
        Assert.IsTrue(!library.Contains(id));
        Assert.AreEqual(0, library.masterAssets.Count);

        yield break;
    }
}
