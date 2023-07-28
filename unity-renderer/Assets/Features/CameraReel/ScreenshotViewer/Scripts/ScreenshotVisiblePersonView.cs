using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotVisiblePersonView : MonoBehaviour
{
    [SerializeField] private GameObject isGuestImage;
    [SerializeField] private Button wearablesListButton;

    [field: SerializeField] public ProfileCardComponentView ProfileCard { get; private set; }
    [field: SerializeField] public NFTIconComponentView WearableTemplate { get; private set; }
    [field: SerializeField] public Transform WearablesListContainer { get; private set; }

    private void Awake()
    {
        wearablesListButton.onClick.AddListener(ShowHideList);
    }

    private void ShowHideList()
    {
        WearablesListContainer.gameObject.SetActive(!WearablesListContainer.gameObject.activeSelf);
    }

    public void SetAsGuest()
    {
        isGuestImage.SetActive(true);
    }
}
