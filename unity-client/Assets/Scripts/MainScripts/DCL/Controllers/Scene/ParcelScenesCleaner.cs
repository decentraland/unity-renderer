using System;
using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public interface IParcelScenesCleaner : IDisposable
    {
        void Start();
        void Stop();
        void MarkForCleanup(IDCLEntity entity);
        void MarkRootEntityForCleanup(ParcelScene scene, IDCLEntity entity);
        void MarkDisposableComponentForCleanup(ParcelScene scene, string componentId);
        void ForceCleanup();
    }

    public class ParcelScenesCleaner : IParcelScenesCleaner
    {
        const float MAX_TIME_BUDGET = 0.01f;

        private struct MarkedEntityInfo
        {
            public ParcelScene scene;
            public IDCLEntity entity;

            public MarkedEntityInfo(ParcelScene scene, IDCLEntity entity)
            {
                this.scene = scene;
                this.entity = entity;
            }
        }

        private struct MarkedSharedComponentInfo
        {
            public ParcelScene scene;
            public string componentId;

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

        public void Start()
        {
            removeEntitiesCoroutine = CoroutineStarter.Start(CleanupEntitiesCoroutine());
        }

        public void Stop()
        {
            if (removeEntitiesCoroutine != null)
                CoroutineStarter.Stop(removeEntitiesCoroutine);
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
        public void MarkRootEntityForCleanup(ParcelScene scene, IDCLEntity entity)
        {
            rootEntitiesMarkedForCleanup.Enqueue(new MarkedEntityInfo(scene, entity));
        }

        public void MarkDisposableComponentForCleanup(ParcelScene scene, string componentId)
        {
            disposableComponentsMarkedForCleanup.Enqueue(new MarkedSharedComponentInfo(scene, componentId));
        }

        public void ForceCleanup()
        {
            while (disposableComponentsMarkedForCleanup.Count > 0)
            {
                MarkedSharedComponentInfo markedSharedComponentInfo = disposableComponentsMarkedForCleanup.Dequeue();
                markedSharedComponentInfo.scene.SharedComponentDispose(markedSharedComponentInfo.componentId);
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
            }

            while (entitiesMarkedForCleanup.Count > 0)
            {
                IDCLEntity entity = entitiesMarkedForCleanup.Dequeue();
                entity.SetParent(null);
                entity.Cleanup();
            }

            foreach (var scene in scenesToRemove)
            {
                if (scene != null && !Environment.i.world.state.loadedScenes.ContainsKey(scene.sceneData.id))
                    Object.Destroy(scene.gameObject);
            }
        }

        IEnumerator CleanupEntitiesCoroutine()
        {
            while (true)
            {
                float lastTime = Time.unscaledTime;

                while (disposableComponentsMarkedForCleanup.Count > 0)
                {
                    MarkedSharedComponentInfo markedSharedComponentInfo = disposableComponentsMarkedForCleanup.Dequeue();
                    markedSharedComponentInfo.scene.SharedComponentDispose(markedSharedComponentInfo.componentId);

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
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

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
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

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                    {
                        yield return null;
                        lastTime = Time.unscaledTime;
                    }
                }

                foreach (var scene in scenesToRemove)
                {
                    if (scene != null && !Environment.i.world.state.loadedScenes.ContainsKey(scene.sceneData.id))
                    {
                        Object.Destroy(scene.gameObject);

                        if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                        {
                            yield return null;
                            lastTime = Time.unscaledTime;
                        }
                    }
                }

                yield return null;
            }
        }

        public void Dispose()
        {
            ForceCleanup();
            Stop();
        }
    }
}