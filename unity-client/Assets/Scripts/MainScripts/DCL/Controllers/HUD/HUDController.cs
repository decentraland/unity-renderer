using DCL.HelpAndSupportHUD;
using DCL.SettingsHUD;
using DCL.SettingsPanelHUD;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HUDController : MonoBehaviour
{
    private const string TOGGLE_UI_VISIBILITY_ASSET_NAME = "ToggleUIVisibility";

    static bool VERBOSE = false;

    public static HUDController i { get; private set; }

    private InputAction_Trigger toggleUIVisibilityTrigger;

    private readonly Notification.Model hiddenUINotification = new Notification.Model()
    {
        timer = 3,
        type = NotificationFactory.Type.UI_HIDDEN,
        groupID = "UIHiddenNotification"
    };

    private void Awake()
    {
        i = this;

        toggleUIVisibilityTrigger = Resources.Load<InputAction_Trigger>(TOGGLE_UI_VISIBILITY_ASSET_NAME);
        toggleUIVisibilityTrigger.OnTriggered += ToggleUIVisibility_OnTriggered;

        CommonScriptableObjects.allUIHidden.OnChange += AllUIHiddenOnOnChange;
        UserContextMenu.OnOpenPrivateChatRequest += OpenPrivateChatWindow;
    }

    public ProfileHUDController profileHud => GetHUDElement(HUDElementID.PROFILE_HUD) as ProfileHUDController;

    public NotificationHUDController notificationHud =>
        GetHUDElement(HUDElementID.NOTIFICATION) as NotificationHUDController;

    public MinimapHUDController minimapHud => GetHUDElement(HUDElementID.MINIMAP) as MinimapHUDController;

    public AvatarEditorHUDController avatarEditorHud =>
        GetHUDElement(HUDElementID.AVATAR_EDITOR) as AvatarEditorHUDController;

    // TODO (Santi): Remove once the new Settings HUD is implemented
    public SettingsHUDController settingsHud => GetHUDElement(HUDElementID.SETTINGS) as SettingsHUDController;

    public SettingsPanelHUDController settingsPanelHud => GetHUDElement(HUDElementID.SETTINGS_PANEL) as SettingsPanelHUDController;

    public ExpressionsHUDController expressionsHud =>
        GetHUDElement(HUDElementID.EXPRESSIONS) as ExpressionsHUDController;

    public PlayerInfoCardHUDController playerInfoCardHud =>
        GetHUDElement(HUDElementID.PLAYER_INFO_CARD) as PlayerInfoCardHUDController;

    public WelcomeHUDController messageOfTheDayHud =>
        GetHUDElement(HUDElementID.MESSAGE_OF_THE_DAY) as WelcomeHUDController;

    public AirdroppingHUDController airdroppingHud =>
        GetHUDElement(HUDElementID.AIRDROPPING) as AirdroppingHUDController;

    public TermsOfServiceHUDController termsOfServiceHud =>
        GetHUDElement(HUDElementID.TERMS_OF_SERVICE) as TermsOfServiceHUDController;

    public TaskbarHUDController taskbarHud => GetHUDElement(HUDElementID.TASKBAR) as TaskbarHUDController;

    public WorldChatWindowHUDController worldChatWindowHud =>
        GetHUDElement(HUDElementID.WORLD_CHAT_WINDOW) as WorldChatWindowHUDController;

    public PrivateChatWindowHUDController privateChatWindowHud =>
        GetHUDElement(HUDElementID.PRIVATE_CHAT_WINDOW) as PrivateChatWindowHUDController;

    public FriendsHUDController friendsHud => GetHUDElement(HUDElementID.FRIENDS) as FriendsHUDController;

    public TeleportPromptHUDController teleportHud => GetHUDElement(HUDElementID.TELEPORT_DIALOG) as TeleportPromptHUDController;

    public ControlsHUDController controlsHud => GetHUDElement(HUDElementID.CONTROLS_HUD) as ControlsHUDController;

    public EmailPromptHUDController emailPromptHud => GetHUDElement(HUDElementID.EMAIL_PROMPT) as EmailPromptHUDController;

    public ExploreHUDController exploreHud => GetHUDElement(HUDElementID.EXPLORE_HUD) as ExploreHUDController;

    public HelpAndSupportHUDController helpAndSupportHud => GetHUDElement(HUDElementID.HELP_AND_SUPPORT_HUD) as HelpAndSupportHUDController;

    public UsersAroundListHUDController usersAroundListHud => GetHUDElement(HUDElementID.USERS_AROUND_LIST_HUD) as UsersAroundListHUDController;

    public BuildModeHUDController buildModeHud => GetHUDElement(HUDElementID.BUILD_MODE) as BuildModeHUDController;

    public Dictionary<HUDElementID, IHUD> hudElements { get; private set; } = new Dictionary<HUDElementID, IHUD>();

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private WearableDictionary wearableCatalog => CatalogController.wearableCatalog;

    private void ShowAvatarEditor()
    {
        avatarEditorHud?.SetVisibility(true);
    }

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
        bool anyInputFieldIsSelected = EventSystem.current != null &&
                                       EventSystem.current.currentSelectedGameObject != null &&
                                       EventSystem.current.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>() != null &&
                                       (!worldChatWindowHud.view.chatHudView.inputField.isFocused || !worldChatWindowHud.view.isInPreview);

        if (anyInputFieldIsSelected ||
            settingsHud.view.isOpen || // TODO (Santi): Remove once the new Settings HUD is implemented
            settingsPanelHud.view.isOpen ||
            avatarEditorHud.view.isOpen ||
            DCL.NavmapView.isOpen ||
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

    public enum HUDElementID
    {
        NONE = 0,
        MINIMAP = 1,
        PROFILE_HUD = 2,
        NOTIFICATION = 3,
        AVATAR_EDITOR = 4,
        SETTINGS = 5,
        EXPRESSIONS = 6,
        PLAYER_INFO_CARD = 7,
        AIRDROPPING = 8,
        TERMS_OF_SERVICE = 9,
        WORLD_CHAT_WINDOW = 10,
        TASKBAR = 11,
        MESSAGE_OF_THE_DAY = 12,
        FRIENDS = 13,
        OPEN_EXTERNAL_URL_PROMPT = 14,
        PRIVATE_CHAT_WINDOW = 15,
        NFT_INFO_DIALOG = 16,
        TELEPORT_DIALOG = 17,
        CONTROLS_HUD = 18,
        EXPLORE_HUD = 19,
        HELP_AND_SUPPORT_HUD = 20,
        EMAIL_PROMPT = 21,
        USERS_AROUND_LIST_HUD = 22,
        GRAPHIC_CARD_WARNING = 23,
        BUILD_MODE = 24,
        SETTINGS_PANEL = 25,
        COUNT = 26
    }

    [System.Serializable]
    class ConfigureHUDElementMessage
    {
        public HUDElementID hudElementId;
        public HUDConfiguration configuration;
        public string extraPayload;
    }

    public void ConfigureHUDElement(string payload)
    {
        ConfigureHUDElementMessage message = JsonUtility.FromJson<ConfigureHUDElementMessage>(payload);

        HUDElementID id = message.hudElementId;
        HUDConfiguration configuration = message.configuration;
        string extraPayload = message.extraPayload;

        ConfigureHUDElement(id, configuration, extraPayload);
    }

    public void ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration, string extraPayload = null)
    {
        //TODO(Brian): For now, the factory code is using this switch approach.
        //             In order to avoid the factory upkeep we can transform the IHUD elements
        //             To ScriptableObjects. In this scenario, we can make each element handle its own
        //             specific initialization details.
        //
        //             This will allow us to unify the serialized factory objects design,
        //             like we already do with ECS components.

        switch (hudElementId)
        {
            case HUDElementID.NONE:
                break;
            case HUDElementID.MINIMAP:
                CreateHudElement<MinimapHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.PROFILE_HUD:
                CreateHudElement<ProfileHUDController>(configuration, hudElementId);
                if (profileHud != null)
                {
                    profileHud?.AddBackpackWindow(avatarEditorHud);
                }
                break;
            case HUDElementID.NOTIFICATION:
                CreateHudElement<NotificationHUDController>(configuration, hudElementId);
                NotificationsController.i?.Initialize(notificationHud);
                break;
            case HUDElementID.AVATAR_EDITOR:
                CreateHudElement<AvatarEditorHUDController>(configuration, hudElementId);
                if (avatarEditorHud != null)
                {
                    avatarEditorHud.Initialize(ownUserProfile, wearableCatalog);
                }
                break;
            case HUDElementID.SETTINGS:
                // TODO (Santi): Remove once the new Settings HUD is implemented
                CreateHudElement<SettingsHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.SETTINGS_PANEL:
                CreateHudElement<SettingsPanelHUDController>(configuration, hudElementId);
                if (settingsPanelHud != null)
                    settingsPanelHud.Initialize();
                break;
            case HUDElementID.EXPRESSIONS:
                CreateHudElement<ExpressionsHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.PLAYER_INFO_CARD:
                CreateHudElement<PlayerInfoCardHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.AIRDROPPING:
                CreateHudElement<AirdroppingHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.TERMS_OF_SERVICE:
                CreateHudElement<TermsOfServiceHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.WORLD_CHAT_WINDOW:
                if (worldChatWindowHud == null)
                {
                    CreateHudElement<WorldChatWindowHUDController>(configuration, hudElementId);

                    if (worldChatWindowHud != null)
                    {
                        worldChatWindowHud.Initialize(ChatController.i, DCL.InitialSceneReferences.i?.mouseCatcher);
                        worldChatWindowHud.OnPressPrivateMessage -= OpenPrivateChatWindow;
                        worldChatWindowHud.OnPressPrivateMessage += OpenPrivateChatWindow;
                        worldChatWindowHud.view.OnDeactivatePreview -= View_OnDeactivatePreview;
                        worldChatWindowHud.view.OnDeactivatePreview += View_OnDeactivatePreview;

                        taskbarHud?.AddWorldChatWindow(worldChatWindowHud);
                    }
                }
                else
                {
                    UpdateHudElement<WorldChatWindowHUDController>(configuration, hudElementId);
                }

                break;
            case HUDElementID.FRIENDS:
                if (friendsHud == null)
                {
                    CreateHudElement<FriendsHUDController>(configuration, hudElementId);

                    if (friendsHud != null)
                    {
                        friendsHud.Initialize(FriendsController.i, UserProfile.GetOwnUserProfile());
                        friendsHud.OnPressWhisper -= OpenPrivateChatWindow;
                        friendsHud.OnPressWhisper += OpenPrivateChatWindow;

                        taskbarHud?.AddFriendsWindow(friendsHud);
                    }
                }
                else
                {
                    UpdateHudElement<FriendsHUDController>(configuration, hudElementId);

                    if (!configuration.active)
                        taskbarHud?.DisableFriendsWindow();
                }

                if (privateChatWindowHud == null)
                {
                    CreateHudElement<PrivateChatWindowHUDController>(configuration, HUDElementID.PRIVATE_CHAT_WINDOW);

                    if (privateChatWindowHud != null)
                    {
                        privateChatWindowHud.Initialize(ChatController.i);
                        privateChatWindowHud.OnPressBack -= PrivateChatWindowHud_OnPressBack;
                        privateChatWindowHud.OnPressBack += PrivateChatWindowHud_OnPressBack;

                        taskbarHud?.AddPrivateChatWindow(privateChatWindowHud);
                    }
                }

                break;
            case HUDElementID.TASKBAR:
                if (taskbarHud == null)
                {
                    CreateHudElement<TaskbarHUDController>(configuration, hudElementId);

                    if (taskbarHud != null)
                    {
                        taskbarHud.Initialize(DCL.InitialSceneReferences.i?.mouseCatcher, ChatController.i, FriendsController.i);
                        taskbarHud.OnAnyTaskbarButtonClicked -= TaskbarHud_onAnyTaskbarButtonClicked;
                        taskbarHud.OnAnyTaskbarButtonClicked += TaskbarHud_onAnyTaskbarButtonClicked;

                        bool addOldSettingsHUD = false;
                        if (!string.IsNullOrEmpty(extraPayload))
                        {
                            var config = JsonUtility.FromJson<TaskbarHUDController.Configuration>(extraPayload);
                            if (config.enableVoiceChat)
                            {
                                taskbarHud.OnAddVoiceChat();
                            }

                            // TODO (Santi): Remove once the new Settings HUD is implemented
                            addOldSettingsHUD = config.enableOldSettings;
                        }

                        if (addOldSettingsHUD)
                            taskbarHud.AddSettingsWindow(settingsHud);
                        else
                            taskbarHud.AddSettingsWindow(settingsPanelHud);
                    }
                }
                else
                {
                    UpdateHudElement<TaskbarHUDController>(configuration, hudElementId);
                }

                break;
            case HUDElementID.MESSAGE_OF_THE_DAY:
                CreateHudElement<WelcomeHUDController>(configuration, hudElementId);
                messageOfTheDayHud?.Initialize(JsonUtility.FromJson<MessageOfTheDayConfig>(extraPayload));
                break;
            case HUDElementID.OPEN_EXTERNAL_URL_PROMPT:
                CreateHudElement<ExternalUrlPromptHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.NFT_INFO_DIALOG:
                CreateHudElement<NFTPromptHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.TELEPORT_DIALOG:
                CreateHudElement<TeleportPromptHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.CONTROLS_HUD:
                CreateHudElement<ControlsHUDController>(configuration, hudElementId);
                taskbarHud?.AddControlsMoreOption();
                break;
            case HUDElementID.EMAIL_PROMPT:
                if (emailPromptHud == null)
                {
                    CreateHudElement<EmailPromptHUDController>(configuration, hudElementId);
                }
                emailPromptHud?.SetEnable(configuration.active);
                break;
            case HUDElementID.EXPLORE_HUD:
                CreateHudElement<ExploreHUDController>(configuration, hudElementId);
                if (exploreHud != null)
                {
                    exploreHud.Initialize(FriendsController.i);
                    taskbarHud?.AddExploreWindow(exploreHud);
                }
                break;
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                CreateHudElement<HelpAndSupportHUDController>(configuration, hudElementId);
                taskbarHud?.AddHelpAndSupportWindow(helpAndSupportHud);
                break;
            case HUDElementID.USERS_AROUND_LIST_HUD:
                CreateHudElement<UsersAroundListHUDController>(configuration, hudElementId);
                if (usersAroundListHud != null)
                {
                    minimapHud?.AddUsersAroundIndicator(usersAroundListHud);
                }
                break;
            case HUDElementID.GRAPHIC_CARD_WARNING:
                CreateHudElement<GraphicCardWarningHUDController>(configuration, hudElementId);
                break;
            case HUDElementID.BUILD_MODE:
                CreateHudElement<BuildModeHUDController>(configuration, hudElementId);
                break;
        }

        var hudElement = GetHUDElement(hudElementId);

        if (hudElement != null)
            hudElement.SetVisibility(configuration.active && configuration.visible);
    }

    private void OpenPrivateChatWindow(string targetUserId)
    {
        taskbarHud?.OpenPrivateChatTo(targetUserId);
    }

    private void View_OnDeactivatePreview()
    {
        playerInfoCardHud?.CloseCard();
    }

    private void PrivateChatWindowHud_OnPressBack()
    {
        taskbarHud?.OpenFriendsWindow();
    }

    private void TaskbarHud_onAnyTaskbarButtonClicked()
    {
        playerInfoCardHud?.CloseCard();
    }

    public void CreateHudElement<T>(HUDConfiguration config, HUDElementID id)
        where T : IHUD, new()
    {
        bool controllerCreated = hudElements.ContainsKey(id);

        if (config.active && !controllerCreated)
        {
            hudElements.Add(id, new T());

            if (VERBOSE)
                Debug.Log($"Adding {id} .. type {hudElements[id].GetType().Name}");
        }
    }

    public void UpdateHudElement<T>(HUDConfiguration config, HUDElementID id)
        where T : IHUD, new()
    {
        if (!hudElements.ContainsKey(id)) return;

        if (VERBOSE)
            Debug.Log($"Updating {id}, type {hudElements[id].GetType().Name}, active: {config.active} visible: {config.visible}");

        hudElements[id].SetVisibility(config.visible);
    }

    public void TriggerSelfUserExpression(string id)
    {
        UserProfile.GetOwnUserProfile().SetAvatarExpression(id);
    }

    public void AirdroppingRequest(string payload)
    {
        var model = JsonUtility.FromJson<AirdroppingHUDController.Model>(payload);
        airdroppingHud.AirdroppingRequested(model);
    }

    public void ShowTermsOfServices(string payload)
    {
        var model = JsonUtility.FromJson<TermsOfServiceHUDController.Model>(payload);
        termsOfServiceHud?.ShowTermsOfService(model);
    }

    public void SetPlayerTalking(string talking)
    {
        taskbarHud?.SetVoiceChatRecording("true".Equals(talking));
    }

    public void SetVoiceChatEnabledByScene(int enabledPayload)
    {
        bool isEnabled = enabledPayload != 0;
        taskbarHud?.SetVoiceChatEnabledByScene(isEnabled);
    }

    public void SetUserTalking(string payload)
    {
        var model = JsonUtility.FromJson<UserTalkingModel>(payload);
        usersAroundListHud?.SetUserRecording(model.userId, model.talking);
    }

    public void SetUsersMuted(string payload)
    {
        var model = JsonUtility.FromJson<UserMutedModel>(payload);
        usersAroundListHud?.SetUsersMuted(model.usersId, model.muted);
    }

    public void RequestTeleport(string teleportDataJson)
    {
        teleportHud?.RequestTeleport(teleportDataJson);
    }

    public void UpdateBalanceOfMANA(string balance)
    {
        profileHud?.SetManaBalance(balance);
    }

    public void ShowAvatarEditorInSignUp()
    {
        if (avatarEditorHud != null)
        {
            avatarEditorHud.IsSignUpFlowValue = true;
            ShowAvatarEditor();
        }
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    public void Cleanup()
    {
        toggleUIVisibilityTrigger.OnTriggered -= ToggleUIVisibility_OnTriggered;
        CommonScriptableObjects.allUIHidden.OnChange -= AllUIHiddenOnOnChange;

        if (worldChatWindowHud != null)
        {
            worldChatWindowHud.OnPressPrivateMessage -= OpenPrivateChatWindow;
            worldChatWindowHud.view.OnDeactivatePreview -= View_OnDeactivatePreview;
        }

        if (privateChatWindowHud != null)
            privateChatWindowHud.OnPressBack -= PrivateChatWindowHud_OnPressBack;

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



    public IHUD GetHUDElement(HUDElementID id)
    {
        if (!hudElements.ContainsKey(id))
            return null;

        return hudElements[id];
    }

#if UNITY_EDITOR
    [ContextMenu("Trigger fake PlayerInfoCard")]
    public void TriggerFakePlayerInfoCard()
    {
        var newModel = ownUserProfile.CloneModel();
        newModel.name = "FakePassport";
        newModel.description = "Fake Description for Testing";
        newModel.userId = "test-id";
        newModel.inventory = new[]
        {
            "dcl://halloween_2019/machete_headband_top_head",
            "dcl://halloween_2019/bee_suit_upper_body",
            "dcl://halloween_2019/bride_of_frankie_upper_body",
            "dcl://halloween_2019/creepy_nurse_upper_body",
        };

        UserProfileController.i.AddUserProfileToCatalog(newModel);
        Resources.Load<StringVariable>("CurrentPlayerInfoCardId").Set(newModel.userId);
    }
#endif
}
