using System.Collections.Generic;
using DCLPlugins.UUIDEventComponentsPlugin.UUIDComponent.Interfaces;
using UnityEngine;

namespace DCL
{
    public static class DataStore_ECS7_Extensions
    {
        public static void AddPointerEvent( this DataStore_ECS7 self, long entityId, IPointerInputEvent pointerEvent )
        {
            if (self.entityEvents.TryGetValue(entityId, out List<IPointerInputEvent> events))
            {
               events.Add(pointerEvent);
            }
            else
            {
                events = new List<IPointerInputEvent>()
                {
                    pointerEvent
                };
                self.entityEvents.Add(entityId,events);
            }
        }
        
        public static void RemovePointerEvent( this DataStore_ECS7 self, long entityId, IPointerInputEvent pointerEvent )
        {
            if (self.entityEvents.TryGetValue(entityId, out List<IPointerInputEvent> events))
            {
                events.Remove(pointerEvent);
                if (events.Count == 0)
                    self.entityEvents.Remove(entityId);
            }
        }
        
        public static void AddPendingResource( this DataStore_ECS7 self, string sceneId, object model )
        {
            if (self.pendingSceneResources.TryGetValue(sceneId, out BaseRefCountedCollection<object> pendingResoruces))
            {
                pendingResoruces.IncreaseRefCount(model);
            }
            else
            {
                BaseRefCountedCollection<object>  newCountedCollection = new BaseRefCountedCollection<object>();
                newCountedCollection.IncreaseRefCount(model);
                self.pendingSceneResources.Add(sceneId,newCountedCollection);
            }
        }

        public static void RemovePendingResource( this DataStore_ECS7 self, string sceneId, object model  )
        {
            if (self.pendingSceneResources.TryGetValue(sceneId, out BaseRefCountedCollection<object> pendingResoruces))
            {
                pendingResoruces.DecreaseRefCount(model);
            }
        }
        
        public static void AddReadyAnimatorShape( this DataStore_ECS7 self, long entityId, GameObject gameObject )
        {
            self.shapesReady.AddOrSet(entityId,gameObject);
        }

        public static void RemoveReadyAnimatorShape( this DataStore_ECS7 self, long entityId)
        {
            if(self.shapesReady.ContainsKey(entityId))
                self.shapesReady.Remove(entityId);
        }
    }
}