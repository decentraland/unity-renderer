using System;
using DCL.Bots;
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
        private readonly IBotsController botsController;

        public DebugView debugView;

        public readonly CrashPayloadPositionTracker positionTracker;

        public event Action OnDebugModeSet;

        public DebugController(IBotsController botsController)
        {
            positionTracker = new CrashPayloadPositionTracker();

            GameObject view = Object.Instantiate(UnityEngine.Resources.Load("DebugView")) as GameObject;
            debugView = view.GetComponent<DebugView>();
            this.botsController = botsController;
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

        public void InstantiateBotsAtWorldPos(string configJson)
        {
            var config = new DCL.Bots.WorldPosInstantiationConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            CoroutineStarter.Start(botsController.InstantiateBotsAtWorldPos(config));
        }

        public void InstantiateBotsAtCoords(string configJson)
        {
            var config = new DCL.Bots.CoordsInstantiationConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            CoroutineStarter.Start(botsController.InstantiateBotsAtCoords(config));
        }

        public void RemoveBot(string targetEntityId) { botsController.RemoveBot(targetEntityId); }

        public void ClearBots() { botsController.ClearBots(); }

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