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
        void InstantiateBotsAtWorldPos(string config);
        void InstantiateBotsAtCoords(string config);
        void StartBotsRandomizedMovement(string configJson);
        void StopBotsMovement();
        public void RemoveBot(long targetEntityId);
        public void ClearBots();
        List<Vector3> GetTrackedTeleportPositions();
        List<Vector3> GetTrackedMovements();
        void ShowInfoPanel(string network, string realm);
        void SetRealm(string realm);
        void SetAnimationCulling(bool enabled);
    }
}