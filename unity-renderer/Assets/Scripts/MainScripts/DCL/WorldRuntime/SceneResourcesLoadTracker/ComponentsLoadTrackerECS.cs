using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.ECSRuntime;
using DCL.WorldRuntime;
using UnityEngine;

public class ComponentsLoadTrackerECS : IComponentsLoadTracker
{
    public int pendingResourcesCount => componentsNotReady.Count;
    
    public float loadingProgress 
    {
        get
        {
            int sharedComponentsCount = componentsReady.Count;
            return sharedComponentsCount > 0 ? (sharedComponentsCount - pendingResourcesCount) * 100f / sharedComponentsCount : 100f;
        }
    }
    
    public event Action OnComponentsLoaded;
    public event Action OnStatusUpdate;
    
    // We are storing the whole component for debugging purposes, this way we can see which models are not loading if needed 
    private List<IECSComponent> componentsNotReady = new List<IECSComponent>();
    private List<IECSComponent> componentsReady = new List<IECSComponent>();
    private IECSComponentsManager componentsManager;
    
    public ComponentsLoadTrackerECS(IECSComponentsManager componentsManager)
    {
        this.componentsManager = componentsManager;
        componentsManager.OnComponentAdded += ComponentAdded;
    }

    private void ComponentAdded(IECSComponent component)
    {
        component.OnComponentReady += ComponentReady;
        componentsNotReady.Add(component);
    }

    private void ComponentReady(IECSComponent component)
    {
        componentsNotReady.Remove(component);
        componentsReady.Add(component);
        
        if (componentsNotReady.Count == 0)
        {
            OnComponentsLoaded?.Invoke();
        }
        else
        {
            OnStatusUpdate?.Invoke();
        }
    }

    public void Dispose()
    {
        componentsManager.OnComponentAdded -= ComponentAdded;
    }

    public void PrintWaitingResourcesDebugInfo()
    {
        foreach (IECSComponent ecsComponent in componentsReady)
        {
            Debug.Log($"ComponentReady: {ecsComponent.ToString()}");
        }
        
        foreach (IECSComponent ecsComponent in componentsNotReady)
        {
            Debug.Log($"Waiting for: {ecsComponent.ToString()}");
        }
    }

    public string GetStateString()
    {
        int totalComponents = componentsNotReady.Count + componentsReady.Count;
        if (totalComponents > 0)
            return $"left to ready:{totalComponents - componentsReady.Count}/{totalComponents} ({loadingProgress}%)";

        return $"no components. waiting...";
    }

    public bool CheckPendingResources()
    {
        return pendingResourcesCount > 0;
    }
}
