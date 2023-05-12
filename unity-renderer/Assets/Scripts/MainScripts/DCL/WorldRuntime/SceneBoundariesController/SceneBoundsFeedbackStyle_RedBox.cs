using System.Collections.Generic;
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
                    wireframeObjects[i].transform.parent = null;
                    Utils.SafeDestroy(wireframeObjects[i]);
                }

                OnResetAll?.Invoke();
            }
        }

        const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

        private Dictionary<GameObject, InvalidMeshInfo> invalidMeshesInfo;
        private HashSet<Bounds> invalidObjects;

        public SceneBoundsFeedbackStyle_RedBox()
        {
            invalidMeshesInfo = new Dictionary<GameObject, InvalidMeshInfo>();
            invalidObjects = new HashSet<Bounds>();
        }

        public void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (isInsideBoundaries)
                RemoveInvalidMeshEffect(meshesInfo);
            else
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
            invalidObjects.Clear();
        }

        void RemoveInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (meshesInfo == null)
                return;

            if (meshesInfo.innerGameObject == null || !invalidMeshesInfo.ContainsKey(meshesInfo.innerGameObject))
                return;

            PoolableObject poolableObject = PoolManager.i.GetPoolable(meshesInfo.meshRootGameObject);

            if (poolableObject != null)
                poolableObject.OnRelease -= invalidMeshesInfo[meshesInfo.innerGameObject].ResetAll;

            var renderers = meshesInfo.renderers;

            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Bounds target = renderers[i].bounds;

                    if (invalidObjects.Contains(target))
                        invalidObjects.Remove(target);
                }
            }

            foreach (Collider collider in meshesInfo.colliders)
            {
                Bounds target = collider.bounds;

                if (invalidObjects.Contains(target))
                    invalidObjects.Remove(target);
            }

            invalidMeshesInfo[meshesInfo.innerGameObject].ResetAll();
            invalidMeshesInfo.Remove(meshesInfo.innerGameObject);
        }

        void AddInvalidMeshEffect(MeshesInfo meshesInfo)
        {
            if (meshesInfo == null)
                return;

            if (meshesInfo.innerGameObject == null || invalidMeshesInfo.ContainsKey(meshesInfo.innerGameObject))
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
                Bounds target = entityRenderers[i].bounds;

                if (invalidObjects.Contains(target))
                    continue;

                var box = PutBoxAroundObject(target, meshesInfo.innerGameObject.transform);

                invalidMeshInfo.wireframeObjects.Add(box);
                invalidObjects.Add(target);
            }

            foreach (Collider collider in meshesInfo.colliders)
            {
                Bounds target = collider.bounds;

                if (invalidObjects.Contains(target))
                    continue;

                var box = PutBoxAroundObject(target, meshesInfo.innerGameObject.transform);

                invalidMeshInfo.wireframeObjects.Add(box);
                invalidObjects.Add(target);
            }

            invalidMeshesInfo.Add(meshesInfo.innerGameObject, invalidMeshInfo);
        }

        private GameObject PutBoxAroundObject(Bounds target, Transform parent)
        {
            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            GameObject wireframeObject = Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
            wireframeObject.transform.position = target.center;
            wireframeObject.transform.localScale = target.size * 1.01f;
            wireframeObject.transform.SetParent(parent);
            return wireframeObject;
        }
        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            return meshesInfo.renderers.SelectMany((x) => x.sharedMaterials).ToList();
        }
    }
}
