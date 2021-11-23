using System;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
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
        private IKernelCommunication kernelCommunication;

        private PluginSystem pluginSystem;

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

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                performanceMetricsController = new PerformanceMetricsController();
                RenderProfileManifest.i.Initialize();
                SetupEnvironment();
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
                kernelCommunication = new WebSocketCommunication();
            }
#endif

            // TODO(Brian): This is a temporary fix to address elevators issue in the xmas event.
            // We should re-enable this later as produces a performance regression.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.i.platform.cullingController.SetAnimationCulling(false);
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

        private void Start()
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
            Environment.i.world.sceneController.Update();
            performanceMetricsController?.Update();
        }

        protected virtual void LateUpdate()
        {
            Environment.i.world.sceneController.LateUpdate();
        }

        protected virtual void OnDestroy()
        {
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.Dispose();
            pluginSystem?.Dispose();
            kernelCommunication?.Dispose();
        }

        protected virtual void InitializeSceneDependencies()
        {
            var bridges = Init("Bridges");
            var mouseCatcher = Init("MouseCatcher").GetComponent<MouseCatcher>();
            var environment = Init("Environment").GetComponent<EnvironmentReferences>();
            var playerReferences = Init("Player").GetComponent<PlayerReferences>();

            Init("HUDController");
            Init("HUDAudioHandler");
            Init("NavMap");
            Init("SettingsController");

            SceneReferences.i.Initialize(
                mouseCatcher,
                environment.ground,
                playerReferences.biwCameraRoot,
                playerReferences.inputController,
                playerReferences.cursorCanvas,
                gameObject,
                playerReferences.avatarController,
                playerReferences.cameraController,
                playerReferences.mainCamera,
                bridges,
                environment.environmentLight,
                environment.postProcessVolume,
                playerReferences.thirdPersonCamera,
                playerReferences.firstPersonCamera);
        }

        private static GameObject Init(string name)
        {
            GameObject instance = Instantiate(Resources.Load(name)) as GameObject;
            instance.name = name;
            return instance;
        }
    }
}