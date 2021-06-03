using DCL.Components;
using DCL.Controllers;
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

        public void RunPerformanceMeterTool(float durationInSeconds) { Environment.i.platform.debugController.RunPerformanceMeterTool(durationInSeconds); }

        public void InstantiateBotsAtWorldPos(string configJson) { Environment.i.platform.debugController.InstantiateBotsAtWorldPos(configJson); }

        public void InstantiateBotsAtCoords(string configJson) { Environment.i.platform.debugController.InstantiateBotsAtCoords(configJson); }

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

#if UNITY_EDITOR
        [ContextMenu("Run Performance Meter Tool for 10 seconds")]
        public void DebugPerformanceMeter() { RunPerformanceMeterTool(10); }

        [ContextMenu("Instantiate 3 bots at player position")]
        public void DebugBotsInstantiation() { InstantiateBotsAtCoords("{ \"amount\":3 }"); }
#endif
    }
}