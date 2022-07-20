using System.Collections.Generic;
using DCL.Models;
using DCLPlugins.ECSComponents;
using UnityEngine;

namespace DCL.ECSComponents.Utils
{
    public static class OnPointerEventUtils
    {
        public static void DisposeEvent(IDCLEntity entity, DataStore_ECS7 dataStore, PointerInputRepresentantion representantion)
        {
            // We remove the event from the pool
            dataStore.entitiesOnPointerEventCounter.RemoveRefCount(entity.entityId);
            
            // If it was the last event on the entity, we also removed the colliders created for the event
            if (!dataStore.entitiesOnPointerEventCounter.ContainsKey(entity.entityId))
                DisposeEventCollider(entity, dataStore);

            // We dispose all the information related to the event
            representantion?.Dispose();
            dataStore.RemovePointerEvent(entity.entityId,representantion);
        }
        
        public static void DisposeEventCollider(IDCLEntity entity, DataStore_ECS7 dataStore)
        {
            List<GameObject> collidersToDestroy = dataStore.entityOnPointerEventColliderGameObject[entity.entityId];
            for (int x = 0; x < collidersToDestroy.Count; x++)
            {
                GameObject.Destroy(collidersToDestroy[x]);
            }
            dataStore.entityOnPointerEventColliderGameObject.Remove(entity.entityId);
        }
    }
}