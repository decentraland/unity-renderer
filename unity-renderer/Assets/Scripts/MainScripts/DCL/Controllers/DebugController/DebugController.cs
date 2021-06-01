using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public class DebugController : IDebugController
    {
        private DebugConfig debugConfig => DataStore.i.debugConfig;
        private readonly PerformanceMeterController performanceMeterController = new PerformanceMeterController();
        private readonly BotsController botsController = new BotsController();

        public DebugView debugView;

        public event Action OnDebugModeSet;

        public DebugController()
        {
            GameObject view = Object.Instantiate(UnityEngine.Resources.Load("DebugView")) as GameObject;
            debugView = view.GetComponent<DebugView>();
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
            var config = new BotsController.WorldPosInstantiationConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            botsController.InstantiateBotsAtWorldPos(config);
        }

        public void InstantiateBotsAtCoords(string configJson)
        {
            var config = new BotsController.CoordsInstantiationConfig();
            JsonUtility.FromJsonOverwrite(configJson, config);

            botsController.InstantiateBotsAtCoords(config);
        }

        public void Dispose()
        {
            if (debugView != null)
                Object.Destroy(debugView.gameObject);
        }
    }
}