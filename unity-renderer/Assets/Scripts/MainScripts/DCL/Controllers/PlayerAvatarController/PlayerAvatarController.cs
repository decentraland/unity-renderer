using System.Runtime.CompilerServices;
using DCL;
using UnityEngine;

public class PlayerAvatarController : MonoBehaviour
{
    public AvatarRenderer avatarRenderer;
    public Collider avatarCollider;
    public AvatarVisibility avatarVisibility;
    public float cameraDistanceToDeactivate = 1.0f;

    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    private bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private bool enableCameraCheck = false;
    private Camera mainCamera;

    private void Start()
    {
        //NOTE(Brian): We must wait for loading to finish before deactivating the renderer, or the GLTF Loader won't finish.
        avatarRenderer.OnSuccessEvent -= OnAvatarRendererReady;
        avatarRenderer.OnFailEvent -= OnAvatarRendererReady;
        avatarRenderer.OnSuccessEvent += OnAvatarRendererReady;
        avatarRenderer.OnFailEvent += OnAvatarRendererReady;
        CommonScriptableObjects.rendererState.AddLock(this);

        mainCamera = Camera.main;
    }

    private void OnAvatarRendererReady()
    {
        enableCameraCheck = true;
        avatarCollider.gameObject.SetActive(true);
        CommonScriptableObjects.rendererState.RemoveLock(this);
        avatarRenderer.OnSuccessEvent -= OnAvatarRendererReady;
        avatarRenderer.OnFailEvent -= OnAvatarRendererReady;
    }

    private void Update()
    {
        if (!enableCameraCheck || repositioningWorld)
            return;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
                return;
        }

        bool shouldBeVisible = Vector3.Distance(mainCamera.transform.position, transform.position) > cameraDistanceToDeactivate;
        avatarVisibility.SetVisibility("PLAYER_AVATAR_CONTROLLER", shouldBeVisible);
    }

    private void OnEnable()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
        userProfile.OnAvatarExpressionSet += OnAvatarExpression;
    }

    private void OnAvatarExpression(string id, long timestamp)
    {
        avatarRenderer.UpdateExpressions(id, timestamp);
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        avatarRenderer.ApplyModel(profile.avatar, null, null);
    }

    private void OnDisable()
    {
        userProfile.OnUpdate -= OnUserProfileOnUpdate;
        userProfile.OnAvatarExpressionSet -= OnAvatarExpression;
    }
}