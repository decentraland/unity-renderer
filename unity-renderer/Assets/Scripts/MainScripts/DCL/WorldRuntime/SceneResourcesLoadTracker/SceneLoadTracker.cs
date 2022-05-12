using System;
using UnityEngine;

namespace DCL.WorldRuntime
{
    public class SceneLoadTracker : IDisposable
    {
        public int pendingResourcesCount => tracker.pendingResourcesCount;
        public float loadingProgress => tracker.loadingProgress;

        public event Action OnResourcesLoaded;
        public event Action OnResourcesStatusUpdate;

        private IComponentsLoadTracker tracker;

        public void Track(IECSComponentsManager componentsManager)
        {
            if (tracker != null)
                return;

            tracker = new ComponentsLoadTrackerECS(componentsManager);
            tracker.OnResourcesLoaded += OnTrackerResourcesLoaded;
            tracker.OnStatusUpdate += OnTrackerResourcesStatusUpdate;
        }
        
        public void Track(IECSComponentsManagerLegacy componentsManagerLegacy, IWorldState worldState)
        {
            if (tracker != null)
                return;

            tracker = new ComponentsLoadTrackerLegacyECS(componentsManagerLegacy, worldState);
            tracker.OnResourcesLoaded += OnTrackerResourcesLoaded;
            tracker.OnStatusUpdate += OnTrackerResourcesStatusUpdate;
        }

        public void Dispose()
        {
            DisposeCurrentTracker();
        }

        public bool ShouldWaitForPendingResources()
        {
            if (tracker.pendingResourcesCount == 0)
            {
                return false;
            }
            return tracker.CheckPendingResources();
        }

        public void PrintWaitingResourcesDebugInfo()
        {
            if (tracker == null)
            {
                Debug.Log($"PrintWaitingResourcesDebugInfo failed. SceneResourcesLoadTracker tracker == null");
                return;
            }

            tracker.PrintWaitingResourcesDebugInfo();
        }

        public string GetStateString()
        {
            if (tracker == null)
            {
                return "SceneResourcesLoadTracker tracker == null";
            }

            return tracker.GetStateString();
        }

        private void DisposeCurrentTracker()
        {
            if (tracker == null)
            {
                return;
            }
            tracker.OnResourcesLoaded -= OnTrackerResourcesLoaded;
            tracker.OnStatusUpdate -= OnTrackerResourcesStatusUpdate;
            tracker.Dispose();
        }

        private void OnTrackerResourcesLoaded()
        {
            OnResourcesLoaded?.Invoke();
        }

        private void OnTrackerResourcesStatusUpdate()
        {
            OnResourcesStatusUpdate?.Invoke();
        }
    }
}