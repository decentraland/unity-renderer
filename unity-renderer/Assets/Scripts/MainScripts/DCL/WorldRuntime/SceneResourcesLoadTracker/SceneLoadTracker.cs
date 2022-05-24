using System;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL.WorldRuntime
{
    public class SceneLoadTracker : IDisposable
    {
        public int pendingResourcesCount => tracker.pendingResourcesCount;
        public float loadingProgress => tracker.loadingProgress;

        public event Action OnResourcesLoaded;
        public event Action OnResourcesStatusUpdate;

        internal IComponentsLoadTracker tracker;

        public void Track(BaseCollection<IECSResourceLoaderTracker> baseList)
        {
            if (tracker is ResourcesLoadTrackerECS)
                return;

            tracker = new ResourcesLoadTrackerECS(baseList);
            tracker.OnResourceLoaded += TrackerResourceLoaded;
            tracker.OnStatusUpdate += OnTrackerResourcesStatusUpdate;
        }
        
        public void Track(IECSComponentsManagerLegacy componentsManagerLegacy, IWorldState worldState)
        {
            if (tracker is ComponentsLoadTrackerLegacyECS)
                return;

            tracker = new ComponentsLoadTrackerLegacyECS(componentsManagerLegacy, worldState);
            tracker.OnResourceLoaded += TrackerResourceLoaded;
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
            tracker.OnResourceLoaded -= TrackerResourceLoaded;
            tracker.OnStatusUpdate -= OnTrackerResourcesStatusUpdate;
            tracker.Dispose();
        }

        private void TrackerResourceLoaded()
        {
            OnResourcesLoaded?.Invoke();
        }

        private void OnTrackerResourcesStatusUpdate()
        {
            OnResourcesStatusUpdate?.Invoke();
        }
    }
}