using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AvatarSystem;
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

        [SerializeField] internal AvatarOnPointerDown onPointerDown;
        internal IPlayerName playerName;
        internal IAvatarReporterController avatarReporterController;

        private StringVariable currentPlayerInfoCardId;

        private AvatarModel oldModel = new AvatarModel();

        public bool everythingIsLoaded;

        private Vector3? lastAvatarPosition = null;
        bool initializedPosition = false;

        private Player player = null;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        private IAvatarAnchorPoints anchorPoints = new AvatarAnchorPoints();
        private Avatar avatar;
        private LOD avatarLOD;
        private readonly AvatarModel currentAvatar = new AvatarModel { wearables = new List<string>() };
        private CancellationTokenSource loadingCts;

        private void Awake()
        {
            model = new AvatarModel();
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
            avatarLOD = new LOD(avatarContainer);
            avatar = new Avatar(
                new AvatarCurator(new WearableItemResolver()),
                new Loader(new WearableLoaderFactory(), avatarContainer),
                GetComponentInChildren<AvatarAnimatorLegacy>(),
                new Visibility(avatarContainer),
                avatarLOD,
                new SimpleGPUSkinning(),
                new GPUSkinningThrottler_New());
            //avatarRenderer.OnImpostorAlphaValueUpdate += OnImpostorAlphaValueUpdate;

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

            //avatarRenderer.OnImpostorAlphaValueUpdate -= OnImpostorAlphaValueUpdate;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
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

                float characterHeight = DCLCharacterController.i != null ? DCLCharacterController.i.characterController.height : 0.8f;

                avatarMovementController.MoveTo(
                    entity.gameObject.transform.localPosition - Vector3.up * characterHeight / 2,
                    entity.gameObject.transform.localRotation, true);
            }

            var wearableItems = model.wearables.ToList();
            wearableItems.Add(model.bodyShape);
            if (avatar.status != IAvatar.Status.Loaded || needsLoading)
            {
                loadingCts?.Cancel();
                loadingCts = new CancellationTokenSource();
                yield return avatar.Load(wearableItems, new AvatarSettings
                {
                    playerName = model.name,
                    bodyshapeId = model.bodyShape,
                    eyesColor = model.eyeColor,
                    skinColor = model.skinColor,
                    hairColor = model.hairColor,
                }, loadingCts.Token);
            }

            avatar.SetExpression(model.expressionTriggerId, model.expressionTriggerTimestamp);

            onPointerDown.Initialize(
                new OnPointerDown.Model()
                {
                    type = OnPointerDown.NAME,
                    button = WebInterface.ACTION_BUTTON.POINTER.ToString(),
                    hoverText = "view profile"
                },
                entity
            );

            entity.OnTransformChange -= avatarMovementController.OnTransformChanged;
            entity.OnTransformChange += avatarMovementController.OnTransformChanged;

            entity.OnTransformChange -= OnEntityTransformChanged;
            entity.OnTransformChange += OnEntityTransformChanged;

            onPointerDown.OnPointerDownReport -= PlayerClicked;
            onPointerDown.OnPointerDownReport += PlayerClicked;
            onPointerDown.OnPointerEnterReport -= PlayerPointerEnter;
            onPointerDown.OnPointerEnterReport += PlayerPointerEnter;
            onPointerDown.OnPointerExitReport -= PlayerPointerExit;
            onPointerDown.OnPointerExitReport += PlayerPointerExit;


            UpdatePlayerStatus(model);

            avatarCollider.gameObject.SetActive(true);

            everythingIsLoaded = true;
            OnAvatarShapeUpdated?.Invoke(entity, this);

            EnablePassport();
        }

        private void PlayerPointerExit() { playerName?.SetForceShow(false); }
        private void PlayerPointerEnter() { playerName?.SetForceShow(true); }

        private void UpdatePlayerStatus(AvatarModel model)
        {
            // Remove the player status if the userId changes
            if (player != null && (player.id != model.id || player.name != model.name))
                otherPlayers.Remove(player.id);

            if (string.IsNullOrEmpty(model?.id))
                return;

            bool isNew = false;
            if (player == null)
            {
                player = new Player();
                isNew = true;
            }

            player.id = model.id;
            player.name = model.name;
            player.isTalking = model.talking;
            player.worldPosition = entity.gameObject.transform.position;
            player.avatar = avatar;
            player.onPointerDownCollider = onPointerDown;

            if (isNew)
            {
                player.playerName = playerName;
                player.playerName.SetName(player.name);
                player.playerName.Show();
                player.anchorPoints = anchorPoints;
                otherPlayers.Add(player.id, player);
                avatarReporterController.ReportAvatarRemoved();
            }

            avatarReporterController.SetUp(entity.scene.sceneData.id, entity.entityId, player.id);
            //anchorPoints.Prepare(avatarRenderer.transform, avatarRenderer.GetBones(), avatarRenderer.maxY);

            player.playerName.SetIsTalking(model.talking);
            player.playerName.SetYOffset(Mathf.Max(MINIMUM_PLAYERNAME_HEIGHT, avatar.bounds.max.y));
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

            onPointerDown.SetColliderEnabled(false);
        }

        public void EnablePassport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.SetColliderEnabled(true);
        }

        private void OnEntityTransformChanged(object newModel)
        {
            DCLTransform.Model newTransformModel = (DCLTransform.Model)newModel;
            lastAvatarPosition = newTransformModel.position;
        }

        public override void OnPoolGet()
        {
            base.OnPoolGet();

            everythingIsLoaded = false;
            initializedPosition = false;
            oldModel = new AvatarModel();
            model = new AvatarModel();
            lastAvatarPosition = null;
            player = null;
        }

        void OnImpostorAlphaValueUpdate(float newAlphaValue)
        {
            Debug.Log("TODO");
            return;
            avatarMovementController.movementLerpWait = newAlphaValue > 0.01f ? AvatarRendererHelpers.IMPOSTOR_MOVEMENT_INTERPOLATION : 0f;
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
    }
}