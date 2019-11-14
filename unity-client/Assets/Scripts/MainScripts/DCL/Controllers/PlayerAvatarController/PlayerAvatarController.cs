using System;
using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;
    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    [SerializeField] CameraStateSO cameraState;

    private void Awake()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
        OnCameraStateOnChange(cameraState, cameraState);
        cameraState.OnChange += OnCameraStateOnChange;
    }

    private void OnCameraStateOnChange(CameraController.CameraState current, CameraController.CameraState previous)
    {
        switch (current)
        {
            case CameraController.CameraState.ThirdPerson:
                avatarRenderer.SetVisibility(true);
                break;
            case CameraController.CameraState.FirstPerson:
            default:
                avatarRenderer.SetVisibility(false);
                break;
        }
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        OverrideModel(profile.avatar, null);
    }

    public void OverrideModel(AvatarModel model, Action onReady)
    {
        avatarRenderer.ApplyModel(model, onReady, null);
    }
}
