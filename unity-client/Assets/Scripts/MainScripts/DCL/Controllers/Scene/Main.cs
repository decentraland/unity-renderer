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
        public static Main i { get; private set; }

        public RuntimeComponentFactory componentFactory;

        public DebugConfig debugConfig;

        private PerformanceMetricsController performanceMetricsController;
        private EntryPoint_World worldEntryPoint;

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

            DataStore.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.debugConfig.msgStepByStep = debugConfig.msgStepByStep;

            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
            {
                performanceMetricsController = new PerformanceMetricsController();
                RenderProfileManifest.i.Initialize();
                Environment.SetupWithBuilders(worldRuntimeBuilder: RuntimeContextBuilder);
            }

#if !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);
            Debug.unityLogger.logEnabled = false;

            worldEntryPoint = new EntryPoint_World(Environment.i.world.sceneController);
#endif

            // TODO(Brian): This is a temporary fix to address elevators issue in the xmas event.
            // We should re-enable this later as produces a performance regression.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.i.platform.cullingController.SetAnimationCulling(false);
        }

        WorldRuntimeContext RuntimeContextBuilder()
        {
            return new WorldRuntimeContext(
                state: new WorldState(),
                sceneController: new SceneController(),
                pointerEventsController: new PointerEventsController(),
                sceneBoundsChecker: new SceneBoundsChecker(),
                blockersController: new WorldBlockersController(),
                componentFactory: componentFactory);
        }

        private void Start()
        {
            Environment.i.world.sceneController.Start();
        }

        private void Update()
        {
            Environment.i.world.sceneController.Update();
            performanceMetricsController?.Update();
        }

        private void LateUpdate()
        {
            Environment.i.world.sceneController.LateUpdate();
        }

        private void OnDestroy()
        {
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.Dispose();
        }

        #region RuntimeMessagingBridge

        public void LoadParcelScenes(string payload)
        {
            Environment.i.world.sceneController.LoadParcelScenes(payload);
        }

        public void SendSceneMessage(string payload)
        {
            Environment.i.world.sceneController.SendSceneMessage(payload);
        }

        public void UnloadScene(string sceneId)
        {
            Environment.i.world.sceneController.UnloadScene(sceneId);
        }

        public void CreateUIScene(string payload)
        {
            Environment.i.world.sceneController.CreateUIScene(payload);
        }

        public void UpdateParcelScenes(string payload)
        {
            Environment.i.world.sceneController.UpdateParcelScenes(payload);
        }

        #endregion

        public void BuilderReady()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}