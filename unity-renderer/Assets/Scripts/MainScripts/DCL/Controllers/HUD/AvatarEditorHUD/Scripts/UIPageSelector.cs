using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPageSelector : MonoBehaviour
{
    public event Action<int> OnValueChanged;

    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private UIPageButton pageButtonPrefab;
    [SerializeField] private RectTransform pageButtonsParent;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private bool limitedPages = false;
    [SerializeField] private int maxVisiblePages;
    private List<UIPageButton> buttons = new List<UIPageButton>();
    private int totalPages;
    private int currentPage;

    private void Awake()
    {
        previousButton.onClick.AddListener(OnPreviousButtonDown);
        nextButton.onClick.AddListener(OnNextButtonDown);

        gameObject.SetActive(false);
    }

    private void OnNextButtonDown()
    {
        currentPage = (currentPage + 1 ) % totalPages;
        UpdateButtonsStatus();
    }

    private void OnPreviousButtonDown()
    {
        if (currentPage - 1 < 0)
        {
            currentPage = totalPages - 1;
        }
        else
        {
            currentPage = (currentPage - 1 ) % totalPages;
        }

        UpdateButtonsStatus();
    }

    public void SelectPage(int pageNumber)
    {
        currentPage = pageNumber;
        UpdateButtonsStatus();
    }

    public void Setup(int maxTotalPages, bool forceRebuild = false)
    {
        if (maxTotalPages == this.totalPages && !forceRebuild)
        {
            return;
        }

        this.totalPages = maxTotalPages;

        currentPage = Mathf.Clamp(currentPage, 0, maxTotalPages-1);

        if (maxTotalPages <= 1)
        {
            gameObject.SetActive(false);
            OnValueChanged?.Invoke(0);
            return;
        }

        gameObject.SetActive(true);

        EnsureButtons();
        UpdateButtonsStatus(false);

        LayoutRebuilder.ForceRebuildLayoutImmediate(pageButtonsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }

    private void EnsureButtons()
    {
        if (buttons.Count != totalPages)
        {
            var diff = totalPages - buttons.Count;

            if (diff > 0)
            {
                for (int i = 0; i < diff; i++)
                {
                    var instance = Instantiate(pageButtonPrefab, pageButtonsParent);
                    buttons.Add(instance);
                }
            }
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            UIPageButton uiPageButton = buttons[i];

            if (i >= totalPages)
            {
                uiPageButton.gameObject.SetActive(false);

                continue;
            }

            uiPageButton.Initialize(i);
            uiPageButton.gameObject.SetActive(true);
            uiPageButton.OnPageClicked -= SelectPage;
            uiPageButton.OnPageClicked += SelectPage;
        }
    }

    private bool ShouldShowButton(int buttonIndex)
    {
        if (buttonIndex >= totalPages)
            return false;

        if (currentPage+1 <= maxVisiblePages / 2)
        {
            return buttonIndex < maxVisiblePages;
        }
        else
        {
            return buttonIndex < currentPage+1 + (maxVisiblePages / 2) && buttonIndex+1 > currentPage - (maxVisiblePages / 2);
        }
    }

    private void UpdateButtonsStatus(bool notifyEvent = true)
    {
        UpdateToggleStatus();
        if (notifyEvent)
            OnValueChanged?.Invoke(currentPage);
    }

    private void UpdateToggleStatus()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var currentButton = buttons[i];
            if (limitedPages)
                currentButton.gameObject.SetActive(ShouldShowButton(i));
            currentButton.Toggle(i == currentPage);
        }
    }

}
