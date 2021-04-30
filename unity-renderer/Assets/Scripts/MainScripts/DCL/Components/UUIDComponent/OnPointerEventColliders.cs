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

        Collider[] colliders;
        Dictionary<Collider, string> colliderNames = new Dictionary<Collider, string>();

        public string GetMeshName(Collider collider)
        {
            if (colliderNames.ContainsKey(collider))
                return colliderNames[collider];

            return null;
        }

        private IDCLEntity ownerEntity;

        public void Initialize(IDCLEntity entity)
        {
            Renderer[] rendererList = entity?.meshesInfo?.renderers;

            if (rendererList == null || rendererList.Length == 0)
                return;

            IShape shape = entity.meshesInfo.currentShape;
            
            ownerEntity = entity;

            DestroyColliders();

            if (shape == null)
                return;

            colliders = new Collider[rendererList.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i] = CreateCollider(rendererList[i]);
            }
        }

        Collider CreateCollider(Renderer renderer)
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
            t.ResetLocalTRS();

            return meshCollider;
        }

        public void Dispose() { DestroyColliders(); }

        void DestroyColliders()
        {
            if (colliders == null)
                return;

            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];

                if (collider != null)
                    UnityEngine.Object.Destroy(collider.gameObject);
            }
        }
    }
}