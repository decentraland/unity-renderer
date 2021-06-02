using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public interface IDebugController : IDisposable
    {
        event Action OnDebugModeSet;
        void SetDebug();
        void HideFPSPanel();
        void ShowFPSPanel();
        void SetSceneDebugPanel();
        void SetEngineDebugPanel();
        void RunPerformanceMeterTool(float durationInSeconds);

        List<Vector3> GetTrackedTeleportPositions();
        List<Vector3> GetTrackedMovements();
    }
}