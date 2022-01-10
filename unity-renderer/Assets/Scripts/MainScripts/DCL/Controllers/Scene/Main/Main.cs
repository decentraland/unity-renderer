using System;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.SettingsCommon;
using UnityEngine;
using UnityEngine.Serialization;

namespace DCL
{
    /// <summary>
    /// This is the InitialScene entry point.
    /// Most of the application subsystems should be initialized from this class Awake() event.
    /// </summary>
    public class Main : MonoBehaviour
    {
        [SerializeField] private bool disableSceneDependencies;
        public static Main i { get; private set; }

        public PoolableComponentFactory componentFactory;

        private PerformanceMetricsController performanceMetricsController;
        protected IKernelCommunication kernelCommunication;

        private PluginSystem pluginSystem;

        protected virtual void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;
            
            Settings.CreateSharedInstance(new DefaultSettingsFactory());

            if (!disableSceneDependencies)
                InitializeSceneDependencies();

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                performanceMetricsController = new PerformanceMetricsController();
                RenderProfileManifest.i.Initialize();
                SetupEnvironment();
                
                DataStore.i.HUDs.loadingHUD.visible.OnChange += OnLoadingScreenVisibleStateChange;
            }

            SetupPlugins();

#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);
            Debug.unityLogger.logEnabled = false;

            kernelCommunication = new NativeBridgeCommunication(Environment.i.world.sceneController);
#else
            // TODO(Brian): Remove this branching once we finish migrating all tests out of the
            //              IntegrationTestSuite_Legacy base class.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
#if UNITY_STANDALONE && !UNITY_EDITOR
                kernelCommunication = new WebSocketCommunication(true);
#else
                kernelCommunication = new WebSocketCommunication(DebugConfigComponent.i.webSocketSSL);
#endif
            }
#endif

            // TODO(Brian): This is a temporary fix to address elevators issue in the xmas event.
            // We should re-enable this later as produces a performance regression.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.i.platform.cullingController.SetAnimationCulling(false);
        }

        void OnLoadingScreenVisibleStateChange(bool newVisibleValue, bool previousVisibleValue)
        {
            if (newVisibleValue)
            {
                // Prewarm shader variants
                Resources.Load<ShaderVariantCollection>("ShaderVariantCollections/shaderVariants-selected").WarmUp();
                DataStore.i.HUDs.loadingHUD.visible.OnChange -= OnLoadingScreenVisibleStateChange;
            }
        }

        protected virtual void SetupPlugins()
        {
            pluginSystem = PluginSystemFactory.Create();
        }

        protected virtual void SetupEnvironment()
        {
            Environment.SetupWithBuilders(
                messagingBuilder: MessagingContextBuilder,
                platformBuilder: PlatformContextBuilder,
                worldRuntimeBuilder: WorldRuntimeContextBuilder,
                hudBuilder: HUDContextBuilder);
        }

        protected virtual MessagingContext MessagingContextBuilder() { return MessagingContextFactory.CreateDefault(); }

        protected virtual PlatformContext PlatformContextBuilder() { return PlatformContextFactory.CreateDefault(); }

        protected virtual WorldRuntimeContext WorldRuntimeContextBuilder() { return WorldRuntimeContextFactory.CreateDefault(componentFactory); }

        protected virtual HUDContext HUDContextBuilder() { return HUDContextFactory.CreateDefault(); }

        protected virtual void Start()
        {
            // this event should be the last one to be executed after initialization
            // it is used by the kernel to signal "EngineReady" or something like that
            // to prevent race conditions like "SceneController is not an object",
            // aka sending events before unity is ready
            DCL.Interface.WebInterface.SendSystemInfoReport();

            // We trigger the Decentraland logic once everything is initialized.
            DCL.Interface.WebInterface.StartDecentraland();

            Environment.i.world.sceneController.Start();
        }

        protected virtual void Update()
        {
            Environment.i.platform.Update();
            performanceMetricsController?.Update();
        }

        protected virtual void OnDestroy()
        {
            DataStore.i.HUDs.loadingHUD.visible.OnChange -= OnLoadingScreenVisibleStateChange;
            
            DataStore.i.common.isWorldBeingDestroyed.Set(true);
            
            pluginSystem?.Dispose();

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.Dispose();
            
            kernelCommunication?.Dispose();
        }

        protected virtual void InitializeSceneDependencies()
        {
            gameObject.AddComponent<UserProfileController>();
            gameObject.AddComponent<RenderingController>();
            gameObject.AddComponent<CatalogController>();
            gameObject.AddComponent<MinimapMetadataController>();
            gameObject.AddComponent<ChatController>();
            gameObject.AddComponent<FriendsController>();
            gameObject.AddComponent<LoadingFeedbackController>();
            gameObject.AddComponent<HotScenesController>();
            gameObject.AddComponent<GIFProcessingBridge>();
            gameObject.AddComponent<RenderProfileBridge>();
            gameObject.AddComponent<AssetCatalogBridge>();
            gameObject.AddComponent<ScreenSizeWatcher>();
            gameObject.AddComponent<SceneControllerBridge>();

            MainSceneFactory.CreateBuilderInWorldBridge(gameObject);
            MainSceneFactory.CreateBridges();
            MainSceneFactory.CreateMouseCatcher();
            MainSceneFactory.CreatePlayerSystems();
            CreateEnvironment();
            MainSceneFactory.CreateAudioHandler();
            MainSceneFactory.CreateHudController();
            MainSceneFactory.CreateSettingsController();
            MainSceneFactory.CreateNavMap();
            MainSceneFactory.CreateEventSystem();
            MainSceneFactory.CreateInteractionHoverCanvas();
        }

        protected virtual void CreateEnvironment() => MainSceneFactory.CreateEnvironment();
    }
}