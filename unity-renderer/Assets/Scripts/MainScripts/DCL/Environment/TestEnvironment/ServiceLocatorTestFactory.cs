using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Controllers;
using DCL.Helpers.NFT.Markets;
using DCL.Providers;
using DCL.ProfanityFiltering;
using DCL.Rendering;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using MainScripts.DCL.Helpers.SentryUtils;
using NSubstitute;
using Sentry;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL
{
    public static class ServiceLocatorTestFactory
    {
        public static ServiceLocator CreateMocked()
        {
            var result = new ServiceLocator();

            // Platform
            result.Register<IMemoryManager>(() => Substitute.For<IMemoryManager>());
            result.Register<ICullingController>(() => Substitute.For<ICullingController>());
            result.Register<IParcelScenesCleaner>(() => Substitute.For<IParcelScenesCleaner>());
            result.Register<IClipboard>(() => Substitute.For<IClipboard>());
            result.Register<IPhysicsSyncController>(() => Substitute.For<IPhysicsSyncController>());
            result.Register<IWebRequestController>(() => Substitute.For<IWebRequestController>());
            result.Register<IWebRequestMonitor>(() =>
            {
                var subs = Substitute.For<IWebRequestMonitor>();
                subs.TrackWebRequest(default, default).Returns(new DisposableTransaction(Substitute.For<ISpan>()));
                return subs;
            });

            result.Register<IServiceProviders>(
                () =>
                {
                    var mockedProviders = Substitute.For<IServiceProviders>();
                    mockedProviders.theGraph.Returns(Substitute.For<ITheGraph>());
                    mockedProviders.analytics.Returns(Substitute.For<IAnalytics>());
                    mockedProviders.catalyst.Returns(Substitute.For<ICatalyst>());
                    mockedProviders.openSea.Returns(Substitute.For<INFTMarket>());
                    return mockedProviders;
                });

            result.Register<IUpdateEventHandler>(() => Substitute.For<IUpdateEventHandler>());

            result.Register<ICharacterPreviewFactory>(() =>
            {
                var mockedFactory = Substitute.For<ICharacterPreviewFactory>();

                mockedFactory.Create(default, default, default, default)
                             .ReturnsForAnyArgs(Substitute.For<ICharacterPreviewController>());

                return mockedFactory;
            });

            result.Register<IAvatarFactory>(() =>
                {
                    var mockedFactory = Substitute.For<IAvatarFactory>();

                    mockedFactory.CreateAvatar(default, default, default, default)
                                 .ReturnsForAnyArgs(Substitute.For<IAvatar>());

                    mockedFactory.CreateAvatarWithHologram(default, default, default, default,
                                      default, default)
                                 .ReturnsForAnyArgs(Substitute.For<IAvatar>());

                    return mockedFactory;
                }
            );

            result.Register<IProfanityFilter>(() => Substitute.For<IProfanityFilter>());

            var editorBundleProvider = Substitute.For<IAssetBundleProvider>();

            editorBundleProvider.GetAssetBundleAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                                .Returns(UniTask.FromResult<AssetBundle>(null));

            DataStore.i.featureFlags.flags.Set(new FeatureFlag { flags = { [AssetResolverLogger.VERBOSE_LOG_FLAG] = true } });

            result.Register<IAssetBundleResolver>(() => new AssetBundleResolver(new Dictionary<AssetSource, IAssetBundleProvider>
            {
                { AssetSource.WEB, new AssetBundleWebLoader(DataStore.i.featureFlags, DataStore.i.performance) }
            }, editorBundleProvider, DataStore.i.featureFlags));

            result.Register<ITextureAssetResolver>(() => new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                { AssetSource.WEB, new AssetTextureWebLoader() }
            }, DataStore.i.featureFlags));

            // World runtime
            result.Register<IIdleChecker>(() => Substitute.For<IIdleChecker>());
            result.Register<IAvatarsLODController>(() => Substitute.For<IAvatarsLODController>());
            result.Register<IFeatureFlagController>(() => Substitute.For<IFeatureFlagController>());
            result.Register<IMessagingControllersManager>(() => Substitute.For<IMessagingControllersManager>());
            result.Register<IWorldState>(() => Substitute.For<IWorldState>());
            result.Register<ISceneController>(() => Substitute.For<ISceneController>());
            result.Register<ITeleportController>(() => Substitute.For<ITeleportController>());
            result.Register<ISceneBoundsChecker>(() => Substitute.For<ISceneBoundsChecker>());
            result.Register<IWorldBlockersController>(() => Substitute.For<IWorldBlockersController>());
            result.Register<IRuntimeComponentFactory>(() => Substitute.For<IRuntimeComponentFactory>());

            // HUD
            result.Register<IHUDFactory>(() => Substitute.For<IHUDFactory>());
            result.Register<IHUDController>(() => Substitute.For<IHUDController>());

            return result;
        }
    }
}
