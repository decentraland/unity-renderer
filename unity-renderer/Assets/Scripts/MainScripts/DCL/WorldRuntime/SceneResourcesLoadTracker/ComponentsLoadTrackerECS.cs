using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.ECSRuntime;
using DCL.WorldRuntime;
using UnityEngine;

public class ComponentsLoadTrackerECS : IComponentsLoadTracker
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
    
    public event Action OnResourceLoaded;
    public event Action OnStatusUpdate;
    

    private int resourcesNotReady;
    private int resourcesReady;
    private BaseCollection<IECSResourceLoaderTracker> resourceList;
    
    public ComponentsLoadTrackerECS(BaseCollection<IECSResourceLoaderTracker> resourceList)
    {
        this.resourceList = resourceList;
        resourceList.OnAdded += ResourceAdded;
    }

    private void ResourceAdded(IECSResourceLoaderTracker tracker)
    {
        resourcesNotReady++;
        tracker.OnResourceReady += ComponentReady;
    }

    private void ComponentReady(IECSResourceLoaderTracker tracker)
    {
        tracker.OnResourceReady -= ComponentReady;
        resourcesNotReady--;
        resourcesReady++;
        
        if (resourcesNotReady == 0)
        {
            OnResourceLoaded?.Invoke();
        }
        else
        {
            OnStatusUpdate?.Invoke();
        }
    }

    public void Dispose()
    {
        resourceList.OnAdded -= ResourceAdded;
    }

    public void PrintWaitingResourcesDebugInfo()
    {
        // TODO
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
