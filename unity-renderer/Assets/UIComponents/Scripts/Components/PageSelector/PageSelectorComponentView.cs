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
        private int currentIndex = -1;

        public event Action<int> OnValueChanged;
        public event Action<int> OnTotalPagesChanged;

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
            SelectIndex(model.CurrentPage - 1);
        }

        public void SelectIndex(int index)
        {
            currentIndex = index;
            UpdateButtonsStatus();
        }

        public void SetTotalPages(int maxTotalPages)
        {
            if (maxTotalPages == this.totalPages) return;

            this.totalPages = maxTotalPages;

            currentIndex = Mathf.Clamp(currentIndex, 0, maxTotalPages - 1);

            EnsureButtons();
            UpdateButtonsStatus(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(pageButtonsParent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            OnTotalPagesChanged?.Invoke(totalPages);
        }

        private void OnNextButtonDown()
        {
            currentIndex = (currentIndex + 1) % totalPages;
            UpdateButtonsStatus();
        }

        private void OnPreviousButtonDown()
        {
            if (currentIndex - 1 < 0)
                currentIndex = totalPages - 1;
            else
                currentIndex = (currentIndex - 1) % totalPages;

            UpdateButtonsStatus();
        }

        private void EnsureButtons()
        {
            if (buttons.Count != totalPages)
            {
                int diff = totalPages - buttons.Count;

                if (diff > 0)
                {
                    for (var i = 0; i < diff; i++)
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
                    PageNumber = i + 1,
                });
                uiPageButton.gameObject.SetActive(true);
                uiPageButton.OnPageClicked -= SelectIndex;
                uiPageButton.OnPageClicked += SelectIndex;
            }
        }

        private bool ShouldShowButton(int buttonIndex)
        {
            if (buttonIndex >= totalPages)
                return false;

            if (currentIndex + 1 <= maxVisiblePages / 2)
                return buttonIndex < maxVisiblePages;

            return buttonIndex < currentIndex + 1 + (maxVisiblePages / 2) && buttonIndex + 1 > currentIndex - (maxVisiblePages / 2);
        }

        private void UpdateButtonsStatus(bool notifyEvent = true)
        {
            UpdateToggleStatus();

            if (notifyEvent)
                OnValueChanged?.Invoke(currentIndex + 1);
        }

        private void UpdateToggleStatus()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var currentButton = buttons[i];

                if (limitedPages)
                    currentButton.gameObject.SetActive(ShouldShowButton(i));

                currentButton.Toggle(i == currentIndex);
            }
        }
    }
}
