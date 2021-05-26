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
            bool prevLogValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            foreach (var kvp in DCL.Environment.i.world.state.loadedScenes)
            {
                ParcelScene scene = kvp.Value as ParcelScene;
                Debug.Log("Dumping state for scene: " + kvp.Value.sceneData.id);
                scene.GetWaitingComponentsDebugInfo();
            }

            Debug.unityLogger.logEnabled = prevLogValue;
        }

        public void SetDisableAssetBundles() { RendereableAssetLoadHelper.loadingType = RendereableAssetLoadHelper.LoadingType.GLTF_ONLY; }

        [ContextMenu("Dump Renderers Lockers Info")]
        public void DumpRendererLockersInfo()
        {
            bool prevLogValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            RenderingController renderingController = FindObjectOfType<RenderingController>();
            if (renderingController == null)
            {
                Debug.Log("RenderingController not found. Aborting.");
                return;
            }

            Debug.Log($"Renderer is locked? {!renderingController.renderingActivatedAckLock.isUnlocked}");
            Debug.Log($"Renderer is active? {CommonScriptableObjects.rendererState.Get()}");

            System.Collections.Generic.HashSet<object> lockIds =
                renderingController.renderingActivatedAckLock.GetLockIdsCopy();

            foreach (var lockId in lockIds)
            {
                Debug.Log($"Renderer is locked by id: {lockId} of type {lockId.GetType()}");
            }

            Debug.unityLogger.logEnabled = prevLogValue;
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
                Debug.Log($"Crash payload ({field.Key}): {dump}");
            }

            string fullDump = JsonConvert.SerializeObject(payload);
            Debug.Log($"Full crash payload size: {fullDump.Length}");
        }
    }
}