using System;
using System.Collections;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    private static HUDController instance;

    public static HUDController i
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<HUDController>();

                if (instance == null)
                {
                    GameObject instanceContainer = new GameObject("HUDController");
                    instance = instanceContainer.AddComponent<HUDController>();
                }
            }

            return instance;
        }
    }

    public AvatarHUDController avatarHud { get; private set; }
    public NotificationHUDController notificationHud { get; private set; }
    public MinimapHUDController minimapHud { get; private set; }
    public AvatarEditorHUDController avatarEditorHud { get; private set; }

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();
    private WearableDictionary wearableCatalog => CatalogController.wearableCatalog;

    private void ShowAvatarEditor()
    {
        avatarEditorHud?.SetVisibility(true);
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

    private void UpdateAvatarHUD()
    {
        avatarHud?.UpdateData(new AvatarHUDModel()
        {
            name = ownUserProfile.userName,
            mail =  ownUserProfile.email,
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
        }

        minimapHud?.Dispose();
        notificationHud?.Dispose();
        avatarEditorHud?.Dispose();
    }
}
