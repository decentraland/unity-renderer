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

        private Queue<DecentralandEntity> entitiesMarkedForCleanup = new Queue<DecentralandEntity>();
        Queue<ParcelEntity> queue = new Queue<ParcelEntity>();

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

        public void RemoveEntity(ParcelScene scene, DecentralandEntity entity)
        {
            queue.Enqueue(new ParcelEntity(scene, entity));
        }

        IEnumerator CleanupEntitiesCoroutine()
        {
            while (true)
            {
                float lastTime = Time.unscaledTime;
                ParcelScene scene = null;

                while (queue.Count > 0)
                {
                    // If the next scene is different to the last one
                    // we removed all the entities from the parcel scene
                    if (scene != null && queue.Peek().scene != scene)
                        break;

                    ParcelEntity parcelEntity = queue.Dequeue();

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
