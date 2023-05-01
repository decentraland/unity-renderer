using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.Social.Friends;
using DCl.Social.Passports;
using DCL.Social.Passports;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using SocialFeaturesAnalytics;
using System.Threading;
using UnityEngine;

public class PlayerPassportPlugin : IPlugin
{
    private PlayerPassportHUDController passportController;
    private CancellationTokenSource cts;

    public PlayerPassportPlugin()
    {
        cts = new CancellationTokenSource();
        Initialize(cts.Token).Forget();
    }

    private async UniTaskVoid Initialize(CancellationToken cancellationToken)
    {
        PlayerPassportReferenceContainer referenceContainer = await Environment.i.serviceLocator
                                                                               .Get<IAddressableResourceProvider>()
                                                                               .Instantiate<PlayerPassportReferenceContainer>("PlayerPassport", cancellationToken: cancellationToken);

        var wearablesCatalogService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();

        passportController = new PlayerPassportHUDController(
                        referenceContainer.PassportView,
                        new PassportPlayerInfoComponentController(
                            referenceContainer.PlayerInfoView,
                            DataStore.i,
                            Environment.i.serviceLocator.Get<IProfanityFilter>(),
                            FriendsController.i,
                            new UserProfileWebInterfaceBridge(),
                            new SocialAnalytics(
                                Environment.i.platform.serviceProviders.analytics,
                                new UserProfileWebInterfaceBridge()),
                            Environment.i.platform.clipboard,
                            new WebInterfacePassportApiBridge()),
                        new PassportPlayerPreviewComponentController(
                            referenceContainer.PlayerPreviewView,
                            new SocialAnalytics(
                                Environment.i.platform.serviceProviders.analytics,
                                new UserProfileWebInterfaceBridge())),
                        new PassportNavigationComponentController(
                            referenceContainer.PassportNavigationView,
                            Environment.i.serviceLocator.Get<IProfanityFilter>(),
                            new WearableItemResolver(wearablesCatalogService),
                            wearablesCatalogService,
                            Environment.i.serviceLocator.Get<IEmotesCatalogService>(),
                            Environment.i.serviceLocator.Get<INamesService>(),
                            Environment.i.serviceLocator.Get<ILandsService>(),
                            new UserProfileWebInterfaceBridge(),
                            DataStore.i,
                            new ViewAllComponentController(
                                referenceContainer.ViewAllView,
                                DataStore.i.HUDs,
                                Environment.i.serviceLocator.Get<IWearablesCatalogService>(),
                                Environment.i.serviceLocator.Get<ILandsService>(),
                                Environment.i.serviceLocator.Get<INamesService>(),
                                NotificationsController.i)),
                        new UserProfileWebInterfaceBridge(),
                        new WebInterfacePassportApiBridge(),
                        new SocialAnalytics(
                            Environment.i.platform.serviceProviders.analytics,
                            new UserProfileWebInterfaceBridge()),
                        DataStore.i,
                        SceneReferences.i.mouseCatcher,
                        CommonScriptableObjects.playerInfoCardVisibleState);

        cts.Cancel();
        cts.Dispose();
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();

        passportController?.Dispose();
    }
}
