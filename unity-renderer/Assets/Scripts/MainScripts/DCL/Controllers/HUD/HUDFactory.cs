using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Chat;
using DCL.HelpAndSupportHUD;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.SettingsCommon;
using DCL.SettingsPanelHUD;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;
using static MainScripts.DCL.Controllers.HUD.HUDAssetPath;
using Environment = DCL.Environment;
using Analytics;
using DCL.MyAccount;
using DCL.Social.Chat;
using DCLServices.CopyPaste.Analytics;
using DCLServices.PlacesAPIService;
using UnityEngine;
using Object = UnityEngine.Object;

public class HUDFactory : IHUDFactory
{
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
    private readonly List<IDisposable> disposableViews;

    private Service<IAddressableResourceProvider> assetsProviderService;
    private IAddressableResourceProvider assetsProviderRef;

    private IAddressableResourceProvider assetsProvider => assetsProviderRef ??= assetsProviderService.Ref;
    private ICopyPasteAnalyticsService copyPasteAnalyticsService => Environment.i.serviceLocator.Get<ICopyPasteAnalyticsService>();

    protected HUDFactory()
    {
        disposableViews = new List<IDisposable>();
    }

    public HUDFactory(IAddressableResourceProvider assetsProvider)
    {
        disposableViews = new List<IDisposable>();
        assetsProviderRef = assetsProvider;
    }

    public void Initialize() { }

    public void Dispose()
    {
        foreach (IDisposable view in disposableViews)
            view.Dispose();
    }

    public virtual async UniTask<IHUD> CreateHUD(HUDElementID hudElementId, CancellationToken cancellationToken = default)
    {
        switch (hudElementId)
        {
            case HUDElementID.NONE:
                break;
            case HUDElementID.MINIMAP:
                return new MinimapHUDController(MinimapMetadataController.i,
                    new WebInterfaceHomeLocationController(), Environment.i,
                    Environment.i.serviceLocator.Get<IPlacesAPIService>(),
                    new PlacesAnalytics(), Clipboard.Create(),
                    copyPasteAnalyticsService);
            case HUDElementID.PROFILE_HUD:
                ProfileHUDViewV2 view = Object.Instantiate(Resources.Load<ProfileHUDViewV2>("ProfileHUD_V2"));

                var userProfileBridge = new UserProfileWebInterfaceBridge();
                var webInterfaceBrowserBridge = new WebInterfaceBrowserBridge();

                return new ProfileHUDController(
                    view,
                    userProfileBridge,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        userProfileBridge),
                    DataStore.i,
                    new MyAccountCardController(
                        view.MyAccountCardView,
                        DataStore.i,
                        userProfileBridge,
                        Settings.i,
                        webInterfaceBrowserBridge),
                    webInterfaceBrowserBridge);
            case HUDElementID.NOTIFICATION:
                return new NotificationHUDController( await CreateHUDView<NotificationHUDView>(VIEW_PATH, cancellationToken));
            case HUDElementID.SETTINGS_PANEL:
                return new SettingsPanelHUDController();
            case HUDElementID.TERMS_OF_SERVICE:
                return new TermsOfServiceHUDController();
            case HUDElementID.FRIENDS:
                return new FriendsHUDController(DataStore.i,
                    Environment.i.serviceLocator.Get<IFriendsController>(),
                    new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher);
            case HUDElementID.WORLD_CHAT_WINDOW:
                return new WorldChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IFriendsController>(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    DataStore.i,
                    SceneReferences.i.mouseCatcher,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>(),
                    new WebInterfaceBrowserBridge(),
                    CommonScriptableObjects.rendererState,
                    DataStore.i.mentions,
                    Clipboard.Create(),
                    copyPasteAnalyticsService);
            case HUDElementID.PRIVATE_CHAT_WINDOW:
                return new PrivateChatWindowController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    Environment.i.serviceLocator.Get<IFriendsController>(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    SceneReferences.i.mouseCatcher,
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i),
                    Clipboard.Create(),
                    copyPasteAnalyticsService);
            case HUDElementID.PUBLIC_CHAT:
                return new PublicChatWindowController(
                    Environment.i.serviceLocator.Get<IChatController>(),
                    new UserProfileWebInterfaceBridge(),
                    DataStore.i,
                    Environment.i.serviceLocator.Get<IProfanityFilter>(),
                    SceneReferences.i.mouseCatcher,
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Clipboard.Create(),
                    copyPasteAnalyticsService);
            case HUDElementID.CHANNELS_CHAT:
                return new ChatChannelHUDController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IProfanityFilter>(),
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i),
                    Clipboard.Create(),
                    copyPasteAnalyticsService);
            case HUDElementID.CHANNELS_SEARCH:
                return new SearchChannelsWindowController(
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher,
                    DataStore.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>());
            case HUDElementID.CHANNELS_CREATE:
                return new CreateChannelWindowController(Environment.i.serviceLocator.Get<IChatController>(), DataStore.i);
            case HUDElementID.CHANNELS_LEAVE_CONFIRMATION:
                return new LeaveChannelConfirmationWindowController(Environment.i.serviceLocator.Get<IChatController>());
            case HUDElementID.TASKBAR:
                return new TaskbarHUDController(Environment.i.serviceLocator.Get<IChatController>(), Environment.i.serviceLocator.Get<IFriendsController>(), new SupportAnalytics(Environment.i.platform.serviceProviders.analytics));
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                return new ExternalUrlPromptHUDController(DataStore.i.rpc.context.restrictedActions);
            case HUDElementID.NFT_INFO_DIALOG:
                return new NFTPromptHUDController(DataStore.i.rpc.context.restrictedActions, DataStore.i.common.onOpenNFTPrompt);
            case HUDElementID.CONTROLS_HUD:
                return new ControlsHUDController();
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                return new HelpAndSupportHUDController(await CreateHUDView<IHelpAndSupportHUDView>(HELP_AND_SUPPORT_HUD, cancellationToken), new SupportAnalytics(Environment.i.platform.serviceProviders.analytics));
            case HUDElementID.USERS_AROUND_LIST_HUD:
                return new VoiceChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IFriendsController>(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i,
                    Settings.i,
                    SceneReferences.i.mouseCatcher);
            case HUDElementID.GRAPHIC_CARD_WARNING:
                return new GraphicCardWarningHUDController();
        }

        return null;
    }

    public async UniTask<T> CreateHUDView<T>(string assetAddress, CancellationToken cancellationToken = default, string name = null) where T:IDisposable
    {
        var view = await assetsProvider.Instantiate<T>(assetAddress, $"_{assetAddress}",  cancellationToken: cancellationToken);
        disposableViews.Add(view);

        return view;
    }
}
