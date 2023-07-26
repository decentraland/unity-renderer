using DCL.Components;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Utils = DCL.Helpers.Utils;

namespace DCL
{
    public class DebugConfigComponent : MonoBehaviour
    {
        private Stopwatch loadingStopwatch;
        private static DebugConfigComponent sharedInstance;
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;

        public static DebugConfigComponent i
        {
            get
            {
                if (sharedInstance == null)
                    sharedInstance = FindObjectOfType<DebugConfigComponent>();

                return sharedInstance;
            }

            private set => sharedInstance = value;
        }

        public DebugConfig debugConfig;

        public enum DebugPanel
        {
            Off,
            Scene,
            Engine
        }

        public enum BaseUrl
        {
            ZONE,
            ORG,
            LOCAL_HOST,
            CUSTOM,
        }

        public enum Network
        {
            MAINNET,
            SEPOLIA,
        }

        [Header("General Settings")] public bool OpenBrowserOnStart;
        public bool webSocketSSL = false;

        [Header("Kernel General Settings")] public string kernelVersion;
        public bool useCustomContentServer = false;

        public string customContentServerUrl = "http://localhost:1338/";

        [Space(10)] public BaseUrl baseUrlMode = BaseUrl.ZONE;
        [DrawIf("baseUrlMode", BaseUrl.CUSTOM)]
        public string customURL = "https://play.decentraland.zone/?";

        [Space(10)] public Network network;

        [Tooltip(
            "Set this field to force the realm (server). On the latin-american zone, recommended realms are fenrir-amber, baldr-amber and thor. Other realms can give problems to debug from Unity editor due to request certificate issues.\n\nFor auto selection leave this field blank.\n\nCheck out all the realms at https://catalyst-monitor.vercel.app/?includeDevServers")]
        public string realm;

        public Vector2 startInCoords = new Vector2(-99, 109);

        [Tooltip("Set this value to load the catalog from another wallet for debug purposes")]
        public string overrideUserID = "";

        [Header("Kernel Misc Settings")] public bool forceLocalComms = true;

        public bool enableTutorial = false;
        public bool builderInWorld = false;
        public bool soloScene = true;
        public bool disableAssetBundles = false;
        public bool enableDebugMode = false;
        public DebugPanel debugPanelMode = DebugPanel.Off;

        [Header("Performance")]
        public bool disableGLTFDownloadThrottle = false;
        public bool multithreaded = false;
        public bool runPerformanceMeterToolDuringLoading = false;
        private PerformanceMeterController performanceMeterController;

        private void Awake()
        {
            if (sharedInstance == null)
                sharedInstance = this;

            DataStore.i.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.i.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.i.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.i.debugConfig.msgStepByStep = debugConfig.msgStepByStep;
            DataStore.i.debugConfig.overrideUserID = overrideUserID;
            DataStore.i.performance.multithreading.Set(multithreaded);
            if (disableGLTFDownloadThrottle) DataStore.i.performance.maxDownloads.Set(999);
            Texture.allowThreadedTextureCreation = multithreaded;
        }

        private void Start()
        {
            lock (DataStore.i.wsCommunication.communicationReady)
            {
                if (DataStore.i.wsCommunication.communicationReady.Get()) { InitConfig(); }
                else { DataStore.i.wsCommunication.communicationReady.OnChange += OnCommunicationReadyChangedValue; }
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

            if (OpenBrowserOnStart)
                OpenWebBrowser();

            if (runPerformanceMeterToolDuringLoading)
            {
                CommonScriptableObjects.forcePerformanceMeter.Set(true);
                performanceMeterController = new PerformanceMeterController();

                StartSampling();
                CommonScriptableObjects.rendererState.OnChange += EndSampling;
            }
        }

        private void StartSampling()
        {
            loadingStopwatch = new Stopwatch();
            loadingStopwatch.Start();
            performanceMeterController.StartSampling(999);
        }

        private void EndSampling(bool current, bool previous)
        {
            if (current)
            {
                loadingStopwatch.Stop();
                CommonScriptableObjects.rendererState.OnChange -= EndSampling;
                performanceMeterController.StopSampling();
                Debug.Log($"Loading time: {loadingStopwatch.Elapsed.Seconds} seconds");
            }
        }

        private void OpenWebBrowser()
        {
#if (UNITY_EDITOR)
            string baseUrl = "";
            string debugString = "";

            if (baseUrlMode.Equals(BaseUrl.CUSTOM))
            {
                baseUrl = this.customURL;
                if (string.IsNullOrEmpty(this.customURL))
                {
                    Debug.LogError("Custom url cannot be empty");
                    QuitGame();
                    return;
                }
            }
            else if (baseUrlMode.Equals(BaseUrl.LOCAL_HOST))
            {
                baseUrl = "http://localhost:8080/?";
            }
            else if (baseUrlMode.Equals(BaseUrl.ORG))
            {
                baseUrl = "http://play.decentraland.org/?";
                if (!webSocketSSL)
                {
                    Debug.LogError(
                        "play.decentraland.org only works with WebSocket SSL, please change the base URL to play.decentraland.zone");
                    QuitGame();
                    return;
                }
            }
            else
            {
                baseUrl = "http://play.decentraland.zone/?";
            }

            switch (network)
            {
                case Network.SEPOLIA:
                    debugString = "NETWORK=sepolia&";
                    break;
                case Network.MAINNET:
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

            if (disableAssetBundles)
            {
                debugString += "DISABLE_ASSET_BUNDLES&DISABLE_WEARABLE_ASSET_BUNDLES&";
            }

            if (enableDebugMode)
            {
                debugString += "DEBUG_MODE&";
            }

            if (!string.IsNullOrEmpty(realm))
            {
                debugString += $"realm={realm}&";
            }

            string debugPanelString = "";

            if (debugPanelMode == DebugPanel.Engine)
            {
                debugPanelString = "ENGINE_DEBUG_PANEL&";
            }
            else if (debugPanelMode == DebugPanel.Scene)
            {
                debugPanelString = "SCENE_DEBUG_PANEL&";
            }

            if (webSocketSSL)
            {
                Debug.Log(
                    "[REMINDER] To be able to connect with SSL you should start Chrome with the --ignore-certificate-errors argument specified (or enabling the following option: chrome://flags/#allow-insecure-localhost). In Firefox set the configuration option `network.websocket.allowInsecureFromHTTPS` to true, then use the ws:// rather than the wss:// address.");
            }

            Application.OpenURL(
                $"{baseUrl}{debugString}{debugPanelString}position={startInCoords.x}%2C{startInCoords.y}&ws={DataStore.i.wsCommunication.url}");
#endif
        }

        private void OnDestroy()
        {
            DataStore.i.wsCommunication.communicationReady.OnChange -= OnCommunicationReadyChangedValue;
        }

        private void QuitGame()
        {
            Utils.QuitApplication();
        }
    }
}
