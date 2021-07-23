using System;
using DCL.Components;
using DCL.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
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
        private AvatarOnPointerDown onPointerDown;

        private StringVariable currentPlayerInfoCardId;

        private AvatarModel oldModel = new AvatarModel();

        public bool everythingIsLoaded;

        private Vector3? lastAvatarPosition = null;
        bool initializedPosition = false;

        private PlayerStatus playerStatus = null;

        private void Awake()
        {
            model = new AvatarModel();
            currentPlayerInfoCardId = Resources.Load<StringVariable>(CURRENT_PLAYER_ID);
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
            DisablePassport();

            var model = (AvatarModel) newModel;

            everythingIsLoaded = false;

            bool avatarDone = false;
            bool avatarFailed = false;

            yield return null; //NOTE(Brian): just in case we have a Object.Destroy waiting to be resolved.

            avatarRenderer.ApplyModel(model, () =>
            {
                if (avatarRenderer.lodRenderer != null)
                    Environment.i.platform.avatarsLODController.RegisterAvatar(avatarRenderer);

                avatarDone = true;
            }, () => avatarFailed = true);

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
        }

        private void UpdatePlayerStatus(AvatarModel model)
        {
            // Remove the player status if the userId changes
            if ( playerStatus != null && playerStatus.id != model.id)
                DataStore.i.player.otherPlayersStatus.Remove(playerStatus.id);

            if (string.IsNullOrEmpty(model?.id))
                return;

            bool isNew = false;
            if (playerStatus == null)
            {
                playerStatus = new PlayerStatus();
                isNew = true;
            }
            playerStatus.id = model.id;
            playerStatus.name = model.name;
            playerStatus.isTalking = model.talking;
            playerStatus.worldPosition = entity.gameObject.transform.position;
            if (isNew)
                DataStore.i.player.otherPlayersStatus.Add(playerStatus.id, playerStatus);
        }

        private void Update()
        {
            if (playerStatus != null)
                playerStatus.worldPosition = entity.gameObject.transform.position;
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
        }

        public override void Cleanup()
        {
            base.Cleanup();

            if (playerStatus != null)
                DataStore.i.player.otherPlayersStatus.Remove(playerStatus.id);

            Environment.i.platform.avatarsLODController.RemoveAvatar(avatarRenderer);

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