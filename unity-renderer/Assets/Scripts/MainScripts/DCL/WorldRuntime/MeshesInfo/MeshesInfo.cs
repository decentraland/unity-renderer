using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Components;
using TMPro;
using UnityEngine;

namespace DCL.Models
{
    /// <summary>
    /// (NOTE) Kinerius: this class is a data holder that was made in the past for multiple systems to have access to this specific entity data.
    /// This has a big architectural issue where if we decide to remove any component (renderer, mesh filter, collider) from the gameObject
    /// from anywhere within the system, we have to explicitly update this class values, it makes no sense,
    /// we need to reconsider this class functionality and purpose since it has an ambiguous design and it might be the source of many bugs to come.
    /// </summary>
    [Serializable]
    public class MeshesInfo
    {
        public static event Action OnAnyUpdated;
        public event Action OnUpdated;
        public event Action OnCleanup;

        public GameObject innerGameObject;

        public GameObject meshRootGameObject
        {
            get
            {
                return meshRootGameObjectValue;
            }

            set
            {
                meshRootGameObjectValue = value;
                UpdateRenderersCollection();
            }
        }

        GameObject meshRootGameObjectValue;

        public IShape currentShape;
        public Renderer[] renderers { get; private set; }
        public MeshFilter[] meshFilters;
        public HashSet<Collider> colliders = new HashSet<Collider>();
        public Animation animation { get; private set; }

        Vector3 lastBoundsCalculationPosition;
        Vector3 lastBoundsCalculationScale;
        Quaternion lastBoundsCalculationRotation;
        Bounds mergedBoundsValue;

        public Bounds mergedBounds
        {
            get
            {
                if (meshRootGameObject == null) { RecalculateBounds(); }
                else
                {
                    if (meshRootGameObject.transform.position != lastBoundsCalculationPosition)
                    {
                        mergedBoundsValue.center += meshRootGameObject.transform.position - lastBoundsCalculationPosition;
                        lastBoundsCalculationPosition = meshRootGameObject.transform.position;
                    }

                    if (meshRootGameObject.transform.lossyScale != lastBoundsCalculationScale ||
                        meshRootGameObject.transform.rotation != lastBoundsCalculationRotation)
                        RecalculateBounds();
                }

                return mergedBoundsValue;
            }

            set
            {
                mergedBoundsValue = value;
            }
        }

        public void UpdateRenderersCollection(Renderer[] renderers, MeshFilter[] meshFilters, Animation animation = null)
        {
            if (meshRootGameObjectValue != null)
            {
                this.renderers = renderers;
                this.meshFilters = meshFilters;
                this.animation = animation;

                RecalculateBounds();
                OnAnyUpdated?.Invoke();
                OnUpdated?.Invoke();
            }
        }

        public void UpdateRenderersCollection()
        {
            if (meshRootGameObjectValue == null)
                return;

            renderers = meshRootGameObjectValue.GetComponentsInChildren<Renderer>(true);
            meshFilters = meshRootGameObjectValue.GetComponentsInChildren<MeshFilter>(true);
            animation = meshRootGameObjectValue.GetComponentInChildren<Animation>();

            TextMeshPro[] tmpros = meshRootGameObjectValue.GetComponentsInChildren<TextMeshPro>(true);

            if (tmpros.Length > 0)
            {
                renderers = renderers.Union(tmpros.Select(x => x.renderer)).ToArray();
                meshFilters = meshFilters.Union(tmpros.Select(x => x.meshFilter)).ToArray();
            }

            RecalculateBounds();
            OnAnyUpdated?.Invoke();
            OnUpdated?.Invoke();
        }

        public async UniTask RecalculateBounds()
        {
            if ((renderers == null || renderers.Length == 0) && colliders.Count == 0)
            {
                mergedBoundsValue = new Bounds();
                return;
            }

            await UniTask.WaitForFixedUpdate();

            if (meshRootGameObjectValue == null) return;

            lastBoundsCalculationPosition = meshRootGameObjectValue.transform.position;
            lastBoundsCalculationScale = meshRootGameObjectValue.transform.lossyScale;
            lastBoundsCalculationRotation = meshRootGameObjectValue.transform.rotation;

            mergedBoundsValue = MeshesInfoUtils.BuildMergedBounds(renderers, colliders);
        }

        public void CleanReferences()
        {
            OnCleanup?.Invoke();
            meshRootGameObjectValue = null;
            animation = null;
            currentShape = null;
            renderers = null;
            colliders.Clear();

            var arrayLength = meshFilters.Length;
            for (int i = 0; i < arrayLength; i++)
            {
                meshFilters[i] = null;
            }
            innerGameObject = null;

            OnCleanup = null;
        }

        public void UpdateExistingMeshAtIndex(Mesh mesh, uint meshFilterIndex = 0)
        {
            if (meshFilters != null && meshFilters.Length > meshFilterIndex)
            {
                meshFilters[meshFilterIndex].sharedMesh = mesh;
                OnUpdated?.Invoke();
            }
            else
            {
                Debug.LogError(
                    $"MeshFilter index {meshFilterIndex} out of bounds - MeshesInfo.UpdateExistingMesh failed");
            }
        }

        public void OverrideRenderers(Renderer[] renderers)
        {
            this.renderers = renderers.Where(r => r != null).ToArray();
        }
    }
}
