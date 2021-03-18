using System;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Components
{
    public class OnPointerEventColliders : IDisposable
    {
        public const string COLLIDER_NAME = "OnPointerEventCollider";

        [System.NonSerialized]
        public int refCount;

        Collider[] pointerEventColliders;
        Dictionary<Collider, string> colliderNames = new Dictionary<Collider, string>();

        public string GetMeshName(Collider collider)
        {
            if (colliderNames.ContainsKey(collider))
                return colliderNames[collider];

            return null;
        }

        private DecentralandEntity ownerEntity;

        public void Initialize(DecentralandEntity entity)
        {
            if (entity == null || entity.meshesInfo == null)
                return;

            Renderer[] rendererList = entity.meshesInfo.renderers;

            if (rendererList == null || rendererList.Length == 0) return;

            this.ownerEntity = entity;

            DestroyOnPointerEventColliders();

            pointerEventColliders = new Collider[rendererList.Length];

            for (int i = 0; i < pointerEventColliders.Length; i++)
            {
                pointerEventColliders[i] = CreatePointerEventCollider(rendererList[i]);
            }
        }

        Collider CreatePointerEventCollider(Renderer renderer)
        {
            GameObject go = new GameObject(COLLIDER_NAME);

            go.name = COLLIDER_NAME;
            go.layer = PhysicsLayers.onPointerEventLayer; // to avoid character collisions with onclick collider

            var meshCollider = go.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = renderer.GetComponent<MeshFilter>().sharedMesh;
            meshCollider.enabled = renderer.enabled;

            if (renderer.transform.parent != null)
                colliderNames.Add(meshCollider, renderer.transform.parent.name);

            CollidersManager.i.AddOrUpdateEntityCollider(ownerEntity, meshCollider);

            // Reset objects position, rotation and scale once it's been parented
            Transform t = go.transform;

            t.SetParent(renderer.transform);
            Utils.ResetLocalTRS(t);

            return meshCollider;
        }

        public void Dispose()
        {
            DestroyOnPointerEventColliders();
        }

        void DestroyOnPointerEventColliders()
        {
            if (pointerEventColliders == null)
                return;

            for (int i = 0; i < pointerEventColliders.Length; i++)
            {
                Collider collider = pointerEventColliders[i];

                if (collider != null)
                    GameObject.Destroy(collider.gameObject);
            }
        }
    }
}