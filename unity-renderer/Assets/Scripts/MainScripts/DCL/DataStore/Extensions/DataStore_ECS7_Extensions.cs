using UnityEngine;

namespace DCL
{
    public static class DataStore_ECS7_Extensions
    {
        public static void AddPendingResource(this DataStore_ECS7 self, string sceneId, object model)
        {
            if (self.pendingSceneResources.TryGetValue(sceneId, out BaseRefCountedCollection<object> pendingResoruces))
            {
                pendingResoruces.IncreaseRefCount(model);
            }
            else
            {
                BaseRefCountedCollection<object> newCountedCollection = new BaseRefCountedCollection<object>();
                newCountedCollection.IncreaseRefCount(model);
                self.pendingSceneResources.Add(sceneId, newCountedCollection);
            }
        }

        public static void RemovePendingResource(this DataStore_ECS7 self, string sceneId, object model)
        {
            if (self.pendingSceneResources.TryGetValue(sceneId, out BaseRefCountedCollection<object> pendingResoruces))
            {
                pendingResoruces.DecreaseRefCount(model);
            }
        }

        public static void AddShapeReady(this DataStore_ECS7 self, long entityId, GameObject gameObject)
        {
            self.shapesReady.AddOrSet(entityId, gameObject);
        }

        public static void RemoveShapeReady(this DataStore_ECS7 self, long entityId)
        {
            if (self.shapesReady.ContainsKey(entityId))
                self.shapesReady.Remove(entityId);
        }
    }
}