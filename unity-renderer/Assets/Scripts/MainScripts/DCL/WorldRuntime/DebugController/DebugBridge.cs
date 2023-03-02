using System;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Profiling;

namespace DCL
{
    public class DebugBridge : MonoBehaviour
    {
        [Serializable]
        class ToggleSceneBoundingBoxesPayload
        {
            public int sceneNumber; // TODO: change something kernel-side ???
            public bool enabled;
        }

        [Serializable]
        class MemoryDescriptionPayload
        {
            public long jsHeapSizeLimit;
            public long totalJSHeapSize;
            public long usedJSHeapSize;
        }

        private ILogger debugLogger = new Logger(Debug.unityLogger.logHandler);
        private IDebugController debugController;

        public void Setup(IDebugController debugController)
        {
            this.debugController = debugController;
        }

        // Beware this SetDebug() may be called before Awake() somehow...
        [ContextMenu("Set Debug mode")]
        public void SetDebug()
        {
            debugController.SetDebug();
        }

        public void HideFPSPanel()
        {
            debugController.ToggleFPSPanel();
        }

        public void ShowFPSPanel()
        {
            debugController.ToggleFPSPanel();
        }

        public void SetSceneDebugPanel()
        {
            debugController.SetSceneDebugPanel();
        }
        
        public void SetMemoryUsage(string payload)
        {
            var data = JsonUtility.FromJson<MemoryDescriptionPayload>(payload);
            DataStore.i.debugConfig.jsUsedHeapSize.Set(data.usedJSHeapSize);
            DataStore.i.debugConfig.jsHeapSizeLimit.Set(data.jsHeapSizeLimit);
            DataStore.i.debugConfig.jsTotalHeapSize.Set(data.totalJSHeapSize);
        }

        public void SetEngineDebugPanel()
        {
            debugController.SetEngineDebugPanel();
        }

        [ContextMenu("Dump Scenes Load Info")]
        public void DumpScenesLoadInfo()
        {
            bool originalLoggingValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;
            foreach (var kvp in DCL.Environment.i.world.state.GetLoadedScenes())
            {
                IParcelScene scene = kvp.Value;
                debugLogger.Log("Dumping state for scene: " + kvp.Value.sceneData.sceneNumber);
                scene.GetWaitingComponentsDebugInfo();
            }

            Debug.unityLogger.logEnabled = originalLoggingValue;
        }

        [ContextMenu("Dump Scene Metrics Offenders")]
        public void DumpSceneMetricsOffenders()
        {
            bool originalLoggingValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            var worstMetricOffenses = DataStore.i.Get<DataStore_SceneMetrics>().worstMetricOffenses;
            foreach (var offense in worstMetricOffenses)
            {
                debugLogger.Log($"Scene: {offense.Key} ... Metrics: {offense.Value}");
            }

            Debug.unityLogger.logEnabled = originalLoggingValue;
        }

        public void SetDisableAssetBundles()
        {
            RendereableAssetLoadHelper.defaultLoadingType = RendereableAssetLoadHelper.LoadingType.GLTF_ONLY;
        }

        [ContextMenu("Dump Renderers Lockers Info")]
        public void DumpRendererLockersInfo()
        {
            bool originalLoggingValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            RenderingController renderingController = FindObjectOfType<RenderingController>();
            if (renderingController == null)
            {
                debugLogger.Log("RenderingController not found. Aborting.");
                return;
            }

            debugLogger.Log($"Renderer is locked? {!renderingController.renderingActivatedAckLock.isUnlocked}");
            debugLogger.Log($"Renderer is active? {CommonScriptableObjects.rendererState.Get()}");

            System.Collections.Generic.HashSet<object> lockIds =
                renderingController.renderingActivatedAckLock.GetLockIdsCopy();

            foreach (var lockId in lockIds)
            {
                debugLogger.Log($"Renderer is locked by id: {lockId} of type {lockId.GetType()}");
            }

            Debug.unityLogger.logEnabled = originalLoggingValue;
        }

        public void CrashPayloadRequest()
        {
            bool originalLoggingValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            var crashPayload = CrashPayloadUtils.ComputePayload
            (
                DCL.Environment.i.world.state.GetLoadedScenes(),
                debugController.GetTrackedMovements(),
                debugController.GetTrackedTeleportPositions()
            );

            CrashPayloadResponse(crashPayload);

            Debug.unityLogger.logEnabled = originalLoggingValue;
        }

        public void CrashPayloadResponse(CrashPayload payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            WebInterface.MessageFromEngine("CrashPayloadResponse", json);
        }

        [ContextMenu("Dump Crash Payload")]
        public void DumpCrashPayload()
        {
            bool originalLoggingValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            debugLogger.Log(
                $"MEMORY -- total {Profiler.GetTotalAllocatedMemoryLong()} ... used by mono {Profiler.GetMonoUsedSizeLong()}");

            var payload = CrashPayloadUtils.ComputePayload
            (
                DCL.Environment.i.world.state.GetLoadedScenes(),
                debugController.GetTrackedMovements(),
                debugController.GetTrackedTeleportPositions()
            );

            foreach (var field in payload.fields)
            {
                string dump = JsonConvert.SerializeObject(field.Value);
                debugLogger.Log($"Crash payload ({field.Key}): {dump}");
            }

            string fullDump = JsonConvert.SerializeObject(payload);
            debugLogger.Log($"Full crash payload size: {fullDump.Length}");

            Debug.unityLogger.logEnabled = originalLoggingValue;
        }

        public void RunPerformanceMeterTool(float durationInSeconds)
        {
            debugController.RunPerformanceMeterTool(durationInSeconds);
        }

        public void InstantiateBotsAtWorldPos(string configJson)
        {
            debugController.InstantiateBotsAtWorldPos(configJson);
        }

        public void InstantiateBotsAtCoords(string configJson)
        {
            debugController.InstantiateBotsAtCoords(configJson);
        }

        public void StartBotsRandomizedMovement(string configJson)
        {
            debugController.StartBotsRandomizedMovement(configJson);
        }

        public void StopBotsMovement()
        {
            debugController.StopBotsMovement();
        }

        public void RemoveBot(long targetEntityId)
        {
            debugController.RemoveBot(targetEntityId);
        }

        public void ClearBots()
        {
            debugController.ClearBots();
        }

        public void ToggleSceneBoundingBoxes(string payload)
        {
            ToggleSceneBoundingBoxesPayload data = JsonUtility.FromJson<ToggleSceneBoundingBoxesPayload>(payload);
            DataStore.i.debugConfig.showSceneBoundingBoxes.AddOrSet(data.sceneNumber, data.enabled);
        }

        public void TogglePreviewMenu(string payload)
        {
            PreviewMenuPayload data = JsonUtility.FromJson<PreviewMenuPayload>(payload);
            DataStore.i.debugConfig.isPreviewMenuActive.Set(data.enabled);
        }

        public void ToggleSceneSpawnPoints(string payload)
        {
            ToggleSpawnPointsPayload data = Utils.FromJsonWithNulls<ToggleSpawnPointsPayload>(payload);
            // handle `undefined` `enabled` which means to update "spawnpoints" without changing it enabled state
            if (!data.enabled.HasValue)
            {
                var prevData = DataStore.i.debugConfig.showSceneSpawnPoints.Get(data.sceneId);
                data.enabled = prevData == null || !prevData.enabled.HasValue ? false : prevData.enabled;
            }

            DataStore.i.debugConfig.showSceneSpawnPoints.AddOrSet(data.sceneId, data);
        }
        
        [ContextMenu("Enable Animation Culling")]
        public void EnableAnimationCulling()
        {
            debugController.SetAnimationCulling(true);
        }
        
        [ContextMenu("Disable Animation Culling")]
        public void DisableAnimationCulling()
        {
            debugController.SetAnimationCulling(false);
        }

#if UNITY_EDITOR
        [ContextMenu("Run Performance Meter Tool for 2 seconds")]
        public void ShortDebugPerformanceMeter()
        {
            RunPerformanceMeterTool(2);
        }

        [ContextMenu("Run Performance Meter Tool for 10 seconds")]
        public void ShortDebugPerformanceMeter10()
        {
            RunPerformanceMeterTool(10);
        }

        [ContextMenu("Run Performance Meter Tool for 30 seconds")]
        public void DebugPerformanceMeter()
        {
            RunPerformanceMeterTool(30);
        }

        [ContextMenu("Instantiate 3 bots at player coordinates")]
        public void DebugBotsInstantiation()
        {
            InstantiateBotsAtCoords("{ " +
                                    "\"amount\":3, " +
                                    "\"areaWidth\":15, " +
                                    "\"areaDepth\":15 " +
                                    "}");
        }
        
        [ContextMenu("Instantiate 50 bots at player coordinates")]
        public void DebugBotsInstantiation2()
        {
            InstantiateBotsAtCoords("{ " +
                                    "\"amount\":50, " +
                                    "\"areaWidth\":15, " +
                                    "\"areaDepth\":15 " +
                                    "}");
        }
#endif
    }
}