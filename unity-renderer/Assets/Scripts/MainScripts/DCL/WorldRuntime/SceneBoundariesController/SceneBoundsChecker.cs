using DCL.Models;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace DCL.Controllers
{
    public class SceneBoundsChecker : ISceneBoundsChecker
    {
        public const int TRIGGER_HIGHPRIO_VALUE = 1000;
        public event Action<IDCLEntity, bool> OnEntityBoundsCheckerStatusChanged;
        public bool enabled => entitiesCheckRoutine != null;
        public float timeBetweenChecks { get; set; } = 0.5f;
        public int entitiesToCheckCount => entitiesToCheck.Count;
        public int highPrioEntitiesToCheckCount => highPrioEntitiesToCheck.Count;

        private const bool VERBOSE = false;
        private Logger logger = new Logger("SceneBoundsChecker") {verboseEnabled = VERBOSE};
        private HashSet<IDCLEntity> highPrioEntitiesToCheck = new HashSet<IDCLEntity>();
        private HashSet<IDCLEntity> entitiesToCheck = new HashSet<IDCLEntity>();
        private HashSet<IDCLEntity> checkedEntities = new HashSet<IDCLEntity>();
        private HashSet<IDCLEntity> persistentEntities = new HashSet<IDCLEntity>();
        private ISceneBoundsFeedbackStyle feedbackStyle;
        private Coroutine entitiesCheckRoutine = null;
        private float lastCheckTime;

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
                        {
                            if(VERBOSE)
                                logger.Verbose("Time budget reached, escaping entities processing until next iteration... ");
                            return;
                        }

                        using HashSet<IDCLEntity>.Enumerator iterator = entities.GetEnumerator();
                        while (iterator.MoveNext())
                        {
                            if (messagingManager != null && messagingManager.timeBudgetCounter <= 0f)
                            {
                                if(VERBOSE)
                                    logger.Verbose("Time budget reached, escaping entities processing until next iteration... ");
                                return;
                            }

                            float startTime = Time.realtimeSinceStartup;

                            RunEntityEvaluation(iterator.Current);
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
                            IDCLEntity entity = iterator.Current;
                            if (!persistentEntities.Contains(entity))
                            {
                                entitiesToCheck.Remove(entity);
                                highPrioEntitiesToCheck.Remove(entity);
                            }
                        }
                    }
                    
                    if(VERBOSE)
                        logger.Verbose($"Finished checking entities: checked entities {checkedEntities.Count}; entitiesToCheck left: {entitiesToCheck.Count}; highPriorityEntities left: {highPrioEntitiesToCheck.Count}");

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

        public void AddEntityToBeChecked(IDCLEntity entity, bool runPreliminaryEvaluation = false)
        {
            if (!enabled)
                return;

            OnAddEntity(entity, runPreliminaryEvaluation);
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

        public void RemovePersistent(IDCLEntity entity)
        {
            persistentEntities.Remove(entity);
        }

        /// <summary>
        /// Returns whether an entity was added to be consistently checked
        /// </summary>
        ///
        public bool WasAddedAsPersistent(IDCLEntity entity) { return persistentEntities.Contains(entity); }

        public void RemoveEntityToBeCheckedAndResetState(IDCLEntity entity)
        {
            if (!enabled)
                return;

            OnRemoveEntity(entity);
        }

        public void RunEntityEvaluation(IDCLEntity entity)
        {
            RunEntityEvaluation(entity, false);
        }

        public void RunEntityEvaluation(IDCLEntity entity, bool onlyOuterBoundsCheck)
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
                        RunEntityEvaluation(iterator.Current.Value, onlyOuterBoundsCheck);
                    }
                }
            }

            // If it has a mesh we don't evaluate its position due to artists "pivot point sloppiness", we evaluate its mesh merged bounds
            if (HasMesh(entity))
                EvaluateMeshBounds(entity, onlyOuterBoundsCheck);
            else
                EvaluateEntityPosition(entity, onlyOuterBoundsCheck);
        }
        
        void EvaluateMeshBounds(IDCLEntity entity, bool onlyOuterBoundsCheck = false)
        {
            // TODO: Can we cache the MaterialTransitionController somewhere to avoid this GetComponent() call?
            // If the mesh is being loaded we should skip the evaluation (it will be triggered again later when the loading finishes)
            if (entity.meshRootGameObject.GetComponent<MaterialTransitionController>()) // the object's MaterialTransitionController is destroyed when it finishes loading
                return;

            var loadWrapper = Environment.i.world.state.GetLoaderForEntity(entity);
            if (loadWrapper != null && !loadWrapper.alreadyLoaded)
                return;
            
            bool isInsideOuterBounds = entity.scene.IsInsideSceneOuterBoundaries(entity.meshesInfo.mergedBounds);
            
            if (!isInsideOuterBounds)
                SetMeshesAndComponentsInsideBoundariesState(entity, false);
            else if (!onlyOuterBoundsCheck)
                SetMeshesAndComponentsInsideBoundariesState(entity, IsEntityMeshInsideSceneBoundaries(entity));
        }
        
        void EvaluateEntityPosition(IDCLEntity entity, bool onlyOuterBoundsCheck = false)
        {
            Vector3 entityGOPosition = entity.gameObject.transform.position;
            bool isInsideOuterBounds = entity.scene.IsInsideSceneOuterBoundaries(entityGOPosition);
            
            if (!isInsideOuterBounds)
            {
                UpdateEntityInsideBoundariesState(entity, false);
                UpdateComponents(entity, false);
            }
            else if (!onlyOuterBoundsCheck)
            {
                bool isInsideBoundaries = entity.scene.IsInsideSceneBoundaries(entityGOPosition + CommonScriptableObjects.worldOffset.Get());
                UpdateEntityInsideBoundariesState(entity, isInsideBoundaries);
                UpdateComponents(entity, isInsideBoundaries);
            }
        }

        void UpdateEntityInsideBoundariesState(IDCLEntity entity, bool isInsideBoundaries)
        {
            if (entity.isInsideBoundaries != isInsideBoundaries)
            {
                entity.isInsideBoundaries = isInsideBoundaries;
                OnEntityBoundsCheckerStatusChanged?.Invoke(entity, isInsideBoundaries);
            }
        }

        private bool HasMesh(IDCLEntity entity)
        {
            return entity.meshRootGameObject != null
                    && (entity.meshesInfo.colliders.Count > 0
                    || (entity.meshesInfo.renderers != null
                    && entity.meshesInfo.renderers.Length > 0));
        }

        public bool IsEntityMeshInsideSceneBoundaries(IDCLEntity entity)
        {
            if (entity.meshesInfo == null 
                || entity.meshesInfo.meshRootGameObject == null 
                || entity.meshesInfo.mergedBounds == null)
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

        void SetMeshesAndComponentsInsideBoundariesState(IDCLEntity entity, bool isInsideBoundaries)
        {
            UpdateEntityInsideBoundariesState(entity, isInsideBoundaries);

            UpdateEntityMeshesValidState(entity.meshesInfo, isInsideBoundaries);
            UpdateEntityCollidersValidState(entity.meshesInfo, isInsideBoundaries);
            UpdateComponents(entity, isInsideBoundaries);
        }

        protected void UpdateEntityMeshesValidState(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            feedbackStyle.ApplyFeedback(meshesInfo, isInsideBoundaries);
        }

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
                    if (meshesInfo.colliders[i] == null)
                        continue;
                        
                    meshesInfo.colliders[i].enabled = isInsideBoundaries;
                }
            }
        }

        protected void UpdateComponents(IDCLEntity entity, bool isInsideBoundaries)
        {
            if(!DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries.ContainsKey(entity.entityId))
                return;
            
            List<IOutOfSceneBoundariesHandler> components = DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries[entity.entityId];

            for (int i = 0; i < components.Count; i++)
            {
                components[i].UpdateOutOfBoundariesState(isInsideBoundaries);
            }
        }

        protected void OnAddEntity(IDCLEntity entity, bool runPreliminaryEvaluation = false)
        {
            if (runPreliminaryEvaluation)
            {
                // The outer bounds check is cheaper than the regular check
                RunEntityEvaluation(entity, onlyOuterBoundsCheck: true);

                // No need to add the entity to be checked later if we already found it outside its boundaries.
                // When the correct events are triggered again, the entity will be checked again.
                if (!entity.isInsideBoundaries)
                    return;
            }
            
            AddEntityBasedOnPriority(entity);
        }

        protected void OnRemoveEntity(IDCLEntity entity)
        {
            highPrioEntitiesToCheck.Remove(entity);
            entitiesToCheck.Remove(entity);
            persistentEntities.Remove(entity);
            
            SetMeshesAndComponentsInsideBoundariesState(entity, true);
        }

        protected void AddEntityBasedOnPriority(IDCLEntity entity)
        {
            if (IsHighPrioEntity(entity))
            {
                highPrioEntitiesToCheck.Add(entity);
                return;
            }
            
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