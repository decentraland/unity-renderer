﻿using System;
using DCL.Bots;
using System.Collections.Generic;
using DCL.Helpers;
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
        private BaseVariable<bool> isFPSPanelVisible;

        public event Action OnDebugModeSet;

        public DebugController(IBotsController botsController)
        {
            positionTracker = new CrashPayloadPositionTracker();
            isFPSPanelVisible = DataStore.i.debugConfig.isFPSPanelVisible;
            isFPSPanelVisible.OnChange += OnFPSPanelToggle;
            GameObject view = Object.Instantiate(UnityEngine.Resources.Load("DebugView")) as GameObject;
            debugView = view.GetComponent<DebugView>();
            this.botsController = botsController;
        }
        private void OnFPSPanelToggle(bool current, bool previous)
        {
            if (current == previous || debugView == null)
                return;
            if (current)
            {
                debugView.ShowFPSPanel();
            }
            else
            {
                debugView.HideFPSPanel();
            }
        }

        private void OnToggleDebugMode(bool current, bool previous)
        {
            if (current == previous)
                return;
            
            if (current)
            {
                Debug.unityLogger.logEnabled = true;
                ShowFPSPanel();
            }
            else
            {
                Debug.unityLogger.logEnabled = false;
                HideFPSPanel();
            }
            
            OnDebugModeSet?.Invoke();
        }

        public void SetDebug() { debugConfig.isDebugMode.Set(true); }

        public void HideFPSPanel() { isFPSPanelVisible.Set(false); }

        public void ShowFPSPanel() { isFPSPanelVisible.Set(true); }

        public void ShowInfoPanel(string network, string realm)
        {
            if (debugView != null)
                debugView.ShowInfoPanel(network, realm);
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
        
        public void HideInfoPanel()
        {
            if (debugView != null)
                debugView.HideInfoPanel();
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

        public void StartBotsRandomizedMovement(string configJson)
        {
            var config = new DCL.Bots.CoordsRandomMovementConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            botsController.StartRandomMovement(config);
        }

        public void StopBotsMovement() { botsController.StopRandomMovement(); }

        public void RemoveBot(string targetEntityId) { botsController.RemoveBot(targetEntityId); }

        public void ClearBots() { botsController.ClearBots(); }

        public List<Vector3> GetTrackedTeleportPositions() { return positionTracker.teleportPositions; }

        public List<Vector3> GetTrackedMovements() { return positionTracker.movePositions; }

        public void Dispose()
        {
            positionTracker.Dispose();
            isFPSPanelVisible.OnChange -= OnFPSPanelToggle;

            if (debugView != null)
                Object.Destroy(debugView.gameObject);
        }
        
    }
}