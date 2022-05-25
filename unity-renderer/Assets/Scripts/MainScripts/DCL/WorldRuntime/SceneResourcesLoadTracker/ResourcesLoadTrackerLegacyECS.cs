using System;
using System.Collections.Generic;
using System.Linq;
using DCL.Components;
using UnityEngine;

namespace DCL.WorldRuntime
{
    internal class ResourcesLoadTrackerLegacyECS : IResourcesLoadTracker
    {
        public int pendingResourcesCount => disposableNotReady?.Count ?? 0;
        public float loadingProgress
        {
            get
            {
                int sharedComponentsCount = componentsManager.GetSceneSharedComponentsCount();
                return sharedComponentsCount > 0 ? (sharedComponentsCount - pendingResourcesCount) * 100f / sharedComponentsCount : 100f;
            }
        }

        public event Action OnResourcesLoaded;
        public event Action OnStatusUpdate;

        private readonly IECSComponentsManagerLegacy componentsManager;
        private readonly IWorldState worldState;

        private List<string> disposableNotReady = new List<string>();

        public ResourcesLoadTrackerLegacyECS(IECSComponentsManagerLegacy componentsManager, IWorldState worldState)
        {
            this.componentsManager = componentsManager;
            this.worldState = worldState;

            componentsManager.OnAddSharedComponent += OnSharedComponentAdded;
        }

        public void Dispose()
        {
            componentsManager.OnAddSharedComponent -= OnSharedComponentAdded;
            disposableNotReady = null;
        }

        public bool CheckPendingResources()
        {
            List<ISharedComponent> disposableComponents = componentsManager.GetSceneSharedComponentsDictionary().Values.ToList();
            for (int i = 0; i < disposableComponents.Count; i++)
            {
                if (!disposableNotReady.Contains(disposableComponents[i].id))
                {
                    disposableNotReady.Add(disposableComponents[i].id);
                }
                disposableComponents[i].CallWhenReady(OnDisposableReady);
            }
            return disposableNotReady.Count > 0;
        }

        public void PrintWaitingResourcesDebugInfo()
        {
            foreach (string componentId in disposableNotReady)
            {
                if (componentsManager.HasSceneSharedComponent(componentId))
                {
                    var component = componentsManager.GetSceneSharedComponent(componentId);

                    Debug.Log($"Waiting for: {component.ToString()}");

                    foreach (var entity in component.GetAttachedEntities())
                    {
                        var loader = worldState.GetLoaderForEntity(entity);

                        string loadInfo = "No loader";

                        if (loader != null)
                        {
                            loadInfo = loader.ToString();
                        }

                        Debug.Log($"This shape is attached to {entity.entityId} entity. Click here for highlight it.\nLoading info: {loadInfo}", entity.gameObject);
                    }
                }
                else
                {
                    Debug.Log($"Waiting for missing component? id: {componentId}");
                }
            }
        }

        public string GetStateString()
        {
            int sharedComponentsCount = componentsManager.GetSceneSharedComponentsCount();
            if (sharedComponentsCount > 0)
                return $"left to ready:{sharedComponentsCount - pendingResourcesCount}/{sharedComponentsCount} ({loadingProgress}%)";

            return $"no components. waiting...";
        }

        private void OnSharedComponentAdded(string id, ISharedComponent component)
        {
            disposableNotReady.Add(id);
        }

        private void OnDisposableReady(ISharedComponent component)
        {
            if (disposableNotReady == null)
                return;

            if (!disposableNotReady.Remove(component.id))
                return;

            if (disposableNotReady.Count == 0)
            {
                OnResourcesLoaded?.Invoke();
            }
            else
            {
                OnStatusUpdate?.Invoke();
            }
        }
    }
}