﻿using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL
{
    public interface IDebugController : IDisposable
    {
        event Action OnDebugModeSet;
        void SetDebug();
        void HideFPSPanel();
        void ShowFPSPanel();
        void SetSceneDebugPanel();
        void SetEngineDebugPanel();
        void RunPerformanceMeterTool(float durationInMilliseconds);
    }

    public class DebugController : IDebugController
    {
        private DebugConfig debugConfig => DataStore.i.debugConfig;
        private PerformanceMeterController performanceMeterController;

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

        public void RunPerformanceMeterTool(float durationInMilliseconds)
        {
            if (performanceMeterController == null)
                performanceMeterController = new PerformanceMeterController();

            performanceMeterController.StartSampling(durationInMilliseconds);
        }

        public void Dispose()
        {
            if (debugView != null)
                Object.Destroy(debugView.gameObject);
        }
    }
}