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
            int sharedComponentsCount = resourcesReady.Count;
            return sharedComponentsCount > 0 ? (sharedComponentsCount - pendingResourcesCount) * 100f / sharedComponentsCount : 100f;
        }
    }
    
    public event Action OnResourcesLoaded;
    public event Action OnStatusUpdate;
    
    private readonly List<object> resourcesNotReady = new List<object>();
    private readonly List<object> resourcesReady = new List<object>();
    private readonly string sceneId;
    
    public ResourcesLoadTrackerECS(string sceneId)
    {
        this.sceneId = sceneId;
        DataStore.i.ecs7.pendingSceneResources.OnRefCountUpdated += ResourcesUpdate;
    }
    
    public void Dispose()
    {
        DataStore.i.ecs7.pendingSceneResources.OnRefCountUpdated -= ResourcesUpdate;
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
        int totalComponents = resourcesNotReady.Count + resourcesReady.Count;
        if (totalComponents > 0)
            return $"left to ready:{totalComponents - resourcesReady.Count}/{totalComponents} ({loadingProgress}%)";

        return $"no components. waiting...";
    }

    public bool CheckPendingResources()
    {
        return pendingResourcesCount > 0;
    }

    private void ResourcesUpdate((string sceneId, object model) kvp, int refCount)
    {
        if (sceneId != kvp.sceneId)
            return;
        
        if (refCount > 0)
            PendingResourceAdded(kvp.model);
        else
            ResourceReady(kvp.model);
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
      
        if(!resourcesReady.Contains(model))
            resourcesReady.Add(model);
        
        if (resourcesNotReady.Count == 0)
            OnResourcesLoaded?.Invoke();
        else
            OnStatusUpdate?.Invoke();
    }
}
