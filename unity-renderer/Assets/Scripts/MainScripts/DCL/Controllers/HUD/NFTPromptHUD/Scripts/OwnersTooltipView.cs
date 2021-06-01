using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal interface IOwnersTooltipView
{
    event Action OnViewAllPressed;
    event Action OnFocusLost;
    void Show();
    void Hide(bool instant);
    void SetElements(List<IOwnerInfoElement> elements);
    bool IsActive();
}

internal class OwnersTooltipView : MonoBehaviour, IOwnersTooltipView, IDeselectHandler
{
    internal const int MAX_ELEMENTS = 5;
    private const int ADDRESS_MAX_CHARS = 11;

    [SerializeField] internal Transform ownerElementsContainer;
    [SerializeField] internal Button_OnPointerDown viewAllButton;
    [SerializeField] private ShowHideAnimator showHideAnimator;

    public event Action OnViewAllPressed;
    public event Action OnFocusLost;

    void IOwnersTooltipView.Show()
    {
        gameObject.SetActive(true);
        showHideAnimator.Show();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    void IOwnersTooltipView.Hide(bool instant)
    {
        showHideAnimator.Hide();
        if (instant)
        {
            gameObject.SetActive(false);
        }
    }

    void IOwnersTooltipView.SetElements(List<IOwnerInfoElement> elements)
    {
        for (int i = 0; i < elements.Count && i < MAX_ELEMENTS; i++)
        {
            elements[i].SetParent(ownerElementsContainer);
            elements[i].SetAddressLength(ADDRESS_MAX_CHARS);
            elements[i].SetActive(true);
        }
        viewAllButton.gameObject.SetActive(elements.Count > MAX_ELEMENTS);
    }

    bool IOwnersTooltipView.IsActive() { return gameObject.activeSelf; }

    private void Awake()
    {
        viewAllButton.onPointerDown += () => OnViewAllPressed?.Invoke();
        gameObject.SetActive(false);
    }

    void IDeselectHandler.OnDeselect(BaseEventData eventData) { OnFocusLost?.Invoke(); }
}