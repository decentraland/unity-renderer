using System.Collections;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    private AvatarHUDController avatarHudValue;

    private AvatarHUDController avatarHud
    {
        get
        {
            if (avatarHudValue == null)
            {
                avatarHudValue = new AvatarHUDController(new AvatarHUDModel());
            }

            return avatarHudValue;
        }
    }

    private NotificationHUDController notificationHudValue;

    private NotificationHUDController notificationHud
    {
        get
        {
            if (notificationHudValue == null)
            {
                notificationHudValue = new NotificationHUDController();
            }

            return notificationHudValue;
        }
    }

    private MinimapHUDController minimapHudValue;

    private MinimapHUDController minimapHud
    {
        get
        {
            if (minimapHudValue == null)
            {
                minimapHudValue = new MinimapHUDController(new MinimapHUDModel());
            }

            return minimapHudValue;
        }
    }

    private AvatarEditorHUDController avatarEditorHudValue;

    private AvatarEditorHUDController avatarEditorHud
    {
        get
        {
            if (avatarEditorHudValue == null)
            {
                avatarEditorHudValue = new AvatarEditorHUDController(ownUserProfile, wearableCatalog);
            }

            return avatarEditorHudValue;
        }
    }

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private WearableDictionary wearableCatalog => CatalogController.wearableCatalog;

    private void Awake()
    {
        HUDConfiguration defaultConfiguration = new HUDConfiguration()
        {
            active = false
        };

        avatarHud.SetConfiguration(defaultConfiguration);
        minimapHud.SetConfiguration(defaultConfiguration);
        notificationHud.SetConfiguration(defaultConfiguration);
        avatarEditorHud.SetConfiguration(defaultConfiguration);

        avatarHud.OnEditAvatarPressed += ShowAvatarEditor;
        ownUserProfile.OnUpdate += OwnUserProfileUpdated;
        OwnUserProfileUpdated(ownUserProfile);
    }

    private void ShowAvatarEditor()
    {
        avatarEditorHud.SetConfiguration(new HUDConfiguration()
        {
            active = true
        });
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

    public void ConfigureMinimapHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        minimapHud.SetConfiguration(configuration);
    }

    public void ConfigureAvatarHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        avatarHud.SetConfiguration(configuration);
    }

    public void ConfigureNotificationHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        notificationHud.SetConfiguration(configuration);
    }

    public void ConfigureAvatarEditorHUD(string configurationJson)
    {
        HUDConfiguration configuration = JsonUtility.FromJson<HUDConfiguration>(configurationJson);
        avatarEditorHud.SetConfiguration(configuration);
    }

    private void UpdateAvatarHUD()
    {
        avatarHud.UpdateData(new AvatarHUDModel()
        {
            name = ownUserProfile.userName,
            mail =  ownUserProfile.email,
            avatarPic = ownUserProfile.faceSnapshot
        });
    }

    private void OnDestroy()
    {
        ownUserProfile.OnUpdate -= OwnUserProfileUpdated;
        minimapHud.Dispose();
    }
}