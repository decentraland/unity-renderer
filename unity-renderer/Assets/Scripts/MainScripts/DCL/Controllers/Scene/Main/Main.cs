using Cysharp.Threading.Tasks;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Interface;
using DCL.Map;
using DCL.Providers;
using DCL.SettingsCommon;
using DCl.Social.Friends;
using DCL.Social.Friends;
using MainScripts.DCL.Controllers.HotScenes;
using DCLServices.WearablesCatalogService;
using UnityEngine;
#if UNITY_EDITOR
using DG.Tweening;
#endif

namespace DCL
{
    /// <summary>
    ///     This is the InitialScene entry point.
    ///     Most of the application subsystems should be initialized from this class Awake() event.
    /// </summary>
    public class Main : MonoBehaviour
    {
        [SerializeField] private bool disableSceneDependencies;

        private HotScenesController hotScenesController;

        public PoolableComponentFactory componentFactory;
        private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
        protected IKernelCommunication kernelCommunication;

        private PerformanceMetricsController performanceMetricsController;

        protected PluginSystem pluginSystem;
        public static Main i { get; private set; }

        protected virtual void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

            if (!disableSceneDependencies)
                InitializeSceneDependencies();

            #if UNITY_EDITOR
            // Prevent warning when starting on unity editor mode
            // TODO: Are we instantiating 500 different kinds of animations?
            DOTween.SetTweensCapacity(500,50);
            #endif

            Settings.CreateSharedInstance(new DefaultSettingsFactory());

            if (!EnvironmentSettings.RUNNING_TESTS)
            {
                SetupServices();
                performanceMetricsController = new PerformanceMetricsController();

                dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.OnChange += OnLoadingScreenVisibleStateChange;
            }

            // TODO (NEW FRIEND REQUESTS): remove when the kernel bridge is production ready
            WebInterfaceFriendsApiBridge webInterfaceFriendsApiBridge = GetComponent<WebInterfaceFriendsApiBridge>();

            FriendsController.CreateSharedInstance(new WebInterfaceFriendsApiBridgeProxy(
                webInterfaceFriendsApiBridge,
                RPCFriendsApiBridge.CreateSharedInstance(Environment.i.serviceLocator.Get<IRPC>(), webInterfaceFriendsApiBridge),
                DataStore.i));

#if UNITY_STANDALONE || UNITY_EDITOR
            Application.quitting += () => DataStore.i.common.isApplicationQuitting.Set(true);
#endif

            InitializeDataStore();
            SetupPlugins();
            InitializeCommunication();
        }

        protected virtual void Start()
        {
            // this event should be the last one to be executed after initialization
            // it is used by the kernel to signal "EngineReady" or something like that
            // to prevent race conditions like "SceneController is not an object",
            // aka sending events before unity is ready
            WebInterface.SendSystemInfoReport();

            // We trigger the Decentraland logic once everything is initialized.
            WebInterface.StartDecentraland();
        }

        protected virtual void Update()
        {
            performanceMetricsController?.Update();
        }

        protected virtual void InitializeDataStore()
        {
            DataStore.i.textureConfig.gltfMaxSize.Set(TextureCompressionSettings.GLTF_TEX_MAX_SIZE_WEB);
            DataStore.i.textureConfig.generalMaxSize.Set(TextureCompressionSettings.GENERAL_TEX_MAX_SIZE_WEB);
            DataStore.i.avatarConfig.useHologramAvatar.Set(true);
        }

        protected virtual void InitializeCommunication()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);

            kernelCommunication = new NativeBridgeCommunication(Environment.i.world.sceneController);
#else

            // TODO(Brian): Remove this branching once we finish migrating all tests out of the
            //              IntegrationTestSuite_Legacy base class.
            if (!EnvironmentSettings.RUNNING_TESTS) { kernelCommunication = new WebSocketCommunication(DebugConfigComponent.i.webSocketSSL); }
#endif
        }

        private void OnLoadingScreenVisibleStateChange(bool newVisibleValue, bool previousVisibleValue)
        {
            if (newVisibleValue)
            {
                dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.OnChange -= OnLoadingScreenVisibleStateChange;
                PrewarmShaderVariants().Forget();

                async UniTask PrewarmShaderVariants()
                {
                    var collection = await Environment.i.serviceLocator.Get<IAddressableResourceProvider>()
                                                      .GetAddressable<ShaderVariantCollection>("shadervariants-selected");
                    collection.WarmUp();
                }
            }
        }

        protected virtual void SetupPlugins()
        {
            pluginSystem = PluginSystemFactory.Create();
            pluginSystem.Initialize();
        }

        protected virtual void SetupServices()
        {
            var serviceLocator = ServiceLocatorFactory.CreateDefault();
            serviceLocator.Register<IHotScenesController>(() => hotScenesController);
            Environment.Setup(serviceLocator);
        }

        [RuntimeInitializeOnLoadMethod]
        private static void RunOnStart()
        {
            Application.wantsToQuit += ApplicationWantsToQuit;
        }

        private static bool ApplicationWantsToQuit()
        {
            if (i != null)
                i.Dispose();

            return true;
        }

        protected virtual void Dispose()
        {
            dataStoreLoadingScreen.Ref.decoupledLoadingHUD.visible.OnChange -= OnLoadingScreenVisibleStateChange;

            DataStore.i.common.isApplicationQuitting.Set(true);
            Settings.i.SaveSettings();

            pluginSystem?.Dispose();

            if (!EnvironmentSettings.RUNNING_TESTS)
                Environment.Dispose();

            kernelCommunication?.Dispose();
        }

        protected virtual void InitializeSceneDependencies()
        {
            gameObject.AddComponent<UserProfileController>();
            gameObject.AddComponent<RenderingController>();
            gameObject.AddComponent<WebInterfaceWearablesCatalogService>();
            gameObject.AddComponent<WebInterfaceMinimapApiBridge>();
            gameObject.AddComponent<MinimapMetadataController>();
            gameObject.AddComponent<WebInterfaceFriendsApiBridge>();
            hotScenesController = gameObject.AddComponent<HotScenesController>();
            gameObject.AddComponent<GIFProcessingBridge>();
            gameObject.AddComponent<RenderProfileBridge>();
            gameObject.AddComponent<AssetCatalogBridge>();
            gameObject.AddComponent<ScreenSizeWatcher>();
            gameObject.AddComponent<SceneControllerBridge>();

            MainSceneFactory.CreateBridges();
            MainSceneFactory.CreateMouseCatcher();
            MainSceneFactory.CreatePlayerSystems();
            CreateEnvironment();
            MainSceneFactory.CreateAudioHandler();
            MainSceneFactory.CreateHudController();
            MainSceneFactory.CreateNavMap();
            MainSceneFactory.CreateEventSystem();
        }

        protected virtual void CreateEnvironment() =>
            MainSceneFactory.CreateEnvironment();
    }
}
