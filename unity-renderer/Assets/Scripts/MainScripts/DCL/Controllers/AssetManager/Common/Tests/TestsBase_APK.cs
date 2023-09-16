using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.CustomNftCollection;
using DCLServices.WearablesCatalogService;
using System.Collections;
using MainScripts.DCL.Analytics.PerformanceAnalytics;
using MainScripts.DCL.Controllers.AssetManager;
using NSubstitute;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

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
            var serviceLocator = ServiceLocatorFactory.CreateDefault();

            serviceLocator.Register<IMemoryManager>(() => Substitute.For<IMemoryManager>());
            serviceLocator.Register<IEmotesCatalogService>(() =>
            {
                IEmotesCatalogService emotesCatalogService = Substitute.For<IEmotesCatalogService>();
                emotesCatalogService.RequestEmoteCollectionAsync(default, default, default)
                                    .ReturnsForAnyArgs(UniTask.FromResult((IReadOnlyList<WearableItem>)Array.Empty<WearableItem>()));

                emotesCatalogService.RequestEmoteCollectionInBuilderAsync(default, default)
                                    .ReturnsForAnyArgs(UniTask.FromResult((IReadOnlyList<WearableItem>)Array.Empty<WearableItem>()));

                emotesCatalogService.RequestEmoteFromBuilderAsync(default, default)
                                    .ReturnsForAnyArgs(null);
                return emotesCatalogService;
            });

            serviceLocator.Register<ICustomNftCollectionService>(() =>
            {
                ICustomNftCollectionService customNftCollectionService = Substitute.For<ICustomNftCollectionService>();
                customNftCollectionService.GetConfiguredCustomNftCollectionAsync(default)
                                          .ReturnsForAnyArgs(UniTask.FromResult<IReadOnlyList<string>>(Array.Empty<string>()));
                customNftCollectionService.GetConfiguredCustomNftItemsAsync(default)
                                          .ReturnsForAnyArgs(UniTask.FromResult<IReadOnlyList<string>>(Array.Empty<string>()));
                return customNftCollectionService;
            });

            serviceLocator.Register<IWearablesCatalogService>(() =>
            {
                IWearablesCatalogService wearablesCatalogService = Substitute.For<IWearablesCatalogService>();

                wearablesCatalogService.RequestWearableCollectionInBuilder(default, default, default)
                                       .ReturnsForAnyArgs(UniTask.FromResult((IReadOnlyList<WearableItem>) Array.Empty<WearableItem>()));

                wearablesCatalogService.RequestWearableFromBuilderAsync(default, default)
                                       .ReturnsForAnyArgs(null);

                wearablesCatalogService.RequestWearableCollection(default, default, default)
                                       .ReturnsForAnyArgs(UniTask.FromResult((IReadOnlyList<WearableItem>)Array.Empty<WearableItem>()));
                return wearablesCatalogService;
            });

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
            PerformanceAnalytics.ABTracker.Reset();
            keeper.Cleanup();
            yield break;
        }
    }
}
