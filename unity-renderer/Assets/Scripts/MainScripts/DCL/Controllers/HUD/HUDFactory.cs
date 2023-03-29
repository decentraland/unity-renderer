using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Chat;
using DCL.Chat.HUD;
using DCL.HelpAndSupportHUD;
using DCL.Huds.QuestsPanel;
using DCL.Huds.QuestsTracker;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.SettingsCommon;
using DCL.SettingsPanelHUD;
using DCL.Social.Chat.Mentions;
using DCL.Social.Friends;
using DCLServices.WearablesCatalogService;
using SignupHUD;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Threading;
using static MainScripts.DCL.Controllers.HUD.HUDAssetPath;
using Environment = DCL.Environment;

public class HUDFactory : IHUDFactory
{
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
    private readonly List<IDisposable> disposableViews;

    private Service<IAddressableResourceProvider> assetsProviderService;
    private IAddressableResourceProvider assetsProviderRef;

    private IAddressableResourceProvider assetsProvider => assetsProviderRef ??= assetsProviderService.Ref;

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
                return new MinimapHUDController(MinimapMetadataController.i, new WebInterfaceHomeLocationController(), Environment.i);
            case HUDElementID.PROFILE_HUD:
                return new ProfileHUDController(new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i);
            case HUDElementID.NOTIFICATION:
                return new NotificationHUDController();
            case HUDElementID.AVATAR_EDITOR:
                return new AvatarEditorHUDController(
                    DataStore.i.featureFlags,
                    Environment.i.platform.serviceProviders.analytics,
                    Environment.i.serviceLocator.Get<IWearablesCatalogService>());
            case HUDElementID.SETTINGS_PANEL:
                return new SettingsPanelHUDController();
            case HUDElementID.TERMS_OF_SERVICE:
                return new TermsOfServiceHUDController();
            case HUDElementID.FRIENDS:
                return new FriendsHUDController(DataStore.i,
                    FriendsController.i,
                    new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher);
            case HUDElementID.WORLD_CHAT_WINDOW:
                return new WorldChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i,
                    Environment.i.serviceLocator.Get<IChatController>(),
                    DataStore.i,
                    SceneReferences.i.mouseCatcher,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>(),
                    new WebInterfaceBrowserBridge(),
                    CommonScriptableObjects.rendererState,
                    DataStore.i.mentions);
            case HUDElementID.PRIVATE_CHAT_WINDOW:
                return new PrivateChatWindowController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    SceneReferences.i.mouseCatcher,
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i));
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
                        new UserProfileWebInterfaceBridge()));
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
                    new MemoryChatMentionSuggestionProvider(UserProfileController.i, DataStore.i));
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
                return new TaskbarHUDController(Environment.i.serviceLocator.Get<IChatController>(), FriendsController.i);
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                return new ExternalUrlPromptHUDController();
            case HUDElementID.NFT_INFO_DIALOG:
                return new NFTPromptHUDController();
            case HUDElementID.TELEPORT_DIALOG:
                return new TeleportPromptHUDController();
            case HUDElementID.CONTROLS_HUD:
                return new ControlsHUDController();
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                return new HelpAndSupportHUDController(await CreateHUDView<IHelpAndSupportHUDView>(HELP_AND_SUPPORT_HUD, cancellationToken));
            case HUDElementID.USERS_AROUND_LIST_HUD:
                return new VoiceChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i,
                    Settings.i,
                    SceneReferences.i.mouseCatcher);
            case HUDElementID.GRAPHIC_CARD_WARNING:
                return new GraphicCardWarningHUDController();
            case HUDElementID.QUESTS_PANEL:
                return new QuestsPanelHUDController();
            case HUDElementID.QUESTS_TRACKER:
                return new QuestsTrackerHUDController(await CreateHUDView<IQuestsTrackerHUDView>(QUESTS_TRACKER_HUD, cancellationToken));
            case HUDElementID.SIGNUP:
                return new SignupHUDController(Environment.i.platform.serviceProviders.analytics, await CreateSignupHUDView(cancellationToken), dataStoreLoadingScreen.Ref);
        }

        return null;
    }

    public async UniTask<ISignupHUDView> CreateSignupHUDView(CancellationToken cancellationToken = default) =>
        await CreateHUDView<ISignupHUDView>(SIGNUP_HUD, cancellationToken);

    protected async UniTask<T> CreateHUDView<T>(string assetAddress, CancellationToken cancellationToken = default) where T:IDisposable
    {
        var view = await assetsProvider.Instantiate<T>(assetAddress, $"_{assetAddress}", cancellationToken);
        disposableViews.Add(view);

        return view;
    }
}
