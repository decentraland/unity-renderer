using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

internal interface IOwnersPopupView
{
    event Action OnClosePopup;
    void Show();
    void Hide(bool instant);
    void SetElements(List<IOwnerInfoElement> elements);
    bool IsActive();
}

internal class OwnersPopupView : MonoBehaviour, IOwnersPopupView
{
    private const int ADDRESS_MAX_CHARS = 19;
    private const float ELEMENT_FONT_SIZE = 17;
    private const float ELEMENT_HORIZONTAL_SPACING = 17;

    [SerializeField] internal Transform ownerElementsContainer;
    [SerializeField] internal Button closeButton;
    [SerializeField] internal Button backgroundCloseButton;
    [SerializeField] private ShowHideAnimator showHideAnimator;
    [SerializeField] private ScrollRect scrollRect;

    public event Action OnClosePopup;

    void IOwnersPopupView.Show()
    {
        gameObject.SetActive(true);
        scrollRect.verticalNormalizedPosition = 1;
        showHideAnimator.Show();
    }

    void IOwnersPopupView.Hide(bool instant)
    {
        showHideAnimator.Hide();
        if (instant)
        {
            gameObject.SetActive(false);
        }
    }

    void IOwnersPopupView.SetElements(List<IOwnerInfoElement> elements)
    {
        for (int i = 0; i < elements.Count; i++)
        {
            elements[i].SetParent(ownerElementsContainer);
            elements[i].SetAddressLength(ADDRESS_MAX_CHARS);
            elements[i].SetColorIndex(i);
            elements[i].SetConfig(ELEMENT_FONT_SIZE, ELEMENT_HORIZONTAL_SPACING);
            elements[i].SetActive(true);
        }
    }

    bool IOwnersPopupView.IsActive() { return gameObject.activeSelf; }

    private void Awake()
    {
        closeButton.onClick.AddListener(() => OnClosePopup?.Invoke());
        backgroundCloseButton.onClick.AddListener(() => OnClosePopup?.Invoke());
    }
}