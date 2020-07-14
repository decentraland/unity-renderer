using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class ParcelScenesCleaner
    {
        const float MAX_TIME_BUDGET = 0.01f;

        private struct ParcelEntity
        {
            public ParcelScene scene;
            public DecentralandEntity entity;

            public ParcelEntity(ParcelScene scene, DecentralandEntity entity)
            {
                this.scene = scene;
                this.entity = entity;
            }
        }

        private struct ParcelDisposableComponent
        {
            public ParcelScene scene;
            public string componentId;

            public ParcelDisposableComponent(ParcelScene scene, string componentId)
            {
                this.scene = scene;
                this.componentId = componentId;
            }
        }

        Queue<DecentralandEntity> entitiesMarkedForCleanup = new Queue<DecentralandEntity>();
        Queue<ParcelEntity> rootEntitiesMarkedForCleanup = new Queue<ParcelEntity>();
        Queue<ParcelDisposableComponent> disposableComponentsMarkedForCleanup = new Queue<ParcelDisposableComponent>();

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

        public void MarkForCleanup(DecentralandEntity entity)
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
        public void MarkRootEntityForCleanup(ParcelScene scene, DecentralandEntity entity)
        {
            rootEntitiesMarkedForCleanup.Enqueue(new ParcelEntity(scene, entity));
        }

        public void MarkDisposableComponentForCleanup(ParcelScene scene, string componentId)
        {
            disposableComponentsMarkedForCleanup.Enqueue(new ParcelDisposableComponent(scene, componentId));
        }

        public void ForceCleanup()
        {
            ParcelScene scene = null;

            while (disposableComponentsMarkedForCleanup.Count > 0)
            {
                ParcelDisposableComponent parcelDisposableComponent = disposableComponentsMarkedForCleanup.Dequeue();
                parcelDisposableComponent.scene.SharedComponentDispose(parcelDisposableComponent.componentId);
            }

            // If we have root entities queued for removal, we call Parcel Scene's RemoveEntity()
            // so that the child entities end up recursively in the entitiesMarkedForCleanup queue
            while (rootEntitiesMarkedForCleanup.Count > 0)
            {
                // If the next scene is different to the last one
                // we removed all the entities from the parcel scene
                if (scene != null && rootEntitiesMarkedForCleanup.Peek().scene != scene)
                    break;

                ParcelEntity parcelEntity = rootEntitiesMarkedForCleanup.Dequeue();

                scene = parcelEntity.scene;
                scene.RemoveEntity(parcelEntity.entity.entityId, false);
            }

            while (entitiesMarkedForCleanup.Count > 0)
            {
                DecentralandEntity entity = entitiesMarkedForCleanup.Dequeue();
                entity.SetParent(null);
                entity.Cleanup();
            }

            if (scene != null)
                Object.Destroy(scene.gameObject);
        }

        IEnumerator CleanupEntitiesCoroutine()
        {
            while (true)
            {
                float lastTime = Time.unscaledTime;
                ParcelScene scene = null;

                while (disposableComponentsMarkedForCleanup.Count > 0)
                {
                    ParcelDisposableComponent parcelDisposableComponent = disposableComponentsMarkedForCleanup.Dequeue();
                    parcelDisposableComponent.scene.SharedComponentDispose(parcelDisposableComponent.componentId);

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                    {
                        yield return null;
                        lastTime = Time.unscaledTime;
                    }
                }

                // If we have root entities queued for removal, we call Parcel Scene's RemoveEntity()
                // so that the child entities end up recursively in the entitiesMarkedForCleanup queue
                while (rootEntitiesMarkedForCleanup.Count > 0)
                {
                    // If the next scene is different to the last one
                    // we removed all the entities from the parcel scene
                    if (scene != null && rootEntitiesMarkedForCleanup.Peek().scene != scene)
                        break;

                    ParcelEntity parcelEntity = rootEntitiesMarkedForCleanup.Dequeue();

                    scene = parcelEntity.scene;
                    scene.RemoveEntity(parcelEntity.entity.entityId, false);

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                    {
                        yield return null;
                        lastTime = Time.unscaledTime;
                    }
                }

                while (entitiesMarkedForCleanup.Count > 0)
                {
                    DecentralandEntity entity = entitiesMarkedForCleanup.Dequeue();
                    entity.SetParent(null);
                    entity.Cleanup();

                    if (DCLTime.realtimeSinceStartup - lastTime >= MAX_TIME_BUDGET)
                    {
                        yield return null;
                        lastTime = Time.unscaledTime;
                    }
                }

                if (scene != null)
                    GameObject.Destroy(scene.gameObject);

                yield return null;
            }
        }
    }
}