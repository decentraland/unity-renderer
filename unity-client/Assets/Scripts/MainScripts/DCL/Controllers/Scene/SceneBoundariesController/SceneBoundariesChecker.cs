using UnityEngine;
using DCL.Models;
using DCL.Components;
using DCL.Helpers;

namespace DCL.Controllers
{
    public class SceneBoundariesChecker
    {
        protected ParcelScene scene;

        public SceneBoundariesChecker(ParcelScene ownerScene)
        {
            scene = ownerScene;
        }

        public void EvaluateEntityPosition(DecentralandEntity entity)
        {
            // TODO: Remove once we fix at least the main plazas geometry to not surpass their scene limits...
            if (!SceneController.i.isDebugMode) return;

            if (entity == null || !scene.entities.ContainsValue(entity)) return;

            // Recursively evaluate entity children as well, we need to check this up front because this entity may not have meshes of its own, but the children may.
            if (entity.children.Count > 0)
            {
                using (var iterator = entity.children.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        EvaluateEntityPosition(iterator.Current.Value);
                    }
                }
            }

            if (entity.meshRootGameObject == null || entity.meshesInfo.renderers == null || entity.meshesInfo.renderers.Length == 0) return;

            // If the mesh is being loaded we should skip the evaluation (it will be triggered again later when the loading finishes)
            if (entity.meshRootGameObject.GetComponent<MaterialTransitionController>()) // the object's MaterialTransitionController is destroyed when it finishes loading
            {
                return;
            }
            else
            {
                var loadWrapper = entity.meshRootGameObject.GetComponent<LoadWrapper>();
                if (loadWrapper != null && !loadWrapper.alreadyLoaded) return;
            }

            EvaluateMeshBounds(entity);
        }

        void EvaluateMeshBounds(DecentralandEntity entity)
        {
            Bounds meshBounds = Utils.BuildMergedBounds(entity.meshesInfo.renderers);

            // 1st check (full mesh AABB)
            bool isInsideBoundaries = scene.IsInsideSceneBoundaries(meshBounds);

            // 2nd check (submeshes AABB)
            if (!isInsideBoundaries)
            {
                isInsideBoundaries = AreSubmeshesInsideBoundaries(entity);
            }

            UpdateEntityMeshesValidState(entity, isInsideBoundaries, meshBounds);

            UpdateEntityCollidersValidState(entity, isInsideBoundaries);
        }

        protected virtual bool AreSubmeshesInsideBoundaries(DecentralandEntity entity)
        {
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                if (!scene.IsInsideSceneBoundaries(entity.meshesInfo.renderers[i].bounds))
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual void UpdateEntityMeshesValidState(DecentralandEntity entity, bool isInsideBoundaries, Bounds meshBounds)
        {
            if (isInsideBoundaries != entity.meshesInfo.renderers[0].enabled && entity.meshesInfo.currentShape.IsVisible())
            {
                for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
                {
                    entity.meshesInfo.renderers[i].enabled = isInsideBoundaries;
                }
            }
        }

        protected virtual void UpdateEntityCollidersValidState(DecentralandEntity entity, bool isInsideBoundaries)
        {
            int collidersCount = entity.meshesInfo.colliders.Count;
            if (collidersCount > 0 && isInsideBoundaries != entity.meshesInfo.colliders[0].enabled && entity.meshesInfo.currentShape.HasCollisions())
            {
                for (int i = 0; i < collidersCount; i++)
                {
                    entity.meshesInfo.colliders[i].enabled = isInsideBoundaries;
                }
            }
        }
    }
}