using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.FatalErrorReporter;
using DCL.Interface;
using DCL.NotificationModel;
using GPUSkinning;
using UnityEngine;
using Type = DCL.NotificationModel.Type;

public class PlayerAvatarController : MonoBehaviour
{
    private const string LOADING_WEARABLES_ERROR_MESSAGE = "There was a problem loading your wearables";

    private AvatarSystem.Avatar avatar;
    private CancellationTokenSource avatarLoadingCts = null;
    public GameObject avatarContainer;
    private readonly AvatarModel currentAvatar = new AvatarModel { wearables = new List<string>() };

    public Collider avatarCollider;
    public AvatarVisibility avatarVisibility;
    public float cameraDistanceToDeactivate = 1.0f;

    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    private bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private bool enableCameraCheck = false;
    private Camera mainCamera;
    private PlayerAvatarAnalytics playerAvatarAnalytics;
    private IFatalErrorReporter fatalErrorReporter; // TODO?

    private void Start()
    {
        DataStore.i.common.isPlayerRendererLoaded.Set(false);
        playerAvatarAnalytics = new PlayerAvatarAnalytics(Analytics.i, CommonScriptableObjects.playerCoords);

        avatar = new AvatarSystem.Avatar(
            new AvatarCurator(new WearableItemResolver()),
            new Loader(new WearableLoaderFactory(), avatarContainer),
            GetComponentInChildren<AvatarAnimatorLegacy>(),
            new Visibility(),
            new NoLODs(),
            new SimpleGPUSkinning(),
            new GPUSkinningThrottler_New());

        if ( UserProfileController.i != null )
        {
            UserProfileController.i.OnBaseWereablesFail -= OnBaseWereablesFail;
            UserProfileController.i.OnBaseWereablesFail += OnBaseWereablesFail;
        }

        CommonScriptableObjects.rendererState.AddLock(this);

#if UNITY_WEBGL
        fatalErrorReporter = new WebFatalErrorReporter();
#else
        fatalErrorReporter = new DefaultFatalErrorReporter(DataStore.i);
#endif

        mainCamera = Camera.main;
    }

    private void OnBaseWereablesFail()
    {
        UserProfileController.i.OnBaseWereablesFail -= OnBaseWereablesFail;

        if (enableCameraCheck)
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

    public void SetAvatarVisibility(bool isVisible) { avatar.SetVisibility(isVisible); }

    private void OnEnable()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
        userProfile.OnAvatarExpressionSet += OnAvatarExpression;
    }

    private void OnAvatarExpression(string id, long timestamp)
    {
        avatar.SetExpression(id, timestamp);
        playerAvatarAnalytics.ReportExpression(id);
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        avatarLoadingCts?.Cancel();
        avatarLoadingCts?.Dispose();
        avatarLoadingCts = new CancellationTokenSource();
        LoadingRoutine(profile, avatarLoadingCts.Token);
    }

    private async UniTaskVoid LoadingRoutine(UserProfile profile, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (avatar.status != IAvatar.Status.Loaded || !profile.avatar.HaveSameWearablesAndColors(currentAvatar))
        {
            try
            {
                currentAvatar.CopyFrom(profile.avatar);

                var wearableItems = profile.avatar.wearables.ToList();
                wearableItems.Add(profile.avatar.bodyShape);
                await avatar.Load(wearableItems, new AvatarSettings
                {
                    bodyshapeId = profile.avatar.bodyShape,
                    eyesColor = profile.avatar.eyeColor,
                    skinColor = profile.avatar.skinColor,
                    hairColor = profile.avatar.hairColor,
                }, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                WebInterface.ReportAvatarFatalError();
                return;
            }
        }

        IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        anchorPoints.Prepare(avatarContainer.transform, avatar.GetBones(), avatar.extents.y);

        var player = new Player()
        {
            id = userProfile.userId,
            name = userProfile.name,
            avatar = avatar,
            anchorPoints = anchorPoints
        };
        DataStore.i.player.ownPlayer.Set(player);

        enableCameraCheck = true;
        avatarCollider.gameObject.SetActive(true);
        CommonScriptableObjects.rendererState.RemoveLock(this);
        DataStore.i.common.isPlayerRendererLoaded.Set(true);
    }

    private void OnDisable()
    {
        userProfile.OnUpdate -= OnUserProfileOnUpdate;
        userProfile.OnAvatarExpressionSet -= OnAvatarExpression;
    }

    private void OnDestroy()
    {
        avatarLoadingCts?.Cancel();
        avatarLoadingCts?.Dispose();
        avatarLoadingCts = null;
        avatar?.Dispose();
    }
}