﻿using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Components;
using TMPro;
using UnityEngine;

namespace DCL.Models
{
    [Serializable]
    public class MeshesInfo
    {
        public static event Action OnAnyUpdated;
        public event Action OnUpdated;
        public event Action OnCleanup;

        public GameObject innerGameObject;

        public GameObject meshRootGameObject
        {
            get { return meshRootGameObjectValue; }
            set
            {
                meshRootGameObjectValue = value;

                UpdateRenderersCollection();
            }
        }

        GameObject meshRootGameObjectValue;

        public IShape currentShape;
        public Renderer[] renderers;
        public MeshFilter[] meshFilters;
        public List<Collider> colliders = new List<Collider>();

        Vector3 lastBoundsCalculationPosition;
        Vector3 lastBoundsCalculationScale;
        Quaternion lastBoundsCalculationRotation;
        Bounds mergedBoundsValue;

        public Bounds mergedBounds
        {
            get
            {
                if (meshRootGameObject.transform.position != lastBoundsCalculationPosition)
                {
                    mergedBoundsValue.center += meshRootGameObject.transform.position - lastBoundsCalculationPosition;
                    lastBoundsCalculationPosition = meshRootGameObject.transform.position;
                }

                if (meshRootGameObject.transform.lossyScale != lastBoundsCalculationScale || meshRootGameObject.transform.rotation != lastBoundsCalculationRotation)
                    RecalculateBounds();

                return mergedBoundsValue;
            }
            set { mergedBoundsValue = value; }
        }

        public void UpdateRenderersCollection()
        {
            if (meshRootGameObjectValue != null)
            {
                renderers = meshRootGameObjectValue.GetComponentsInChildren<Renderer>(true);
                meshFilters = meshRootGameObjectValue.GetComponentsInChildren<MeshFilter>(true);

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
        }

        public void RecalculateBounds()
        {
            if (renderers == null || renderers.Length == 0)
                return;

            lastBoundsCalculationPosition = meshRootGameObject.transform.position;
            lastBoundsCalculationScale = meshRootGameObject.transform.lossyScale;
            lastBoundsCalculationRotation = meshRootGameObject.transform.rotation;

            mergedBoundsValue = MeshesInfoUtils.BuildMergedBounds(renderers);
        }

        public void CleanReferences()
        {
            OnCleanup?.Invoke();
            meshRootGameObjectValue = null;
            currentShape = null;
            renderers = null;
            colliders.Clear();
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
                Debug.LogError($"MeshFilter index {meshFilterIndex} out of bounds - MeshesInfo.UpdateExistingMesh failed");
            }
        }
    }
}