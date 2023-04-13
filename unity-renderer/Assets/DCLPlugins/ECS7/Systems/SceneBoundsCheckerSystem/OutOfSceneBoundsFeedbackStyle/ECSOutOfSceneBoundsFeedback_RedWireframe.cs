using DCL.ECS7.InternalComponents;
using DCL.Helpers;
using DCL.Models;
using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.ECSSceneBoundsCheckerSystem
{
    public class ECSOutOfSceneBoundsFeedback_RedWireframe : IECSOutOfSceneBoundsFeedbackStyle
    {
        const string WIREFRAME_PREFAB_NAME = "Prefabs/WireframeCubeMesh";

        private Dictionary<IDCLEntity, GameObject> wireframeGameObjects = new Dictionary<IDCLEntity, GameObject>();

        public void ApplyFeedback(IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel, bool isVisible, bool isInsideBounds)
        {
            if (isInsideBounds)
                RemoveInvalidMeshEffect(entity);
            else
                AddInvalidMeshEffect(entity, sbcComponentModel);

            KeyValueSet<Collider, int> physicsColliders = sbcComponentModel.physicsColliders;
            KeyValueSet<Collider, int> pointerColliders = sbcComponentModel.pointerColliders;

            if (physicsColliders != null)
            {
                var pairs = physicsColliders.Pairs;
                for (int i = 0; i < pairs.Count; i++)
                {
                    pairs[i].key.enabled = isInsideBounds;
                }
            }

            if (pointerColliders != null)
            {
                var pairs = pointerColliders.Pairs;
                for (int i = 0; i < pairs.Count; i++)
                {
                    pairs[i].key.enabled = isInsideBounds;
                }
            }
        }

        private void AddInvalidMeshEffect(IDCLEntity entity, InternalSceneBoundsCheck sbcComponentModel)
        {
            if (wireframeGameObjects.ContainsKey(entity)) return;

            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            wireframeGameObjects.Add(entity, PutWireframeAroundObject(sbcComponentModel.entityLocalMeshBounds, entity.gameObject.transform));
        }

        private void RemoveInvalidMeshEffect(IDCLEntity entity)
        {
            if (!wireframeGameObjects.ContainsKey(entity)) return;

            Utils.SafeDestroy(wireframeGameObjects[entity]);
            wireframeGameObjects.Remove(entity);
        }

        private GameObject PutWireframeAroundObject(Bounds target, Transform parent)
        {
            // Wireframe that shows the boundaries to the dev (We don't use the GameObject.Instantiate(prefab, parent)
            // overload because we need to set the position and scale before parenting, to deal with scaled objects)
            GameObject wireframeGO = Object.Instantiate(Resources.Load<GameObject>(WIREFRAME_PREFAB_NAME));
            wireframeGO.transform.localScale = target.size * 1.01f;
            wireframeGO.transform.SetParent(parent);
            wireframeGO.transform.localPosition = target.min + (target.max - target.min) * 0.5f;
            return wireframeGO;
        }
    }
}
