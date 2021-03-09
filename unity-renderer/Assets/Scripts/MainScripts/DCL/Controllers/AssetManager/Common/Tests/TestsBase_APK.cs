using DCL;
using System.Collections;
using NSubstitute;
using UnityEngine.TestTools;

namespace AssetPromiseKeeper_Tests
{
    public abstract class TestsBase_APK<APKType, AssetPromiseType, AssetType, AssetLibraryType>
        where AssetPromiseType : AssetPromise<AssetType>
        where AssetType : Asset, new()
        where AssetLibraryType : AssetLibrary<AssetType>, new()
        where APKType : AssetPromiseKeeper<AssetType, AssetLibraryType, AssetPromiseType>, new()
    {
        protected APKType keeper;

        [UnitySetUp]
        protected virtual IEnumerator SetUp()
        {
            keeper = new APKType();
            yield break;
        }

        [UnityTearDown]
        protected virtual IEnumerator TearDown()
        {
            keeper.Cleanup();
            yield break;
        }
    }
}