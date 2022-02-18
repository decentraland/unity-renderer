﻿using System.Collections.Generic;
using System.Linq;
using DCL.Helpers;
using DCL.Models;
using UnityEngine;

namespace DCL.Controllers
{
    public class SceneBoundsFeedbackStyle_RedBox : ISceneBoundsFeedbackStyle
    {
        class InvalidMeshInfo
        {
            public List<GameObject> wireframeObjects = new List<GameObject>();
            public MeshesInfo meshesInfo;
            public System.Action OnResetAll;

            public InvalidMeshInfo(MeshesInfo meshesInfo) { this.meshesInfo = meshesInfo; }

            public void ResetAll()
            {
                if (meshesInfo.meshRootGameObject == null)
                    return;

                int count = wireframeObjects.Count;

                for (int i = 0; i < count; i++)
                {
                    Utils.SafeDestroy(wireframeObjects[i]);
                }

                OnResetAll?.Invoke();
            }
        }

        const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

        private Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo;
        private HashSet<Renderer> invalidRenderers;

        public SceneBoundsFeedbackStyle_RedBox()
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidRenderers = new HashSet<Renderer>();
        }

        public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (isInsideBoundaries)
            {
                RemoveInvalidMeshEffect(meshesInfo);
                return;
            }

            AddInvalidMeshEffect(meshesInfo);
        }

        public void CleanFeedback()
        {
            IEnumerable<MeshesInfo> distinctMeshesInfo = invalidMeshesInfo.Values.Select(x => x.meshesInfo).Distinct();

            foreach (var info in distinctMeshesInfo)
            {
                RemoveInvalidMeshEffect(info);
            }

            invalidMeshesInfo.Clear();
            invalidRenderers.Clear();
        }

        void RemoveInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (meshesInfo == null)
                return;

            if (!invalidMeshesInfo.ContainsKey(meshesInfo.innerGameObject))
                return;

            PoolableObject poolableObject = PoolManager.i.GetPoolable(meshesInfo.meshRootGameObject);

            if (poolableObject != null)
                poolableObject.OnRelease -= invalidMeshesInfo[meshesInfo.innerGameObject].ResetAll;

            var renderers = meshesInfo.renderers;

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    if (invalidRenderers.Contains(renderers[i]))
                        invalidRenderers.Remove(renderers[i]);
                }
            }

            invalidMeshesInfo[meshesInfo.innerGameObject].ResetAll();
            invalidMeshesInfo.Remove(meshesInfo.innerGameObject);
        }

        void AddInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (meshesInfo == null)
                return;

            if (invalidMeshesInfo.ContainsKey(meshesInfo.innerGameObject))
                return;

            InvalidMeshInfo invalidMeshInfo = new InvalidMeshInfo(meshesInfo);
            PoolableObject poolableObject = PoolManager.i.GetPoolable(meshesInfo.meshRootGameObject);

            if (poolableObject != null)
            {
                poolableObject.OnRelease -= invalidMeshInfo.ResetAll;
                poolableObject.OnRelease += invalidMeshInfo.ResetAll;
            }

            // Apply invalid effect
            Renderer[] entityRenderers = meshesInfo.renderers;

            for (int i = 0; i < entityRenderers.Length; i++)
            {
                if (invalidRenderers.Contains(entityRenderers[i]))
                    continue;

                var box = PutBoxAroundRenderer(entityRenderers[i], meshesInfo.innerGameObject.transform);

                invalidMeshInfo.wireframeObjects.Add(box);
                invalidRenderers.Add(entityRenderers[i]);
            }

            invalidMeshesInfo.Add(meshesInfo.innerGameObject, invalidMeshInfo);
        }

        private GameObject PutBoxAroundRenderer(Renderer target, Transform parent)
        {
            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            GameObject wireframeObject = Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
            wireframeObject.transform.position = target.bounds.center;
            wireframeObject.transform.localScale = target.bounds.size * 1.01f;
            wireframeObject.transform.SetParent(parent);
            return wireframeObject;
        }
        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            return meshesInfo.renderers.SelectMany((x) => x.sharedMaterials).ToList();
        }
    }
}