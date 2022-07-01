using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.ECSRuntime;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Models;
using GPUSkinning;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using Avatar = UnityEngine.Avatar;
using LOD = UnityEngine.LOD;
using Visibility = UnityEngine.UIElements.Visibility;

namespace DCL.ECSComponents
{
    public class AvatarShapeComponentHandler : IECSComponentHandler<PBAvatarShape>
    {
        private readonly DataStore_ECS7 dataStore;
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";
        private const float MINIMUM_PLAYERNAME_HEIGHT = 2.7f;
        private const float AVATAR_PASSPORT_TOGGLE_ALPHA_THRESHOLD = 0.9f;
        private const string IN_HIDE_AREA = "IN_HIDE_AREA";

        public static event Action<IDCLEntity, AvatarShape> OnAvatarShapeUpdated;

        public GameObject avatarContainer;
        public Collider avatarCollider;
        public AvatarMovementController avatarMovementController;
        [SerializeField] private Transform avatarRevealContainer;
        [SerializeField] private GameObject armatureContainer;

        [SerializeField] internal AvatarOnPointerDown onPointerDown;
        [SerializeField] internal GameObject playerNameContainer;
        internal IPlayerName playerName;
        internal IAvatarReporterController avatarReporterController;

        private StringVariable currentPlayerInfoCardId;

        public bool everythingIsLoaded;

        bool initializedPosition = false;

        private Player player = null;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        private IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        private IAvatar avatar;
        private CancellationTokenSource loadingCts;
        private ILazyTextureObserver currentLazyObserver;
        private bool isGlobalSceneAvatar = true;
        
        public AvatarShapeComponentHandler(DataStore_ECS7 dataStoreEcs7)
        {
            dataStore = dataStoreEcs7;
            
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
            Visibility visibility = new Visibility();
            LOD avatarLOD = new LOD(avatarContainer, visibility, avatarMovementController);
            AvatarAnimatorLegacy animator = GetComponentInChildren<AvatarAnimatorLegacy>();
            BaseAvatar baseAvatar = new BaseAvatar(avatarRevealContainer, armatureContainer, avatarLOD);
            avatar = new Avatar(
                baseAvatar,
                new AvatarCurator(new WearableItemResolver()),
                new Loader(new WearableLoaderFactory(), avatarContainer, new AvatarMeshCombinerHelper()),
                animator,
                visibility,
                avatarLOD,
                new SimpleGPUSkinning(),
                new GPUSkinningThrottler(),
                new EmoteAnimationEquipper(animator, DataStore.i.emotes));

            if (avatarReporterController == null)
            {
                avatarReporterController = new AvatarReporterController(Environment.i.world.state);
            }
        }

        public void OnComponentCreated(IParcelScene scene, IDCLEntity entity)
        {
            playerName = entity.gameObject.GetComponentInChildren<IPlayerName>();
            playerName?.Hide(true);
        }

        public void OnComponentRemoved(IParcelScene scene, IDCLEntity entity)
        {
            Cleanup();

            if (poolableObject != null && poolableObject.isInsidePool)
                poolableObject.RemoveFromPool();
        }

        public void OnComponentModelUpdated(IParcelScene scene, IDCLEntity entity, PBAvatarShape model)
        {
            isGlobalSceneAvatar = scene.sceneData.id == EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID;

            DisablePassport();

            bool needsLoading = !model.HaveSameWearablesAndColors(currentAvatar);
            currentAvatar.CopyFrom(model);

            if (string.IsNullOrEmpty(model.BodyShape) || model.Wearables.Count == 0)
                return;
#if UNITY_EDITOR
            entity.gameObject.name = $"Avatar Shape {model.Name}";
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

            var wearableItems = model.Wearables.ToList();
            wearableItems.Add(model.BodyShape);

            //temporarily hardcoding the embedded emotes until the user profile provides the equipped ones
            var embeddedEmotesSo = Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes");
            wearableItems.AddRange(embeddedEmotesSo.emotes.Select(x => x.id));

            if (avatar.status != IAvatar.Status.Loaded || needsLoading)
            {
                //TODO Add Collider to the AvatarSystem
                //TODO Without this the collider could get triggered disabling the avatar container,
                // this would stop the loading process due to the underlying coroutines of the AssetLoader not starting
                avatarCollider.gameObject.SetActive(false);

                SetImpostor(model.Id);
                loadingCts?.Cancel();
                loadingCts?.Dispose();
                loadingCts = new CancellationTokenSource();
                playerName.SetName(model.Name);
                playerName.Show(true);
                avatar.Load(wearableItems, new AvatarSettings
                {
                    playerName = model.Name,
                    bodyshapeId = model.BodyShape,
                    eyesColor = ProtoConvertUtils.UnityVectorToPBVector(model.EyeColor),
                    skinColor = ProtoConvertUtils.UnityVectorToPBVector(model.SkinColor),
                    hairColor = ProtoConvertUtils.UnityVectorToPBVector(model.HairColor),
                }, loadingCts.Token);

                // Yielding a UniTask doesn't do anything, we manually wait until the avatar is ready
                yield return new WaitUntil(() => avatar.status == IAvatar.Status.Loaded);
            }

            avatar.PlayEmote(model.ExpressionTriggerId, model.ExpressionTriggerTimestamp);

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
        
        private void PlayerClicked()
        {
            if (model == null)
                return;
            currentPlayerInfoCardId.Set(((AvatarModel) model).id);
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

        private void UpdatePlayerStatus(PBAvatarShape model)
        {
            // Remove the player status if the userId changes
            if (player != null && (player.id != model.Id || player.name != model.Name))
                otherPlayers.Remove(player.id);

            if (isGlobalSceneAvatar && string.IsNullOrEmpty(model?.Id))
                return;

            bool isNew = player == null;
            if (isNew)
            {
                player = new Player();
            }

            bool isNameDirty = player.name != model.Name;

            player.id = model.Id;
            player.name = model.Name;
            player.isTalking = model.Talking;
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

            avatarReporterController.SetUp(entity.scene.sceneData.id, player.id);

            float height = AvatarSystemUtils.AVATAR_Y_OFFSET + avatar.extents.y;

            anchorPoints.Prepare(avatarContainer.transform, avatar.GetBones(), height);

            player.playerName.SetIsTalking(model.Talking);
            player.playerName.SetYOffset(Mathf.Max(MINIMUM_PLAYERNAME_HEIGHT, height));
            if (isNameDirty)
                player.playerName.SetName(model.Name);
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

        public void OnPoolGet()
        {
            everythingIsLoaded = false;
            initializedPosition = false;
            model = new AvatarModel();
            player = null;
        }

        public void ApplyHideModifier()
        {
            avatar.AddVisibilityConstrain(IN_HIDE_AREA);
            onPointerDown.gameObject.SetActive(false);
            playerNameContainer.SetActive(false);

        }

        public void RemoveHideModifier()
        {
            avatar.RemoveVisibilityConstrain(IN_HIDE_AREA);
            onPointerDown.gameObject.SetActive(true);
            playerNameContainer.SetActive(true);
        }

        public void Cleanup()
        {
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
    }
}