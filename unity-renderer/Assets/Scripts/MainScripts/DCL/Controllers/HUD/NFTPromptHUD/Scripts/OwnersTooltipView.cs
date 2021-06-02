using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal interface IOwnersTooltipView
{
    event Action OnViewAllPressed;
    event Action OnFocusLost;
    void Show();
    void Hide(bool instant);
    void KeepOnScreen();
    void SetElements(List<IOwnerInfoElement> elements);
    bool IsActive();
}

internal class OwnersTooltipView : MonoBehaviour, IOwnersTooltipView
{
    internal const int MAX_ELEMENTS = 5;
    private const int ADDRESS_MAX_CHARS = 11;
    private const float HIDE_DELAY = 0.3f;
    private const float ELEMENT_FONT_SIZE = 16;
    private const float ELEMENT_HORIZONTAL_SPACING = 10;

    [SerializeField] internal Transform ownerElementsContainer;
    [SerializeField] internal Button_OnPointerDown viewAllButton;
    [SerializeField] private ShowHideAnimator showHideAnimator;
    [SerializeField] private UIHoverCallback hoverArea;

    public event Action OnViewAllPressed;
    public event Action OnFocusLost;
    public event Action OnFocus;

    private Coroutine hideRoutine;

    void IOwnersTooltipView.Show()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        gameObject.SetActive(true);
        showHideAnimator.Show();
    }

    void IOwnersTooltipView.Hide(bool instant)
    {
        if (instant)
        {
            gameObject.SetActive(false);
        }
        else
        {
            hideRoutine = StartCoroutine(HideRoutine());
        }
    }

    void IOwnersTooltipView.KeepOnScreen()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        showHideAnimator.Show(true);
    }

    void IOwnersTooltipView.SetElements(List<IOwnerInfoElement> elements)
    {
        for (int i = 0; i < elements.Count && i < MAX_ELEMENTS; i++)
        {
            elements[i].SetParent(ownerElementsContainer);
            elements[i].SetAddressLength(ADDRESS_MAX_CHARS);
            elements[i].SetColorIndex(i);
            elements[i].SetConfig(ELEMENT_FONT_SIZE, ELEMENT_HORIZONTAL_SPACING);
            elements[i].SetActive(true);
        }
        viewAllButton.gameObject.SetActive(elements.Count > MAX_ELEMENTS);
    }

    bool IOwnersTooltipView.IsActive() { return gameObject.activeSelf; }

    private void Awake()
    {
        viewAllButton.onPointerDown += ViewAllPressed;
        hoverArea.OnPointerEnter += OnPointerEnter;
        hoverArea.OnPointerExit += OnPointerExit;
    }

    private void OnDestroy()
    {
        viewAllButton.onPointerDown -= ViewAllPressed;
        hoverArea.OnPointerEnter -= OnPointerEnter;
        hoverArea.OnPointerExit -= OnPointerExit;
    }

    private void ViewAllPressed() { OnViewAllPressed?.Invoke(); }

    private void OnPointerEnter() { OnFocus?.Invoke(); }

    private void OnPointerExit() { OnFocusLost?.Invoke(); }

    IEnumerator HideRoutine()
    {
        yield return new WaitForSeconds(HIDE_DELAY);
        showHideAnimator.Hide();
        hideRoutine = null;
    }
}