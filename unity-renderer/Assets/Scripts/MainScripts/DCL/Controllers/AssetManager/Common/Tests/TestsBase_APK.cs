using DCL;
using System.Collections;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Tests
{
    public abstract class TestsBase_APK<APKType, AssetPromiseType, AssetType, AssetLibraryType> : TestsBase
        where AssetPromiseType : AssetPromise<AssetType>
        where AssetType : Asset, new()
        where AssetLibraryType : AssetLibrary<AssetType>, new()
        where APKType : AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>, new()
    {
        protected APKType keeper;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            Environment.i.Initialize(new DummyMessageHandler(), isTesting: true);
            keeper = new APKType();
            yield break;
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            keeper.Cleanup();
            yield return TearDown_Memory();
        }
    }
}