using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using UnityEngine.Serialization;
using LOD = AvatarSystem.LOD;

namespace DCL
{
    public class AvatarShape : BaseComponent, IHideAvatarAreaHandler, IHidePassportAreaHandler, IOutOfSceneBoundariesHandler
    {
        private const float MINIMUM_PLAYERNAME_HEIGHT = 2.7f;
        private const string VISIBILITY_CONSTRAINT_HIDE_AREA = "IN_HIDE_AREA";
        private const string VISIBILITY_CONSTRAINT_OUTSIDE_SCENE_BOUNDS = "OUTSIDE_SCENE_BOUNDS";
        private const string OPEN_PASSPORT_SOURCE = "World";

        public static event Action<IDCLEntity, AvatarShape> OnAvatarShapeUpdated;

        public GameObject avatarContainer;
        public Collider avatarCollider;
        public AvatarMovementController avatarMovementController;
        public StickersController stickersControllers;

        [SerializeField] internal AvatarOnPointerDown onPointerDown;
        [SerializeField] internal AvatarOutlineOnHoverEvent outlineOnHover;
        [SerializeField] internal GameObject playerNameContainer;
        [SerializeField] private Transform baseAvatarContainer;
        [SerializeField] internal BaseAvatarReferences baseAvatarReferencesPrefab;
        internal IPlayerName playerName;
        internal IAvatarReporterController avatarReporterController;

        private BaseVariable<(string playerId, string source)> currentPlayerInfoCardId;

        public bool everythingIsLoaded;

        bool initializedPosition = false;

        private Player player = null;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        private IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        internal IAvatar avatar;
        private readonly AvatarModel currentAvatar = new AvatarModel { wearables = new List<string>() };
        private CancellationTokenSource loadingCts;
        private ILazyTextureObserver currentLazyObserver;
        private bool isGlobalSceneAvatar = true;
        private BaseRefCounter<AvatarModifierAreaID> currentActiveModifiers;
        private IUserProfileBridge userProfileBridge;
        private Service<IEmotesCatalogService> emotesCatalogService;
        public override string componentName => "avatarShape";

        private void Awake()
        {
            model = new AvatarModel();
            currentPlayerInfoCardId = DataStore.i.HUDs.currentPlayerId;
            // TODO: user profile bridge should be retrieved from the service locator
            userProfileBridge = new UserProfileWebInterfaceBridge();

            if (DataStore.i.avatarConfig.useHologramAvatar.Get())
                avatar = GetAvatarWithHologram();
            else
                avatar = GetStandardAvatar();

            if (avatarReporterController == null)
            {
                avatarReporterController = new AvatarReporterController(Environment.i.world.state);
            }
        }

        public override void Initialize(IParcelScene scene, IDCLEntity entity)
        {
            base.Initialize(scene, entity);
            DataStore.i.sceneBoundariesChecker?.Add(entity,this);
        }

        private IAvatar GetStandardAvatar()
        {
            var visibility = new Visibility();

            return Environment.i.serviceLocator.Get<IAvatarFactory>()
                              .CreateAvatar(
                                   avatarContainer,
                                   GetComponentInChildren<AvatarAnimatorLegacy>(),
                                   new LOD(avatarContainer, visibility, avatarMovementController),
                                   visibility
                               );
        }

        private IAvatar GetAvatarWithHologram()
        {
            Visibility visibility = new Visibility();

            // Due to how we set our pools (and how the objets are cloned in), we might find that the original item already had the baseAvatar when returned to the pool.
            var baseAvatarReferences = baseAvatarContainer.GetComponentInChildren<IBaseAvatarReferences>() ?? Instantiate(baseAvatarReferencesPrefab, baseAvatarContainer);

            return Environment.i.serviceLocator.Get<IAvatarFactory>()
                              .CreateAvatarWithHologram(
                                   avatarContainer,
                                   new BaseAvatar(baseAvatarReferences),
                                   GetComponentInChildren<AvatarAnimatorLegacy>(),
                                   new LOD(avatarContainer, visibility, avatarMovementController),
                                   visibility
                               );
        }

        private void Start()
        {
            playerName = GetComponentInChildren<IPlayerName>();
            playerName?.Hide(true);
            currentActiveModifiers ??= new BaseRefCounter<AvatarModifierAreaID>();
        }

        private void PlayerClicked()
        {
            if (model == null)
                return;

            currentPlayerInfoCardId.Set((((AvatarModel)model).id, OPEN_PASSPORT_SOURCE));
        }

        public void OnDestroy()
        {
            if(entity != null)
                DataStore.i.sceneBoundariesChecker?.Remove(entity,this);

            Cleanup();

            if (poolableObject != null && poolableObject.isInsidePool)
                poolableObject.RemoveFromPool();
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            isGlobalSceneAvatar = scene.sceneData.sceneNumber == EnvironmentSettings.AVATAR_GLOBAL_SCENE_NUMBER;

            var model = (AvatarModel) newModel;
            bool needsLoading = !model.HaveSameWearablesAndColors(currentAvatar);
            currentAvatar.CopyFrom(model);

            if (string.IsNullOrEmpty(model.bodyShape) || model.wearables.Count == 0)
                yield break;
#if UNITY_EDITOR
            gameObject.name = $"Avatar Shape {model.name}";
#endif
            everythingIsLoaded = false;

            bool avatarDone = false;
            bool avatarFailed = false;

            yield return null; //NOTE(Brian): just in case we have a Object.Destroy waiting to be resolved.

            // To deal with the cases in which the entity transform was configured before the AvatarShape
            if (!initializedPosition && scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.TRANSFORM))
            {
                initializedPosition = true;
                OnEntityTransformChanged(entity.gameObject.transform.localPosition,
                    entity.gameObject.transform.localRotation, true);
            }

            // NOTE: we subscribe here to transform changes since we might "lose" the message
            // if we subscribe after a any yield
            entity.OnTransformChange -= OnEntityTransformChanged;
            entity.OnTransformChange += OnEntityTransformChanged;

            var wearableItems = model.wearables.ToList();
            wearableItems.Add(model.bodyShape);

            if (avatar.status != IAvatar.Status.Loaded || needsLoading)
            {
                HashSet<string> emotes = new HashSet<string>(currentAvatar.emotes.Select(x => x.urn));
                UniTask<EmbeddedEmotesSO>.Awaiter embeddedEmotesTask = emotesCatalogService.Ref.GetEmbeddedEmotes().GetAwaiter();
                yield return new WaitUntil(() => embeddedEmotesTask.IsCompleted);
                var embeddedEmoteIds = embeddedEmotesTask.GetResult().emotes.Select(x => x.id);
                //here we add emote ids to both new and old emote loading flow to merge the results later
                //because some users might have emotes as wearables and others only as emotes
                foreach (var emoteId in embeddedEmoteIds)
                {
                    emotes.Add(emoteId);
                    wearableItems.Add(emoteId);
                }

                //TODO Add Collider to the AvatarSystem
                //TODO Without this the collider could get triggered disabling the avatar container,
                // this would stop the loading process due to the underlying coroutines of the AssetLoader not starting
                avatarCollider.gameObject.SetActive(false);

                SetImpostor(model.id);
                loadingCts?.Cancel();
                loadingCts?.Dispose();
                loadingCts = new CancellationTokenSource();
                if (DataStore.i.avatarConfig.useHologramAvatar.Get())
                {
                    UserProfile profile = userProfileBridge.Get(model.id);
                    playerName.SetName(model.name, profile?.hasClaimedName ?? false, profile?.isGuest ?? false);
                    playerName.Show(true);
                }

                var forceRender = new HashSet<string>(userProfileBridge.Get(model.id)?.avatar.forceRender ?? new HashSet<string>());

                var avatarSettings = new AvatarSettings
                {
                    playerName = model.name,
                    bodyshapeId = model.bodyShape,
                    eyesColor = model.eyeColor,
                    skinColor = model.skinColor,
                    hairColor = model.hairColor,
                    forceRender = forceRender,
                };

                yield return avatar.Load(wearableItems, emotes.ToList(), avatarSettings, loadingCts.Token).ToCoroutine(e =>
                {
                    if (e is not OperationCanceledException)
                        throw e;
                });
            }

            avatar.PlayEmote(model.expressionTriggerId, model.expressionTriggerTimestamp);

            onPointerDown.OnPointerDownReport -= PlayerClicked;
            onPointerDown.OnPointerDownReport += PlayerClicked;
            onPointerDown.OnPointerEnterReport -= PlayerPointerEnter;
            onPointerDown.OnPointerEnterReport += PlayerPointerEnter;
            onPointerDown.OnPointerExitReport -= PlayerPointerExit;
            onPointerDown.OnPointerExitReport += PlayerPointerExit;

            outlineOnHover.OnPointerEnterReport -= PlayerPointerEnter;
            outlineOnHover.OnPointerEnterReport += PlayerPointerEnter;
            outlineOnHover.OnPointerExitReport -= PlayerPointerExit;
            outlineOnHover.OnPointerExitReport += PlayerPointerExit;

            UpdatePlayerStatus(model);

            onPointerDown.Initialize(
                new OnPointerDown.Model()
                {
                    type = OnPointerDown.NAME,
                    button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                    hoverText = "View Profile"
                },
                entity, player
            );

            outlineOnHover.Initialize(new OnPointerDown.Model(), entity, player?.avatar);

            avatarCollider.gameObject.SetActive(true);

            everythingIsLoaded = true;
            OnAvatarShapeUpdated?.Invoke(entity, this);

            onPointerDown.SetColliderEnabled(isGlobalSceneAvatar);
            onPointerDown.SetOnClickReportEnabled(isGlobalSceneAvatar);
        }

        public void SetImpostor(string userId)
        {
            currentLazyObserver?.RemoveListener(avatar.SetImpostorTexture);
            if (string.IsNullOrEmpty(userId))
                return;

            UserProfile userProfile = UserProfileController.GetProfileByUserId(userId);
            if (userProfile == null)
                return;

            currentLazyObserver = userProfile.bodySnapshotObserver;
            currentLazyObserver.AddListener(avatar.SetImpostorTexture);
        }

        private void PlayerPointerExit()
        {
            playerName?.SetForceShow(false);
        }

        private void PlayerPointerEnter()
        {
            playerName?.SetForceShow(true);
        }

        private void UpdatePlayerStatus(AvatarModel model)
        {
            // Remove the player status if the userId changes
            if (player != null && (player.id != model.id || player.name != model.name))
                otherPlayers.Remove(player.id);

            if (isGlobalSceneAvatar && string.IsNullOrEmpty(model?.id))
                return;

            bool isNew = player == null;
            if (isNew)
            {
                player = new Player();
            }

            bool isNameDirty = player.name != model.name;

            player.id = model.id;
            player.name = model.name;
            player.isTalking = model.talking;
            player.worldPosition = entity.gameObject.transform.position;
            player.avatar = avatar;
            player.onPointerDownCollider = onPointerDown;
            player.collider = avatarCollider;

            if (isNew)
            {
                player.playerName = playerName;
                player.playerName.Show();
                player.anchorPoints = anchorPoints;
                if (isGlobalSceneAvatar)
                {
                    // TODO: Note: This is having a problem, sometimes the users has been detected as new 2 times and it shouldn't happen
                    // we should investigate this
                    if (otherPlayers.ContainsKey(player.id))
                        otherPlayers.Remove(player.id);
                    otherPlayers.Add(player.id, player);
                }
                avatarReporterController.ReportAvatarRemoved();
            }

            avatarReporterController.SetUp(entity.scene.sceneData.sceneNumber, player.id);

            float height = AvatarSystemUtils.AVATAR_Y_OFFSET + avatar.extents.y;

            anchorPoints.Prepare(avatarContainer.transform, avatar.GetBones(), height);

            player.playerName.SetIsTalking(model.talking);
            player.playerName.SetYOffset(Mathf.Max(MINIMUM_PLAYERNAME_HEIGHT, height));

            if (isNameDirty)
            {
                UserProfile profile = userProfileBridge.Get(model.id);
                player.playerName.SetName(model.name, profile?.hasClaimedName ?? false, profile?.isGuest ?? false);
            }
        }

        private void Update()
        {
            if (player != null)
            {
                player.worldPosition = entity.gameObject.transform.position;
                player.forwardDirection = entity.gameObject.transform.forward;
                avatarReporterController.ReportAvatarPosition(player.worldPosition);
            }
        }

        private void OnEntityTransformChanged(object newModel)
        {
            DCLTransform.Model newTransformModel = (DCLTransform.Model)newModel;
            OnEntityTransformChanged(newTransformModel.position, newTransformModel.rotation, !initializedPosition);
        }

        private void OnEntityTransformChanged(in Vector3 position, in Quaternion rotation, bool inmediate)
        {
            if (isGlobalSceneAvatar)
            {
                avatarMovementController.OnTransformChanged(position, rotation, inmediate);
            }
            else
            {
                var scenePosition = Utils.GridToWorldPosition(entity.scene.sceneData.basePosition.x, entity.scene.sceneData.basePosition.y);
                avatarMovementController.OnTransformChanged(scenePosition + position, rotation, inmediate);
            }
            initializedPosition = true;
        }

        public override void OnPoolGet()
        {
            base.OnPoolGet();

            everythingIsLoaded = false;
            initializedPosition = false;
            model = new AvatarModel();
            player = null;
        }

        public void ApplyHideAvatarModifier()
        {
            if (!currentActiveModifiers.ContainsKey(AvatarModifierAreaID.HIDE_AVATAR))
            {
                avatar.AddVisibilityConstraint(VISIBILITY_CONSTRAINT_HIDE_AREA);
                onPointerDown.gameObject.SetActive(false);
                playerNameContainer.SetActive(false);
                stickersControllers.ToggleHideArea(true);
            }
            currentActiveModifiers.AddRefCount(AvatarModifierAreaID.HIDE_AVATAR);
        }

        public void RemoveHideAvatarModifier()
        {
            currentActiveModifiers.RemoveRefCount(AvatarModifierAreaID.HIDE_AVATAR);
            if (!currentActiveModifiers.ContainsKey(AvatarModifierAreaID.HIDE_AVATAR))
            {
                avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAINT_HIDE_AREA);
                onPointerDown.gameObject.SetActive(true);
                playerNameContainer.SetActive(true);
                stickersControllers.ToggleHideArea(false);
            }
        }

        public void ApplyHidePassportModifier()
        {
            if (!currentActiveModifiers.ContainsKey(AvatarModifierAreaID.DISABLE_PASSPORT))
            {
                DisablePassport();
            }
            currentActiveModifiers.AddRefCount(AvatarModifierAreaID.DISABLE_PASSPORT);
        }

        public void RemoveHidePassportModifier()
        {
            currentActiveModifiers.RemoveRefCount(AvatarModifierAreaID.DISABLE_PASSPORT);
            if (!currentActiveModifiers.ContainsKey(AvatarModifierAreaID.DISABLE_PASSPORT))
            {
                EnablePasssport();
            }
        }

        private void EnablePasssport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.SetPassportEnabled(true);
        }

        private void DisablePassport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.SetPassportEnabled(false);
        }

        public override void Cleanup()
        {
            base.Cleanup();

            playerName?.Hide(true);
            if (player != null)
            {
                // AvatarShape used from the SDK doesn't register the avatars in 'otherPlayers'
                if(!string.IsNullOrEmpty(player.id))
                    otherPlayers.Remove(player.id);
                player = null;
            }

            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = null;
            currentLazyObserver?.RemoveListener(avatar.SetImpostorTexture);
            avatar.Dispose();

            if (poolableObject != null)
            {
                poolableObject.OnRelease -= Cleanup;
            }

            onPointerDown.OnPointerDownReport -= PlayerClicked;
            onPointerDown.OnPointerEnterReport -= PlayerPointerEnter;
            onPointerDown.OnPointerExitReport -= PlayerPointerExit;
            outlineOnHover.OnPointerEnterReport -= PlayerPointerEnter;
            outlineOnHover.OnPointerExitReport -= PlayerPointerExit;

            if (entity != null)
            {
                entity.OnTransformChange = null;
                entity = null;
            }

            avatarReporterController.ReportAvatarRemoved();
        }

        public void UpdateOutOfBoundariesState(bool isInsideBoundaries)
        {
            if (scene.isPersistent)
                isInsideBoundaries = true;

            if(isInsideBoundaries)
                avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAINT_OUTSIDE_SCENE_BOUNDS);
            else
                avatar.AddVisibilityConstraint(VISIBILITY_CONSTRAINT_OUTSIDE_SCENE_BOUNDS);

            onPointerDown.gameObject.SetActive(isInsideBoundaries);
            playerNameContainer.SetActive(isInsideBoundaries);
            stickersControllers.ToggleHideArea(!isInsideBoundaries);
        }

        public override int GetClassId() { return (int) CLASS_ID_COMPONENT.AVATAR_SHAPE; }

        [ContextMenu("Print current profile")]
        private void PrintCurrentProfile() { Debug.Log(JsonUtility.ToJson(model)); }

    }
}
