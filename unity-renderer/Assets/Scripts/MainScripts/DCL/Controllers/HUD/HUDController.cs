using DCL.SettingsHUD;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    static bool VERBOSE = false;

    public static HUDController i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public AvatarHUDController avatarHud => GetHUDElement(HUDElementID.AVATAR) as AvatarHUDController;
    public NotificationHUDController notificationHud => GetHUDElement(HUDElementID.NOTIFICATION) as NotificationHUDController;
    public MinimapHUDController minimapHud => GetHUDElement(HUDElementID.MINIMAP) as MinimapHUDController;
    public AvatarEditorHUDController avatarEditorHud => GetHUDElement(HUDElementID.AVATAR_EDITOR) as AvatarEditorHUDController;
    public SettingsHUDController settingsHud => GetHUDElement(HUDElementID.SETTINGS) as SettingsHUDController;
    public ExpressionsHUDController expressionsHud => GetHUDElement(HUDElementID.EXPRESSIONS) as ExpressionsHUDController;
    public PlayerInfoCardHUDController playerInfoCardHud => GetHUDElement(HUDElementID.PLAYER_INFO_CARD) as PlayerInfoCardHUDController;
    public WelcomeHUDController messageOfTheDayHud => GetHUDElement(HUDElementID.MESSAGE_OF_THE_DAY) as WelcomeHUDController;
    public AirdroppingHUDController airdroppingHud => GetHUDElement(HUDElementID.AIRDROPPING) as AirdroppingHUDController;
    public TermsOfServiceHUDController termsOfServiceHud => GetHUDElement(HUDElementID.TERMS_OF_SERVICE) as TermsOfServiceHUDController;
    public TaskbarHUDController taskbarHud => GetHUDElement(HUDElementID.TASKBAR) as TaskbarHUDController;
    public WorldChatWindowHUDController worldChatWindowHud => GetHUDElement(HUDElementID.WORLD_CHAT_WINDOW) as WorldChatWindowHUDController;
    public FriendsHUDController friendsHud => GetHUDElement(HUDElementID.FRIENDS) as FriendsHUDController;

    public Dictionary<HUDElementID, IHUD> hudElements { get; private set; } = new Dictionary<HUDElementID, IHUD>();

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private WearableDictionary wearableCatalog => CatalogController.wearableCatalog;

    private void ShowAvatarEditor()
    {
        avatarEditorHud?.SetVisibility(true);
    }

    private void ShowSettings()
    {
        settingsHud?.SetVisibility(true);
    }

    private void OwnUserProfileUpdated(UserProfile profile)
    {
        UpdateAvatarHUD();
    }

    public enum HUDElementID
    {
        NONE = 0,
        MINIMAP = 1,
        AVATAR = 2,
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
        COUNT = 13
    }

    [System.Serializable]
    class ConfigureHUDElementMessage
    {
        public HUDElementID hudElementId;
        public HUDConfiguration configuration;
    }

    public void ConfigureHUDElement(string payload)
    {
        ConfigureHUDElementMessage message = JsonUtility.FromJson<ConfigureHUDElementMessage>(payload);

        HUDConfiguration configuration = message.configuration;
        HUDElementID id = message.hudElementId;

        ConfigureHUDElement(id, configuration);
    }

    public void ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration)
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
            case HUDElementID.AVATAR:
                CreateHudElement<AvatarHUDController>(configuration, hudElementId);

                if (avatarHud != null)
                {
                    avatarHud.Initialize();
                    avatarHud.OnEditAvatarPressed += ShowAvatarEditor;
                    avatarHud.OnSettingsPressed += ShowSettings;
                    ownUserProfile.OnUpdate += OwnUserProfileUpdated;
                    OwnUserProfileUpdated(ownUserProfile);
                }
                break;
            case HUDElementID.NOTIFICATION:
                CreateHudElement<NotificationHUDController>(configuration, hudElementId);
                NotificationsController.i?.Initialize(notificationHud);
                break;
            case HUDElementID.AVATAR_EDITOR:
                CreateHudElement<AvatarEditorHUDController>(configuration, hudElementId);
                avatarEditorHud?.Initialize(ownUserProfile, wearableCatalog);
                break;
            case HUDElementID.SETTINGS:
                CreateHudElement<SettingsHUDController>(configuration, hudElementId);
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
                CreateHudElement<WorldChatWindowHUDController>(configuration, hudElementId);
                worldChatWindowHud?.Initialize(ChatController.i, DCL.InitialSceneReferences.i?.mouseCatcher);
                taskbarHud?.AddChatWindow(worldChatWindowHud);
                break;
            case HUDElementID.FRIENDS:
                CreateHudElement<FriendsHUDController>(configuration, hudElementId);
                friendsHud?.Initialize(FriendsController.i, UserProfile.GetOwnUserProfile());
                friendsHud.OnPressWhisper -= FriendsHud_OnPressWhisper;
                friendsHud.OnPressWhisper += FriendsHud_OnPressWhisper;
                taskbarHud?.AddFriendsWindow(friendsHud);
                break;
            case HUDElementID.TASKBAR:
                CreateHudElement<TaskbarHUDController>(configuration, hudElementId);
                // ConfigureTaskbar();
                break;
            case HUDElementID.MESSAGE_OF_THE_DAY:
                CreateHudElement<WelcomeHUDController>(configuration, hudElementId);
                messageOfTheDayHud?.Initialize(ownUserProfile.hasConnectedWeb3);
                break;
        }

        GetHUDElement(hudElementId)?.SetVisibility(configuration.active && configuration.visible);
    }

    private void FriendsHud_OnPressWhisper(string userName)
    {
        worldChatWindowHud.ForceFocus($"/w {userName} ");
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

    public void ShowNewWearablesNotification(string wearableCountString)
    {
        if (int.TryParse(wearableCountString, out int wearableCount))
        {
            avatarHud.SetNewWearablesNotification(wearableCount);
        }
    }

    public void TriggerSelfUserExpression(string id)
    {
        expressionsHud?.ExpressionCalled(id);
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

    private void UpdateAvatarHUD()
    {
        avatarHud?.UpdateData(new AvatarHUDModel()
        {
            name = ownUserProfile.userName,
            mail = ownUserProfile.email,
            avatarPic = ownUserProfile.faceSnapshot
        });
    }

    private void OnDestroy()
    {
        if (ownUserProfile != null)
            ownUserProfile.OnUpdate -= OwnUserProfileUpdated;

        if (avatarHud != null)
        {
            avatarHud.OnEditAvatarPressed -= ShowAvatarEditor;
            avatarHud.OnSettingsPressed -= ShowSettings;
        }

        foreach (var kvp in hudElements)
        {
            kvp.Value?.Dispose();
        }
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
        newModel.inventory = new[]
        {
            "dcl://halloween_2019/machete_headband_top_head",
            "dcl://halloween_2019/bee_suit_upper_body",
            "dcl://halloween_2019/bride_of_frankie_upper_body",
            "dcl://halloween_2019/creepy_nurse_upper_body",
        };

        UserProfileController.i.AddUserProfileToCatalog(newModel);
        Resources.Load<StringVariable>("CurrentPlayerInfoCardName").Set(newModel.name);
    }
#endif
}
