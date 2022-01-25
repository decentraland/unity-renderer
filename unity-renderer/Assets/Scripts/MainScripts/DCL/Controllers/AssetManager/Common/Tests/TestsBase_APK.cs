using DCL;
using System.Collections;
using NSubstitute;
using UnityEngine;
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
            DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
            var serviceLocator = DCL.ServiceLocatorFactory.CreateDefault();
            serviceLocator.Register<IMemoryManager>(() => Substitute.For<IMemoryManager>());
            Environment.Setup(serviceLocator);
            keeper = new APKType();
            yield break;
        }

        [UnityTearDown]
        protected virtual IEnumerator TearDown()
        {
            // If the asset bundles cache is not cleared, the tests are going to stop working on successive runs
            Caching.ClearCache();

            Environment.Dispose();
            keeper.Cleanup();
            yield break;
        }
    }
}