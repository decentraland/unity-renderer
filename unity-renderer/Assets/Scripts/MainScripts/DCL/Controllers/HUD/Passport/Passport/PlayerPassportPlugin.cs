using AvatarSystem;
using DCL;
using DCL.ProfanityFiltering;
using DCL.Social.Friends;
using DCl.Social.Passports;
using DCL.Social.Passports;
using DCLServices.CopyPaste.Analytics;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using SocialFeaturesAnalytics;
using UnityEngine;

public class PlayerPassportPlugin : IPlugin
{
    private readonly PlayerPassportHUDController passportController;

    public PlayerPassportPlugin()
    {
        PlayerPassportReferenceContainer referenceContainer = Object.Instantiate(Resources.Load<GameObject>("PlayerPassport"))
                                                                    .GetComponent<PlayerPassportReferenceContainer>();
        referenceContainer.PlayerPreviewView.Initialize(new PreviewCameraRotationController());

        var wearablesCatalogService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();

        passportController = new PlayerPassportHUDController(
                        referenceContainer.PassportView,
                        new PassportPlayerInfoComponentController(
                            referenceContainer.PlayerInfoView,
                            DataStore.i,
                            Environment.i.serviceLocator.Get<IProfanityFilter>(),
                            Environment.i.serviceLocator.Get<IFriendsController>(),
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
                                NotificationsController.i),
                            referenceContainer.PassportNavigationView,
                            Clipboard.Create(),
                            Environment.i.serviceLocator.Get<ICopyPasteAnalyticsService>()),
                        new UserProfileWebInterfaceBridge(),
                        new WebInterfacePassportApiBridge(),
                        new SocialAnalytics(
                            Environment.i.platform.serviceProviders.analytics,
                            new UserProfileWebInterfaceBridge()),
                        DataStore.i,
                        SceneReferences.i.mouseCatcher,
                        CommonScriptableObjects.playerInfoCardVisibleState);
    }

    public void Dispose()
    {
        passportController?.Dispose();
    }
}
