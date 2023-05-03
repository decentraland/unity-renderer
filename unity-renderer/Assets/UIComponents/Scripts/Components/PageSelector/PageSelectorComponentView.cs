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
        public int CurrentIndex { get; private set; } = -1;

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
            CurrentIndex = model.CurrentPage - 1;
            UpdateButtonsStatus(false);
        }

        public void SelectIndex(int index)
        {
            CurrentIndex = index;
            UpdateButtonsStatus();
        }

        public void SetTotalPages(int maxTotalPages)
        {
            if (maxTotalPages == this.totalPages) return;

            this.totalPages = maxTotalPages;

            CurrentIndex = Mathf.Clamp(CurrentIndex, 0, maxTotalPages - 1);

            EnsureButtons();
            UpdateButtonsStatus(false);

            LayoutRebuilder.ForceRebuildLayoutImmediate(pageButtonsParent);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            OnTotalPagesChanged?.Invoke(totalPages);
        }

        private void OnNextButtonDown()
        {
            CurrentIndex = (CurrentIndex + 1) % totalPages;
            UpdateButtonsStatus();
        }

        private void OnPreviousButtonDown()
        {
            if (CurrentIndex - 1 < 0)
                CurrentIndex = totalPages - 1;
            else
                CurrentIndex = (CurrentIndex - 1) % totalPages;

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

            if (CurrentIndex + 1 <= maxVisiblePages / 2)
                return buttonIndex < maxVisiblePages;

            return buttonIndex < CurrentIndex + 1 + (maxVisiblePages / 2) && buttonIndex + 1 > CurrentIndex - (maxVisiblePages / 2);
        }

        private void UpdateButtonsStatus(bool notifyEvent = true)
        {
            UpdateToggleStatus();

            if (notifyEvent)
                OnValueChanged?.Invoke(CurrentIndex + 1);
        }

        private void UpdateToggleStatus()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var currentButton = buttons[i];

                if (limitedPages)
                    currentButton.gameObject.SetActive(ShouldShowButton(i));

                currentButton.Toggle(i == CurrentIndex);
            }
        }
    }
}
