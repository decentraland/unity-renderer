using System;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    private AvatarHUDController avatarHUD;
    private NotificationHUDController notificationHud;
    private MinimapHUDController minimapHUD;
    private UserProfile ownUserProfile;

    private void Awake()
    {
        avatarHUD = new AvatarHUDController(new AvatarHUDModel());
        minimapHUD = new MinimapHUDController(new MinimapHUDModel());
        notificationHud = new NotificationHUDController();

        ownUserProfile = UserProfile.GetOwnUserProfile();
        ownUserProfile.OnUpdate += OwnUserProfileUpdated;
        OwnUserProfileUpdated(ownUserProfile);
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

    private void UpdateAvatarHUD()
    {
        avatarHUD.UpdateData(new AvatarHUDModel()
        {
            name = ownUserProfile.userName,
            mail =  ownUserProfile.email,
            avatarPic = ownUserProfile.faceSnapshot
        });
    }

    private void OnDestroy()
    {
        ownUserProfile.OnUpdate -= OwnUserProfileUpdated;
        minimapHUD.Dispose();
    }
}