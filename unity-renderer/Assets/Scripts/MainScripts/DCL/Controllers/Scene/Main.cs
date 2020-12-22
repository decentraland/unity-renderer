using System;
using DCL.Components;
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

        public DCLComponentFactory componentFactory;

        public DebugConfig debugConfig;

        void Awake()
        {
            if (i != null)
            {
                Utils.SafeDestroy(this);
                return;
            }

            i = this;

#if !UNITY_EDITOR
            Debug.Log("DCL Unity Build Version: " + DCL.Configuration.ApplicationSettings.version);
            Debug.unityLogger.logEnabled = false;
#endif

            DataStore.debugConfig.soloScene = debugConfig.soloScene;
            DataStore.debugConfig.soloSceneCoords = debugConfig.soloSceneCoords;
            DataStore.debugConfig.ignoreGlobalScenes = debugConfig.ignoreGlobalScenes;
            DataStore.debugConfig.msgStepByStep = debugConfig.msgStepByStep;

            RenderProfileManifest.i.Initialize();
            Environment.i.Initialize();

            // TODO(Brian): This is a temporary fix to address elevators issue in the xmas event.
            // We should re-enable this later as produces a performance regression.
            if (!Configuration.EnvironmentSettings.RUNNING_TESTS)
                Environment.i.cullingController.SetAnimationCulling(false);
        }

        private void Start()
        {
            Environment.i.sceneController.Start();
        }

        private void Update()
        {
            Environment.i.sceneController.Update();
        }

        private void LateUpdate()
        {
            Environment.i.sceneController.LateUpdate();
        }

        private void OnDestroy()
        {
            Environment.i.sceneController.OnDestroy();
        }

        #region RuntimeMessagingBridge

        public void LoadParcelScenes(string payload)
        {
            Environment.i.sceneController.LoadParcelScenes(payload);
        }

        public void SendSceneMessage(string payload)
        {
            Environment.i.sceneController.SendSceneMessage(payload);
        }

        public void UnloadScene(string sceneId)
        {
            Environment.i.sceneController.UnloadScene(sceneId);
        }

        public void CreateUIScene(string payload)
        {
            Environment.i.sceneController.CreateUIScene(payload);
        }

        public void UpdateParcelScenes(string payload)
        {
            Environment.i.sceneController.UpdateParcelScenes(payload);
        }

        #endregion

        public void BuilderReady()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}