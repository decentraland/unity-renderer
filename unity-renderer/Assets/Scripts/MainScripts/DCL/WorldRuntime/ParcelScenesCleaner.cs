using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class ParcelScenesCleaner : IParcelScenesCleaner
    {
        const float MAX_TIME_BUDGET = 0.01f;

        private readonly struct MarkedEntityInfo
        {
            public readonly ParcelScene scene;
            public readonly IDCLEntity entity;

            public MarkedEntityInfo(ParcelScene scene, IDCLEntity entity)
            {
                this.scene = scene;
                this.entity = entity;
            }
        }

        private readonly struct MarkedSharedComponentInfo
        {
            public readonly ParcelScene scene;
            public readonly string componentId;

            public MarkedSharedComponentInfo(ParcelScene scene, string componentId)
            {
                this.scene = scene;
                this.componentId = componentId;
            }
        }

        Queue<IDCLEntity> entitiesMarkedForCleanup = new Queue<IDCLEntity>();
        Queue<MarkedEntityInfo> rootEntitiesMarkedForCleanup = new Queue<MarkedEntityInfo>();
        Queue<MarkedSharedComponentInfo> disposableComponentsMarkedForCleanup = new Queue<MarkedSharedComponentInfo>();

        Coroutine removeEntitiesCoroutine;

        public void Initialize ()
        {
            removeEntitiesCoroutine = CoroutineStarter.Start(CleanupEntitiesCoroutine());
            CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        }

        private void OnRendererStateChange(bool isEnable, bool prevState)
        {
            if (!isEnable)
            {
                CleanMarkedEntities();
                Resources.UnloadUnusedAssets();
            }
        }

        public void MarkForCleanup(IDCLEntity entity)
        {
            if (entity.markedForCleanup)
                return;

            entity.markedForCleanup = true;

            if (entity.gameObject != null)
                entity.gameObject.SetActive(false);

#if UNITY_EDITOR
            if (entity.gameObject != null)
                entity.gameObject.name += "-MARKED-FOR-CLEANUP";
#endif

            entitiesMarkedForCleanup.Enqueue(entity);
        }

        // When removing all entities from a scene, we need to separate the root entities, as stated in ParcelScene,
        // to avoid traversing a lot of child entities in the same frame and other problems
        public void MarkRootEntityForCleanup(IParcelScene scene, IDCLEntity entity) { rootEntitiesMarkedForCleanup.Enqueue(new MarkedEntityInfo((ParcelScene)scene, entity)); }

        public void MarkDisposableComponentForCleanup(IParcelScene scene, string componentId) { disposableComponentsMarkedForCleanup.Enqueue(new MarkedSharedComponentInfo((ParcelScene)scene, componentId)); }

        public void CleanMarkedEntities()
        {
            CleanMarkedEntitiesAsync(true).MoveNext();
        }

        public IEnumerator CleanMarkedEntitiesAsync(bool immediate = false)
        {
            float lastTime = Time.unscaledTime;

            while (disposableComponentsMarkedForCleanup.Count > 0)
            {
                MarkedSharedComponentInfo markedSharedComponentInfo = disposableComponentsMarkedForCleanup.Dequeue();
                markedSharedComponentInfo.scene.componentsManagerLegacy.SceneSharedComponentDispose(markedSharedComponentInfo.componentId);

                if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET && !immediate)
                {
                    yield return null;
                    lastTime = Time.unscaledTime;
                }
            }

            HashSet<ParcelScene> scenesToRemove = new HashSet<ParcelScene>();

            // If we have root entities queued for removal, we call Parcel Scene's RemoveEntity()
            // so that the child entities end up recursively in the entitiesMarkedForCleanup queue
            while (rootEntitiesMarkedForCleanup.Count > 0)
            {
                MarkedEntityInfo markedEntityInfo = rootEntitiesMarkedForCleanup.Dequeue();
                markedEntityInfo.scene.RemoveEntity(markedEntityInfo.entity.entityId, false);

                if (!scenesToRemove.Contains(markedEntityInfo.scene))
                    scenesToRemove.Add(markedEntityInfo.scene);

                if (!immediate && DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                {
                    yield return null;
                    lastTime = Time.unscaledTime;
                }
            }

            while (entitiesMarkedForCleanup.Count > 0)
            {
                IDCLEntity entity = entitiesMarkedForCleanup.Dequeue();
                entity.SetParent(null);
                entity.Cleanup();

                if (!immediate && DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                {
                    yield return null;
                    lastTime = Time.unscaledTime;
                }
            }

            foreach (var scene in scenesToRemove)
            {
                if (scene != null && scene.gameObject)
                    Object.Destroy(scene.gameObject);

                if (!immediate && DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                {
                    yield return null;
                    lastTime = Time.unscaledTime;
                }
            }
        }

        IEnumerator CleanupEntitiesCoroutine()
        {
            yield return null;
            Environment.i.platform.memoryManager.OnCriticalMemory += CleanMarkedEntities;

            while (true)
            {
                yield return CleanMarkedEntitiesAsync();
                yield return null;
            }
        }

        public void Dispose()
        {
            CleanMarkedEntities();

            if (removeEntitiesCoroutine != null)
                CoroutineStarter.Stop(removeEntitiesCoroutine);

            CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChange;
            Environment.i.platform.memoryManager.OnCriticalMemory -= CleanMarkedEntities;
        }
    }
}