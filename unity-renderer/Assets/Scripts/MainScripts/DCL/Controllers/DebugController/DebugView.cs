﻿using System;
using UnityEngine;

namespace DCL
{
    public class DebugView : MonoBehaviour
    {
        [Header("Debug Tools")]
        [SerializeField]
        private GameObject fpsPanel;

        [Header("Debug Panel")]
        [SerializeField]
        private GameObject engineDebugPanel;

        [SerializeField]
        private GameObject sceneDebugPanel;

        [SerializeField] private InfoPanel infoPanel;

        private void Awake() { infoPanel.SetVisible(false); }

        public void ShowFPSPanel() { fpsPanel.SetActive(true); }

        public void HideFPSPanel() { fpsPanel.SetActive(false); }

        public void SetSceneDebugPanel()
        {
            engineDebugPanel.SetActive(false);
            sceneDebugPanel.SetActive(true);
        }

        public void SetEngineDebugPanel()
        {
            sceneDebugPanel.SetActive(false);
            engineDebugPanel.SetActive(true);
        }
        public void ShowInfoPanel(string network, string environment)
        {
            var info = $"{network} / {environment}";
            infoPanel.Setup(info);
            infoPanel.SetVisible(true);
        }
    }
}