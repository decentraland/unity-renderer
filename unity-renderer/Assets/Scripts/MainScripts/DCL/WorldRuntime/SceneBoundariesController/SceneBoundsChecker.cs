using DCL.Models;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

namespace DCL.Controllers
{
    public class SceneBoundsChecker : ISceneBoundsChecker
    {
        public event Action<IDCLEntity, bool> OnEntityBoundsCheckerStatusChanged;
        public bool enabled => entitiesCheckRoutine != null;
        public float timeBetweenChecks { get; set; } = 0.5f;
        public int entitiesToCheckCount => entitiesToCheck.Count;

        private const bool VERBOSE = false;
        private Logger logger = new Logger("SceneBoundsChecker") {verboseEnabled = VERBOSE};
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
        private IEnumerator CheckEntities()
        {   
            while (true)
            {
                float elapsedTime = Time.realtimeSinceStartup - lastCheckTime;
                if ((entitiesToCheck.Count > 0) && (timeBetweenChecks <= 0f || elapsedTime >= timeBetweenChecks))
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

                    processEntitiesList(entitiesToCheck);
                    
                    // As we can't modify the hashset while traversing it, we keep track of the entities that should be removed afterwards
                    using (var iterator = checkedEntities.GetEnumerator())
                    {
                        while (iterator.MoveNext())
                        {
                            RemoveEntity(iterator.Current, removeIfPersistent: false, resetState: false);
                        }
                    }
                    
                    if(VERBOSE)
                        logger.Verbose($"Finished checking entities: checked entities {checkedEntities.Count}; entitiesToCheck left: {entitiesToCheck.Count}");

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

        public void AddEntityToBeChecked(IDCLEntity entity, bool isPersistent = false, bool runPreliminaryEvaluation = false)
        {
            if (!enabled || (entity.scene != null && entity.scene.isPersistent))
                return;

            if (runPreliminaryEvaluation)
            {
                // The outer bounds check is cheaper than the regular check
                RunEntityEvaluation(entity, onlyOuterBoundsCheck: true);

                // No need to add the entity to be checked later if we already found it outside scene outer boundaries.
                // When the correct events are triggered again, the entity will be checked again.
                if (!isPersistent && !entity.isInsideSceneOuterBoundaries)
                    return;
            }
    
            entitiesToCheck.Add(entity);
            
            if (isPersistent)
                persistentEntities.Add(entity);
        }

        public void RemoveEntity(IDCLEntity entity, bool removeIfPersistent = false, bool resetState = false)
        {
            if (!enabled || (!removeIfPersistent && persistentEntities.Contains(entity)))
                return;

            entitiesToCheck.Remove(entity);
            persistentEntities.Remove(entity);
            
            if(resetState)
                SetMeshesAndComponentsInsideBoundariesState(entity, true);
        }

        public bool WasAddedAsPersistent(IDCLEntity entity) { return persistentEntities.Contains(entity); }

        // TODO: When we remove the DCLBuilderEntity class we'll be able to remove this overload
        public void RunEntityEvaluation(IDCLEntity entity)
        {
            RunEntityEvaluation(entity, false);
        }

        public void RunEntityEvaluation(IDCLEntity entity, bool onlyOuterBoundsCheck)
        {
            if (entity == null || entity.gameObject == null || entity.scene == null || entity.scene.isPersistent)
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

            if (HasMesh(entity)) // If it has a mesh we don't evaluate its position due to artists "pivot point sloppiness", we evaluate its mesh merged bounds
                EvaluateMeshBounds(entity, onlyOuterBoundsCheck);
            else if (entity.scene.componentsManagerLegacy.HasComponent(entity, CLASS_ID_COMPONENT.AVATAR_SHAPE)) // AvatarShape is different than any other kind of shape
                EvaluateAvatarMeshBounds(entity, onlyOuterBoundsCheck);
            else
                EvaluateEntityPosition(entity, onlyOuterBoundsCheck);
        }
        
        private void EvaluateMeshBounds(IDCLEntity entity, bool onlyOuterBoundsCheck = false)
        {
            // TODO: Can we cache the MaterialTransitionController somewhere to avoid this GetComponent() call?
            // If the mesh is being loaded we should skip the evaluation (it will be triggered again later when the loading finishes)
            if (entity.meshRootGameObject.GetComponent<MaterialTransitionController>()) // the object's MaterialTransitionController is destroyed when it finishes loading
                return;

            var loadWrapper = Environment.i.world.state.GetLoaderForEntity(entity);
            if (loadWrapper != null && !loadWrapper.alreadyLoaded)
                return;
            
            entity.isInsideSceneOuterBoundaries = entity.scene.IsInsideSceneOuterBoundaries(entity.meshesInfo.mergedBounds);
            
            if (!entity.isInsideSceneOuterBoundaries)
                SetMeshesAndComponentsInsideBoundariesState(entity, false);

            if (onlyOuterBoundsCheck)
                return;
                
            SetMeshesAndComponentsInsideBoundariesState(entity, IsEntityMeshInsideSceneBoundaries(entity));
        }
        
        private void EvaluateEntityPosition(IDCLEntity entity, bool onlyOuterBoundsCheck = false)
        {
            Vector3 entityGOPosition = entity.gameObject.transform.position;
            entity.isInsideSceneOuterBoundaries = entity.scene.IsInsideSceneOuterBoundaries(entityGOPosition);
            
            if (!entity.isInsideSceneOuterBoundaries)
            {
                SetComponentsInsideBoundariesValidState(entity, false);
                SetEntityInsideBoundariesState(entity, false);
            }

            if (onlyOuterBoundsCheck)
                return;
            
            bool isInsideBoundaries = entity.scene.IsInsideSceneBoundaries(entityGOPosition + CommonScriptableObjects.worldOffset.Get());
            SetComponentsInsideBoundariesValidState(entity, isInsideBoundaries);
            SetEntityInsideBoundariesState(entity, isInsideBoundaries);
        }

        private void EvaluateAvatarMeshBounds(IDCLEntity entity, bool onlyOuterBoundsCheck = false)
        {
            Vector3 entityGOPosition = entity.gameObject.transform.position;
            
            // Heuristic using the entity scale for the size of the avatar bounds, otherwise we should configure the 
            // entity's meshRootGameObject, etc. after its GPU skinning runs and use the regular entity mesh evaluation 
            Bounds avatarBounds = new Bounds();
            avatarBounds.center = entityGOPosition;
            avatarBounds.size = entity.gameObject.transform.lossyScale;
            
            entity.isInsideSceneOuterBoundaries = entity.scene.IsInsideSceneOuterBoundaries(avatarBounds);
            
            if (!entity.isInsideSceneOuterBoundaries)
            {
                SetComponentsInsideBoundariesValidState(entity, false);
                SetEntityInsideBoundariesState(entity, false);
            }

            if (onlyOuterBoundsCheck)
                return;
            
            bool isInsideBoundaries = entity.scene.IsInsideSceneBoundaries(avatarBounds);
            SetComponentsInsideBoundariesValidState(entity, isInsideBoundaries);
            SetEntityInsideBoundariesState(entity, isInsideBoundaries);
        }

        private void SetEntityInsideBoundariesState(IDCLEntity entity, bool isInsideBoundaries)
        {
            if (entity.isInsideSceneBoundaries == isInsideBoundaries)
                return;
            
            entity.isInsideSceneBoundaries = isInsideBoundaries;
            OnEntityBoundsCheckerStatusChanged?.Invoke(entity, isInsideBoundaries);
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
            
            // 2nd check (submeshes & colliders AABB)
            if (!isInsideBoundaries)
            {
                isInsideBoundaries = AreSubmeshesInsideBoundaries(entity) && AreCollidersInsideBoundaries(entity);
            }

            return isInsideBoundaries;
        }
        
        private bool AreSubmeshesInsideBoundaries(IDCLEntity entity)
        {
            for (int i = 0; i < entity.meshesInfo.renderers.Length; i++)
            {
                Renderer renderer = entity.meshesInfo.renderers[i];
                if (renderer == null)
                    continue;

                if (!entity.scene.IsInsideSceneBoundaries(MeshesInfoUtils.GetSafeBounds(renderer.bounds, renderer.transform.position)))
                    return false;
            }

            return true;
        }

        private bool AreCollidersInsideBoundaries(IDCLEntity entity)
        {
            foreach (Collider collider in entity.meshesInfo.colliders)
            {
                if (collider == null)
                    continue;

                if (!entity.scene.IsInsideSceneBoundaries(MeshesInfoUtils.GetSafeBounds(collider.bounds, collider.transform.position)))
                    return false;
            }

            return true;
        }

        private void SetMeshesAndComponentsInsideBoundariesState(IDCLEntity entity, bool isInsideBoundaries)
        {
            SetEntityMeshesInsideBoundariesState(entity.meshesInfo, isInsideBoundaries);
            SetEntityCollidersInsideBoundariesState(entity.meshesInfo, isInsideBoundaries);
            SetComponentsInsideBoundariesValidState(entity, isInsideBoundaries);
            
            // Should always be set last as entity.isInsideSceneBoundaries is checked to avoid re-running code unnecessarily
            SetEntityInsideBoundariesState(entity, isInsideBoundaries);
        }

        private void SetEntityMeshesInsideBoundariesState(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            feedbackStyle.ApplyFeedback(meshesInfo, isInsideBoundaries);
        }

        private void SetEntityCollidersInsideBoundariesState(MeshesInfo meshesInfo, bool isInsideBoundaries)
        {
            if (meshesInfo == null || meshesInfo.colliders.Count == 0 || !meshesInfo.currentShape.HasCollisions())
                return;
            
            foreach (Collider collider in meshesInfo.colliders)
            {
                if (collider == null) continue;
                
                if (collider.enabled != isInsideBoundaries)
                    collider.enabled = isInsideBoundaries;
            }
        }

        private void SetComponentsInsideBoundariesValidState(IDCLEntity entity, bool isInsideBoundaries)
        {
            if(entity.isInsideSceneBoundaries == isInsideBoundaries || !DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries.ContainsKey(entity.entityId))
                return;
            
            foreach (IOutOfSceneBoundariesHandler component in DataStore.i.sceneBoundariesChecker.componentsCheckSceneBoundaries[entity.entityId])
            {
                component.UpdateOutOfBoundariesState(isInsideBoundaries);   
            }
        }
    }
}