using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEvent : UUIDComponent<OnPointerEvent.Model>
    {
        public static bool enableInteractionHoverFeedback = true;

        [System.Serializable]
        new public class Model : UUIDComponent.Model
        {
            public string button = WebInterface.ACTION_BUTTON.ANY.ToString();
            public string hoverText = "Interact";
            public float distance = 10f;
            public bool showFeedback = true;
        }

        public OnPointerEventColliders pointerEventColliders
        {
            get;
            private set;
        }

        InteractionHoverCanvasController hoverCanvasController;

        public override void Setup(ParcelScene scene, DecentralandEntity entity, UUIDComponent.Model model)
        {
            this.entity = entity;
            this.scene = scene;

            if (model == null)
                this.model = new OnPointerEvent.Model();
            else
                this.model = (OnPointerEvent.Model)model;

            Initialize();

            entity.OnShapeUpdated -= OnComponentUpdated;
            entity.OnShapeUpdated += OnComponentUpdated;
        }

        public string GetMeshName(Collider collider)
        {
            return pointerEventColliders.GetMeshName(collider);
        }

        public WebInterface.ACTION_BUTTON GetActionButton()
        {
            switch (model.button)
            {
                case "PRIMARY":
                    return WebInterface.ACTION_BUTTON.PRIMARY;
                case "SECONDARY":
                    return WebInterface.ACTION_BUTTON.SECONDARY;
                case "POINTER":
                    return WebInterface.ACTION_BUTTON.POINTER;
                default:
                    return WebInterface.ACTION_BUTTON.ANY;
            }
        }

        public void Initialize()
        {
            // Create OnPointerEventCollider child
            pointerEventColliders = Utils.GetOrCreateComponent<OnPointerEventColliders>(this.gameObject);
            pointerEventColliders.Initialize(entity);
            pointerEventColliders.refCount++;

            if (hoverCanvasController == null)
                hoverCanvasController = PointerEventsController.i.interactionHoverCanvasController;
        }

        public bool IsVisible()
        {
            if (entity == null)
                return false;

            bool isVisible = false;

            if (this is AvatarOnPointerDown)
                isVisible = true;
            else if (entity.meshesInfo != null && entity.meshesInfo.renderers != null && entity.meshesInfo.renderers.Length > 0)
                isVisible = entity.meshesInfo.renderers[0].enabled;

            return isVisible;
        }

        void OnComponentUpdated(DecentralandEntity e)
        {
            Initialize();
        }

        public void SetHoverState(bool hoverState)
        {
            if (!enableInteractionHoverFeedback || !enabled) return;

            hoverCanvasController.enabled = model.showFeedback;
            if (model.showFeedback)
            {
                if (hoverState)
                    hoverCanvasController.Setup(model.button, model.hoverText, entity);

                hoverCanvasController.SetHoverState(hoverState);
            }
        }

        public bool IsAtHoverDistance(Transform other)
        {
            return Vector3.Distance(other.position, transform.position) <= model.distance;
        }
        public bool IsAtHoverDistance(float distance)
        {
            return distance <= model.distance;
        }

        void OnDestroy()
        {
            if (entity != null)
                entity.OnShapeUpdated -= OnComponentUpdated;

            if (pointerEventColliders != null)
            {
                pointerEventColliders.refCount--;

                if (pointerEventColliders.refCount <= 0)
                {
                    Destroy(pointerEventColliders);
                }
            }
        }
    }
}
