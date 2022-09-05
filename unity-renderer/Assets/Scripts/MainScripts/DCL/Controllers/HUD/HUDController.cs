using DCL;
using DCL.HelpAndSupportHUD;
using DCL.Huds.QuestsPanel;
using DCL.Huds.QuestsTracker;
using DCL.QuestsController;
using DCL.SettingsPanelHUD;
using SignupHUD;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using DCL.Chat;
using UnityEngine;
using UnityEngine.EventSystems;

public class HUDController : IHUDController
{
    private const string TOGGLE_UI_VISIBILITY_ASSET_NAME = "ToggleUIVisibility";

    static bool VERBOSE = false;
    public static HUDController i { get; private set; }

    public IHUDFactory hudFactory = null;

    private InputAction_Trigger toggleUIVisibilityTrigger;
    private ISocialAnalytics socialAnalytics;

    private readonly DCL.NotificationModel.Model hiddenUINotification = new DCL.NotificationModel.Model()
    {
        timer = 3,
        type = DCL.NotificationModel.Type.UI_HIDDEN,
        groupID = "UIHiddenNotification"
    };

    public HUDController(IHUDFactory hudFactory = null)
    {
        this.hudFactory = hudFactory;
        socialAnalytics = new SocialAnalytics(
            DCL.Environment.i.platform.serviceProviders.analytics,
            new UserProfileWebInterfaceBridge());
    }

    public void Initialize()
    {
        i = this;

        if (this.hudFactory == null)
            this.hudFactory = DCL.Environment.i.hud.factory;

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

    public AvatarEditorHUDController avatarEditorHud =>
        GetHUDElement(HUDElementID.AVATAR_EDITOR) as AvatarEditorHUDController;

    public SettingsPanelHUDController settingsPanelHud =>
        GetHUDElement(HUDElementID.SETTINGS_PANEL) as SettingsPanelHUDController;

    public PlayerInfoCardHUDController playerInfoCardHud =>
        GetHUDElement(HUDElementID.PLAYER_INFO_CARD) as PlayerInfoCardHUDController;

    public AirdroppingHUDController airdroppingHud =>
        GetHUDElement(HUDElementID.AIRDROPPING) as AirdroppingHUDController;

    public TermsOfServiceHUDController termsOfServiceHud =>
        GetHUDElement(HUDElementID.TERMS_OF_SERVICE) as TermsOfServiceHUDController;

    public TaskbarHUDController taskbarHud => GetHUDElement(HUDElementID.TASKBAR) as TaskbarHUDController;

    public LoadingHUDController loadingHud => GetHUDElement(HUDElementID.LOADING) as LoadingHUDController;

    public WorldChatWindowController worldChatWindowHud =>
        GetHUDElement(HUDElementID.WORLD_CHAT_WINDOW) as WorldChatWindowController;

    public PrivateChatWindowController PrivateChatWindow =>
        GetHUDElement(HUDElementID.PRIVATE_CHAT_WINDOW) as PrivateChatWindowController;

    public PublicChatChannelController PublicChatChannelHud =>
        GetHUDElement(HUDElementID.PUBLIC_CHAT_CHANNEL) as PublicChatChannelController;

    public FriendsHUDController friendsHud => GetHUDElement(HUDElementID.FRIENDS) as FriendsHUDController;

    public TeleportPromptHUDController teleportHud =>
        GetHUDElement(HUDElementID.TELEPORT_DIALOG) as TeleportPromptHUDController;

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
    public LoadingHUDController loadingController => GetHUDElement(HUDElementID.LOADING) as LoadingHUDController;

    public Dictionary<HUDElementID, IHUD> hudElements { get; private set; } = new Dictionary<HUDElementID, IHUD>();

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private BaseDictionary<string, WearableItem> wearableCatalog => CatalogController.wearableCatalog;

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
                                       EventSystem.current.currentSelectedGameObject
                                           .GetComponent<TMPro.TMP_InputField>() != null;

        if (anyInputFieldIsSelected ||
            DataStore.i.exploreV2.isOpen.Get() ||
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

    public void ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration,
        string extraPayload = null)
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
                if (minimapHud == null)
                {
                    CreateHudElement(configuration, hudElementId);

                    if (minimapHud != null)
                    {
                        minimapHud.Initialize();
                    }
                }

                break;
            case HUDElementID.PROFILE_HUD:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.NOTIFICATION:
                CreateHudElement(configuration, hudElementId);
                NotificationsController.i?.Initialize(notificationHud);
                break;
            case HUDElementID.AVATAR_EDITOR:
                CreateHudElement(configuration, hudElementId);
                avatarEditorHud?.Initialize(ownUserProfile, wearableCatalog);
                break;
            case HUDElementID.SETTINGS_PANEL:
                CreateHudElement(configuration, hudElementId);
                if (settingsPanelHud != null)
                    settingsPanelHud.Initialize();
                break;
            case HUDElementID.PLAYER_INFO_CARD:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.AIRDROPPING:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.TERMS_OF_SERVICE:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.WORLD_CHAT_WINDOW:
                if (worldChatWindowHud == null)
                {
                    CreateHudElement(configuration, hudElementId);

                    if (worldChatWindowHud != null)
                    {
                        worldChatWindowHud.Initialize(WorldChatWindowComponentView.Create());
                        worldChatWindowHud.SetVisibility(false);
                        worldChatWindowHud.OnOpenPrivateChat -= OpenPrivateChatWindow;
                        worldChatWindowHud.OnOpenPrivateChat += OpenPrivateChatWindow;
                        worldChatWindowHud.OnOpenPublicChannel -= OpenPublicChannelWindow;
                        worldChatWindowHud.OnOpenPublicChannel += OpenPublicChannelWindow;

                        taskbarHud?.AddWorldChatWindow(worldChatWindowHud);
                    }
                }
                else
                    UpdateHudElement(configuration, hudElementId);

                if (PublicChatChannelHud == null)
                {
                    CreateHudElement(configuration, HUDElementID.PUBLIC_CHAT_CHANNEL);
                    PublicChatChannelHud.Initialize();
                    PublicChatChannelHud.OnBack -= HandlePublicChatChannelBacked;
                    PublicChatChannelHud.OnBack += HandlePublicChatChannelBacked;
                    PublicChatChannelHud.OnClosed -= HandlePublicChatChannelClosed;
                    PublicChatChannelHud.OnClosed += HandlePublicChatChannelClosed;
                    taskbarHud?.AddPublicChatChannel(PublicChatChannelHud);
                    // TODO: this call should be removed when chat notifications are implemented
                    taskbarHud?.OpenPublicChatChannel(ChatUtils.NEARBY_CHANNEL_ID, false);
                    PublicChatChannelHud.ActivatePreviewModeInstantly();
                }
                else
                    UpdateHudElement(configuration, HUDElementID.PUBLIC_CHAT_CHANNEL);

                if (PrivateChatWindow == null)
                {
                    CreateHudElement(configuration, HUDElementID.PRIVATE_CHAT_WINDOW);

                    if (PrivateChatWindow != null)
                    {
                        PrivateChatWindow.Initialize();
                        PrivateChatWindow.SetVisibility(false);
                        PrivateChatWindow.OnPressBack -= PrivateChatWindowHud_OnPressBack;
                        PrivateChatWindow.OnPressBack += PrivateChatWindowHud_OnPressBack;

                        taskbarHud?.AddPrivateChatWindow(PrivateChatWindow);
                    }
                }
                else
                    UpdateHudElement(configuration, HUDElementID.PRIVATE_CHAT_WINDOW);

                break;
            case HUDElementID.FRIENDS:
                if (friendsHud == null)
                {
                    CreateHudElement(configuration, hudElementId);

                    if (friendsHud != null)
                    {
                        friendsHud.Initialize();
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
                    CreateHudElement(configuration, hudElementId);

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
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.NFT_INFO_DIALOG:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.TELEPORT_DIALOG:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.CONTROLS_HUD:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.HELP_AND_SUPPORT_HUD:
                CreateHudElement(configuration, hudElementId);
                settingsPanelHud?.AddHelpAndSupportWindow(helpAndSupportHud);
                break;
            case HUDElementID.USERS_AROUND_LIST_HUD:
                CreateHudElement(configuration, hudElementId);
                if (voiceChatHud != null)
                    taskbarHud?.AddVoiceChatWindow(voiceChatHud);

                break;
            case HUDElementID.GRAPHIC_CARD_WARNING:
                CreateHudElement(configuration, hudElementId);
                break;
            case HUDElementID.QUESTS_PANEL:
                CreateHudElement(configuration, hudElementId);
                if (configuration.active)
                    questsPanelHUD.Initialize(QuestsController.i);
                break;
            case HUDElementID.QUESTS_TRACKER:
                CreateHudElement(configuration, hudElementId);
                if (configuration.active)
                    questsTrackerHUD.Initialize(QuestsController.i);
                break;
            case HUDElementID.SIGNUP:
                CreateHudElement(configuration, hudElementId);
                if (configuration.active)
                {
                    //Same race condition risks as with the ProfileHUD
                    //TODO Refactor the way AvatarEditor sets its visibility to match our data driven pattern
                    //Then this reference can be removed so we just work with a BaseVariable<bool>.
                    //This refactor applies to the ProfileHUD and the way kernel asks the HUDController during signup
                    signupHUD.Initialize(avatarEditorHud);
                }

                break;
            case HUDElementID.LOADING:
                if (loadingHud == null)
                {
                    CreateHudElement(configuration, hudElementId);
                    if (loadingHud != null && configuration.active)
                        loadingController.Initialize();
                }

                break;
            case HUDElementID.AVATAR_NAMES:
                // TODO Remove the HUDElementId once kernel stops sending the Configure HUD message
                break;
        }

        var hudElement = GetHUDElement(hudElementId);

        if (hudElement != null)
            hudElement.SetVisibility(configuration.active && configuration.visible);
    }

    private void HandlePublicChatChannelBacked()
    {
        PublicChatChannelHud.SetVisibility(false);
        taskbarHud?.GoBackFromChat();
    }

    private void OpenPublicChannelWindow(string channelId)
    {
        taskbarHud?.OpenPublicChatChannel(channelId, true);
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
        playerInfoCardHud?.CloseCard();
    }

    public void CreateHudElement(HUDConfiguration config, HUDElementID id)
    {
        bool controllerCreated = hudElements.ContainsKey(id);

        if (config.active && !controllerCreated)
        {
            hudElements.Add(id, hudFactory.CreateHUD(id));

            if (VERBOSE)
                Debug.Log($"Adding {id} .. type {hudElements[id].GetType().Name}");
        }
    }

    public void UpdateHudElement(HUDConfiguration config, HUDElementID id)
    {
        if (!hudElements.ContainsKey(id))
            return;

        if (VERBOSE)
            Debug.Log(
                $"Updating {id}, type {hudElements[id].GetType().Name}, active: {config.active} visible: {config.visible}");

        hudElements[id].SetVisibility(config.visible);
    }

    public void Cleanup()
    {
        toggleUIVisibilityTrigger.OnTriggered -= ToggleUIVisibility_OnTriggered;
        CommonScriptableObjects.allUIHidden.OnChange -= AllUIHiddenOnOnChange;

        if (worldChatWindowHud != null)
        {
            worldChatWindowHud.OnOpenPrivateChat -= OpenPrivateChatWindow;
            worldChatWindowHud.OnOpenPublicChannel -= OpenPublicChannelWindow;
        }

        if (PrivateChatWindow != null)
            PrivateChatWindow.OnPressBack -= PrivateChatWindowHud_OnPressBack;

        if (PublicChatChannelHud != null)
        {
            PublicChatChannelHud.OnClosed -= HandlePublicChatChannelClosed;
            PublicChatChannelHud.OnBack -= HandlePublicChatChannelBacked;
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
        PublicChatChannelHud.SetVisibility(false);
    }

    public IHUD GetHUDElement(HUDElementID id)
    {
        if (!hudElements.ContainsKey(id))
            return null;

        return hudElements[id];
    }

    public static bool IsHUDElementDeprecated(HUDElementID element)
    {
        Type enumType = typeof(HUDElementID);
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
        Resources.Load<StringVariable>("CurrentPlayerInfoCardId").Set(newModel.userId);
    }
#endif
    public void Dispose()
    {
        Cleanup();
    }
}