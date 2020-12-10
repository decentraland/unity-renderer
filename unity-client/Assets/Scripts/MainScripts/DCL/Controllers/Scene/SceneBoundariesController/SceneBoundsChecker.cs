using DCL.Components;
using DCL.Models;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DCL.Controllers
{
    public interface ISceneBoundsFeedbackStyle
    {
        void OnRendererExitBounds(Renderer renderer);
        void ApplyFeedback(MeshesInfo meshesInfo, bool isInsideBoundaries);
        List<Material> GetOriginalMaterials(MeshesInfo meshesInfo);
    }

    public interface IOutOfSceneBoundariesHandler
    {
        void UpdateOutOfBoundariesState(bool enable);
    }

    public class SceneBoundsChecker
    {
        public bool enabled => entitiesCheckRoutine != null;

        [System.NonSerialized] public float timeBetweenChecks = 1f;

        // We use Hashset instead of Queue to be able to have a unique representation of each entity when added.
        HashSet<DecentralandEntity> entitiesToCheck = new HashSet<DecentralandEntity>();
        HashSet<DecentralandEntity> checkedEntities = new HashSet<DecentralandEntity>();
        Coroutine entitiesCheckRoutine = null;
        float lastCheckTime;
        private HashSet<DecentralandEntity> persistentEntities = new HashSet<DecentralandEntity>();

        public int entitiesToCheckCount => entitiesToCheck.Count;

        private ISceneBoundsFeedbackStyle feedbackStyle;

        public SceneBoundsChecker(ISceneBoundsFeedbackStyle feedbackStyle = null)
        {
            this.feedbackStyle = feedbackStyle ?? new SceneBoundsFeedbackStyle_Simple();
        }

        public void SetFeedbackStyle(ISceneBoundsFeedbackStyle feedbackStyle)
        {
            this.feedbackStyle = feedbackStyle;
        }

        public ISceneBoundsFeedbackStyle GetFeedbackStyle()
        {
            return feedbackStyle;
        }

        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo)
        {
            return feedbackStyle.GetOriginalMaterials(meshesInfo);
        }

        // TODO: Improve MessagingControllersManager.i.timeBudgetCounter usage once we have the centralized budget controller for our immortal coroutines
        IEnumerator CheckEntities()
        {
            while (true)
            {
                float elapsedTime = Time.realtimeSinceStartup - lastCheckTime;
                if (entitiesToCheck.Count > 0 && (timeBetweenChecks <= 0f || elapsedTime >= timeBetweenChecks))
                {
                    using (var iterator = entitiesToCheck.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (Environment.i.messagingControllersManager.timeBudgetCounter <= 0f) break;

                            float startTime = Time.realtimeSinceStartup;

                            EvaluateEntityPosition(iterator.Current);
                            checkedEntities.Add(iterator.Current);

                            float finishTime = Time.realtimeSinceStartup;
                            Environment.i.messagingControllersManager.timeBudgetCounter -= (finishTime - startTime);
                        }
                    }

                    // As we can't modify the hashset while traversing it, we keep track of the entities that should be removed afterwards
                    using (var iterator = checkedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (!persistentEntities.Contains(iterator.Current))
                            {
                                entitiesToCheck.Remove(iterator.Current);
                            }
                        }
                    }

                    checkedEntities.Clear();

                    lastCheckTime = Time.realtimeSinceStartup;
                }

                yield return null;
            }
        }

        public void Start()
        {
            if (entitiesCheckRoutine != null)
                return;

            lastCheckTime = Time.realtimeSinceStartup;
            entitiesCheckRoutine = CoroutineStarter.Start(CheckEntities());
        }

        public void Stop()
        {
            if (entitiesCheckRoutine == null)
                return;

            CoroutineStarter.Stop(entitiesCheckRoutine);
            entitiesCheckRoutine = null;
        }

        public void AddEntityToBeChecked(DecentralandEntity entity)
        {
            if (!enabled)
                return;

            OnAddEntity(entity);
        }

        /// <summary>
        /// Add an entity that will be consistently checked, until manually removed from the list.
        /// </summary>
        public void AddPersistent(DecentralandEntity entity)
        {
            if (!enabled)
                return;

            entitiesToCheck.Add(entity);
            persistentEntities.Add(entity);
        }

        /// <summary>
        /// Returns whether an entity was added to be consistently checked
        /// </summary>
        ///
        public bool WasAddedAsPersistent(DecentralandEntity entity)
        {
            return persistentEntities.Contains(entity);
        }

        public void RemoveEntityToBeChecked(DecentralandEntity entity)
        {
            if (!enabled) return;

            OnRemoveEntity(entity);
        }

        public void EvaluateEntityPosition(DecentralandEntity entity)
        {
            if (entity == null || entity.scene == null || entity.gameObject == null) return;

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

            if (entity.meshRootGameObject == null || entity.meshesInfo.renderers == null || entity.meshesInfo.renderers.Length == 0)
            {
                UpdateComponents(entity, entity.scene.IsInsideSceneBoundaries(entity.gameObject.transform.position + CommonScriptableObjects.playerUnityToWorldOffset.Get()));
                return;
            }

            // If the mesh is being loaded we should skip the evaluation (it will be triggered again later when the loading finishes)
            if (entity.meshRootGameObject.GetComponent<MaterialTransitionController>()) // the object's MaterialTransitionController is destroyed when it finishes loading
            {
                return;
            }

            var loadWrapper = LoadableShape.GetLoaderForEntity(entity);
            if (loadWrapper != null && !loadWrapper.alreadyLoaded)
                return;

            EvaluateMeshBounds(entity);
        }

        public bool IsEntityInsideSceneBoundaries(DecentralandEntity entity)
        {
            if (entity.meshesInfo == null || entity.meshesInfo.meshRootGameObject == null || entity.meshesInfo.mergedBounds == null) return false;

            // 1st check (full mesh AABB)
            bool isInsideBoundaries = entity.scene.IsInsideSceneBoundaries(entity.meshesInfo.mergedBounds);

            // 2nd check (submeshes AABB)
            if (!isInsideBoundaries)
            {
                isInsideBoundaries = AreSubmeshesInsideBoundaries(entity);
            }

            return isInsideBoundaries;
        }

        void EvaluateMeshBounds(DecentralandEntity entity)
        {
            bool isInsideBoundaries = IsEntityInsideSceneBoundaries(entity);

            UpdateEntityMeshesValidState(entity.meshesInfo, isInsideBoundaries);
            UpdateEntityCollidersValidState(entity.meshesInfo, isInsideBoundaries);
            UpdateComponents(entity, isInsideBoundaries);
        }

        protected bool AreSubmeshesInsideBoundaries(DecentralandEntity entity)
        {
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                if (!entity.scene.IsInsideSceneBoundaries(entity.meshesInfo.renderers[i].bounds))
                {
                    feedbackStyle.OnRendererExitBounds(entity.meshesInfo.renderers[i]);
                    return false;
                }
            }

            return true;
        }

        protected void UpdateEntityMeshesValidState(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            feedbackStyle.ApplyFeedback(meshesInfo, isInsideBoundaries);
        }

        protected void UpdateEntityCollidersValidState(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            int collidersCount = meshesInfo.colliders.Count;
            if (collidersCount > 0 && isInsideBoundaries != meshesInfo.colliders[0].enabled && meshesInfo.currentShape.HasCollisions())
            {
                for (int i = 0; i < collidersCount; i++)
                {
                    if (meshesInfo.colliders[i] != null)
                        meshesInfo.colliders[i].enabled = isInsideBoundaries;
                }
            }
        }

        protected void UpdateComponents(DecentralandEntity entity, bool isInsideBoundaries)
        {
            IOutOfSceneBoundariesHandler[] components = entity.gameObject.GetComponentsInChildren<IOutOfSceneBoundariesHandler>();

            for (int i = 0; i < components.Length; i++)
            {
                components[i].UpdateOutOfBoundariesState(isInsideBoundaries);
            }
        }

        protected void OnAddEntity(DecentralandEntity entity)
        {
            entitiesToCheck.Add(entity);
        }

        protected void OnRemoveEntity(DecentralandEntity entity)
        {
            entitiesToCheck.Remove(entity);
            persistentEntities.Remove(entity);
            feedbackStyle.ApplyFeedback(entity.meshesInfo, true);
        }
    }
}