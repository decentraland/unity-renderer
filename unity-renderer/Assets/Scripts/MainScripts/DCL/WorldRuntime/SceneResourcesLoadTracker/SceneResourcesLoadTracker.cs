using System;
using DCL.ECSRuntime;
using UnityEngine;

namespace DCL.WorldRuntime
{
    public class SceneResourcesLoadTracker : IDisposable
    {
        public int pendingResourcesCount => tracker.pendingResourcesCount;
        public float loadingProgress => tracker.loadingProgress;

        public event Action OnResourcesLoaded;
        public event Action OnResourcesStatusUpdate;

        internal IResourcesLoadTracker tracker;

        public void Track(int sceneNumber)
        {
            if (tracker is ResourcesLoadTrackerECS)
                return;

            tracker = new ResourcesLoadTrackerECS(DataStore.i.ecs7, sceneNumber);
            tracker.OnResourcesLoaded += TrackerResourcesLoaded;
            tracker.OnStatusUpdate += OnTrackerResourcesStatusUpdate;
        }
        
        public void Track(IECSComponentsManagerLegacy componentsManagerLegacy, IWorldState worldState)
        {
            if (tracker is ResourcesLoadTrackerLegacyECS)
                return;

            tracker = new ResourcesLoadTrackerLegacyECS(componentsManagerLegacy, worldState);
            tracker.OnResourcesLoaded += TrackerResourcesLoaded;
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
            tracker.OnResourcesLoaded -= TrackerResourcesLoaded;
            tracker.OnStatusUpdate -= OnTrackerResourcesStatusUpdate;
            tracker.Dispose();
        }

        private void TrackerResourcesLoaded()
        {
            OnResourcesLoaded?.Invoke();
        }

        private void OnTrackerResourcesStatusUpdate()
        {
            OnResourcesStatusUpdate?.Invoke();
        }
    }
}