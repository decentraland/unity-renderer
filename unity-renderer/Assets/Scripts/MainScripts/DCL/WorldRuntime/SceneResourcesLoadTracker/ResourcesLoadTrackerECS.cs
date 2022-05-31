using System;
using System.Collections.Generic;
using DCL;
using DCL.WorldRuntime;
using UnityEngine;

public class ResourcesLoadTrackerECS : IResourcesLoadTracker
{
    public int pendingResourcesCount => resourcesNotReady.Count;
    
    public float loadingProgress 
    {
        get
        {
            int resourcesReadyCount = resourcesReady;
            return resourcesReadyCount > 0 ? (resourcesReadyCount - pendingResourcesCount) * 100f / resourcesReadyCount : 100f;
        }
    }
    
    public event Action OnResourcesLoaded;
    public event Action OnStatusUpdate;
    
    private int resourcesReady;
    
    private readonly List<object> resourcesNotReady = new List<object>();
    private readonly string sceneId;
    private readonly DataStore_ECS7 dataStore;
    
    public ResourcesLoadTrackerECS(DataStore_ECS7 dataStoreEcs7,string sceneId)
    {
        this.dataStore = dataStoreEcs7;
        this.sceneId = sceneId;
        dataStore.pendingSceneResources.AddOrSet(sceneId, new BaseRefCountedCollection<object>());
        dataStore.pendingSceneResources[sceneId].OnRefCountUpdated += ResourcesUpdate;
    }

    public void Dispose()
    {
        dataStore.pendingSceneResources[sceneId].OnRefCountUpdated -= ResourcesUpdate;
    }

    public void PrintWaitingResourcesDebugInfo()
    {
        foreach (var model in resourcesNotReady)
        {
            Debug.Log($"This model is waiting to be loaded: " + model);
        }
    }

    public string GetStateString()
    {
        int totalComponents = resourcesNotReady.Count + resourcesReady;
        if (totalComponents > 0)
            return $"left to ready:{totalComponents - resourcesReady}/{totalComponents} ({loadingProgress}%)";

        return $"no components. waiting...";
    }

    public bool CheckPendingResources()
    {
        return pendingResourcesCount > 0;
    }

    private void ResourcesUpdate(object model, int refCount)
    {
        if (refCount > 0)
            PendingResourceAdded(model);
        else
            ResourceReady(model);
    }

    private void PendingResourceAdded(object model)
    {
        if(!resourcesNotReady.Contains(model))
            resourcesNotReady.Add(model);
    }

    private void ResourceReady(object model)
    {
        if(resourcesNotReady.Contains(model))
            resourcesNotReady.Remove(model);

        resourcesReady++;
        
        if (resourcesNotReady.Count == 0)
            OnResourcesLoaded?.Invoke();
        else
            OnStatusUpdate?.Invoke();
    }
}
