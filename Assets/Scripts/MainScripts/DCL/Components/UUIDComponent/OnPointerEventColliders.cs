using DCL.Helpers;
using DCL.Models;
using UnityEngine;
using System.Collections.Generic;

namespace DCL.Components
{

    public class OnPointerEventColliders : MonoBehaviour
    {
        public const string COLLIDER_LAYER = "OnPointerEvent";
        public const string COLLIDER_NAME = "OnPointerEventCollider";

        [System.NonSerialized] public int refCount;

        GameObject[] pointerEventColliderGameObjects;
        Dictionary<Collider, string> colliderNames = new Dictionary<Collider, string>();

        public string GetMeshName(Collider collider)
        {
            if (colliderNames.ContainsKey(collider))
                return colliderNames[collider];

            return null;
        }

        public void Initialize(DecentralandEntity entity)
        {
            refCount++;

            var renderers = entity.meshGameObject.GetComponentsInChildren<Renderer>(true);

            // Cache mesh name
            DestroyOnPointerEventColliders();

            pointerEventColliderGameObjects = new GameObject[renderers.Length];
            for (int i = 0; i < pointerEventColliderGameObjects.Length; i++)
            {
                pointerEventColliderGameObjects[i] = CreatePointerEventCollider(renderers[i]);
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

            if (renderer.transform.parent != null)
                colliderNames.Add(meshCollider, renderer.transform.parent.name);

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
            if (pointerEventColliderGameObjects == null)
            {
                return;
            }

            for (int i = 0; i < pointerEventColliderGameObjects.Length; i++)
            {
                Destroy(pointerEventColliderGameObjects[i]);
            }
        }
    }
}