using System;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class DebugController : IDebugController
    {
        private DebugConfig debugConfig => DataStore.i.debugConfig;
        private readonly PerformanceMeterController performanceMeterController = new PerformanceMeterController();

        public DebugView debugView;

        public readonly CrashPayloadPositionTracker positionTracker;

        public event Action OnDebugModeSet;

        public DebugController()
        {
            positionTracker = new CrashPayloadPositionTracker();

            GameObject view = Object.Instantiate(UnityEngine.Resources.Load("DebugView")) as GameObject;
            debugView = view.GetComponent<DebugView>();
        }

        public void SetDebug()
        {
            Debug.unityLogger.logEnabled = true;

            debugConfig.isDebugMode = true;

            ShowFPSPanel();

            OnDebugModeSet?.Invoke();
        }

        public void HideFPSPanel()
        {
            if (debugView != null)
                debugView.HideFPSPanel();
        }

        public void ShowFPSPanel()
        {
            if (debugView != null)
                debugView.ShowFPSPanel();
        }

        public void SetSceneDebugPanel()
        {
            if (debugView != null)
                debugView.SetSceneDebugPanel();
        }

        public void SetEngineDebugPanel()
        {
            if (debugView != null)
                debugView.SetEngineDebugPanel();
        }

        public void RunPerformanceMeterTool(float durationInSeconds) { performanceMeterController.StartSampling(durationInSeconds); }

        public List<Vector3> GetTrackedTeleportPositions() { return positionTracker.teleportPositions; }

        public List<Vector3> GetTrackedMovements() { return positionTracker.movePositions; }

        public void Dispose()
        {
            positionTracker.Dispose();
            if (debugView != null)
                Object.Destroy(debugView.gameObject);
        }
    }
}