using DCL.Configuration;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class GltfContainerCollidersHandler
    {
        private readonly List<Collider> invisibleMeshesColliders = new List<Collider>(10); // colliders from `_collider`
        private readonly List<Collider> visibleMeshesColliders = new List<Collider>(10); // colliders from renderers
        private readonly List<Collider> visibleMeshesInvalidColliders = new List<Collider>(10); // colliders created from sdk6 but not valid for sdk7
        private IReadOnlyCollection<Renderer> cachedRenderers;

        // We restore all modifications on colliders to avoid issues with sdk6 GLTFShape
        public void CleanUp()
        {
            for (int i = 0; i < invisibleMeshesColliders.Count; i++)
            {
                Collider collider = invisibleMeshesColliders[i];
                collider.enabled = true;
                collider.gameObject.layer = PhysicsLayers.characterOnlyLayer;
            }

            for (int i = 0; i < visibleMeshesColliders.Count; i++)
            {
                Collider collider = visibleMeshesColliders[i];
                collider.enabled = true;
                collider.gameObject.layer = PhysicsLayers.onPointerEventLayer;
            }

            for (int i = 0; i < visibleMeshesInvalidColliders.Count; i++)
            {
                Collider collider = visibleMeshesInvalidColliders[i];
                collider.enabled = true;
                collider.gameObject.layer = PhysicsLayers.onPointerEventLayer;
            }

            invisibleMeshesColliders.Clear();
            visibleMeshesColliders.Clear();
            visibleMeshesInvalidColliders.Clear();
        }

        // Since gltf are modified when creating colliders and stored in cache library with those modifications
        // we check if colliders already exists and disable them
        // otherwise we create them (disabled)
        public IReadOnlyList<Collider> InitInvisibleMeshesColliders(IReadOnlyList<MeshFilter> meshFilters)
        {
            invisibleMeshesColliders.Clear();

            for (int i = 0; i < meshFilters.Count; i++)
            {
                if (meshFilters[i].gameObject.layer == PhysicsLayers.characterOnlyLayer)
                {
                    Collider collider = meshFilters[i].gameObject.GetComponent<Collider>();
                    collider.enabled = false;
                    invisibleMeshesColliders.Add(collider);
                    continue;
                }

                if (!IsCollider(meshFilters[i]))
                    continue;

                MeshCollider newCollider = meshFilters[i].gameObject.AddComponent<MeshCollider>();
                newCollider.sharedMesh = meshFilters[i].sharedMesh;
                newCollider.enabled = false;
                invisibleMeshesColliders.Add(newCollider);
            }

            return invisibleMeshesColliders;
        }

        // Since gltf are modified when creating colliders and stored in cache library with those modifications
        // we check if colliders already exists and disable them
        // if `shouldCreateThem` we create them (disabled)
        public IReadOnlyList<Collider> InitVisibleMeshesColliders(IReadOnlyCollection<Renderer> renderers, bool shouldCreateThem)
        {
            cachedRenderers = renderers;

            try { SetVisibleMeshesColliders(renderers, !shouldCreateThem); }
            catch (Exception e) { Debug.LogException(e); }

            return visibleMeshesColliders;
        }

        // Create visible meshes colliders if there are none created yet
        // otherwise return the already created colliders
        public IReadOnlyList<Collider> GetVisibleMeshesColliders()
        {
            try
            {
                if (visibleMeshesColliders.Count == 0)
                    SetVisibleMeshesColliders(cachedRenderers, false);
            }
            catch (Exception e) { Debug.LogException(e); }

            return visibleMeshesColliders;
        }

        public IReadOnlyList<Collider> GetInVisibleMeshesColliders()
        {
            return invisibleMeshesColliders;
        }

        private void SetVisibleMeshesColliders(IReadOnlyCollection<Renderer> renderers, bool onlyGltfStoredColliders)
        {
            const string POINTER_COLLIDER_NAME = "OnPointerEventCollider";

            if (visibleMeshesColliders.Count > 0)
                return;

            foreach (Renderer renderer in renderers)
            {
                Transform rendererT = renderer.transform;

                if (TryFindAlreadyCreatedCollider(rendererT, out Collider collider))
                {
                    collider.enabled = false;

                    if (IsValidRendererForCollider(renderer)) { visibleMeshesColliders.Add(collider); }
                    else { visibleMeshesInvalidColliders.Add(collider); }

                    continue;
                }

                if (onlyGltfStoredColliders)
                    continue;

                MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                Mesh colliderMesh = meshFilter ? meshFilter.sharedMesh : null;

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
            const StringComparison IGNORE_CASE = StringComparison.CurrentCultureIgnoreCase;

            return meshFilter.name.Contains("_collider", IGNORE_CASE)
                   || meshFilter.transform.parent.name.Contains("_collider", IGNORE_CASE);
        }

        // Try to find if a collider is already created
        private static bool TryFindAlreadyCreatedCollider(Transform transform, out Collider outCollider)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);

                if (child.gameObject.layer != PhysicsLayers.onPointerEventLayer)
                    continue;

                outCollider = child.GetComponent<Collider>();
                return outCollider;
            }

            outCollider = null;
            return false;
        }

        // Disable colliders for SkinnedMeshes
        private static bool IsValidRendererForCollider(Renderer renderer)
        {
            return !(renderer is SkinnedMeshRenderer);
        }
    }
}
