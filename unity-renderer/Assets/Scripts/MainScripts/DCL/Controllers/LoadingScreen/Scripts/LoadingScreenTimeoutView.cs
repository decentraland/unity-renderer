using DCL.Interface;
using MainScripts.DCL.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenTimeoutView : MonoBehaviour
{

    [SerializeField] public GameObject websocketTimeout;
    [SerializeField] public GameObject sceneTimeout;


    [SerializeField] public Button exitButton;
    [SerializeField] public Button goBackHomeButton;


    private void Awake()
    {
        exitButton.onClick.AddListener(OnExit);
        goBackHomeButton.onClick.AddListener(GoBackHome);
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

    public void ShowSceneTimeout()
    {
        sceneTimeout.SetActive(true);
    }

    public void HideSceneTimeout()
    {
        sceneTimeout.SetActive(false);
    }

    private void OnDestroy()
    {
        exitButton.onClick.RemoveAllListeners();
        goBackHomeButton.onClick.RemoveAllListeners();

    }

    private void OnExit()
    {
        DesktopUtils.Quit();
    }

}
