using System;
using System.Collections.Generic;
using DCL.Bots;
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
        private PreviewSceneLimitsWarning previewSceneLimitsWarning;

        public event Action OnDebugModeSet;

        public DebugController(IBotsController botsController)
        {
            positionTracker = new CrashPayloadPositionTracker();
            isFPSPanelVisible = DataStore.i.debugConfig.isFPSPanelVisible;
            isFPSPanelVisible.OnChange += OnFPSPanelToggle;
            GameObject view = Object.Instantiate(Resources.Load("DebugView")) as GameObject;
            debugView = view.GetComponent<DebugView>();
            this.botsController = botsController;

            OnKernelConfigChanged(KernelConfig.i.Get(), null);
            KernelConfig.i.OnChange += OnKernelConfigChanged;

            debugConfig.isDebugMode.OnChange += OnToggleDebugMode;
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
                Debug.Log("Client logging ENABLED");
                ShowFPSPanel();
            }
            else
            {
                Debug.Log("Client logging DISABLED");
                Debug.unityLogger.logEnabled = false;
                HideFPSPanel();
            }

            OnDebugModeSet?.Invoke();
        }

        public void SetDebug()
        {
            debugConfig.isDebugMode.Set(true);
        }

        public void HideFPSPanel()
        {
            isFPSPanelVisible.Set(false);
        }

        public void ShowFPSPanel()
        {
            isFPSPanelVisible.Set(true);
        }

        public void ShowInfoPanel(string network, string realm)
        {
            if (debugView != null)
                debugView.ShowInfoPanel(network, realm);
        }

        public void SetRealm(string realm)
        {
            if (debugView != null)
                debugView.SetRealm(realm);
        }

        public void SetSceneDebugPanel()
        {
            if (debugView != null)
                debugView.SetSceneDebugPanel();
        }

        public void SetEngineDebugPanel()
        {
            var kernelConfig = KernelConfig.i.Get();
            if (kernelConfig.debugConfig.sceneDebugPanelEnabled)
                return;

            var newConfig = kernelConfig.Clone();
            newConfig.debugConfig.sceneDebugPanelEnabled = true;
            KernelConfig.i.Set(newConfig);
        }

        public void HideInfoPanel()
        {
            if (debugView != null)
                debugView.HideInfoPanel();
        }

        public void RunPerformanceMeterTool(float durationInSeconds)
        {
            performanceMeterController.StartSampling(durationInSeconds);
        }

        public void InstantiateBotsAtWorldPos(string configJson)
        {
            var config = new WorldPosInstantiationConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            CoroutineStarter.Start(botsController.InstantiateBotsAtWorldPos(config));
        }

        public void InstantiateBotsAtCoords(string configJson)
        {
            var config = new CoordsInstantiationConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            CoroutineStarter.Start(botsController.InstantiateBotsAtCoords(config));
        }

        public void StartBotsRandomizedMovement(string configJson)
        {
            var config = new CoordsRandomMovementConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            botsController.StartRandomMovement(config);
        }

        public void StopBotsMovement()
        {
            botsController.StopRandomMovement();
        }

        public void RemoveBot(long targetEntityId)
        {
            botsController.RemoveBot(targetEntityId);
        }

        public void ClearBots()
        {
            botsController.ClearBots();
        }

        public List<Vector3> GetTrackedTeleportPositions()
        {
            return positionTracker.teleportPositions;
        }

        public List<Vector3> GetTrackedMovements()
        {
            return positionTracker.movePositions;
        }

        public void SetAnimationCulling(bool enabled)
        {
            Environment.i.platform.cullingController.SetAnimationCulling(enabled);
        }

        public void Dispose()
        {
            positionTracker.Dispose();
            previewSceneLimitsWarning?.Dispose();
            debugConfig.isDebugMode.OnChange -= OnToggleDebugMode;
            isFPSPanelVisible.OnChange -= OnFPSPanelToggle;
            KernelConfig.i.OnChange -= OnKernelConfigChanged;

            if (debugView != null)
                Object.Destroy(debugView.gameObject);
        }

        private void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
        {
            if (debugView == null)
                return;

            if (current.debugConfig.sceneDebugPanelEnabled)
            {
                debugView.SetSceneDebugPanel();
            }

            bool enablePrevewSceneLimitWarning = !string.IsNullOrEmpty(current.debugConfig.sceneLimitsWarningSceneId);
            if (enablePrevewSceneLimitWarning && previewSceneLimitsWarning == null)
            {
                previewSceneLimitsWarning = new PreviewSceneLimitsWarning(Environment.i.world.state);
            }

            previewSceneLimitsWarning?.SetActive(enablePrevewSceneLimitWarning);
        }
    }
}