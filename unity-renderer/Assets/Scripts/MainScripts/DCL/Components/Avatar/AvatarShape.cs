using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using GPUSkinning;
using UnityEngine;
using Avatar = AvatarSystem.Avatar;
using LOD = AvatarSystem.LOD;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";
        private const float MINIMUM_PLAYERNAME_HEIGHT = 2.7f;
        private const float AVATAR_PASSPORT_TOGGLE_ALPHA_THRESHOLD = 0.9f;

        public static event Action<IDCLEntity, AvatarShape> OnAvatarShapeUpdated;

        public GameObject avatarContainer;
        public Collider avatarCollider;
        public AvatarMovementController avatarMovementController;
        [SerializeField] private GameObject onloadParticlePrefab;

        [SerializeField] internal AvatarOnPointerDown onPointerDown;
        internal IPlayerName playerName;
        internal IAvatarReporterController avatarReporterController;

        private StringVariable currentPlayerInfoCardId;

        public bool everythingIsLoaded;

        bool initializedPosition = false;

        private Player player = null;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        private IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        private IAvatar avatar;
        private readonly AvatarModel currentAvatar = new AvatarModel { wearables = new List<string>() };
        private CancellationTokenSource loadingCts;
        private ILazyTextureObserver currentLazyObserver;
        private bool isGlobalSceneAvatar = true;

        private void Awake()
        {
            model = new AvatarModel();
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
            Visibility visibility = new Visibility();
            LOD avatarLOD = new LOD(avatarContainer, visibility, avatarMovementController);
            avatar = new Avatar(
                new AvatarCurator(new WearableItemResolver()),
                new Loader(new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper()),
                GetComponentInChildren<AvatarAnimatorLegacy>(),
                visibility,
                avatarLOD,
                new SimpleGPUSkinning(),
                new GPUSkinningThrottler());

            if (avatarReporterController == null)
            {
                avatarReporterController = new AvatarReporterController(Environment.i.world.state);
            }
        }

        private void Start()
        {
            playerName = GetComponentInChildren<IPlayerName>();
            playerName?.Hide(true);
        }

        private void PlayerClicked()
        {
            if (model == null)
                return;
            currentPlayerInfoCardId.Set(((AvatarModel) model).id);
        }

        public void OnDestroy()
        {
            Cleanup();

            if (poolableObject != null && poolableObject.isInsidePool)
                poolableObject.RemoveFromPool();
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            isGlobalSceneAvatar = scene.sceneData.id == EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID;

            DisablePassport();

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
            if (!initializedPosition && entity.components.ContainsKey(DCL.Models.CLASS_ID_COMPONENT.TRANSFORM))
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
                //TODO Add Collider to the AvatarSystem
                //TODO Without this the collider could get triggered disabling the avatar container,
                // this would stop the loading process due to the underlying coroutines of the AssetLoader not starting
                avatarCollider.gameObject.SetActive(false);

                SetImpostor(model.id);
                loadingCts?.Cancel();
                loadingCts?.Dispose();
                loadingCts = new CancellationTokenSource();

                avatar.Load(wearableItems, new AvatarSettings
                {
                    playerName = model.name,
                    bodyshapeId = model.bodyShape,
                    eyesColor = model.eyeColor,
                    skinColor = model.skinColor,
                    hairColor = model.hairColor,
                }, loadingCts.Token);

                // Yielding a UniTask doesn't do anything, we manually wait until the avatar is ready
                yield return new WaitUntil(() => avatar.status == IAvatar.Status.Loaded);

                if (avatar.lodLevel <= 1)
                    AvatarSystemUtils.SpawnAvatarLoadedParticles(avatarContainer.transform, onloadParticlePrefab);
            }

            avatar.SetExpression(model.expressionTriggerId, model.expressionTriggerTimestamp);

            onPointerDown.OnPointerDownReport -= PlayerClicked;
            onPointerDown.OnPointerDownReport += PlayerClicked;
            onPointerDown.OnPointerEnterReport -= PlayerPointerEnter;
            onPointerDown.OnPointerEnterReport += PlayerPointerEnter;
            onPointerDown.OnPointerExitReport -= PlayerPointerExit;
            onPointerDown.OnPointerExitReport += PlayerPointerExit;

            UpdatePlayerStatus(model);
            
            onPointerDown.Initialize(
                new OnPointerDown.Model()
                {
                    type = OnPointerDown.NAME,
                    button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                    hoverText = "view profile"
                },
                entity, player
            );            

            avatarCollider.gameObject.SetActive(true);

            everythingIsLoaded = true;
            OnAvatarShapeUpdated?.Invoke(entity, this);

            EnablePassport();

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

        private void PlayerPointerExit() { playerName?.SetForceShow(false); }
        private void PlayerPointerEnter() { playerName?.SetForceShow(true); }

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
                player.playerName.SetName(player.name);
                player.playerName.Show();
                player.anchorPoints = anchorPoints;
                if (isGlobalSceneAvatar)
                {
                    otherPlayers.Add(player.id, player);
                }
                avatarReporterController.ReportAvatarRemoved();
            }

            avatarReporterController.SetUp(entity.scene.sceneData.id, entity.entityId, player.id);

            float height = AvatarSystemUtils.AVATAR_Y_OFFSET + avatar.extents.y;

            anchorPoints.Prepare(avatarContainer.transform, avatar.GetBones(), height);

            player.playerName.SetIsTalking(model.talking);
            player.playerName.SetYOffset(Mathf.Max(MINIMUM_PLAYERNAME_HEIGHT, height));
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

        public void DisablePassport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.SetPassportEnabled(false);
        }

        public void EnablePassport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.SetPassportEnabled(true);
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

        public override void Cleanup()
        {
            base.Cleanup();

            playerName?.Hide(true);
            if (player != null)
            {
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

            if (entity != null)
            {
                entity.OnTransformChange = null;
                entity = null;
            }

            avatarReporterController.ReportAvatarRemoved();
        }

        public override int GetClassId() { return (int) CLASS_ID_COMPONENT.AVATAR_SHAPE; }

        [ContextMenu("Print current profile")]
        private void PrintCurrentProfile() { Debug.Log(JsonUtility.ToJson(model)); }
    }
}