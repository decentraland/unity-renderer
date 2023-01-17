using AvatarSystem;
using DCL;
using DCL.ProfanityFiltering;
using DCL.Social.Friends;
using DCl.Social.Passports;
using DCL.Social.Passports;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using SocialFeaturesAnalytics;
using UnityEngine;

public class PlayerPassportPlugin : IPlugin
{
    private readonly PlayerPassportHUDController passportController;

    public PlayerPassportPlugin()
    {
        PlayerPassportHUDView view = PlayerPassportHUDView.CreateView();
        passportController = new PlayerPassportHUDController(
                        view,
                        new PassportPlayerInfoComponentController(
                            Resources.Load<StringVariable>("CurrentPlayerInfoCardId"),
                            view.PlayerInfoView,
                            DataStore.i,
                            Environment.i.serviceLocator.Get<IProfanityFilter>(),
                            FriendsController.i,
                            new UserProfileWebInterfaceBridge(),
                            new SocialAnalytics(
                                Environment.i.platform.serviceProviders.analytics,
                                new UserProfileWebInterfaceBridge()),
                            Environment.i.platform.clipboard,
                            new WebInterfacePassportApiBridge()),
                        new PassportPlayerPreviewComponentController(view.PlayerPreviewView),
                        new PassportNavigationComponentController(
                            view.PassportNavigationView,
                            Environment.i.serviceLocator.Get<IProfanityFilter>(),
                            new WearableItemResolver(),
                            new WearablesCatalogControllerBridge(),
                            Environment.i.serviceLocator.Get<IEmotesCatalogService>(),
                            Environment.i.serviceLocator.Get<INamesService>(),
                            Environment.i.serviceLocator.Get<ILandsService>(),
                            new UserProfileWebInterfaceBridge(),
                            DataStore.i),
                        Resources.Load<StringVariable>("CurrentPlayerInfoCardId"),
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
