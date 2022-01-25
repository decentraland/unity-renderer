using DCL.Components;
using DCL.Models;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using DCL.Helpers;

namespace DCL.Controllers
{
    public class SceneBoundsChecker : ISceneBoundsChecker
    {
        public const int TRIGGER_HIGHPRIO_VALUE = 1000;
        public event Action<IDCLEntity, bool> OnEntityBoundsCheckerStatusChanged;
        public bool enabled => entitiesCheckRoutine != null;
        public float timeBetweenChecks { get; set; } = 0.5f;

        // We use Hashset instead of Queue to be able to have a unique representation of each entity when added.
        HashSet<IDCLEntity> highPrioEntitiesToCheck = new HashSet<IDCLEntity>();
        HashSet<IDCLEntity> entitiesToCheck = new HashSet<IDCLEntity>();
        HashSet<IDCLEntity> checkedEntities = new HashSet<IDCLEntity>();
        Coroutine entitiesCheckRoutine = null;
        float lastCheckTime;
        private HashSet<IDCLEntity> persistentEntities = new HashSet<IDCLEntity>();

        public int entitiesToCheckCount => entitiesToCheck.Count;
        public int highPrioEntitiesToCheckCount => highPrioEntitiesToCheck.Count;

        private ISceneBoundsFeedbackStyle feedbackStyle;

        public void Initialize()
        {
            Start();
        }

        public SceneBoundsChecker(ISceneBoundsFeedbackStyle feedbackStyle = null)
        {
            this.feedbackStyle = feedbackStyle ?? new SceneBoundsFeedbackStyle_Simple();
        }

        public void SetFeedbackStyle(ISceneBoundsFeedbackStyle feedbackStyle)
        {
            this.feedbackStyle.CleanFeedback();
            this.feedbackStyle = feedbackStyle;
            Restart();
        }

        public ISceneBoundsFeedbackStyle GetFeedbackStyle() { return feedbackStyle; }

        public List<Material> GetOriginalMaterials(MeshesInfo meshesInfo) { return feedbackStyle.GetOriginalMaterials(meshesInfo); }


        // TODO: Improve MessagingControllersManager.i.timeBudgetCounter usage once we have the centralized budget controller for our immortal coroutines
        IEnumerator CheckEntities()
        {
            while (true)
            {
                float elapsedTime = Time.realtimeSinceStartup - lastCheckTime;
                if ((entitiesToCheck.Count > 0 || highPrioEntitiesToCheck.Count > 0) && (timeBetweenChecks <= 0f || elapsedTime >= timeBetweenChecks))
                {
                    //TODO(Brian): Remove later when we implement a centralized way of handling time budgets
                    var messagingManager = Environment.i.messaging.manager as MessagingControllersManager;

                    void processEntitiesList(HashSet<IDCLEntity> entities)
                    {
                        if (messagingManager != null && messagingManager.timeBudgetCounter <= 0f)
                            return;

                        using HashSet<IDCLEntity>.Enumerator iterator = entities.GetEnumerator();
                        while (iterator.MoveNext())
                        {
                            if (messagingManager != null && messagingManager.timeBudgetCounter <= 0f)
                                break;

                            float startTime = Time.realtimeSinceStartup;

                            EvaluateEntityPosition(iterator.Current);
                            checkedEntities.Add(iterator.Current);

                            float finishTime = Time.realtimeSinceStartup;

                            if ( messagingManager != null )
                                messagingManager.timeBudgetCounter -= (finishTime - startTime);
                        }
                    }

                    processEntitiesList(highPrioEntitiesToCheck);
                    processEntitiesList(entitiesToCheck);

                    // As we can't modify the hashset while traversing it, we keep track of the entities that should be removed afterwards
                    using (var iterator = checkedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            if (!persistentEntities.Contains(iterator.Current))
                            {
                                entitiesToCheck.Remove(iterator.Current);
                                highPrioEntitiesToCheck.Remove(iterator.Current);
                            }
                        }
                    }

                    checkedEntities.Clear();

                    lastCheckTime = Time.realtimeSinceStartup;
                }

                yield return null;
            }
        }

        public void Restart()
        {
            Stop();
            Start();
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

        public void Dispose()
        {
            Stop();
        }

        public void AddEntityToBeChecked(IDCLEntity entity)
        {
            if (!enabled)
                return;

            OnAddEntity(entity);
        }

        /// <summary>
        /// Add an entity that will be consistently checked, until manually removed from the list.
        /// </summary>
        public void AddPersistent(IDCLEntity entity)
        {
            if (!enabled)
                return;

            AddEntityBasedOnPriority(entity);

            persistentEntities.Add(entity);
        }

        /// <summary>
        /// Returns whether an entity was added to be consistently checked
        /// </summary>
        ///
        public bool WasAddedAsPersistent(IDCLEntity entity) { return persistentEntities.Contains(entity); }

        public void RemoveEntityToBeChecked(IDCLEntity entity)
        {
            if (!enabled)
                return;

            OnRemoveEntity(entity);
        }

        public void EvaluateEntityPosition(IDCLEntity entity)
        {
            if (entity == null || entity.scene == null || entity.gameObject == null)
                return;

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
                UpdateComponents(entity, entity.scene.IsInsideSceneBoundaries(entity.gameObject.transform.position + CommonScriptableObjects.worldOffset.Get()));
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

        public bool IsEntityInsideSceneBoundaries(IDCLEntity entity)
        {
            if (entity.meshesInfo == null || entity.meshesInfo.meshRootGameObject == null || entity.meshesInfo.mergedBounds == null)
                return false;

            // 1st check (full mesh AABB)
            bool isInsideBoundaries = entity.scene.IsInsideSceneBoundaries(entity.meshesInfo.mergedBounds);

            // 2nd check (submeshes AABB)
            if (!isInsideBoundaries)
            {
                isInsideBoundaries = AreSubmeshesInsideBoundaries(entity);
            }

            return isInsideBoundaries;
        }

        void EvaluateMeshBounds(IDCLEntity entity)
        {
            bool isInsideBoundaries = IsEntityInsideSceneBoundaries(entity);
            if (entity.isInsideBoundaries != isInsideBoundaries)
            {
                entity.isInsideBoundaries = isInsideBoundaries;
                OnEntityBoundsCheckerStatusChanged?.Invoke(entity, isInsideBoundaries);
            }

            UpdateEntityMeshesValidState(entity.meshesInfo, isInsideBoundaries);
            UpdateEntityCollidersValidState(entity.meshesInfo, isInsideBoundaries);
            UpdateComponents(entity, isInsideBoundaries);
        }

        protected bool AreSubmeshesInsideBoundaries(IDCLEntity entity)
        {
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                if (entity.meshesInfo.renderers[i] == null)
                    continue;

                if (!entity.scene.IsInsideSceneBoundaries(entity.meshesInfo.renderers[i].GetSafeBounds()))
                    return false;
            }

            return true;
        }

        protected void UpdateEntityMeshesValidState(MeshesInfo meshesInfo, bool isInsideBoundaries) { feedbackStyle.ApplyFeedback(meshesInfo, isInsideBoundaries); }

        protected void UpdateEntityCollidersValidState(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (meshesInfo == null || meshesInfo.colliders == null)
                return;

            int collidersCount = meshesInfo.colliders.Count;

            if (collidersCount == 0)
                return;

            if (meshesInfo.colliders[0] == null)
                return;

            if (collidersCount > 0 && isInsideBoundaries != meshesInfo.colliders[0].enabled && meshesInfo.currentShape.HasCollisions())
            {
                for (int i = 0; i < collidersCount; i++)
                {
                    if (meshesInfo.colliders[i] != null)
                        meshesInfo.colliders[i].enabled = isInsideBoundaries;
                }
            }
        }

        protected void UpdateComponents(IDCLEntity entity, bool isInsideBoundaries)
        {
            IOutOfSceneBoundariesHandler[] components = entity.gameObject.GetComponentsInChildren<IOutOfSceneBoundariesHandler>();

            for (int i = 0; i < components.Length; i++)
            {
                components[i].UpdateOutOfBoundariesState(isInsideBoundaries);
            }
        }

        protected void OnAddEntity(IDCLEntity entity)
        {
            AddEntityBasedOnPriority(entity);
        }

        protected void OnRemoveEntity(IDCLEntity entity)
        {
            highPrioEntitiesToCheck.Remove(entity);
            entitiesToCheck.Remove(entity);
            persistentEntities.Remove(entity);
            feedbackStyle.ApplyFeedback(entity.meshesInfo, true);
        }

        protected void AddEntityBasedOnPriority(IDCLEntity entity)
        {
            if (IsHighPrioEntity(entity) && !highPrioEntitiesToCheck.Contains(entity))
                highPrioEntitiesToCheck.Add(entity);
            else if (!entitiesToCheck.Contains(entity))
                entitiesToCheck.Add(entity);
        }

        protected bool IsHighPrioEntity(IDCLEntity entity)
        {
            if (entity.gameObject == null)
                return false;

            Vector3 scale = entity.gameObject.transform.lossyScale;
            Vector3 position = entity.gameObject.transform.localPosition;
            return scale.x > TRIGGER_HIGHPRIO_VALUE || scale.y > TRIGGER_HIGHPRIO_VALUE || scale.z > TRIGGER_HIGHPRIO_VALUE || position.x > TRIGGER_HIGHPRIO_VALUE || position.y > TRIGGER_HIGHPRIO_VALUE || position.z > TRIGGER_HIGHPRIO_VALUE;
        }
    }
}