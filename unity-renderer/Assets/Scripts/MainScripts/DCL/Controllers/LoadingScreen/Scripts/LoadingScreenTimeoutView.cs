using DCL.Interface;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.LoadingScreen
{
    public class LoadingScreenTimeoutView : MonoBehaviour, ILoadingScreenTimeoutView
    {
        [SerializeField] public GameObject websocketTimeout;
        [SerializeField] public GameObject sceneTimeoutWebGL;
        [SerializeField] public GameObject sceneTimeoutDesktop;

        private GameObject currentSceneTimeoutContainer;

        [SerializeField] public Button[] exitButtons;
        [SerializeField] public Button[] goBackHomeButtons;

        private bool goHomeRequested;

        public event Action OnExitButtonClicked;
        public event Action OnJumpHomeButtonClicked;

        private void Awake()
        {
            foreach (Button exitButton in exitButtons)
                exitButton.onClick.AddListener(() => OnExitButtonClicked?.Invoke());

            foreach (Button goBackHomeButton in goBackHomeButtons)
                goBackHomeButton.onClick.AddListener(() => OnJumpHomeButtonClicked?.Invoke());

            //In desktop, first timeout corresponds to websocket. Thats why we have to define what is the first message we want to show
            currentSceneTimeoutContainer = Application.platform != RuntimePlatform.WebGLPlayer ? sceneTimeoutWebGL : websocketTimeout;
        }

        public void ShowSceneTimeout()
        {
            currentSceneTimeoutContainer.SetActive(true);
        }

        public void HideSceneTimeout()
        {
            currentSceneTimeoutContainer.SetActive(false);

            //Once the websocket has connected and the first fadeout has been done, its always a scene timeout
            currentSceneTimeoutContainer = Application.platform == RuntimePlatform.WebGLPlayer ? sceneTimeoutWebGL : sceneTimeoutDesktop;
        }

        public void Dispose()
        {
            foreach (Button exitButton in exitButtons)
                exitButton.onClick.RemoveAllListeners();

            foreach (Button goBackHomeButton in goBackHomeButtons)
                goBackHomeButton.onClick.RemoveAllListeners();
        }
    }
}
