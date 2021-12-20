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

        internal MeshCollider[] colliders;
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
            {
                if (colliders != null && colliders.Length > 0)
                    DestroyColliders();

                return;
            }

            if (AreCollidersCreated(rendererList))
            {
                UpdateCollidersWithRenderersMesh(rendererList);
                return;
            }

            IShape shape = entity.meshesInfo.currentShape;

            ownerEntity = entity;

            DestroyColliders();

            if (shape == null)
                return;

            colliders = new MeshCollider[rendererList.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                if (rendererList[i] == null)
                    continue;
                colliders[i] = CreateCollider(rendererList[i]);
            }
        }

        void UpdateCollidersWithRenderersMesh(Renderer[] rendererList)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if(colliders[i].sharedMaterial == null)
                    colliders[i].sharedMesh = rendererList[i].GetComponent<MeshFilter>().sharedMesh;
            }
        }

        MeshCollider CreateCollider(Renderer renderer)
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

        private bool AreCollidersCreated(Renderer[] rendererList)
        {
            if (colliders == null || colliders.Length == 0)
                return false;

            if (rendererList.Length != colliders.Length)
                return false;

            for (int i = 0; i < rendererList.Length; i++)
            {
                bool foundChildCollider = false;
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (colliders[j].transform.parent == rendererList[i].transform)
                    {
                        foundChildCollider = true;
                        break;
                    }
                }

                if (!foundChildCollider)
                    return false;
            }

            return true;
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

            colliders = null;
        }
    }
}