using DCL.Controllers;
using DCL.Models;
using DCL.Helpers;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEventComponent : UUIDComponent
    {
        Rigidbody rigidBody;
        OnPointerEventColliders pointerEventColliders;

        public override void Setup(ParcelScene scene, DecentralandEntity entity, string uuid, string type)
        {
            this.entity = entity;
            this.scene = scene;
            this.model.uuid = uuid;
            this.model.type = type;

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
            if (!entity.meshGameObject) return;

            var renderers = entity.meshGameObject.GetComponentsInChildren<Renderer>(true);

            if (renderers == null || renderers.Length == 0) return;

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
        }

        void OnComponentUpdated(DecentralandEntity e)
        {
            Initialize();
        }

        void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;

            if (pointerEventColliders)
            {
                pointerEventColliders.refCount--;

                if (pointerEventColliders.refCount <= 0)
                {
                    Destroy(rigidBody);
                    Destroy(pointerEventColliders);
                }
            }
        }
    }
}