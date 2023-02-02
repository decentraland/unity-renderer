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
                hudElement = new MinimapHUDController(MinimapMetadataController.i, new WebInterfaceHomeLocationController(), DCL.Environment.i);
                break;
            case HUDElementID.PROFILE_HUD:
                hudElement = new ProfileHUDController(new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()));
                break;
            case HUDElementID.NOTIFICATION:
                hudElement = new NotificationHUDController();
                break;
            case HUDElementID.AVATAR_EDITOR:
                hudElement = new AvatarEditorHUDController(DataStore.i.featureFlags,
                    Environment.i.platform.serviceProviders.analytics);

                break;
            case HUDElementID.SETTINGS_PANEL:
                hudElement = new SettingsPanelHUDController();
                break;
            case HUDElementID.AIRDROPPING:
                hudElement = new AirdroppingHUDController(await CreateHUDView<AirdroppingHUDView>(AIRDROPPING_HUD));
                break;
            case HUDElementID.TERMS_OF_SERVICE:
                hudElement = new TermsOfServiceHUDController();
                break;
            case HUDElementID.FRIENDS:
                hudElement = new FriendsHUDController(DataStore.i,
                    FriendsController.i,
                    new UserProfileWebInterfaceBridge(),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher);

                break;
            case HUDElementID.WORLD_CHAT_WINDOW:
                hudElement = new WorldChatWindowController(
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
                    CommonScriptableObjects.rendererState);

                break;
            case HUDElementID.PRIVATE_CHAT_WINDOW:
                hudElement = new PrivateChatWindowController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    SceneReferences.i.mouseCatcher,
                    Resources.Load<InputAction_Trigger>("ToggleWorldChat"));

                break;
            case HUDElementID.PUBLIC_CHAT:
                hudElement = new PublicChatWindowController(
                    Environment.i.serviceLocator.Get<IChatController>(),
                    new UserProfileWebInterfaceBridge(),
                    DataStore.i,
                    Environment.i.serviceLocator.Get<IProfanityFilter>(),
                    SceneReferences.i.mouseCatcher,
                    Resources.Load<InputAction_Trigger>("ToggleWorldChat"));

                break;
            case HUDElementID.CHANNELS_CHAT:
                hudElement = new ChatChannelHUDController(
                    DataStore.i,
                    new UserProfileWebInterfaceBridge(),
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher,
                    Resources.Load<InputAction_Trigger>("ToggleWorldChat"),
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IProfanityFilter>());

                break;
            case HUDElementID.CHANNELS_SEARCH:
                hudElement = new SearchChannelsWindowController(
                    Environment.i.serviceLocator.Get<IChatController>(),
                    SceneReferences.i.mouseCatcher,
                    DataStore.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    Environment.i.serviceLocator.Get<IChannelsFeatureFlagService>());

                break;
            case HUDElementID.CHANNELS_CREATE:
                hudElement = new CreateChannelWindowController(Environment.i.serviceLocator.Get<IChatController>(),
                    DataStore.i);
                break;
            case HUDElementID.CHANNELS_LEAVE_CONFIRMATION:
                hudElement = new LeaveChannelConfirmationWindowController(Environment.i.serviceLocator.Get<IChatController>());
                break;
            case HUDElementID.TASKBAR:
                hudElement = new TaskbarHUDController(Environment.i.serviceLocator.Get<IChatController>(),
                    FriendsController.i);
                break;
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                hudElement = new ExternalUrlPromptHUDController();
                break;
            case HUDElementID.NFT_INFO_DIALOG:
                hudElement = new NFTPromptHUDController();
                break;
            case HUDElementID.TELEPORT_DIALOG:
                hudElement = new TeleportPromptHUDController();
                break;
            case HUDElementID.CONTROLS_HUD:
                hudElement = new ControlsHUDController();
                break;
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                hudElement = new HelpAndSupportHUDController();
                break;
            case HUDElementID.USERS_AROUND_LIST_HUD:
                hudElement = new VoiceChatWindowController(
                    new UserProfileWebInterfaceBridge(),
                    FriendsController.i,
                    new SocialAnalytics(
                        Environment.i.platform.serviceProviders.analytics,
                        new UserProfileWebInterfaceBridge()),
                    DataStore.i,
                    Settings.i,
                    SceneReferences.i.mouseCatcher);

                break;
            case HUDElementID.GRAPHIC_CARD_WARNING:
                hudElement = new GraphicCardWarningHUDController();
                break;
            case HUDElementID.BUILDER_IN_WORLD_MAIN:
                break;
            case HUDElementID.QUESTS_PANEL:
                hudElement = new QuestsPanelHUDController();
                break;
            case HUDElementID.QUESTS_TRACKER:
                return new QuestsTrackerHUDController(
                    await CreateHUDView<IQuestsTrackerHUDView>(QUESTS_TRACKER_HUD));
            case HUDElementID.SIGNUP:
                return new SignupHUDController(
                    Environment.i.platform.serviceProviders.analytics, await CreateHUDView<ISignupHUDView>(SIGNUP_HUD), dataStoreLoadingScreen.Ref);
            case HUDElementID.BUILDER_PROJECTS_PANEL:
                break;
            case HUDElementID.LOADING:
                hudElement = new LoadingHUDController();
                break;
        }

        return hudElement;
    }

    private async UniTask<T> CreateHUDView<T>(string assetAddress) where T:IDisposable
    {
        var view = await assetsProvider.Instantiate<T>(assetAddress, $"_{assetAddress}");
        disposableViews.Add(view);

        return view;
    }
}
