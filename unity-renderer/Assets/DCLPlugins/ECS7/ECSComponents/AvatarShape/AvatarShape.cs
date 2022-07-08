using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Emotes;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using GPUSkinning;
using UnityEngine;
using LOD = AvatarSystem.LOD;

namespace DCL.ECSComponents
{
    public interface IAvatarShape
    {
        /// <summary>
        /// Clean up the avatar shape so we can reutilizate it using the pool
        /// </summary>
        void Cleanup();
        
        /// <summary>
        /// Apply the model of the avatar, it will reload if necessary
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="entity"></param>
        /// <param name="newModel"></param>
        void ApplyModel(IParcelScene scene, IDCLEntity entity, PBAvatarShape newModel);

        /// <summary>
        /// Get the transform of the avatar shape
        /// </summary>
        Transform transform { get; }
    }
    
    public class AvatarShape : MonoBehaviour, IHideAvatarAreaHandler, IPoolableObjectContainer, IAvatarShape, IPoolLifecycleHandler
    {
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";
        private const float MINIMUM_PLAYERNAME_HEIGHT = 2.7f;
        internal const string IN_HIDE_AREA = "IN_HIDE_AREA";

        // NOTE: Don't use the reference, use the avatarMovementController
        // Unity can't serialize interfaces, so we assign the reference on the prefab and assign it in the Awake method so we can avoid 1 GetComponent call
        [SerializeField] private AvatarMovementController avatarMovementControllerReference;
        internal IAvatarMovementController avatarMovementController;
        
        [SerializeField] private GameObject avatarContainer;
        [SerializeField] internal Collider avatarCollider;
        [SerializeField] private Transform avatarRevealContainer;
        [SerializeField] private GameObject armatureContainer;

        [SerializeField] internal AvatarOnPointerDown onPointerDown;
        [SerializeField] internal GameObject playerNameContainer;
        internal IPlayerName playerName;
        internal IAvatarReporterController avatarReporterController;

        private StringVariable currentPlayerInfoCardId;

        internal bool initializedPosition = false;

        internal Player player = null;
        internal BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        
        private IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        internal IAvatar avatar;
        internal CancellationTokenSource loadingCts;
        private ILazyTextureObserver currentLazyObserver;
        private bool isGlobalSceneAvatar = true;

        public IPoolableObject poolableObject { get; set; }
        internal PBAvatarShape model;
        internal IDCLEntity entity;
        
        private void Awake()
        {
            // NOTE: Unity can't serialize interfaces, so we assign the reference and assign it there so we can avoid 1 GetComponent call
            avatarMovementController = avatarMovementControllerReference;
            
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
            Visibility visibility = new Visibility();
            avatarMovementController.SetAvatarTransform(transform);
            
            LOD avatarLOD = new LOD(avatarContainer, visibility, avatarMovementController);
            AvatarAnimatorLegacy animator = GetComponentInChildren<AvatarAnimatorLegacy>();
            BaseAvatar baseAvatar = new BaseAvatar(avatarRevealContainer, armatureContainer, avatarLOD);
            avatar = new AvatarWithHologram(
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
                avatarReporterController = new AvatarReporterController(Environment.i.world.state);
            
            onPointerDown.OnPointerDownReport += PlayerClicked;
            onPointerDown.OnPointerEnterReport += PlayerPointerEnter;
            onPointerDown.OnPointerExitReport += PlayerPointerExit;
        }

        private void Start()
        {
            SetPlayerNameReference();
        }

        private void SetPlayerNameReference()
        {
            if (playerName != null)
                return;
            playerName = GetComponentInChildren<IPlayerName>();
            playerName?.Hide(true);
        }
        
        private void PlayerClicked()
        {
            if (model == null)
                return;
            currentPlayerInfoCardId.Set(model.Id);
        }

        public void OnDestroy()
        {
            Cleanup();

            if (poolableObject != null && poolableObject.isInsidePool)
                poolableObject.RemoveFromPool();
        }

        public async void ApplyModel(IParcelScene scene, IDCLEntity entity, PBAvatarShape newModel)
        {
            this.entity = entity;
            
            isGlobalSceneAvatar = scene.sceneData.id == EnvironmentSettings.AVATAR_GLOBAL_SCENE_ID;

            DisablePassport();
            
            bool needsLoading = !AvatarUtils.HaveSameWearablesAndColors(model,newModel);
            model = newModel;

            if (string.IsNullOrEmpty(model.BodyShape) || model.Wearables.Count == 0)
                return;
#if UNITY_EDITOR
            gameObject.name = $"Avatar Shape {model.Name}";
#endif

            // To deal with the cases in which the entity transform was configured before the AvatarShape
            if (!initializedPosition)
            {
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
                await LoadAvatar(wearableItems);

            avatar.PlayEmote(model.ExpressionTriggerId, model.ExpressionTriggerTimestamp);

            UpdatePlayerStatus(entity,model);

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

            EnablePassport();

            onPointerDown.SetColliderEnabled(isGlobalSceneAvatar);
            onPointerDown.SetOnClickReportEnabled(isGlobalSceneAvatar);
        }

        private async UniTask LoadAvatar(List<string> wearableItems, CancellationToken ct = default)
        {
            //TODO Add Collider to the AvatarSystem
            //TODO Without this the collider could get triggered disabling the avatar container,
            // this would stop the loading process due to the underlying coroutines of the AssetLoader not starting
            avatarCollider.gameObject.SetActive(false);

            SetImpostor(model.Id);
            loadingCts?.Cancel();
            loadingCts?.Dispose();
            loadingCts = new CancellationTokenSource();
            CancellationToken linkedCt = CancellationTokenSource.CreateLinkedTokenSource(ct, loadingCts.Token).Token;
            linkedCt.ThrowIfCancellationRequested();

            try
            {
                playerName.SetName(model.Name);
                playerName.Show(true);
                await avatar.Load(wearableItems, new AvatarSettings
                {
                    playerName = model.Name,
                    bodyshapeId = model.BodyShape,
                    eyesColor = ProtoConvertUtils.PBColorToUnityColor(model.EyeColor),
                    skinColor = ProtoConvertUtils.PBColorToUnityColor(model.SkinColor),
                    hairColor = ProtoConvertUtils.PBColorToUnityColor(model.HairColor),
                }, loadingCts.Token);
            }
            catch (OperationCanceledException)
            {
                Cleanup();
                throw;
            }
            catch (Exception e)
            {
                Cleanup();
                Debug.Log($"Avatar.Load failed with wearables:[{string.Join(",", wearableItems)}] for bodyshape:{model.BodyShape} and player {model.Name}");
            }
            finally
            {
                loadingCts?.Dispose();
                loadingCts = null;
            }
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

        internal void UpdatePlayerStatus(IDCLEntity entity,PBAvatarShape model)
        {
            // Remove the player status if the userId changes
            if (player != null && (player.id != model.Id || player.name != model.Name))
                otherPlayers.Remove(player.id);

            if (isGlobalSceneAvatar && string.IsNullOrEmpty(model?.Id))
                return;

            bool isNew = player == null;
            if (isNew)
                player = new Player();
            
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

        private void OnEntityTransformChanged(in UnityEngine.Vector3 position, in Quaternion rotation, bool inmediate)
        {
            if (entity == null)
                return;
            
            if (isGlobalSceneAvatar)
            {
                avatarMovementController.OnTransformChanged(position, rotation, inmediate);
            }
            else
            {
                var scenePosition = DCL.Helpers.Utils.GridToWorldPosition(entity.scene.sceneData.basePosition.x, entity.scene.sceneData.basePosition.y);
                avatarMovementController.OnTransformChanged(scenePosition + position, rotation, inmediate);
            }
            initializedPosition = true;
        }

        public void OnPoolRelease()
        {
            Cleanup();
        }
        
        public void OnPoolGet()
        {
            initializedPosition = false;
            model = new PBAvatarShape();
            player = null;
            SetPlayerNameReference();
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

        [ContextMenu("Print current profile")]
        private void PrintCurrentProfile() { Debug.Log(JsonUtility.ToJson(model)); }
    }
}

