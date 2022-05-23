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
            if (!self.sceneResources.ContainsKey(sceneId))
            {
                var collection = new BaseCollection<IECSResourceLoaderTracker>();
                collection.Add(resourceLoaderTracker);
                self.sceneResources.Add(sceneId, collection);
            }
            else
            {
                self.sceneResources[sceneId].Add(resourceLoaderTracker);
            }
        }
        
        public static void RemoveResourceTracker( this DataStore_ECS7 self, string sceneId, IECSResourceLoaderTracker resourceLoaderTracker)
        {
            if (!self.sceneResources.ContainsKey(sceneId))
                return;

            self.sceneResources[sceneId].Remove(resourceLoaderTracker);

            if (self.sceneResources[sceneId].Count() == 0)
                self.sceneResources.Remove(sceneId);
        }
    }
}

