using System;
using DCL;
using DCL.Interface;
using DCL.FatalErrorReporter;
using DCL.NotificationModel;
using UnityEngine;
using Type = DCL.NotificationModel.Type;

public class PlayerAvatarController : MonoBehaviour
{
    private const string LOADING_WEARABLES_ERROR_MESSAGE = "There was a problem loading your wearables";

    public AvatarRenderer avatarRenderer;
    public Collider avatarCollider;
    public AvatarVisibility avatarVisibility;
    public float cameraDistanceToDeactivate = 1.0f;

    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    private bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private bool enableCameraCheck = false;
    private Camera mainCamera;
    private bool avatarWereablesErrors = false;
    private bool baseWereablesErrors = false;
    private PlayerAvatarAnalytics playerAvatarAnalytics;
    private IFatalErrorReporter fatalErrorReporter;

    private void Start()
    {
        DataStore.i.common.isPlayerRendererLoaded.Set(false);
        IAnalytics analytics = DCL.Environment.i.platform.serviceProviders.analytics;
        playerAvatarAnalytics = new PlayerAvatarAnalytics(analytics, CommonScriptableObjects.playerCoords);

        //NOTE(Brian): We must wait for loading to finish before deactivating the renderer, or the GLTF Loader won't finish.
        avatarRenderer.OnSuccessEvent -= OnAvatarRendererReady;
        avatarRenderer.OnFailEvent -= OnAvatarRendererFail;
        avatarRenderer.OnSuccessEvent += OnAvatarRendererReady;
        avatarRenderer.OnFailEvent += OnAvatarRendererFail;

        if ( UserProfileController.i != null )
        {
            UserProfileController.i.OnBaseWereablesFail -= OnBaseWereablesFail;
            UserProfileController.i.OnBaseWereablesFail += OnBaseWereablesFail;
        }

        DataStore.i.player.playerCollider.Set(avatarCollider);
        CommonScriptableObjects.rendererState.AddLock(this);

#if UNITY_WEBGL
        fatalErrorReporter = new WebFatalErrorReporter();
#else
        fatalErrorReporter = new DefaultFatalErrorReporter(DataStore.i);
#endif

        mainCamera = Camera.main;
    }

    private void OnAvatarRendererReady()
    {
        enableCameraCheck = true;
        avatarCollider.gameObject.SetActive(true);
        CommonScriptableObjects.rendererState.RemoveLock(this);
        avatarRenderer.OnSuccessEvent -= OnAvatarRendererReady;
        avatarRenderer.OnFailEvent -= OnAvatarRendererFail;
        DataStore.i.common.isPlayerRendererLoaded.Set(true);

        IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        anchorPoints.Prepare(avatarRenderer.transform, avatarRenderer.GetBones(), avatarRenderer.maxY);

        var player = new Player()
        {
            id = userProfile.userId,
            name = userProfile.name,
            renderer = avatarRenderer,
            anchorPoints = anchorPoints
        };
        DataStore.i.player.ownPlayer.Set(player);

        if (avatarWereablesErrors || baseWereablesErrors)
            ShowWearablesWarning();
    }

    private void OnAvatarRendererFail(Exception exception)
    {
        avatarWereablesErrors = true;

        if (exception is AvatarLoadFatalException)
            fatalErrorReporter.Report(exception);
        else
            OnAvatarRendererReady();
    }

    private void OnBaseWereablesFail()
    {
        UserProfileController.i.OnBaseWereablesFail -= OnBaseWereablesFail;
        baseWereablesErrors = true;

        if (enableCameraCheck && !avatarWereablesErrors)
            ShowWearablesWarning();
    }

    private void ShowWearablesWarning()
    {
        NotificationsController.i.ShowNotification(new Model
        {
            message = LOADING_WEARABLES_ERROR_MESSAGE,
            type = Type.GENERIC,
            timer = 10f,
            destroyOnFinish = true
        });
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

    public void SetAvatarVisibility(bool isVisible) { avatarRenderer.SetGOVisibility(isVisible); }

    private void OnEnable()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
        userProfile.OnAvatarExpressionSet += OnAvatarExpression;
    }

    private void OnAvatarExpression(string id, long timestamp)
    {
        avatarRenderer.SetExpression(id, timestamp);
        playerAvatarAnalytics.ReportExpression(id);
    }

    private void OnUserProfileOnUpdate(UserProfile profile) { avatarRenderer.ApplyModel(profile.avatar, null, null); }

    private void OnDisable()
    {
        userProfile.OnUpdate -= OnUserProfileOnUpdate;
        userProfile.OnAvatarExpressionSet -= OnAvatarExpression;
    }
}