using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.ECSRuntime;
using DCL.WorldRuntime;
using UnityEngine;

public class ResourcesLoadTrackerECS : IResourcesLoadTracker
{
    public int pendingResourcesCount => resourcesNotReady;
    
    public float loadingProgress 
    {
        get
        {
            int sharedComponentsCount = resourcesReady;
            return sharedComponentsCount > 0 ? (sharedComponentsCount - pendingResourcesCount) * 100f / sharedComponentsCount : 100f;
        }
    }
    
    public event Action OnResourcesLoaded;
    public event Action OnStatusUpdate;
    
    private int resourcesNotReady;
    private int resourcesReady;
    private BaseCollection<IECSResourceLoaderTracker> resourceList;
    
    public ResourcesLoadTrackerECS(BaseCollection<IECSResourceLoaderTracker> resourceList)
    {
        this.resourceList = resourceList;
        resourceList.OnAdded += ResourceAdded;
    }

    private void ResourceAdded(IECSResourceLoaderTracker tracker)
    {
        resourcesNotReady++;
        tracker.OnResourceReady += ResourceReady;
    }

    private void ResourceReady(IECSResourceLoaderTracker tracker)
    {
        tracker.OnResourceReady -= ResourceReady;
        resourcesNotReady--;
        resourcesReady++;
        
        if (resourcesNotReady == 0)
            OnResourcesLoaded?.Invoke();
        else
            OnStatusUpdate?.Invoke();
    }

    public void Dispose()
    {
        resourceList.OnAdded -= ResourceAdded;
    }

    public void PrintWaitingResourcesDebugInfo()
    {
        // Note: if needed we can implement this functionality to the be more exact, we can implement a way to track the current resource 
        // in the IECSResourceLoaderTracker
        Debug.Log(GetStateString());
    }

    public string GetStateString()
    {
        int totalComponents = resourcesNotReady + resourcesReady;
        if (totalComponents > 0)
            return $"left to ready:{totalComponents - resourcesReady}/{totalComponents} ({loadingProgress}%)";

        return $"no components. waiting...";
    }

    public bool CheckPendingResources()
    {
        return pendingResourcesCount > 0;
    }
}
