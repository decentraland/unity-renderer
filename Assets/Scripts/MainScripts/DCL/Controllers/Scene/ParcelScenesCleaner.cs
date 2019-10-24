using DCL.Controllers;
using DCL.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public class ParcelScenesCleaner
    {
        const float MAX_TIME_BUDGET = 0.0025f;
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

        Queue<DecentralandEntity> entitiesMarkedForCleanup = new Queue<DecentralandEntity>();
        Queue<ParcelEntity> rootEntitiesMarkedForCleanup = new Queue<ParcelEntity>();

        Coroutine removeEntitiesCoroutine;

        public void Start()
        {
            removeEntitiesCoroutine = CoroutineStarter.Start(CleanupEntitiesCoroutine());
        }

        public void Stop()
        {
            if (removeEntitiesCoroutine != null)
                SceneController.i.StopCoroutine(removeEntitiesCoroutine);
        }

        public void MarkForCleanup(DecentralandEntity entity)
        {
            if (!entity.markedForCleanup)
            {
                entity.markedForCleanup = true;
                entitiesMarkedForCleanup.Enqueue(entity);
            }
        }

        // When removing all entities from a scene, we need to separate the root entities, as stated in ParcelScene,
        // to avoid traversing a lot of child entities in the same frame and other problems
        public void MarkRootEntityForCleanup(ParcelScene scene, DecentralandEntity entity)
        {
            rootEntitiesMarkedForCleanup.Enqueue(new ParcelEntity(scene, entity));
        }

        IEnumerator CleanupEntitiesCoroutine()
        {
            while (true)
            {
                float lastTime = Time.unscaledTime;
                ParcelScene scene = null;

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
