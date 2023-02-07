using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Components;
using DCL.Emotes;
using DCL.FatalErrorReporter;
using DCL.Interface;
using DCL.Models;
using DCL.NotificationModel;
using GPUSkinning;
using SocialFeaturesAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Environment = DCL.Environment;
using Type = DCL.NotificationModel.Type;

public class PlayerAvatarController : MonoBehaviour, IHideAvatarAreaHandler, IHidePassportAreaHandler
{
    private const string LOADING_WEARABLES_ERROR_MESSAGE = "There was a problem loading your wearables";
    private const string IN_HIDE_AREA = "IN_HIDE_AREA";
    private const string INSIDE_CAMERA = "INSIDE_CAMERA";

    private CancellationTokenSource avatarLoadingCts;
    public GameObject avatarContainer;
    public GameObject armatureContainer;
    public Transform loadingAvatarContainer;
    public StickersController stickersControllers;
    private readonly AvatarModel currentAvatar = new AvatarModel { wearables = new List<string>() };

    public Collider avatarCollider;
    [SerializeField] private GameObject loadingParticlesPrefab;
    public float cameraDistanceToDeactivate = 1.0f;
    [SerializeField] internal AvatarOnPointerDown onPointerDown;

    private UserProfile userProfile => UserProfile.GetOwnUserProfile();
    private bool repositioningWorld => DCLCharacterController.i.characterPosition.RepositionedWorldLastFrame();
    private bool enableCameraCheck;
    private Camera mainCamera;
    private IFatalErrorReporter fatalErrorReporter; // TODO?
    private string visibilityConstrain;
    private BaseRefCounter<AvatarModifierAreaID> currentActiveModifiers;
    private ISocialAnalytics socialAnalytics;
    private StringVariable currentPlayerInfoCardId;
    private IAvatar avatar;

    private void Start()
    {
        DataStore.i.common.isPlayerRendererLoaded.Set(false);

        socialAnalytics = new SocialAnalytics(
            Environment.i.platform.serviceProviders.analytics,
            new UserProfileWebInterfaceBridge());

        if (DataStore.i.avatarConfig.useHologramAvatar.Get())
            avatar = GetAvatarWithHologram();
        else
            avatar = GetStandardAvatar();

        if (UserProfileController.i != null)
        {
            UserProfileController.i.OnBaseWereablesFail -= OnBaseWereablesFail;
            UserProfileController.i.OnBaseWereablesFail += OnBaseWereablesFail;
        }

        CommonScriptableObjects.rendererState.AddLock(this);

        mainCamera = Camera.main;
        currentActiveModifiers = new BaseRefCounter<AvatarModifierAreaID>();
        currentPlayerInfoCardId = Resources.Load<StringVariable>("CurrentPlayerInfoCardId");

        onPointerDown.OnPointerDownReport -= PlayerClicked;
        onPointerDown.OnPointerDownReport += PlayerClicked;
    }

    private void PlayerClicked()
    {
        if (currentAvatar == null) return;
        currentPlayerInfoCardId.Set(currentAvatar.id);
    }

    private IAvatar GetStandardAvatar() =>
        Environment.i.serviceLocator.Get<IAvatarFactory>()
                   .CreateAvatar(avatarContainer, GetComponentInChildren<AvatarAnimatorLegacy>(), NoLODs.i, new Visibility());

    private IAvatar GetAvatarWithHologram()
    {
        AvatarAnimatorLegacy animator = GetComponentInChildren<AvatarAnimatorLegacy>();
        NoLODs noLod = new NoLODs();
        BaseAvatar baseAvatar = new BaseAvatar(loadingAvatarContainer, armatureContainer, noLod);

        return new AvatarWithHologram(
            baseAvatar,
            new AvatarCurator(new WearableItemResolver(), Environment.i.serviceLocator.Get<IEmotesCatalogService>()),
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
            avatar.AddVisibilityConstraint(INSIDE_CAMERA);
    }

    public void SetAvatarVisibility(bool isVisible)
    {
        visibilityConstrain = "own_player_invisible";

        if (isVisible)
            avatar.RemoveVisibilityConstrain(visibilityConstrain);
        else
            avatar.AddVisibilityConstraint(visibilityConstrain);
    }

    private void OnEnable()
    {
        userProfile.OnUpdate += OnUserProfileOnUpdate;
        userProfile.OnAvatarEmoteSet += OnAvatarEmote;
    }

    private void OnAvatarEmote(string id, long timestamp, UserProfile.EmoteSource source)
    {
        avatar.PlayEmote(id, timestamp);

        bool found = DataStore.i.common.wearables.TryGetValue(id, out WearableItem emoteItem);

        if (!found)
        {
            var emotesCatalog = Environment.i.serviceLocator.Get<IEmotesCatalogService>();
            emotesCatalog.TryGetLoadedEmote(id, out emoteItem);
        }

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
                currentAvatar.id = profile.userId;

                List<string> wearableItems = profile.avatar.wearables.ToList();
                wearableItems.Add(profile.avatar.bodyShape);

                HashSet<string> emotes = new HashSet<string>(currentAvatar.emotes.Select(x => x.urn));
                var embeddedEmotesSo = Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes");
                emotes.UnionWith(embeddedEmotesSo.emotes.Select(x => x.id));
                wearableItems.AddRange(embeddedEmotesSo.emotes.Select(x => x.id));

                await avatar.Load(wearableItems, emotes.ToList(), new AvatarSettings
                {
                    bodyshapeId = profile.avatar.bodyShape,
                    eyesColor = profile.avatar.eyeColor,
                    skinColor = profile.avatar.skinColor,
                    hairColor = profile.avatar.hairColor,
                }, ct);

                if (avatar.lodLevel <= 1)
                    AvatarSystemUtils.SpawnAvatarLoadedParticles(avatarContainer.transform, loadingParticlesPrefab);

                avatar.PlayEmote(profile.avatar.expressionTriggerId, profile.avatar.expressionTriggerTimestamp);
            }
        }
        catch (OperationCanceledException ex)
        {
            Debug.LogException(ex);
            return;
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            //WebInterface.ReportAvatarFatalError(e.ToString());
            return;
        }
        finally
        {
            IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
            anchorPoints.Prepare(avatarContainer.transform, avatar.GetBones(), AvatarSystemUtils.AVATAR_Y_OFFSET + avatar.extents.y);

            var player = new Player
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

            onPointerDown.Initialize(
                new OnPointerDown.Model
                {
                    type = OnPointerDown.NAME,
                    button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                    hoverText = "View Profile",
                },
                new DecentralandEntity
                {
                    gameObject = gameObject,
                },
                player
            );
            onPointerDown.SetPassportEnabled(true);
        }
    }

    public void ApplyHideAvatarModifier()
    {
        if (!currentActiveModifiers.ContainsKey(AvatarModifierAreaID.HIDE_AVATAR))
        {
            avatar.AddVisibilityConstraint(IN_HIDE_AREA);
            stickersControllers.ToggleHideArea(true);
        }

        currentActiveModifiers.AddRefCount(AvatarModifierAreaID.HIDE_AVATAR);
        DataStore.i.HUDs.avatarAreaWarnings.AddRefCount(AvatarModifierAreaID.HIDE_AVATAR);
    }

    public void RemoveHideAvatarModifier()
    {
        DataStore.i.HUDs.avatarAreaWarnings.RemoveRefCount(AvatarModifierAreaID.HIDE_AVATAR);
        currentActiveModifiers.RemoveRefCount(AvatarModifierAreaID.HIDE_AVATAR);

        if (!currentActiveModifiers.ContainsKey(AvatarModifierAreaID.HIDE_AVATAR))
        {
            avatar.RemoveVisibilityConstrain(IN_HIDE_AREA);
            stickersControllers.ToggleHideArea(false);
        }
    }

    public void ApplyHidePassportModifier()
    {
        DataStore.i.HUDs.avatarAreaWarnings.AddRefCount(AvatarModifierAreaID.DISABLE_PASSPORT);
        currentActiveModifiers.AddRefCount(AvatarModifierAreaID.DISABLE_PASSPORT);
    }

    public void RemoveHidePassportModifier()
    {
        DataStore.i.HUDs.avatarAreaWarnings.RemoveRefCount(AvatarModifierAreaID.DISABLE_PASSPORT);
        currentActiveModifiers.RemoveRefCount(AvatarModifierAreaID.DISABLE_PASSPORT);
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
