using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerPassportHUDView : MonoBehaviour
{
    [SerializeField] public PassportPlayerInfoComponentView playerInfoView;
    [SerializeField] public PassportPlayerPreviewComponentView playerPreviewView;
    [SerializeField] public PassportNavigationComponentView passportNavigationView;

    [SerializeField] internal Button hideCardButton;
    [SerializeField] internal GameObject container;
    [SerializeField] internal Button addFriendButton;

    public static PlayerPassportHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>("PlayerPassport")).GetComponent<PlayerPassportHUDView>();
    }

    public void Initialize(
        UnityAction cardClosedCallback,
        UnityAction addFriendCallback)
    {
        hideCardButton?.onClick.RemoveAllListeners();
        hideCardButton?.onClick.AddListener(cardClosedCallback);

        //TODO: this is for testing, will be moved to player info component controller and view
        addFriendButton?.onClick.RemoveAllListeners();
        addFriendButton?.onClick.AddListener(addFriendCallback);
    }

    public void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    public void SetPassportPanelVisibility(bool visible)
    {
        container.SetActive(visible);
    }
}
