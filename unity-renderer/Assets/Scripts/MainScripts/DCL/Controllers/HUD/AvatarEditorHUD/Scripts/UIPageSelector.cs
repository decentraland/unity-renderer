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
    private List<UIPageButton> buttons = new List<UIPageButton>();
    private int maxPages;
    private int currentPage;

    private void Awake()
    {
        previousButton.onClick.AddListener(OnPreviousButtonDown);
        nextButton.onClick.AddListener(OnNextButtonDown);
        
        gameObject.SetActive(false);

    }
    private void OnNextButtonDown()
    {
        currentPage = (currentPage + 1 ) % maxPages;
        UpdateButtonsStatus();
    }
    private void OnPreviousButtonDown()
    {
        if (currentPage - 1 < 0)
        {
            currentPage = maxPages - 1;
        }
        else
        {
            currentPage = (currentPage - 1 ) % maxPages;
        }

        UpdateButtonsStatus();
    }

    private void OnPageClicked(int pageNumber)
    {
        currentPage = pageNumber;
        UpdateButtonsStatus();
    }

    public void Setup(int maxPages)
    {
        currentPage = 0;
        this.maxPages = maxPages;

        if (maxPages <= 1)
        {
            gameObject.SetActive(false);
            OnValueChanged?.Invoke(0);
            return;
        }
        
        gameObject.SetActive(true);

        EnsureButtons();
        UpdateButtonsStatus();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(pageButtonsParent);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
    }
    private void EnsureButtons()
    {
        if (buttons.Count != maxPages)
        {
            var diff = maxPages - buttons.Count;

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

            if (i >= maxPages)
            {
                uiPageButton.gameObject.SetActive(false);

                continue;
            }

            uiPageButton.Initialize(i);
            uiPageButton.gameObject.SetActive(true);
            uiPageButton.OnPageClicked += OnPageClicked;
        }
    }
    private void UpdateButtonsStatus()
    {
        UpdateToggleStatus();

        OnValueChanged?.Invoke(currentPage);
    }
    private void UpdateToggleStatus()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var currentButton = buttons[i];
            currentButton.Toggle(i == currentPage);
        }
    }

}