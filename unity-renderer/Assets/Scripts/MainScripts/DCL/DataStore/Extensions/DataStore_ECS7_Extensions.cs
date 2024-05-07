namespace DCL
{
    public static class DataStore_ECS7_Extensions
    {
        public static void AddPendingResource(this DataStore_ECS7 self, int sceneNumber, object model)
        {
            if (self.pendingSceneResources.TryGetValue(sceneNumber, out BaseRefCountedCollection<object> pendingResoruces))
            {
                pendingResoruces.IncreaseRefCount(model);
            }
            else
            {
                BaseRefCountedCollection<object> newCountedCollection = new BaseRefCountedCollection<object>();
                newCountedCollection.IncreaseRefCount(model);
                self.pendingSceneResources.Add(sceneNumber, newCountedCollection);
            }
        }

        public static void RemovePendingResource(this DataStore_ECS7 self, int sceneNumber, object model)
        {
            if (self.pendingSceneResources.TryGetValue(sceneNumber, out BaseRefCountedCollection<object> pendingResoruces))
            {
                pendingResoruces.DecreaseRefCount(model);
            }
        }
    }
}
