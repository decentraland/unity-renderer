using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Emotes;
using DCL.FatalErrorReporter;
using DCL.Interface;
using DCL.NotificationModel;
using GPUSkinning;
using SocialFeaturesAnalytics;
using UnityEngine;
using Type = DCL.NotificationModel.Type;

public class PlayerAvatarController : MonoBehaviour, IHideAvatarAreaHandler
{
    private const string LOADING_WEARABLES_ERROR_MESSAGE = "There was a problem loading your wearables";
    private const string IN_HIDE_AREA = "IN_HIDE_AREA";
    private const string INSIDE_CAMERA = "INSIDE_CAMERA";

    private IAvatar avatar;
    private CancellationTokenSource avatarLoadingCts = null;
    public GameObject avatarContainer;
    public GameObject armatureContainer;
    public Transform loadingAvatarContainer;
    public StickersController stickersControllers;
    private readonly AvatarModel currentAvatar = new AvatarModel { wearables = new List<string>() };

    public Collider avatarCollider;
    [SerializeField] private GameObject loadingParticlesPrefab;
    public float cameraDistanceToDeactivate = 1.0f;

    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    private bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();

    private bool enableCameraCheck = false;
    private Camera mainCamera;
    private IFatalErrorReporter fatalErrorReporter; // TODO?
    private string VISIBILITY_CONSTRAIN;

    internal ISocialAnalytics socialAnalytics;

    private void Start()
    {
        DataStore.i.common.isPlayerRendererLoaded.Set(false);
        socialAnalytics = new SocialAnalytics(
            DCL.Environment.i.platform.serviceProviders.analytics,
            new UserProfileWebInterfaceBridge());

        if (DataStore.i.avatarConfig.useHologramAvatar.Get())
            avatar = GetAvatarWithHologram();
        else
            avatar = GetStandardAvatar();

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

    private AvatarSystem.Avatar GetStandardAvatar()
    {
        AvatarAnimatorLegacy animator = GetComponentInChildren<AvatarAnimatorLegacy>();
        AvatarSystem.NoLODs noLod = new NoLODs();
        return new AvatarSystem.Avatar(
            new AvatarCurator(new WearableItemResolver()),
            new Loader(new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper()),
            animator,
            new Visibility(),
            noLod,
            new SimpleGPUSkinning(),
            new GPUSkinningThrottler(),
            new EmoteAnimationEquipper(animator, DataStore.i.emotes));
    }

    private AvatarWithHologram GetAvatarWithHologram()
    {
        AvatarAnimatorLegacy animator = GetComponentInChildren<AvatarAnimatorLegacy>();
        AvatarSystem.NoLODs noLod = new NoLODs();
        BaseAvatar baseAvatar = new BaseAvatar(loadingAvatarContainer, armatureContainer, noLod);
        return new AvatarSystem.AvatarWithHologram(
            baseAvatar,
            new AvatarCurator(new WearableItemResolver()),
            new Loader(new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper()),
            animator,
            new Visibility(),
            noLod,
            new SimpleGPUSkinning(),
            new GPUSkinningThrottler(),
            new EmoteAnimationEquipper(animator, DataStore.i.emotes));
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

        if (Vector3.Distance(mainCamera.transform.position, transform.position) > cameraDistanceToDeactivate)
            avatar.RemoveVisibilityConstrain(INSIDE_CAMERA);
        else
            avatar.AddVisibilityConstrain(INSIDE_CAMERA);
    }

    public void SetAvatarVisibility(bool isVisible)
    {
        VISIBILITY_CONSTRAIN = "own_player_invisible";
        if (isVisible)
            avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAIN);
        else
            avatar.AddVisibilityConstrain(VISIBILITY_CONSTRAIN);
    }

    private void OnEnable()
    {
        // userProfile.OnUpdate += OnUserProfileOnUpdate;
        userProfile.OnAvatarEmoteSet += OnAvatarEmote;
    }

    private void OnAvatarEmote(string id, long timestamp, UserProfile.EmoteSource source)
    {
        avatar.PlayEmote(id, timestamp);

        DataStore.i.common.wearables.TryGetValue(id, out WearableItem emoteItem);

        if (emoteItem != null)
        {
            socialAnalytics.SendPlayEmote(
                emoteItem.id,
                emoteItem.GetName(),
                emoteItem.rarity,
                emoteItem.data.tags.Contains(WearableLiterals.Tags.BASE_WEARABLE),
                source,
                $"{CommonScriptableObjects.playerCoords.Get().x},{CommonScriptableObjects.playerCoords.Get().y}");
        }
    }

    private void OnUserProfileOnUpdate(UserProfile profile)
    {
        avatarLoadingCts?.Cancel();
        avatarLoadingCts?.Dispose();
        avatarLoadingCts = new CancellationTokenSource();
        LoadingAvatarRoutine(profile, avatarLoadingCts.Token);
    }

    private async UniTaskVoid LoadingAvatarRoutine(UserProfile profile, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(profile.avatar.bodyShape) || profile.avatar.wearables == null)
        {
            avatar.Dispose();
            return;
        }

        try
        {
            ct.ThrowIfCancellationRequested();
            if (avatar.status != IAvatar.Status.Loaded || !profile.avatar.HaveSameWearablesAndColors(currentAvatar))
            {
                currentAvatar.CopyFrom(profile.avatar);

                List<string> wearableItems = profile.avatar.wearables.ToList();
                wearableItems.Add(profile.avatar.bodyShape);

                //temporarily hardcoding the embedded emotes until the user profile provides the equipped ones
                var embeddedEmotesSo = Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes");
                wearableItems.AddRange(embeddedEmotesSo.emotes.Select(x => x.id));

                await avatar.Load(wearableItems, new AvatarSettings
                {
                    bodyshapeId = profile.avatar.bodyShape,
                    eyesColor = profile.avatar.eyeColor,
                    skinColor = profile.avatar.skinColor,
                    hairColor = profile.avatar.hairColor,
                }, ct);

                if (avatar.lodLevel <= 1)
                    AvatarSystemUtils.SpawnAvatarLoadedParticles(avatarContainer.transform, loadingParticlesPrefab);
            }
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

        IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        anchorPoints.Prepare(avatarContainer.transform, avatar.GetBones(), AvatarSystemUtils.AVATAR_Y_OFFSET + avatar.extents.y);

        var player = new Player()
        {
            id = userProfile.userId,
            name = userProfile.name,
            avatar = avatar,
            anchorPoints = anchorPoints,
            collider = avatarCollider
        };
        DataStore.i.player.ownPlayer.Set(player);

        enableCameraCheck = true;
        avatarCollider.gameObject.SetActive(true);
        CommonScriptableObjects.rendererState.RemoveLock(this);
        DataStore.i.common.isPlayerRendererLoaded.Set(true);
    }

    public void ApplyHideModifier()
    {
        avatar.AddVisibilityConstrain(IN_HIDE_AREA);
        stickersControllers.ToggleHideArea(true);
    }
    
    public void RemoveHideModifier()
    {
        avatar.RemoveVisibilityConstrain(IN_HIDE_AREA);
        stickersControllers.ToggleHideArea(false);
    }

    private void OnDisable()
    {
        userProfile.OnUpdate -= OnUserProfileOnUpdate;
        userProfile.OnAvatarEmoteSet -= OnAvatarEmote;
    }

    private void OnDestroy()
    {
        avatarLoadingCts?.Cancel();
        avatarLoadingCts?.Dispose();
        avatarLoadingCts = null;
        avatar?.Dispose();
    }
}