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
using DCL.Social.Chat;
using DCL.Social.Friends;
using SignupHUD;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using UnityEngine;
using static HUDAssetPath;
using Environment = DCL.Environment;

public class HUDFactory : IHUDFactory
{
    private readonly IAddressableResourceProvider assetsProvider;
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
    private readonly List<IDisposable> disposableViews;

    public HUDFactory(IAddressableResourceProvider assetsProvider)
    {
        this.assetsProvider = assetsProvider;
        disposableViews = new List<IDisposable>();
    }

    public void Initialize() { }

    public void Dispose()
    {
        foreach (IDisposable view in disposableViews)
            view.Dispose();
    }

    public virtual async UniTask<IHUD> CreateHUD(HUDElementID hudElementId)
    {
        IHUD hudElement = null;

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
                        new UserProfileWebInterfaceBridge()));
            case HUDElementID.NOTIFICATION:
                return new NotificationHUDController();
            case HUDElementID.AVATAR_EDITOR:
                return new AvatarEditorHUDController(DataStore.i.featureFlags,
                    Environment.i.platform.serviceProviders.analytics);
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
                    ChatController.i,
                    SceneReferences.i.mouseCatcher);
            case HUDElementID.WORLD_CHAT_WINDOW:
                return new WorldChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i,
                    ChatController.i,
                    DataStore.i,
                    SceneReferences.i.mouseCatcher,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>(),
                    new WebInterfaceBrowserBridge(),
                    CommonScriptableObjects.rendererState);
            case HUDElementID.PRIVATE_CHAT_WINDOW:
                return new PrivateChatWindowController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    ChatController.i,
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    SceneReferences.i.mouseCatcher,
                    Resources.Load<InputAction_Trigger>("ToggleWorldChat"));
            case HUDElementID.PUBLIC_CHAT:
                return new PublicChatWindowController(
                    ChatController.i,
                    new UserProfileWebInterfaceBridge(),
                    DataStore.i,
                    Environment.i.serviceLocator.Get<IProfanityFilter>(),
                    SceneReferences.i.mouseCatcher,
                    Resources.Load<InputAction_Trigger>("ToggleWorldChat"));
            case HUDElementID.CHANNELS_CHAT:
                return new ChatChannelHUDController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    ChatController.i,
                    SceneReferences.i.mouseCatcher,
                    Resources.Load<InputAction_Trigger>("ToggleWorldChat"),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IProfanityFilter>());
            case HUDElementID.CHANNELS_SEARCH:
                return new SearchChannelsWindowController(
                    ChatController.i,
                    SceneReferences.i.mouseCatcher,
                    DataStore.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>());
            case HUDElementID.CHANNELS_CREATE:
                return new CreateChannelWindowController(ChatController.i, DataStore.i);
            case HUDElementID.CHANNELS_LEAVE_CONFIRMATION:
                return new LeaveChannelConfirmationWindowController(ChatController.i);
            case HUDElementID.TASKBAR:
                return new TaskbarHUDController(ChatController.i, FriendsController.i);
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                return new ExternalUrlPromptHUDController();
            case HUDElementID.NFT_INFO_DIALOG:
                return new NFTPromptHUDController();
            case HUDElementID.TELEPORT_DIALOG:
                return new TeleportPromptHUDController();
            case HUDElementID.CONTROLS_HUD:
                return new ControlsHUDController();
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                return new HelpAndSupportHUDController();
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
            case HUDElementID.LOADING:
                return new LoadingHUDController(await CreateHUDView<LoadingHUDView>(LOADING_HUD));
            case HUDElementID.QUESTS_TRACKER:
                return new QuestsTrackerHUDController(await CreateHUDView<IQuestsTrackerHUDView>(QUESTS_TRACKER_HUD));
            case HUDElementID.AIRDROPPING:
                return new AirdroppingHUDController(await CreateAirdroppingHUDView());
            case HUDElementID.SIGNUP:
                return new SignupHUDController(Environment.i.platform.serviceProviders.analytics, await CreateSignupHUDView(), dataStoreLoadingScreen.Ref);
        }

        return hudElement;
    }

    public async UniTask<AirdroppingHUDView> CreateAirdroppingHUDView() =>
        await CreateHUDView<AirdroppingHUDView>(AIRDROPPING_HUD);

    public async UniTask<ISignupHUDView> CreateSignupHUDView() =>
        await CreateHUDView<ISignupHUDView>(SIGNUP_HUD);

    private async UniTask<T> CreateHUDView<T>(string assetAddress) where T:IDisposable
    {
        var view = await assetsProvider.Instantiate<T>(assetAddress, $"_{assetAddress}");
        disposableViews.Add(view);

        return view;
    }
}
