using DCL.Controllers;
using DCL.Models;
using DCL.Helpers;
using DCL.Interface;
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
            public float distance = 4f;
            public bool showFeeback = true;
        }

        Rigidbody rigidBody;
        OnPointerEventColliders pointerEventColliders;
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

        public void Initialize()
        {
            if (!entity.meshRootGameObject) return;

            // we add a rigidbody to the entity's gameobject to have a
            // reference to the entity itself on the RaycastHit
            // so we don't need to search for the parents in order to get
            // the OnPointerEventComponent reference
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                rigidBody = gameObject.AddComponent<Rigidbody>();
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }

            // Create OnPointerEventCollider child
            pointerEventColliders = Utils.GetOrCreateComponent<OnPointerEventColliders>(this.gameObject);
            pointerEventColliders.Initialize(entity);

            if (hoverCanvasController == null)
            {
                GameObject hoverCanvasGameObject = Object.Instantiate(Resources.Load("InteractionHoverCanvas"), PointerEventsController.i.transform) as GameObject;
                hoverCanvasController = hoverCanvasGameObject.GetComponent<InteractionHoverCanvasController>();
            }

            hoverCanvasController.enabled = model.showFeeback;
            hoverCanvasController.Setup(model.button, model.hoverText, entity);
        }

        void OnComponentUpdated(DecentralandEntity e)
        {
            Initialize();
        }

        protected override void RemoveComponent<T>(DecentralandEntity entity)
        {
            if (hoverCanvasController != null)
            {
                Destroy(hoverCanvasController.gameObject);
            }
        }

        public void SetHoverState(bool hoverState)
        {
            if (!enableInteractionHoverFeedback) return;

            hoverCanvasController.SetHoverState(hoverState);
        }

        public bool IsAtHoverDistance(float distance)
        {
            return distance <= model.distance;
        }

        void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;

            if (pointerEventColliders != null)
            {
                pointerEventColliders.refCount--;

                if (pointerEventColliders.refCount <= 0)
                {
                    Destroy(rigidBody);
                    Destroy(pointerEventColliders);
                }
            }

            if (hoverCanvasController != null)
            {
                Destroy(hoverCanvasController.gameObject);
            }
        }
    }
}