using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerPassportHUDView : MonoBehaviour
{
    [SerializeField] private PassportPlayerInfoComponentView playerInfoView;
    [SerializeField] private PassportPlayerPreviewComponentView playerPreviewView;
    [SerializeField] private PassportNavigationComponentView passportNavigationView;

    public PassportPlayerInfoComponentView PlayerInfoView => playerInfoView;
    public PassportPlayerPreviewComponentView PlayerPreviewView => playerPreviewView;
    public PassportNavigationComponentView PassportNavigationView => passportNavigationView;

    [SerializeField] internal Button hideCardButton;
    [SerializeField] internal GameObject container;

    public static PlayerPassportHUDView CreateView()
    {
        return Instantiate(Resources.Load<GameObject>("PlayerPassport")).GetComponent<PlayerPassportHUDView>();
    }

    public void Initialize(UnityAction cardClosedCallback)
    {
        hideCardButton?.onClick.RemoveAllListeners();
        hideCardButton?.onClick.AddListener(cardClosedCallback);
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
