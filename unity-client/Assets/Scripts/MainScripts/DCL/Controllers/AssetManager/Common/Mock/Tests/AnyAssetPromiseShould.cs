using DCL;
using System.Collections;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Mock_Tests
{
    public class AnyAssetPromiseShould
    {
        [UnityTest]
        public IEnumerator CallSuccessCallbackOnlyWhenNeeded()
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
        public IEnumerator CallFailedCallbackOnlyWhenNeeded()
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
        public IEnumerator DontCallFailedCallbackAfterIsAlreadyUnloaded()
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

            Assert.IsTrue(!failCalled, "Fail callback was called when it shouldnt'!");

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
    }
}
