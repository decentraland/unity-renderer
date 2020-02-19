using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;
    public float cameraDistanceToDeactivate = 1.0f;

    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    private bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private bool enableCameraCheck = false;
    private Camera mainCamera;

    private void Awake()
    {
        //NOTE(Brian): We must wait for loading to finish before deactivating the renderer, or the GLTF Loader won't finish.
        avatarRenderer.OnSuccessEvent -= Init;
        avatarRenderer.OnFailEvent -= Init;
        avatarRenderer.OnSuccessEvent += Init;
        avatarRenderer.OnFailEvent += Init;

        mainCamera = Camera.main;
    }

    private void Init()
    {
        enableCameraCheck = true;
        avatarRenderer.OnSuccessEvent -= Init;
        avatarRenderer.OnFailEvent -= Init;
    }

    private void Update()
    {
        if (!enableCameraCheck || repositioningWorld)
            return;

        bool shouldBeVisible = Vector3.Distance(mainCamera.transform.position, transform.position) > cameraDistanceToDeactivate;

        if (shouldBeVisible != avatarRenderer.gameObject.activeSelf)
            avatarRenderer.SetVisibility(shouldBeVisible);
    }
    private void OnEnable()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
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
