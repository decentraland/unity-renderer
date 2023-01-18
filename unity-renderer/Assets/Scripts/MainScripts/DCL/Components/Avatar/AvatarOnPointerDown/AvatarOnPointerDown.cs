using DCL.Controllers;
using DCL.Interface;
using UnityEngine;
using DCL.Helpers;
using DCL.Models;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class AvatarOnPointerDown : MonoBehaviour, IAvatarOnPointerDown, IPoolLifecycleHandler,
        IAvatarOnPointerDownCollider, IUnlockedCursorInputEvent
    {
        public new Collider collider;
        private OnPointerEvent.Model model;
        private OnPointerEventHandler eventHandler;

        public InteractionHoverCanvasController hoverCanvas;
        public IDCLEntity entity { get; private set; }
        public event System.Action OnPointerDownReport;
        public event System.Action OnPointerEnterReport;
        public event System.Action OnPointerExitReport;
        private bool isHovering = false;

        private bool passportEnabled = true;
        private bool onClickReportEnabled = true;
        private Player avatarPlayer;

        public WebInterface.ACTION_BUTTON GetActionButton()
        {
            return model.GetActionButton();
        }

        public void SetHoverState(bool state)
        {
            bool isHoveringDirty = state != isHovering;
            isHovering = state;
            eventHandler?.SetFeedbackState(model.showFeedback, state && passportEnabled, model.button, model.hoverText);

            if (!isHoveringDirty)
                return;

            if (isHovering)
                OnPointerEnterReport?.Invoke();
            else
                OnPointerExitReport?.Invoke();
        }

        private void OnDisable()
        {
            if (!isHovering)
                return;

            isHovering = false;
            OnPointerExitReport?.Invoke();
        }

        void Awake()
        {
            CommonScriptableObjects.playerInfoCardVisibleState.OnChange += ReEnableOnInfoCardClosed;
        }

        void OnDestroy()
        {
            CommonScriptableObjects.playerInfoCardVisibleState.OnChange -= ReEnableOnInfoCardClosed;
            eventHandler?.Dispose();
            CollidersManager.i.RemoveEntityCollider(entity, collider);
        }

        public void Initialize(OnPointerEvent.Model model, IDCLEntity entity, Player player)
        {
            this.model = model;
            this.entity = entity;
            this.avatarPlayer = player;

            if (eventHandler == null)
                eventHandler = new OnPointerEventHandler();

            eventHandler?.SetColliders(entity);
            CollidersManager.i.AddOrUpdateEntityCollider(entity, collider);
        }

        public bool IsAtHoverDistance(float distance) =>
            !Utils.IsCursorLocked || distance <= model.distance;

        public bool IsVisible()
        {
            return true;
        }

        private bool ShouldReportPassportInputEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit) =>
            isHovering && (model.button == "ANY" || buttonId.ToString() == model.button);

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled)
                return;

            if (passportEnabled && ShouldReportPassportInputEvent(buttonId, hit))
            {
                eventHandler.SetFeedbackState(model.showFeedback, false, model.button, model.hoverText);
                passportEnabled = false;

                OnPointerDownReport?.Invoke();
            }

            if (onClickReportEnabled && ShouldReportOnClickEvent(buttonId, out IParcelScene playerScene))
            {
                AudioScriptableObjects.buttonClick.Play(true);

                WebInterface.ReportAvatarClick(
                    playerScene.sceneData.sceneNumber,
                    avatarPlayer.id,
                    WorldStateUtils.ConvertUnityToScenePosition(ray.origin, playerScene),
                    ray.direction,
                    hit.distance);
            }
        }

        public PointerInputEventType GetEventType()
        {
            return PointerInputEventType.DOWN;
        }

        void ReEnableOnInfoCardClosed(bool newState, bool prevState)
        {
            if (passportEnabled || newState)
                return;

            passportEnabled = true;
        }

        public void SetColliderEnabled(bool newEnabledState)
        {
            collider.enabled = newEnabledState;
        }

        public void SetPassportEnabled(bool newEnabledState)
        {
            passportEnabled = newEnabledState;
            isHovering = false;
        }

        public void SetOnClickReportEnabled(bool newEnabledState)
        {
            onClickReportEnabled = newEnabledState;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void OnPoolRelease()
        {
            eventHandler.Dispose();
            avatarPlayer = null;
        }

        public void OnPoolGet() { }

        private bool ShouldReportOnClickEvent(WebInterface.ACTION_BUTTON buttonId, out IParcelScene playerScene)
        {
            playerScene = null;

            if (buttonId != WebInterface.ACTION_BUTTON.POINTER) { return false; }

            if (avatarPlayer == null) { return false; }

            int playerSceneNumber = CommonScriptableObjects.sceneNumber.Get();

            if (playerSceneNumber <= 0) { return false; }

            playerScene = WorldStateUtils.GetCurrentScene();

            return playerScene?.IsInsideSceneBoundaries(
                PositionUtils.UnityToWorldPosition(avatarPlayer.worldPosition)) ?? false;
        }

        public bool ShouldShowHoverFeedback()
        {
            return enabled && model.showFeedback;
        }
    }
}
