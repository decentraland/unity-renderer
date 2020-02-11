using System.Collections.Generic;
using UnityEngine;
using DCL.SettingsHUD;

public class HUDController : MonoBehaviour
{
    public static HUDController i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public AvatarHUDController avatarHud { get; private set; }
    public NotificationHUDController notificationHud { get; private set; }
    public MinimapHUDController minimapHud { get; private set; }
    public AvatarEditorHUDController avatarEditorHud { get; private set; }
    public SettingsHUDController settingsHud { get; private set; }
    public ExpressionsHUDController expressionsHud { get; private set; }
    public PlayerInfoCardHUDController playerInfoCardHudController { get; private set; }
    public WelcomeHUDController welcomeHudController { get; private set; }
    public AirdroppingHUDController airdroppingHUDController { get; private set; }

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

    public void ShowNotificationFromJson(string notificationJson)
    {
        NotificationModel model = JsonUtility.FromJson<NotificationModel>(notificationJson);
        ShowNotification(model);
    }

    public void ShowNotification(NotificationModel notification)
    {
        notificationHud.ShowNotification(notification);
    }

    public void ShowNewWearablesNotification(string wearableCountString)
    {
        if (int.TryParse(wearableCountString, out int wearableCount))
        {
            avatarHud.SetNewWearablesNotification(wearableCount);
        }
    }

    public void ConfigureMinimapHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && minimapHud == null)
        {
            minimapHud = new MinimapHUDController();
        }

        minimapHud?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureAvatarHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && avatarHud == null)
        {
            avatarHud = new AvatarHUDController();
            avatarHud.OnEditAvatarPressed += ShowAvatarEditor;
            avatarHud.OnSettingsPressed += ShowSettings;
            ownUserProfile.OnUpdate += OwnUserProfileUpdated;
            OwnUserProfileUpdated(ownUserProfile);
        }

        avatarHud?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureNotificationHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && notificationHud == null)
        {
            notificationHud = new NotificationHUDController();
        }

        notificationHud?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureAvatarEditorHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && avatarEditorHud == null)
        {
            avatarEditorHud = new AvatarEditorHUDController(ownUserProfile, wearableCatalog);
        }

        avatarEditorHud?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureSettingsHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && settingsHud == null)
        {
            settingsHud = new SettingsHUDController();
        }

        settingsHud?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureExpressionsHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && expressionsHud == null)
        {
            expressionsHud = new ExpressionsHUDController();
        }

        expressionsHud?.SetVisibility(configuration.active && configuration.visible);
    }

    public void TriggerSelfUserExpression(string id)
    {
        expressionsHud?.ExpressionCalled(id);
    }

    public void ConfigurePlayerInfoCardHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && playerInfoCardHudController == null)
        {
            playerInfoCardHudController = new PlayerInfoCardHUDController();
        }

        playerInfoCardHudController?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureWelcomeHUD(string configurationJson)
    {
        WelcomeHUDController.Model configuration = JsonUtility.FromJson<WelcomeHUDController.Model>(configurationJson);

        if (configuration.active && welcomeHudController == null)
        {
            welcomeHudController = new WelcomeHUDController();
            welcomeHudController.Initialize(configuration);
        }

        welcomeHudController?.SetVisibility(configuration.active && configuration.visible);
    }

    public void ConfigureAirdroppingHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        if (configuration.active && airdroppingHUDController == null)
        {
            airdroppingHUDController = new AirdroppingHUDController();
        }

        airdroppingHUDController?.SetVisibility(configuration.active && configuration.visible);
    }

    public void AirdroppingRequest(string payload)
    {
        var model = JsonUtility.FromJson<AirdroppingHUDController.Model>(payload);
        airdroppingHUDController.AirdroppingRequested(model);
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

        minimapHud?.Dispose();
        notificationHud?.Dispose();
        avatarEditorHud?.Dispose();
        expressionsHud?.Dispose();
        playerInfoCardHudController?.Dispose();
        welcomeHudController?.Dispose();
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
