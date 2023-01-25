
using DCL.ECS7.InternalComponents;
using DCL.ECSRuntime;
using DCL.Models;
using DCL.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSOutOfSceneBoundsFeedback_RedWireframe : IECSOutOfSceneBoundsFeedbackStyle
    {
        const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

        private Dictionary<IDCLEntity, GameObject> wireframeGameObjects = new Dictionary<IDCLEntity, GameObject>();

        public void ApplyFeedback(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData, IECSReadOnlyComponentData<InternalVisibility> visibilityComponentData, bool isInsideBounds)
        {
            if (isInsideBounds)
                RemoveInvalidMeshEffect(sbcComponentData);
            else
                AddInvalidMeshEffect(sbcComponentData);

            if (sbcComponentData.model.physicsColliders != null)
            {
                int count = sbcComponentData.model.physicsColliders.Count;
                for (var i = 0; i < count; i++)
                {
                    sbcComponentData.model.physicsColliders[i].enabled = isInsideBounds;
                }
            }

            if (sbcComponentData.model.pointerColliders != null)
            {
                int count = sbcComponentData.model.pointerColliders.Count;
                for (var i = 0; i < count; i++)
                {
                    sbcComponentData.model.pointerColliders[i].enabled = isInsideBounds;
                }
            }
        }

        private void AddInvalidMeshEffect(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            if (wireframeGameObjects.ContainsKey(sbcComponentData.entity)) return;

            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            wireframeGameObjects.Add(sbcComponentData.entity, PutWireframeAroundObject(sbcComponentData.model.entityLocalMeshBounds, sbcComponentData.entity.gameObject.transform));
        }

        private void RemoveInvalidMeshEffect(ECSComponentData<InternalSceneBoundsCheck> sbcComponentData)
        {
            if (!wireframeGameObjects.ContainsKey(sbcComponentData.entity)) return;

            Utils.SafeDestroy(wireframeGameObjects[sbcComponentData.entity]);
            wireframeGameObjects.Remove(sbcComponentData.entity);
        }

        private GameObject PutWireframeAroundObject(Bounds target, Transform parent)
        {
            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            GameObject wireframeGO = UnityEngine.Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
            wireframeGO.transform.localScale = target.size * 1.01f;
            wireframeGO.transform.SetParent(parent);
            wireframeGO.transform.localPosition = target.min + (target.max - target.min) * 0.5f;
            return wireframeGO;
        }
    }
}
