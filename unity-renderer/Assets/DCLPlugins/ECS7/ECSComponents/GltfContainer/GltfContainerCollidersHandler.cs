using DCL.Configuration;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.ECSComponents
{
    public class GltfContainerCollidersHandler
    {
        private readonly List<Collider> invisibleMeshesColliders = new List<Collider>(10); // colliders from `_collider`
        private readonly List<Collider> visibleMeshesColliders = new List<Collider>(10); // colliders from renderers
        private IReadOnlyCollection<Renderer> cachedRenderers;

        // We restore all modifications on colliders to avoid issues with sdk6 GLTFShape
        public void CleanUp()
        {
            for (int i = 0; i < invisibleMeshesColliders.Count; i++)
            {
                Collider collider = invisibleMeshesColliders[i];
                collider.enabled = false;
                collider.gameObject.layer = PhysicsLayers.characterOnlyLayer;
            }

            for (int i = 0; i < visibleMeshesColliders.Count; i++)
            {
                Collider collider = visibleMeshesColliders[i];

                if (collider)
                    Object.Destroy(collider);
            }

            invisibleMeshesColliders.Clear();
            visibleMeshesColliders.Clear();
        }

        // Since gltf are modified when creating colliders and stored in cache library with those modifications
        // we check if colliders already exists and disable them
        // otherwise we create them (disabled)
        public IReadOnlyList<Collider> InitInvisibleMeshesColliders(IReadOnlyList<MeshFilter> meshFilters)
        {
            invisibleMeshesColliders.Clear();

            for (int i = 0; i < meshFilters.Count; i++)
            {
                if (!IsCollider(meshFilters[i]))
                    continue;

                Collider collider = meshFilters[i].gameObject.GetComponent<Collider>();

                if (collider != null)
                {
                    collider.enabled = false;
                    invisibleMeshesColliders.Add(collider);
                    continue;
                }

                MeshCollider newCollider = meshFilters[i].gameObject.AddComponent<MeshCollider>();
                newCollider.sharedMesh = meshFilters[i].sharedMesh;
                newCollider.enabled = false;
                invisibleMeshesColliders.Add(newCollider);
            }

            return invisibleMeshesColliders;
        }

        // Since gltf are modified when creating colliders and stored in cache library with those modifications
        // we check if colliders already exists and disable them
        // if `createColliders` we create them (disabled)
        public IReadOnlyList<Collider> InitVisibleMeshesColliders(IReadOnlyCollection<Renderer> renderers, bool createColliders)
        {
            cachedRenderers = renderers;

            try
            {
                if (createColliders)
                {
                    SetVisibleMeshesColliders(renderers);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return visibleMeshesColliders;
        }

        // Create visible meshes colliders if there are none created yet
        // otherwise return the already created colliders
        public IReadOnlyList<Collider> GetVisibleMeshesColliders()
        {
            try
            {
                if (visibleMeshesColliders.Count == 0)
                    SetVisibleMeshesColliders(cachedRenderers);
            }
            catch (Exception e) { Debug.LogException(e); }

            return visibleMeshesColliders;
        }

        public IReadOnlyList<Collider> GetInVisibleMeshesColliders()
        {
            return invisibleMeshesColliders;
        }

        private void SetVisibleMeshesColliders(IReadOnlyCollection<Renderer> renderers)
        {
            const string POINTER_COLLIDER_NAME = "OnPointerEventCollider";

            if (visibleMeshesColliders.Count > 0)
                return;

            foreach (Renderer renderer in renderers)
            {
                Mesh colliderMesh = null;

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    colliderMesh = skinnedMeshRenderer.sharedMesh;
                }
                else
                {
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    colliderMesh = meshFilter ? meshFilter.sharedMesh : null;
                }

                if (!colliderMesh)
                    continue;

                GameObject colliderGo = new GameObject(POINTER_COLLIDER_NAME);
                MeshCollider newCollider = colliderGo.AddComponent<MeshCollider>();

                newCollider.sharedMesh = colliderMesh;
                newCollider.enabled = false;
                colliderGo.transform.SetParent(renderer.transform);
                colliderGo.transform.ResetLocalTRS();
                visibleMeshesColliders.Add(newCollider);
            }
        }

        // Compatibility layer for old GLTF importer and GLTFast
        private static bool IsCollider(MeshFilter meshFilter)
        {
            const StringComparison IGNORE_CASE = StringComparison.OrdinalIgnoreCase;
            const string COLLIDER_SUFFIX = "_collider";

            return meshFilter.name.Contains(COLLIDER_SUFFIX, IGNORE_CASE)
                   || meshFilter.transform.parent.name.Contains(COLLIDER_SUFFIX, IGNORE_CASE);
        }
    }
}
