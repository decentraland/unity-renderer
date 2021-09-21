using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using DCL.Models;
using UnityEngine;

namespace DCL
{
    public class AvatarShape : BaseComponent
    {
        private const string CURRENT_PLAYER_ID = "CurrentPlayerInfoCardId";

        public static event Action<IDCLEntity, AvatarShape> OnAvatarShapeUpdated;

        public AvatarRenderer avatarRenderer;
        public Collider avatarCollider;
        public AvatarMovementController avatarMovementController;

        [SerializeField]
        internal AvatarOnPointerDown onPointerDown;

        private StringVariable currentPlayerInfoCardId;

        private AvatarModel oldModel = new AvatarModel();

        public bool everythingIsLoaded;

        private Vector3? lastAvatarPosition = null;
        bool initializedPosition = false;

        private Player player = null;
        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
        private BaseHashSet<string> visibleNames => DataStore.i.avatarsLOD.visibleNames;

        private void Awake()
        {
            model = new AvatarModel();
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
            avatarRenderer.OnImpostorAlphaValueUpdate += OnImpostorAlphaValueUpdate;
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

            avatarRenderer.OnImpostorAlphaValueUpdate -= OnImpostorAlphaValueUpdate;
        }

        public override IEnumerator ApplyChanges(BaseModel newModel)
        {
            DisablePassport();

            var model = (AvatarModel) newModel;

            everythingIsLoaded = false;

            bool avatarDone = false;
            bool avatarFailed = false;

            yield return null; //NOTE(Brian): just in case we have a Object.Destroy waiting to be resolved.

            avatarRenderer.ApplyModel(model, () => avatarDone = true, () => avatarFailed = true);

            yield return new WaitUntil(() => avatarDone || avatarFailed);

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

            // To deal with the cases in which the entity transform was configured before the AvatarShape
            if (!initializedPosition && entity.components.ContainsKey(DCL.Models.CLASS_ID_COMPONENT.TRANSFORM))
            {
                initializedPosition = true;

                avatarMovementController.MoveTo(
                    entity.gameObject.transform.localPosition - Vector3.up * DCLCharacterController.i.characterController.height / 2,
                    entity.gameObject.transform.localRotation, true);
            }

            UpdatePlayerStatus(model);

            avatarCollider.gameObject.SetActive(true);

            everythingIsLoaded = true;
            OnAvatarShapeUpdated?.Invoke(entity, this);

            EnablePassport();

            KernelConfig.i.EnsureConfigInitialized()
                        .Then(config =>
                        {
                            if(config.features.enableAvatarLODs)
                                avatarRenderer.InitializeImpostor();
                        });
        }

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
            player.renderer = avatarRenderer;
            if (isNew)
            {
                visibleNames.Add(player.id);
                otherPlayers.Add(player.id, player);
            }
        }

        private void Update()
        {
            if (player != null)
            {
                player.worldPosition = entity.gameObject.transform.position;
                player.forwardDirection = entity.gameObject.transform.forward;
            }
        }

        public void DisablePassport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.collider.enabled = false;
        }

        public void EnablePassport()
        {
            if (onPointerDown.collider == null)
                return;

            onPointerDown.collider.enabled = true;
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

        void OnImpostorAlphaValueUpdate(float newAlphaValue) { avatarMovementController.movementLerpWait = newAlphaValue > 0.01f ? AvatarRendererHelpers.IMPOSTOR_MOVEMENT_INTERPOLATION : 0f; }

        public override void Cleanup()
        {
            base.Cleanup();

            if (player != null)
            {
                otherPlayers.Remove(player.id);
                player = null;
            }

            avatarRenderer.CleanupAvatar();

            if (poolableObject != null)
            {
                poolableObject.OnRelease -= Cleanup;
            }

            onPointerDown.OnPointerDownReport -= PlayerClicked;

            if (entity != null)
            {
                entity.OnTransformChange = null;
                entity = null;
            }
        }

        public override int GetClassId() { return (int) CLASS_ID_COMPONENT.AVATAR_SHAPE; }
    }
}