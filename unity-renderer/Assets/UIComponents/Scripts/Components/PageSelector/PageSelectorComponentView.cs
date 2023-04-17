using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.Scripts.Components
{
    public class PageSelectorComponentView : BaseComponentView<PageSelectorModel>
    {
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private PageSelectorButtonComponentView pageButtonPrefab;
        [SerializeField] private RectTransform pageButtonsParent;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private bool limitedPages;
        [SerializeField] private int maxVisiblePages;

        private readonly List<PageSelectorButtonComponentView> buttons = new ();
        private int totalPages;
        private int currentPage = -1;

        public event Action<int> OnValueChanged;

        public override void Awake()
        {
            base.Awake();

            previousButton.onClick.AddListener(OnPreviousButtonDown);
            nextButton.onClick.AddListener(OnNextButtonDown);
            gameObject.SetActive(false);
        }

        public override void RefreshControl()
        {
            SetTotalPages(model.TotalPages);
            SelectPage(model.CurrentPage);
        }

        private void OnNextButtonDown()
        {
            currentPage = (currentPage + 1) % totalPages;
            UpdateButtonsStatus();
        }

        private void OnPreviousButtonDown()
        {
            if (currentPage - 1 < 0)
                currentPage = totalPages - 1;
            else
                currentPage = (currentPage - 1) % totalPages;

            UpdateButtonsStatus();
        }

        public void SelectPage(int pageNumber)
        {
            currentPage = pageNumber;
            UpdateButtonsStatus();
        }

        public void SetTotalPages(int maxTotalPages)
        {
            if (maxTotalPages == this.totalPages) return;

            this.totalPages = maxTotalPages;

            currentPage = Mathf.Clamp(currentPage, 0, maxTotalPages - 1);

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
                int diff = totalPages - buttons.Count;

                if (diff > 0)
                {
                    for (int i = 0; i < diff; i++)
                    {
                        var instance = Instantiate(pageButtonPrefab, pageButtonsParent);
                        buttons.Add(instance);
                    }
                }
            }

            for (var i = 0; i < buttons.Count; i++)
            {
                PageSelectorButtonComponentView uiPageButton = buttons[i];

                if (i >= totalPages)
                {
                    uiPageButton.gameObject.SetActive(false);

                    continue;
                }

                uiPageButton.SetModel(new PageSelectorButtonModel
                {
                    PageNumber = i,
                });
                uiPageButton.gameObject.SetActive(true);
                uiPageButton.OnPageClicked -= SelectPage;
                uiPageButton.OnPageClicked += SelectPage;
            }
        }

        private bool ShouldShowButton(int buttonIndex)
        {
            if (buttonIndex >= totalPages)
                return false;

            if (currentPage + 1 <= maxVisiblePages / 2)
                return buttonIndex < maxVisiblePages;

            return buttonIndex < currentPage + 1 + (maxVisiblePages / 2) && buttonIndex + 1 > currentPage - (maxVisiblePages / 2);
        }

        private void UpdateButtonsStatus(bool notifyEvent = true)
        {
            UpdateToggleStatus();

            if (notifyEvent)
                OnValueChanged?.Invoke(currentPage);
        }

        private void UpdateToggleStatus()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var currentButton = buttons[i];

                if (limitedPages)
                    currentButton.gameObject.SetActive(ShouldShowButton(i));

                currentButton.Toggle(i == currentPage);
            }
        }
    }
}
