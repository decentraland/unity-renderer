using System;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    private AvatarHUDController avatarHUD;
    private UserProfile ownUserProfile;

    private void Awake()
    {
        avatarHUD = new AvatarHUDController(new AvatarHUDModel());

        ownUserProfile = UserProfile.GetOwnUserProfile();
        ownUserProfile.OnUpdate += OwnUserProfileUpdated;
        OwnUserProfileUpdated(ownUserProfile);
    }

    private void OwnUserProfileUpdated(UserProfile profile)
    {
        UpdateAvatarHUD();
    }

    private void UpdateAvatarHUD()
    {
        avatarHUD.UpdateData(new AvatarHUDModel()
        {
            name = ownUserProfile.userName,
            mail =  ownUserProfile.mail,
            avatarPic = ownUserProfile.avatarPic
        });
    }

    private void OnDestroy()
    {
        ownUserProfile.OnUpdate -= OwnUserProfileUpdated;
    }
}