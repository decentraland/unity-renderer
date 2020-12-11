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

        [NonSerialized]
        public SceneController sceneController;

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

            sceneController = new SceneController();
            sceneController.Initialize(componentFactory);
        }

        private void Start()
        {
            sceneController.Start();
        }

        private void Update()
        {
            sceneController.Update();
        }

        private void LateUpdate()
        {
            sceneController.LateUpdate();
        }

        private void OnDestroy()
        {
            sceneController.OnDestroy();
        }

        #region RuntimeMessagingBridge

        public void LoadParcelScenes(string payload)
        {
            sceneController.LoadParcelScenes(payload);
        }

        public void SendSceneMessage(string payload)
        {
            sceneController.SendSceneMessage(payload);
        }

        public void UnloadScene(string sceneId)
        {
            sceneController.UnloadScene(sceneId);
        }

        public void CreateUIScene(string payload)
        {
            sceneController.CreateUIScene(payload);
        }

        public void UpdateParcelScenes(string payload)
        {
            sceneController.UpdateParcelScenes(payload);
        }

        #endregion

        public void BuilderReady()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BuilderScene", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        }
    }
}