using System;
using System.Collections.Generic;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Interface;
using Newtonsoft.Json;
using UnityEngine;

namespace DCL
{
    public class DebugBridge : MonoBehaviour
    {
        private ILogger debugLogger = new Logger(Debug.unityLogger.logHandler);

        // Beware this SetDebug() may be called before Awake() somehow...
        [ContextMenu("Set Debug mode")]
        public void SetDebug() { Environment.i.platform.debugController.SetDebug(); }

        public void HideFPSPanel() { Environment.i.platform.debugController.HideFPSPanel(); }

        public void ShowFPSPanel() { Environment.i.platform.debugController.ShowFPSPanel(); }

        public void SetSceneDebugPanel() { Environment.i.platform.debugController.SetSceneDebugPanel(); }

        public void SetEngineDebugPanel() { Environment.i.platform.debugController.SetEngineDebugPanel(); }

        [ContextMenu("Dump Scenes Load Info")]
        public void DumpScenesLoadInfo()
        {
            foreach (var kvp in DCL.Environment.i.world.state.loadedScenes)
            {
                ParcelScene scene = kvp.Value as ParcelScene;
                debugLogger.Log("Dumping state for scene: " + kvp.Value.sceneData.id);
                scene.GetWaitingComponentsDebugInfo();
            }
        }

        public void SetDisableAssetBundles() { RendereableAssetLoadHelper.loadingType = RendereableAssetLoadHelper.LoadingType.GLTF_ONLY; }

        [ContextMenu("Dump Renderers Lockers Info")]
        public void DumpRendererLockersInfo()
        {
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
        }

        public void CrashPayloadRequest()
        {
            var crashPayload = CrashPayloadUtils.ComputePayload
            (
                DCL.Environment.i.world.state.loadedScenes,
                Environment.i.platform.debugController.GetTrackedMovements(),
                Environment.i.platform.debugController.GetTrackedTeleportPositions()
            );

            CrashPayloadResponse(crashPayload);
        }

        public void CrashPayloadResponse(CrashPayload payload)
        {
            string json = JsonConvert.SerializeObject(payload);
            WebInterface.MessageFromEngine("CrashPayloadResponse", json);
        }

        [ContextMenu("Dump Crash Payload")]
        public void DumpCrashPayload()
        {
            var payload = CrashPayloadUtils.ComputePayload
            (
                DCL.Environment.i.world.state.loadedScenes,
                Environment.i.platform.debugController.GetTrackedMovements(),
                Environment.i.platform.debugController.GetTrackedTeleportPositions()
            );

            foreach ( var field in payload.fields)
            {
                string dump = JsonConvert.SerializeObject(field.Value);
                debugLogger.Log($"Crash payload ({field.Key}): {dump}");
            }

            string fullDump = JsonConvert.SerializeObject(payload);
            debugLogger.Log($"Full crash payload size: {fullDump.Length}");
        }

        public void RunPerformanceMeterTool(float durationInSeconds) { Environment.i.platform.debugController.RunPerformanceMeterTool(durationInSeconds); }

        public void InstantiateBotsAtWorldPos(string configJson) { Environment.i.platform.debugController.InstantiateBotsAtWorldPos(configJson); }

        public void InstantiateBotsAtCoords(string configJson) { Environment.i.platform.debugController.InstantiateBotsAtCoords(configJson); }
        
#if UNITY_EDITOR
        [ContextMenu("Run Performance Meter Tool for 10 seconds")]
        public void DebugPerformanceMeter() { RunPerformanceMeterTool(10); }

        [ContextMenu("Instantiate 3 bots at player position")]
        public void DebugBotsInstantiation()
        {
            InstantiateBotsAtCoords("{ " +
                                    "\"amount\":3, " +
                                    "\"xCoord\":-110, " +
                                    "\"yCoord\":-110, " +
                                    "\"areaWidth\":15, " +
                                    "\"areaDepth\":15 " +
                                    "}");
        }
#endif
    }
}