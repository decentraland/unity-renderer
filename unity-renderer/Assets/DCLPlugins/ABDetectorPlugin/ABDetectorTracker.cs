using System;

namespace DCL
{
    public class ABDetectorTracker : IDisposable
    {
        private readonly DebugConfig debugConfig;

        public ABDetectorTracker(DebugConfig debugConfig)
        {
            this.debugConfig = debugConfig;
            
            debugConfig.showGlobalABDetectionLayer.OnChange += OnGlobalABDetectionChanged;
            debugConfig.showSceneABDetectionLayer.OnChange += OnSceneABDetectionChanged;
        }

        private void OnGlobalABDetectionChanged(bool current, bool previous)
        {
            
        }

        private void OnSceneABDetectionChanged(bool current, bool previous)
        {
            
        }

        public void Dispose()
        {
            debugConfig.showGlobalABDetectionLayer.OnChange -= OnGlobalABDetectionChanged;
            debugConfig.showSceneABDetectionLayer.OnChange -= OnSceneABDetectionChanged;
        }
    }
}