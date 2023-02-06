using System;
using DCL.SettingsCommon;
using DCL.Components;
using MainScripts.DCL.Controllers.HUD.Preloading;
using MainScripts.DCL.Controllers.LoadingFlow;
using MainScripts.DCL.Controllers.SettingsDesktop;
using MainScripts.DCL.Utils;
using UnityEngine;

namespace DCL
{
    /// <summary>
    /// This is the MainDesktop entry point.
    /// Most of the application subsystems should be initialized from this class Awake() event.
    /// </summary>
    public class MainDesktop : Main
    {
        [SerializeField] private bool logWs = false;
        private LoadingFlowController loadingFlowController;
        private PreloadingController preloadingController;
        private bool isConnectionLost;
        private readonly DataStoreRef<DataStore_LoadingScreen> loadingScreenRef;

        private BaseVariable<FeatureFlag> featureFlags => DataStore.i.featureFlags.flags;

        protected override void Awake()
        {
            CommandLineParserUtils.ParseArguments();
            isConnectionLost = false;

            DCLVideoTexture.videoPluginWrapperBuilder = VideoProviderFactory.CreateVideoProvider;

            featureFlags.OnChange += FeatureFlagsReady;

            InitializeSettings();

            base.Awake();
            DataStore.i.wsCommunication.communicationEstablished.OnChange += OnCommunicationEstablished;
            DataStore.i.performance.multithreading.Set(true);
            DataStore.i.performance.maxDownloads.Set(50);
            Texture.allowThreadedTextureCreation = true;
        }

        protected override void InitializeDataStore()
        {
            DataStore.i.textureConfig.gltfMaxSize.Set(TextureCompressionSettingsDesktop.GLTF_TEX_MAX_SIZE_DESKTOP);
            DataStore.i.textureConfig.generalMaxSize.Set(TextureCompressionSettingsDesktop.GENERAL_TEX_MAX_SIZE_DESKTOP);
            DataStore.i.avatarConfig.useHologramAvatar.Set(true);
        }

        private void FeatureFlagsReady(FeatureFlag current, FeatureFlag previous)
        {
            DCLVideoTexture.videoPluginWrapperBuilder = VideoProviderFactory.CreateVideoProvider;

            if (current.IsFeatureEnabled(DataStore.i.featureFlags.DECOUPLED_LOADING_SCREEN_FF))
            {
                loadingFlowController = new LoadingFlowController(
                    loadingScreenRef.Ref.decoupledLoadingHUD.visible,
                    CommonScriptableObjects.rendererState,
                    DataStore.i.wsCommunication.communicationEstablished);
            }
            else
            {
                loadingFlowController = new LoadingFlowController(
                    loadingScreenRef.Ref.loadingHUD.visible,
                    CommonScriptableObjects.rendererState,
                    DataStore.i.wsCommunication.communicationEstablished);
            }

            DataStore.i.featureFlags.flags.OnChange -= FeatureFlagsReady;
        }

        protected override void InitializeCommunication()
        {
            DataStore.i.debugConfig.logWs = logWs;

            // TODO(Brian): Remove this branching once we finish migrating all tests out of the
            //              IntegrationTestSuite_Legacy base class.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                var withSSL = true;
                int startPort = CommandLineParserUtils.startPort;

#if UNITY_EDITOR
                withSSL = DebugConfigComponent.i.webSocketSSL;
                startPort = 5000;
#endif

                int endPort = startPort + 100;
                kernelCommunication = new WebSocketCommunication(withSSL, startPort, endPort);
            }
        }

        protected override void SetupPlugins()
        {
            pluginSystem = PluginSystemFactoryDesktop.Create();
            pluginSystem.Initialize();
        }

        private void InitializeSettings()
        {
            Settings.CreateSharedInstance(new DefaultSettingsFactory()
               .WithGraphicsQualitySettingsPresetPath("DesktopGraphicsQualityPresets"));
        }

        protected override void Dispose()
        {
            try
            {
                DataStore.i.wsCommunication.communicationEstablished.OnChange -= OnCommunicationEstablished;

                base.Dispose();
                DesktopDestroy();
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        private void DesktopDestroy()
        {
            loadingFlowController.Dispose();
            preloadingController.Dispose();
#if !AV_PRO_PRESENT
            DCLVideoPlayer.StopAllThreads();
#endif
        }

        void OnCommunicationEstablished(bool current, bool previous)
        {
            if (current == false && previous) { isConnectionLost = true; }
        }

        protected override void Update()
        {
            base.Update();

            if (isConnectionLost) { DesktopUtils.Quit(); }

            // TODO: Remove this after we refactor InputController to support overrides from desktop or to use the latest Unity Input System
            // This shortcut will help some users to fix the small resolution bugs that may happen if the player prefs are manipulated
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                && Input.GetKeyDown(KeyCode.F11))
            {
                DisplaySettings newDisplaySettings = new DisplaySettings { windowMode = WindowMode.Borderless };
                SettingsDesktop.i.displaySettings.Apply(newDisplaySettings);
                SettingsDesktop.i.displaySettings.Save();
            }
        }

        protected override void InitializeSceneDependencies()
        {
            base.InitializeSceneDependencies();
            preloadingController = new PreloadingController();
        }

        protected override void SetupServices()
        {
            Environment.Setup(ServiceLocatorDesktopFactory.CreateDefault());
        }
    }
}
