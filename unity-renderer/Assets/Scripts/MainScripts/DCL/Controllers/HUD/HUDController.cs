using Cysharp.Threading.Tasks;
using DCL;
using DCL.Chat;
using DCL.Chat.HUD;
using DCL.HelpAndSupportHUD;
using DCL.Huds.QuestsPanel;
using DCL.Huds.QuestsTracker;
using DCL.NotificationModel;
using DCL.QuestsController;
using DCL.SettingsPanelHUD;
using DCL.Social.Friends;
using SignupHUD;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;
using Type = DCL.NotificationModel.Type;

public class HUDController : IHUDController
{
    private const string TOGGLE_UI_VISIBILITY_ASSET_NAME = "ToggleUIVisibility";
    private const string OPEN_PASSPORT_SOURCE = "ProfileHUD";

    static bool VERBOSE = false;
    public static HUDController i { get; private set; }

    public IHUDFactory hudFactory = null;

    private InputAction_Trigger toggleUIVisibilityTrigger;
    private DataStore dataStore;

    private readonly Model hiddenUINotification = new Model()
    {
        timer = 3,
        type = Type.UI_HIDDEN,
        groupID = "UIHiddenNotification"
    };

    public HUDController(DataStore dataStore, IHUDFactory hudFactory = null)
    {
        this.hudFactory = hudFactory;
        this.dataStore = dataStore;
    }

    public void Initialize()
    {
        i = this;

        if (this.hudFactory == null)
            this.hudFactory = Environment.i.hud.factory;

        toggleUIVisibilityTrigger = Resources.Load<InputAction_Trigger>(TOGGLE_UI_VISIBILITY_ASSET_NAME);
        toggleUIVisibilityTrigger.OnTriggered += ToggleUIVisibility_OnTriggered;

        CommonScriptableObjects.allUIHidden.OnChange += AllUIHiddenOnOnChange;
        UserContextMenu.OnOpenPrivateChatRequest += OpenPrivateChatWindow;
    }


    public event Action OnTaskbarCreation;

    public ProfileHUDController profileHud => GetHUDElement(HUDElementID.PROFILE_HUD) as ProfileHUDController;

    public NotificationHUDController notificationHud =>
        GetHUDElement(HUDElementID.NOTIFICATION) as NotificationHUDController;

    public MinimapHUDController minimapHud => GetHUDElement(HUDElementID.MINIMAP) as MinimapHUDController;

    public SettingsPanelHUDController settingsPanelHud =>
        GetHUDElement(HUDElementID.SETTINGS_PANEL) as SettingsPanelHUDController;


    public TermsOfServiceHUDController termsOfServiceHud =>
        GetHUDElement(HUDElementID.TERMS_OF_SERVICE) as TermsOfServiceHUDController;

    public TaskbarHUDController taskbarHud => GetHUDElement(HUDElementID.TASKBAR) as TaskbarHUDController;

    public WorldChatWindowController worldChatWindowHud =>
        GetHUDElement(HUDElementID.WORLD_CHAT_WINDOW) as WorldChatWindowController;

    public PrivateChatWindowController PrivateChatWindow =>
        GetHUDElement(HUDElementID.PRIVATE_CHAT_WINDOW) as PrivateChatWindowController;

    public PublicChatWindowController PublicChatWindowHud =>
        GetHUDElement(HUDElementID.PUBLIC_CHAT) as PublicChatWindowController;

    private ChatChannelHUDController chatChannelHud =>
        GetHUDElement(HUDElementID.CHANNELS_CHAT) as ChatChannelHUDController;

    private SearchChannelsWindowController channelSearchHud =>
        GetHUDElement(HUDElementID.CHANNELS_SEARCH) as SearchChannelsWindowController;

    private CreateChannelWindowController channelCreateHud =>
        GetHUDElement(HUDElementID.CHANNELS_CREATE) as CreateChannelWindowController;

    private LeaveChannelConfirmationWindowController channelLeaveHud =>
        GetHUDElement(HUDElementID.CHANNELS_LEAVE_CONFIRMATION) as LeaveChannelConfirmationWindowController;

    public FriendsHUDController friendsHud => GetHUDElement(HUDElementID.FRIENDS) as FriendsHUDController;

    public ControlsHUDController controlsHud => GetHUDElement(HUDElementID.CONTROLS_HUD) as ControlsHUDController;

    public HelpAndSupportHUDController helpAndSupportHud =>
        GetHUDElement(HUDElementID.HELP_AND_SUPPORT_HUD) as HelpAndSupportHUDController;

    public MinimapHUDController minimapHUD => GetHUDElement(HUDElementID.MINIMAP) as MinimapHUDController;

    public VoiceChatWindowController voiceChatHud =>
        GetHUDElement(HUDElementID.USERS_AROUND_LIST_HUD) as VoiceChatWindowController;

    public QuestsPanelHUDController questsPanelHUD =>
        GetHUDElement(HUDElementID.QUESTS_PANEL) as QuestsPanelHUDController;

    public QuestsTrackerHUDController questsTrackerHUD =>
        GetHUDElement(HUDElementID.QUESTS_TRACKER) as QuestsTrackerHUDController;

    public SignupHUDController signupHUD => GetHUDElement(HUDElementID.SIGNUP) as SignupHUDController;

    public Dictionary<HUDElementID, IHUD> hudElements { get; private set; } = new Dictionary<HUDElementID, IHUD>();

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    private void ShowSettings()
    {
        settingsPanelHud?.SetVisibility(true);
    }

    private void ShowControls()
    {
        controlsHud?.SetVisibility(true);
    }

    private void ToggleUIVisibility_OnTriggered(DCLAction_Trigger action)
    {
        bool anyInputFieldIsSelected = InputProcessor.FocusIsInInputField();

        if (anyInputFieldIsSelected ||
            dataStore.exploreV2.isOpen.Get() ||
            CommonScriptableObjects.tutorialActive)
            return;

        CommonScriptableObjects.allUIHidden.Set(!CommonScriptableObjects.allUIHidden.Get());
    }

    private void AllUIHiddenOnOnChange(bool current, bool previous)
    {
        if (current)
        {
            NotificationsController.i?.ShowNotification(hiddenUINotification);
        }
        else
        {
            NotificationsController.i?.DismissAllNotifications(hiddenUINotification.groupID);
        }
    }

    public async UniTask ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration, CancellationToken cancellationToken = default,
        string extraPayload = null)
    {
        switch (hudElementId)
        {
            case HUDElementID.NONE:
                break;
            case HUDElementID.MINIMAP:
                if (minimapHud == null)
                {
                    // dependencies should be initialized
                    await Environment.WaitUntilInitialized();
                    await CreateHudElement(configuration, hudElementId, cancellationToken);
                    minimapHud?.Initialize();
                }

                break;
            case HUDElementID.PROFILE_HUD:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                break;
            case HUDElementID.NOTIFICATION:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                if (NotificationsController.i != null)
                    NotificationsController.i.Initialize(notificationHud, dataStore.notifications);
                break;
            case HUDElementID.SETTINGS_PANEL:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                settingsPanelHud?.Initialize();
                break;
            case HUDElementID.TERMS_OF_SERVICE:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                break;
            case HUDElementID.WORLD_CHAT_WINDOW:
                if (worldChatWindowHud == null)
                {
                    await CreateHudElement(configuration, hudElementId, cancellationToken);

                    if (worldChatWindowHud != null)
                    {
                        worldChatWindowHud.Initialize(WorldChatWindowComponentView.Create());
                        worldChatWindowHud.SetVisibility(false);
                        worldChatWindowHud.OnOpenPrivateChat -= OpenPrivateChatWindow;
                        worldChatWindowHud.OnOpenPrivateChat += OpenPrivateChatWindow;
                        worldChatWindowHud.OnOpenPublicChat -= OpenPublicChatWindow;
                        worldChatWindowHud.OnOpenPublicChat += OpenPublicChatWindow;
                        worldChatWindowHud.OnOpenChannel -= OpenChannelChatWindow;
                        worldChatWindowHud.OnOpenChannel += OpenChannelChatWindow;
                        worldChatWindowHud.OnOpenChannelSearch -= OpenChannelSearchWindow;
                        worldChatWindowHud.OnOpenChannelSearch += OpenChannelSearchWindow;

                        taskbarHud?.AddWorldChatWindow(worldChatWindowHud);
                    }
                }
                else
                    UpdateHudElement(configuration, hudElementId);

                if (PublicChatWindowHud == null)
                {
                    await CreateHudElement(configuration, HUDElementID.PUBLIC_CHAT, cancellationToken);

                    if (PublicChatWindowHud != null)
                    {
                        PublicChatWindowHud.Initialize();
                        PublicChatWindowHud.Setup(ChatUtils.NEARBY_CHANNEL_ID);
                        PublicChatWindowHud.SetVisibility(false);
                        PublicChatWindowHud.OnBack -= HandlePublicChatChannelBacked;
                        PublicChatWindowHud.OnBack += HandlePublicChatChannelBacked;
                        PublicChatWindowHud.OnClosed -= HandlePublicChatChannelClosed;
                        PublicChatWindowHud.OnClosed += HandlePublicChatChannelClosed;
                        taskbarHud?.AddPublicChatChannel(PublicChatWindowHud);
                    }
                }
                else
                    UpdateHudElement(configuration, HUDElementID.PUBLIC_CHAT);

                if (PrivateChatWindow == null)
                {
                    await CreateHudElement(configuration, HUDElementID.PRIVATE_CHAT_WINDOW, cancellationToken);

                    if (PrivateChatWindow != null)
                    {
                        PrivateChatWindow.Initialize();
                        PrivateChatWindow.SetVisibility(false);
                        PrivateChatWindow.OnBack -= PrivateChatWindowHud_OnPressBack;
                        PrivateChatWindow.OnBack += PrivateChatWindowHud_OnPressBack;
                        taskbarHud?.AddPrivateChatWindow(PrivateChatWindow);
                    }
                }
                else
                    UpdateHudElement(configuration, HUDElementID.PRIVATE_CHAT_WINDOW);

                if (chatChannelHud == null)
                {
                    await CreateHudElement(configuration, HUDElementID.CHANNELS_CHAT, cancellationToken);

                    if (chatChannelHud != null)
                    {
                        chatChannelHud.Initialize();
                        chatChannelHud.SetVisibility(false);
                        chatChannelHud.OnPressBack -= HandleChannelBacked;
                        chatChannelHud.OnPressBack += HandleChannelBacked;

                        taskbarHud?.AddChatChannel(chatChannelHud);
                    }
                }

                if (channelSearchHud == null)
                {
                    await CreateHudElement(configuration, HUDElementID.CHANNELS_SEARCH, cancellationToken);

                    if (channelSearchHud != null)
                    {
                        channelSearchHud.Initialize(SearchChannelsWindowComponentView.Create());
                        channelSearchHud.SetVisibility(false);
                        taskbarHud?.AddChannelSearch(channelSearchHud);
                    }
                }

                if (channelCreateHud == null)
                {
                    await CreateHudElement(configuration, HUDElementID.CHANNELS_CREATE, cancellationToken);

                    if (channelCreateHud != null)
                    {
                        channelCreateHud.Initialize(CreateChannelWindowComponentView.Create());
                        channelCreateHud.SetVisibility(false);
                        taskbarHud?.AddChannelCreation(channelCreateHud);
                    }
                }

                if (channelLeaveHud == null)
                {
                    await CreateHudElement(configuration, HUDElementID.CHANNELS_LEAVE_CONFIRMATION, cancellationToken);

                    if (channelLeaveHud != null)
                    {
                        channelLeaveHud.Initialize(LeaveChannelConfirmationWindowComponentView.Create());
                        channelLeaveHud.SetVisibility(false);
                        taskbarHud?.AddChannelLeaveConfirmation(channelLeaveHud);
                    }
                }

                break;
            case HUDElementID.FRIENDS:
                if (friendsHud == null)
                {
                    await CreateHudElement(configuration, hudElementId, cancellationToken);

                    if (friendsHud != null)
                    {
                        friendsHud.Initialize(FriendsHUDComponentView.Create());
                        friendsHud.OnPressWhisper -= OpenPrivateChatWindow;
                        friendsHud.OnPressWhisper += OpenPrivateChatWindow;

                        taskbarHud?.AddFriendsWindow(friendsHud);
                    }
                }
                else
                {
                    UpdateHudElement(configuration, hudElementId);

                    if (!configuration.active)
                        taskbarHud?.DisableFriendsWindow();
                }

                break;
            case HUDElementID.TASKBAR:
                if (taskbarHud == null)
                {
                    await CreateHudElement(configuration, hudElementId, cancellationToken);

                    if (taskbarHud != null)
                    {
                        taskbarHud.Initialize(SceneReferences.i.mouseCatcher);
                        taskbarHud.OnAnyTaskbarButtonClicked -= TaskbarHud_onAnyTaskbarButtonClicked;
                        taskbarHud.OnAnyTaskbarButtonClicked += TaskbarHud_onAnyTaskbarButtonClicked;

                        OnTaskbarCreation?.Invoke();
                    }
                }
                else
                {
                    UpdateHudElement(configuration, hudElementId);
                }

                break;
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                break;
            case HUDElementID.NFT_INFO_DIALOG:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                break;
            case HUDElementID.CONTROLS_HUD:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                break;
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                settingsPanelHud?.AddHelpAndSupportWindow(helpAndSupportHud);
                break;
            case HUDElementID.USERS_AROUND_LIST_HUD:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                if (voiceChatHud != null)
                    taskbarHud?.AddVoiceChatWindow(voiceChatHud);

                break;
            case HUDElementID.GRAPHIC_CARD_WARNING:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                break;
            case HUDElementID.QUESTS_PANEL:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                if (configuration.active)
                    questsPanelHUD.Initialize(QuestsController.i);
                break;
            case HUDElementID.QUESTS_TRACKER:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                if (configuration.active)
                    questsTrackerHUD.Initialize(QuestsController.i);
                break;
            case HUDElementID.SIGNUP:
                await CreateHudElement(configuration, hudElementId, cancellationToken);
                if (configuration.active)
                    signupHUD.Initialize();
                break;
            case HUDElementID.AVATAR_NAMES:
                // TODO Remove the HUDElementId once kernel stops sending the Configure HUD message
                break;
        }

        GetHUDElement(hudElementId)?
           .SetVisibility(configuration.active && configuration.visible);
    }

    private void OpenChannelSearchWindow()
    {
        taskbarHud?.OpenChannelSearch();
    }

    private void HandleChannelBacked()
    {
        chatChannelHud.SetVisibility(false);
        taskbarHud?.GoBackFromChat();
    }

    private void HandlePublicChatChannelBacked()
    {
        PublicChatWindowHud.SetVisibility(false);
        taskbarHud?.GoBackFromChat();
    }

    private void OpenPublicChatWindow(string channelId)
    {
        taskbarHud?.OpenPublicChat(channelId, true);
    }

    private void OpenChannelChatWindow(string channelId)
    {
        taskbarHud?.OpenChannelChat(channelId);
    }

    private void OpenPrivateChatWindow(string targetUserId)
    {
        taskbarHud?.OpenPrivateChat(targetUserId);
    }

    private void PrivateChatWindowHud_OnPressBack()
    {
        PrivateChatWindow?.SetVisibility(false);
        taskbarHud?.GoBackFromChat();
    }

    private void TaskbarHud_onAnyTaskbarButtonClicked()
    {
    }

    private async UniTask CreateHudElement(HUDConfiguration config, HUDElementID id, CancellationToken cancellationToken = default)
    {
        bool controllerCreated = hudElements.ContainsKey(id);

        if (config.active && !controllerCreated)
        {
            try
            {
                IHUD hudElement = await hudFactory.CreateHUD(id, cancellationToken);
                hudElements.Add(id, hudElement);

                if (VERBOSE)
                    Debug.Log($"Adding {id} .. type {hudElements[id].GetType().Name}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load HUD element resource {id}. Exception message: {e.Message}");
            }
        }
    }

    private void UpdateHudElement(HUDConfiguration config, HUDElementID id)
    {
        if (!hudElements.ContainsKey(id))
            return;

        if (VERBOSE)
            Debug.Log($"Updating {id}, type {hudElements[id].GetType().Name}, active: {config.active} visible: {config.visible}");

        hudElements[id].SetVisibility(config.visible);
    }

    public void Cleanup()
    {
        toggleUIVisibilityTrigger.OnTriggered -= ToggleUIVisibility_OnTriggered;
        CommonScriptableObjects.allUIHidden.OnChange -= AllUIHiddenOnOnChange;

        if (worldChatWindowHud != null)
        {
            worldChatWindowHud.OnOpenPrivateChat -= OpenPrivateChatWindow;
            worldChatWindowHud.OnOpenPublicChat -= OpenPublicChatWindow;
            worldChatWindowHud.OnOpenChannel -= OpenChannelChatWindow;
        }

        if (PrivateChatWindow != null)
            PrivateChatWindow.OnBack -= PrivateChatWindowHud_OnPressBack;

        if (PublicChatWindowHud != null)
        {
            PublicChatWindowHud.OnClosed -= HandlePublicChatChannelClosed;
            PublicChatWindowHud.OnBack -= HandlePublicChatChannelBacked;
        }


        if (friendsHud != null)
            friendsHud.OnPressWhisper -= OpenPrivateChatWindow;

        if (taskbarHud != null)
            taskbarHud.OnAnyTaskbarButtonClicked -= TaskbarHud_onAnyTaskbarButtonClicked;

        UserContextMenu.OnOpenPrivateChatRequest -= OpenPrivateChatWindow;

        foreach (var kvp in hudElements)
        {
            kvp.Value?.Dispose();
        }

        hudElements.Clear();
    }

    private void HandlePublicChatChannelClosed()
    {
        PublicChatWindowHud.SetVisibility(false);
    }

    public IHUD GetHUDElement(HUDElementID id) =>
        hudElements.ContainsKey(id) ? hudElements[id] : null;

    public static bool IsHUDElementDeprecated(HUDElementID element)
    {
        System.Type enumType = typeof(HUDElementID);
        var enumName = enumType.GetEnumName(element);
        var fieldInfo = enumType.GetField(enumName);
        return Attribute.IsDefined(fieldInfo, typeof(ObsoleteAttribute));
    }

#if UNITY_EDITOR
    [ContextMenu("Trigger fake PlayerInfoCard")]
    public void TriggerFakePlayerInfoCard()
    {
        var newModel = ownUserProfile.CloneModel();
        newModel.name = "FakePassport";
        newModel.description = "Fake Description for Testing";
        newModel.userId = "test-id";

        UserProfileController.i.AddUserProfileToCatalog(newModel);
        UserProfileController.GetProfileByUserId(newModel.userId).SetInventory(new[]
        {
            "dcl://halloween_2019/machete_headband_top_head",
            "dcl://halloween_2019/bee_suit_upper_body",
            "dcl://halloween_2019/bride_of_frankie_upper_body",
            "dcl://halloween_2019/creepy_nurse_upper_body",
        });
        dataStore.HUDs.currentPlayerId.Set((newModel.userId, OPEN_PASSPORT_SOURCE));
    }
#endif
    public void Dispose()
    {
        Cleanup();
    }
}
