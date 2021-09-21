using UnityEngine;

[RequireComponent(typeof(ProfileCardComponentView))]
public class ProfileInfoLoader : MonoBehaviour
{
    [SerializeField] internal bool openProfileHudPanelOnClick = true;

    private ProfileCardComponentView profileCard;

    private UserProfile ownUserProfile => UserProfile.GetOwnUserProfile();

    private void Start()
    {
        profileCard = GetComponent<ProfileCardComponentView>();

        if (ownUserProfile.userId == null)
            ownUserProfile.OnUpdate += OnProfileUpdated;
        else
            OnProfileUpdated(ownUserProfile);
    }

    private void OnDestroy()
    {
        ownUserProfile.OnUpdate -= OnProfileUpdated;

        if (profileCard != null)
            profileCard.onClick.RemoveAllListeners();
    }

    private void OnProfileUpdated(UserProfile profile)
    {
        profileCard.SetProfileName(profile.userName);
        profileCard.SetProfileAddress(profile.ethAddress);
        profileCard.SetLoadingIndicatorVisible(true);
        profile.snapshotObserver.AddListener(SetProfileImage);

        if (openProfileHudPanelOnClick)
            profileCard.onClick.AddListener(() => HUDController.i?.profileHud?.ToggleMenu());
    }

    private void SetProfileImage(Texture2D texture)
    {
        ownUserProfile.snapshotObserver.RemoveListener(SetProfileImage);
        profileCard.SetLoadingIndicatorVisible(false);
        profileCard.SetProfilePicture(texture);
    }
}