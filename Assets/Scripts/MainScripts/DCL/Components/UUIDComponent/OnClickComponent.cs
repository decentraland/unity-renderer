using DCL.Controllers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{
    public class OnClickComponent : UUIDComponent
    {
        Rigidbody rigidBody;
        GameObject[] OnClickColliderGameObjects;

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

        public void Initialize()
        {
            if (!entity.meshGameObject) return;

            var renderers = entity.meshGameObject.GetComponentsInChildren<Renderer>(true);

            if (renderers == null || renderers.Length == 0) return;

            // we add a rigidbody to be able to detect the children colliders for the OnClick functionality
            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                rigidBody = gameObject.AddComponent<Rigidbody>();
                rigidBody.useGravity = false;
                rigidBody.isKinematic = true;
            }

            // Create OnClickCollider child
            var onClickColliderObjectName = "OnClickCollider";
            var onClickColliderObjectLayer = LayerMask.NameToLayer("OnClick");

            DestroyOnClickColliders();

            OnClickColliderGameObjects = new GameObject[renderers.Length];
            for (int i = 0; i < OnClickColliderGameObjects.Length; i++)
            {
                OnClickColliderGameObjects[i] = new GameObject();
                OnClickColliderGameObjects[i].name = onClickColliderObjectName;
                OnClickColliderGameObjects[i].layer = onClickColliderObjectLayer; // to avoid character collisions with onclick collider

                var meshCollider = OnClickColliderGameObjects[i].AddComponent<MeshCollider>();
                meshCollider.sharedMesh = renderers[i].GetComponent<MeshFilter>().sharedMesh;
                meshCollider.enabled = renderers[i].enabled;

                // Reset objects position, rotation and scale once it's been parented
                OnClickColliderGameObjects[i].transform.SetParent(renderers[i].transform);
                OnClickColliderGameObjects[i].transform.localScale = Vector3.one;
                OnClickColliderGameObjects[i].transform.localRotation = Quaternion.identity;
                OnClickColliderGameObjects[i].transform.localPosition = Vector3.zero;
            }
        }

        void OnComponentUpdated(DecentralandEntity e)
        {
            Initialize();
        }

        public void OnPointerDown()
        {
            if (!enabled)
            {
                return;
            }

            DCL.Interface.WebInterface.ReportOnClickEvent(scene.sceneData.id, model.uuid);
        }

        void OnDestroy()
        {
            entity.OnShapeUpdated -= OnComponentUpdated;

            Destroy(rigidBody);

            DestroyOnClickColliders();
        }

        void DestroyOnClickColliders()
        {
            if (OnClickColliderGameObjects == null)
            {
                return;
            }

            for (int i = 0; i < OnClickColliderGameObjects.Length; i++)
            {
                Destroy(OnClickColliderGameObjects[i]);
            }
        }
    }
}