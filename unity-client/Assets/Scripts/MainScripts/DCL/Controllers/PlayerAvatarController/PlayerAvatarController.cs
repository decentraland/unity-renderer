using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;

    UserProfile userProfile => UserProfile.GetOwnUserProfile();
    bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private void Awake()
    {
        //NOTE(Brian): We must wait for loading to finish before deactivating the renderer, or the GLTF Loader won't finish.
        avatarRenderer.OnSuccessCallback -= Init;
        avatarRenderer.OnFailCallback -= Init;
        avatarRenderer.OnSuccessCallback += Init;
        avatarRenderer.OnFailCallback += Init;
    }

    private void Init()
    {
        avatarRenderer.SetVisibility(false);
        avatarRenderer.OnSuccessCallback -= Init;
        avatarRenderer.OnFailCallback -= Init;
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
