using System;
using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;
    private UserProfile userProfile => UserProfile.GetOwnUserProfile();

    private void Awake()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("MainCamera"))
            avatarRenderer.SetVisibility(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("MainCamera"))
            avatarRenderer.SetVisibility(true);
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        avatarRenderer.ApplyModel(profile.avatar, null, null);
    }
}
