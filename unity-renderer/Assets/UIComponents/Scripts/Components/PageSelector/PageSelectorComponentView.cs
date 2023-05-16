using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIComponents.Scripts.Components
{
    public class PageSelectorComponentView : BaseComponentView
    {
        public event Action<int> OnValueChanged;

        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private PageSelectorButtonComponentView pageSelectorButtonPrefab;
        [SerializeField] private RectTransform pageButtonsParent;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private bool limitedPages;
        [SerializeField] private int maxVisiblePages;

        private readonly List<PageSelectorButtonComponentView> buttons = new ();

        public int TotalPages { get; private set; }
        public int CurrentPage { get; private set; }

        public override void Awake()
        {
            base.Awake();

            previousButton.onClick.AddListener(OnPreviousButtonDown);
            nextButton.onClick.AddListener(OnNextButtonDown);
            gameObject.SetActive(false);
        }

        public override void RefreshControl() { }

        public void Setup(int maxTotalPages, bool forceRebuild = false)
        {
            if (maxTotalPages == this.TotalPages && !forceRebuild) { return; }

            this.TotalPages = maxTotalPages;

            CurrentPage = Mathf.Clamp(CurrentPage, 0, maxTotalPages - 1);

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

        public void SelectPage(int pageNumber, bool notifyEvent = true)
        {
            CurrentPage = pageNumber;
            UpdateButtonsStatus(notifyEvent);
        }

        private void OnPreviousButtonDown()
        {
            if (CurrentPage - 1 < 0)
                CurrentPage = TotalPages - 1;
            else
                CurrentPage = (CurrentPage - 1) % TotalPages;

            UpdateButtonsStatus();
        }

        private void OnNextButtonDown()
        {
            CurrentPage = (CurrentPage + 1) % TotalPages;
            UpdateButtonsStatus();
        }

        private void EnsureButtons()
        {
            if (buttons.Count != TotalPages)
            {
                int diff = TotalPages - buttons.Count;

                if (diff > 0)
                {
                    for (var i = 0; i < diff; i++)
                    {
                        var instance = Instantiate(pageSelectorButtonPrefab, pageButtonsParent);
                        buttons.Add(instance);
                    }
                }
            }

            for (var i = 0; i < buttons.Count; i++)
            {
                PageSelectorButtonComponentView pageSelectorButtonComponentView = buttons[i];

                if (i >= TotalPages)
                {
                    pageSelectorButtonComponentView.gameObject.SetActive(false);

                    continue;
                }

                pageSelectorButtonComponentView.Initialize(i);
                pageSelectorButtonComponentView.gameObject.SetActive(true);
                pageSelectorButtonComponentView.OnPageClicked -= OnPageClicked;
                pageSelectorButtonComponentView.OnPageClicked += OnPageClicked;
            }
        }

        private bool ShouldShowButton(int buttonIndex)
        {
            if (buttonIndex >= TotalPages)
                return false;

            if (CurrentPage + 1 <= maxVisiblePages / 2)
                return buttonIndex < maxVisiblePages;
            else
                return buttonIndex < CurrentPage + 1 + (maxVisiblePages / 2) && buttonIndex + 1 > CurrentPage - (maxVisiblePages / 2);
        }

        private void UpdateButtonsStatus(bool notifyEvent = true)
        {
            UpdateToggleStatus();

            if (notifyEvent)
                OnValueChanged?.Invoke(CurrentPage);
        }

        private void UpdateToggleStatus()
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                var currentButton = buttons[i];

                if (limitedPages)
                    currentButton.gameObject.SetActive(ShouldShowButton(i));

                currentButton.Toggle(i == CurrentPage);
            }
        }

        private void OnPageClicked(int pageNumber) =>
            SelectPage(pageNumber);
    }
}
