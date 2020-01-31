using System;
using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;

    UserProfile userProfile => UserProfile.GetOwnUserProfile();
    bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private void Awake()
    {
        avatarRenderer.SetVisibility(false);
    }

    private void OnEnable()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!repositioningWorld && other.CompareTag("MainCamera"))
            avatarRenderer.SetVisibility(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!repositioningWorld && other.CompareTag("MainCamera"))
            avatarRenderer.SetVisibility(true);
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        avatarRenderer.ApplyModel(profile.avatar, null, null);
    }

    private void OnDisable()
    {
        userProfile.OnUpdate -= OnUserProfileOnUpdate;
    }
}
