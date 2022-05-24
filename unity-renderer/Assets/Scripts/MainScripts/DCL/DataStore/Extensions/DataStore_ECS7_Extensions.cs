using System.Collections;
using System.Collections.Generic;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL
{
    public static class DataStore_ECS7_Extensions 
    {
        public static void AddResourceTracker( this DataStore_ECS7 self, string sceneId, IECSResourceLoaderTracker resourceLoaderTracker)
        {
            if (self.sceneResources.TryGetValue(sceneId, out BaseCollection<IECSResourceLoaderTracker> baseCollection))
            {
                baseCollection.Add(resourceLoaderTracker);
            }
            else
            {
                var collection = new BaseCollection<IECSResourceLoaderTracker>();
                collection.Add(resourceLoaderTracker);
                self.sceneResources.Add(sceneId, collection);
            }
        }

        public static void RemoveResourceTracker( this DataStore_ECS7 self, string sceneId, IECSResourceLoaderTracker resourceLoaderTracker)
        {
            if (!self.sceneResources.TryGetValue(sceneId, out BaseCollection<IECSResourceLoaderTracker> baseCollection))
                return;

            baseCollection.Remove(resourceLoaderTracker);
            if (baseCollection.Count() == 0)
                self.sceneResources.Remove(sceneId);
        }
    }
}

