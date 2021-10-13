using DCL.Interface;
using UnityEngine;
using DCL.Helpers;
using DCL.Models;
using Ray = UnityEngine.Ray;

namespace DCL.Components
{
    public class AvatarOnPointerDown : MonoBehaviour, IPointerEvent, IPoolLifecycleHandler, IAvatarOnPointerDownCollider
    {
        public new Collider collider;
        private OnPointerEvent.Model model;
        private OnPointerEventHandler eventHandler;
        public IDCLEntity entity { get; private set; }
        public event System.Action OnPointerDownReport;

        public WebInterface.ACTION_BUTTON GetActionButton() { return model.GetActionButton(); }

        public void SetHoverState(bool state) { eventHandler.SetFeedbackState(model.showFeedback, state, model.button, model.hoverText); }

        void Awake() { CommonScriptableObjects.playerInfoCardVisibleState.OnChange += ReEnableOnInfoCardClosed; }

        void OnDestroy()
        {
            CommonScriptableObjects.playerInfoCardVisibleState.OnChange -= ReEnableOnInfoCardClosed;
            eventHandler?.Dispose();
            CollidersManager.i.RemoveEntityCollider(entity, collider);
        }

        public void Initialize(OnPointerEvent.Model model, IDCLEntity entity)
        {
            this.model = model;
            this.entity = entity;

            if (eventHandler == null)
                eventHandler = new OnPointerEventHandler();

            eventHandler?.SetColliders(entity);
            CollidersManager.i.AddOrUpdateEntityCollider(entity, collider);
        }

        public bool IsAtHoverDistance(float distance) { return distance <= model.distance; }

        public bool IsVisible() { return true; }

        public bool ShouldReportEvent(WebInterface.ACTION_BUTTON buttonId, HitInfo hit)
        {
            return IsAtHoverDistance(hit.distance) &&
                   (model.button == "ANY" || buttonId.ToString() == model.button);
        }

        public void Report(WebInterface.ACTION_BUTTON buttonId, Ray ray, HitInfo hit)
        {
            if (!enabled)
                return;

            if (ShouldReportEvent(buttonId, hit))
            {
                string meshName = null;

                if (hit.collider != null)
                    meshName = eventHandler.GetMeshName(hit.collider);

                WebInterface.ReportOnPointerDownEvent(
                    buttonId,
                    entity.scene.sceneData.id,
                    model.uuid,
                    entity.entityId,
                    meshName,
                    ray,
                    hit.point,
                    hit.normal,
                    hit.distance);

                eventHandler.SetFeedbackState(model.showFeedback, false, model.button, model.hoverText);
                enabled = false;

                OnPointerDownReport?.Invoke();
            }
        }

        public PointerEventType GetEventType() { return PointerEventType.DOWN; }

        void ReEnableOnInfoCardClosed(bool newState, bool prevState)
        {
            if (enabled || newState)
                return;

            enabled = true;
        }
        public void ToggleCollider(bool newEnabledState)
        {
            collider.enabled = newEnabledState;
        }

        public Transform GetTransform() { return transform; }

        public void OnPoolRelease() { eventHandler.Dispose(); }

        public void OnPoolGet() { }
    }
}