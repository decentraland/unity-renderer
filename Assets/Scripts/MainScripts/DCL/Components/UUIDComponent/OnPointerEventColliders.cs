using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Components
{

    public class OnPointerEventColliders : MonoBehaviour
    {
        public const string COLLIDER_LAYER = "OnPointerEvent";
        public const string COLLIDER_NAME = "OnPointerEventCollider";
        public int RefCount { get; set; }

        GameObject[] OnPointerEventColliderGameObjects;

        public string meshName
        {
            get;
            private set;
        }

        public void Initialize(DecentralandEntity entity)
        {
            RefCount++;

            var renderers = entity.meshGameObject.GetComponentsInChildren<Renderer>(true);

            // Cache mesh name
            meshName = renderers[0].transform.parent?.name;

            DestroyOnPointerEventColliders();

            OnPointerEventColliderGameObjects = new GameObject[renderers.Length];
            for (int i = 0; i < OnPointerEventColliderGameObjects.Length; i++)
            {
                OnPointerEventColliderGameObjects[i] = CreatePointerEventCollider(renderers[i]);
            }
        }

        GameObject CreatePointerEventCollider(Renderer renderer)
        {
            GameObject go = new GameObject(COLLIDER_NAME);

            go.name = COLLIDER_NAME;
            go.layer = LayerMask.NameToLayer(COLLIDER_LAYER); // to avoid character collisions with onclick collider

            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.enabled = renderer.enabled;

            // Reset objects position, rotation and scale once it's been parented
            Transform t = go.transform;

            t.SetParent(renderer.transform);
            Utils.ResetLocalTRS(t);

            return go;
        }

        void OnDestroy()
        {
            DestroyOnPointerEventColliders();
        }

        void DestroyOnPointerEventColliders()
        {
            if (OnPointerEventColliderGameObjects == null)
            {
                return;
            }

            for (int i = 0; i < OnPointerEventColliderGameObjects.Length; i++)
            {
                Destroy(OnPointerEventColliderGameObjects[i]);
            }
        }
    }
}