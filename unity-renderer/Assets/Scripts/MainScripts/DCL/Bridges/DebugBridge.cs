using System;
using DCL.Components;
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
            Environment.i.debugController.SetDebug();
        }

        public void HideFPSPanel()
        {
            Environment.i.debugController.HideFPSPanel();
        }

        public void ShowFPSPanel()
        {
            Environment.i.debugController.ShowFPSPanel();
        }

        public void SetSceneDebugPanel()
        {
            Environment.i.debugController.SetSceneDebugPanel();
        }

        public void SetEngineDebugPanel()
        {
            Environment.i.debugController.SetEngineDebugPanel();
        }

        public void DumpScenesLoadInfo()
        {
            bool prevLogValue = Debug.unityLogger.logEnabled;
            Debug.unityLogger.logEnabled = true;

            foreach (var scene in DCL.Environment.i.worldState.loadedScenes)
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
    }
}