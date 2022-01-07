using System;
using System.Collections.Generic;
using DCL.Components;
using UnityEditor;
using UnityEngine;

namespace DCL
{
    public class DebugConfigComponent : MonoBehaviour
    {
        public DebugConfig debugConfig;

        public enum DebugPanel
        {
            Off,
            Scene,
            Engine
        }

        public enum BaseUrl
        {
            LOCAL_HOST,
            CUSTOM,
        }

        public enum Environment
        {
            USE_DEFAULT_FROM_URL,
            LOCAL,
            ZONE,
            TODAY,
            ORG
        }

        private const string ENGINE_DEBUG_PANEL = "ENGINE_DEBUG_PANEL";
        private const string SCENE_DEBUG_PANEL = "SCENE_DEBUG_PANEL";

        public bool openBrowserWhenStart;

        [Header("Kernel General Settings")]
        public string kernelVersion;
        public bool useCustomContentServer = false;

        public string customContentServerUrl = "http://localhost:1338/";

        [Space(10)]
        public BaseUrl baseUrlMode;

        public string baseUrlCustom;

        [Space(10)]
        public Environment environment;

        [Tooltip("Set this field to force the realm (server). On the latin-american zone, recommended realms are fenrir-amber, baldr-amber and thor. Other realms can give problems to debug from Unity editor due to request certificate issues.\n\nFor auto selection leave this field blank.\n\nCheck out all the realms at https://catalyst-monitor.vercel.app/?includeDevServers")]
        public string realm;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Header("Kernel Misc Settings")]
        public bool forceLocalComms = true;

        public bool allWearables = false;
        public bool testWearables = false;
        public bool enableTutorial = false;
        public bool builderInWorld = false;
        public bool enableProceduralSkybox = false;
        public bool soloScene = true;
        public bool multithreaded = false;
        public DebugPanel debugPanelMode = DebugPanel.Off;

        private void Awake()
        {
            DataStore.i.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.i.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.i.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.i.debugConfig.msgStepByStep = debugConfig.msgStepByStep;
            DataStore.i.multithreading.enabled.Set(multithreaded);
        }

        private void Start()
        {
            lock (DataStore.i.wsCommunication.communicationReady)
            {
                if (DataStore.i.wsCommunication.communicationReady.Get())
                {
                    InitConfig();
                }
                else
                {
                    DataStore.i.wsCommunication.communicationReady.OnChange += OnCommunicationReadyChangedValue;
                }
            }
        }

        private void OnCommunicationReadyChangedValue(bool newState, bool prevState)
        {
            if (newState && !prevState)
                InitConfig();

            DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue;
        }

        private void InitConfig()
        {
            if (useCustomContentServer)
            {
                RendereableAssetLoadHelper.useCustomContentServerUrl = true;
                RendereableAssetLoadHelper.customContentServerUrl = customContentServerUrl;
            }

            if (openBrowserWhenStart)
                OpenWebBrowser();
        }

        private void OpenWebBrowser()
        {
#if (UNITY_EDITOR || UNITY_STANDALONE)
            string baseUrl = "";
            string debugString = "";

            if (baseUrlMode == BaseUrl.CUSTOM)
                baseUrl = baseUrlCustom;
            else
                baseUrl = "http://localhost:3000/?";

            switch (environment)
            {
                case Environment.USE_DEFAULT_FROM_URL:
                    break;
                case Environment.LOCAL:
                    debugString = "DEBUG_MODE&";
                    break;
                case Environment.ZONE:
                    debugString = "NETWORK=ropsten&";
                    break;
                case Environment.TODAY:
                    debugString = "NETWORK=mainnet&";
                    break;
                case Environment.ORG:
                    debugString = "NETWORK=mainnet&";
                    break;
            }

            if (!string.IsNullOrEmpty(kernelVersion))
            {
                debugString += $"kernel-version={kernelVersion}&";
            }

            if (forceLocalComms)
            {
                debugString += "LOCAL_COMMS&";
            }

            if (allWearables)
            {
                debugString += "ALL_WEARABLES&";
            }

            if (testWearables)
            {
                debugString += "TEST_WEARABLES&";
            }

            if (enableTutorial)
            {
                debugString += "RESET_TUTORIAL&";
            }

            if (soloScene)
            {
                debugString += "LOS=0&";
            }

            if (builderInWorld)
            {
                debugString += "ENABLE_BUILDER_IN_WORLD&";
            }

            if (enableProceduralSkybox)
            {
                debugString += "ENABLE_PROCEDURAL_SKYBOX&";
            }

            if (!string.IsNullOrEmpty(realm))
            {
                debugString += $"realm={realm}&";
            }

            string debugPanelString = "";

            if (debugPanelMode == DebugPanel.Engine)
            {
                debugPanelString = ENGINE_DEBUG_PANEL + "&";
            }
            else if (debugPanelMode == DebugPanel.Scene)
            {
                debugPanelString = SCENE_DEBUG_PANEL + "&";
            }

            Application.OpenURL(
                $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
#endif
        }

        private void OnDestroy() { DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue; }
    }
}