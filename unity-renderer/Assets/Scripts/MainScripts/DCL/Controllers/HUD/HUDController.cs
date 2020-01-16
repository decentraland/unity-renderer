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
    }
}
