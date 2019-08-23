using UnityEngine;

public class HUDController : MonoBehaviour
{
    private AvatarHUDController avatarHUD;
    private MinimapHUDController minimapHUD;
    private UserProfile ownUserProfile;

    private void Awake()
    {
        avatarHUD = new AvatarHUDController(new AvatarHUDModel());
        minimapHUD = new MinimapHUDController(new MinimapHUDModel());

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