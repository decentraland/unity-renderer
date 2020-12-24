using System;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    public class DebugBridge : MonoBehaviour
    {
        // Beware this SetDebug() may be called before Awake() somehow...
        [ContextMenu("Set Debug mode")]
        public void SetDebug()
        {
            Environment.i.platform.debugController.SetDebug();
        }

        public void HideFPSPanel()
        {
            Environment.i.platform.debugController.HideFPSPanel();
        }

        public void ShowFPSPanel()
        {
            Environment.i.platform.debugController.ShowFPSPanel();
        }

        public void SetSceneDebugPanel()
        {
            Environment.i.platform.debugController.SetSceneDebugPanel();
        }

        public void SetEngineDebugPanel()
        {
            Environment.i.platform.debugController.SetEngineDebugPanel();
        }

        public void DumpScenesLoadInfo()
        {
            bool prevLogValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            foreach (var scene in DCL.Environment.i.world.state.loadedScenes)
            {
                Debug.Log("Dumping state for scene: " + scene.Value.sceneData.id);
                scene.Value.GetWaitingComponentsDebugInfo();
            }

            Debug.unityLogger.logEnabled = prevLogValue;
        }

        public void SetDisableAssetBundles()
        {
            RendereableAssetLoadHelper.loadingType = RendereableAssetLoadHelper.LoadingType.GLTF_ONLY;
        }

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
    }
}