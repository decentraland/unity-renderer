using System;
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
    }
}