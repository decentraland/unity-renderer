using DCL.Interface;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenTimeoutView : MonoBehaviour
{

    [SerializeField] public GameObject websocketTimeout;
    [SerializeField] public GameObject sceneTimeoutWebGL;
    [SerializeField] public GameObject sceneTimeoutDesktop;

    private GameObject currentSceneTimeoutContainer;

    [SerializeField] public Button[] exitButtons;
    [SerializeField] public Button[] goBackHomeButtons;


    private void Awake()
    {
        foreach (Button exitButton in exitButtons)
            exitButton.onClick.AddListener(OnExit);
        foreach (Button goBackHomeButton in goBackHomeButtons)
            goBackHomeButton.onClick.AddListener(GoBackHome);

        //In desktop, first timeout corresponds to websocket. Thats why we have to define what is the first message we want to show
        currentSceneTimeoutContainer = Application.platform == RuntimePlatform.WebGLPlayer ? sceneTimeoutWebGL : websocketTimeout;
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

    private void OnDestroy()
    {
        foreach (Button exitButton in exitButtons)
            exitButton.onClick.RemoveAllListeners();
        foreach (Button goBackHomeButton in goBackHomeButtons)
            goBackHomeButton.onClick.RemoveAllListeners();
    }

    private void OnExit()
    {
#if UNITY_EDITOR

        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void GoBackHome()
    {
        WebInterface.SendChatMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.NONE,
            recipient = string.Empty,
            body = "/goto home",
        });
        HideSceneTimeout();
    }

}
