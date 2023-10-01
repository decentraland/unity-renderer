using DCL.Controllers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.ContentModeration
{
    public class ContentModerationReportingComponentView : BaseComponentView, IContentModerationReportingComponentView
    {
        [Header("REFERENCES")]
        [SerializeField] internal Button backgroundButton;
        [SerializeField] internal ButtonComponentView cancelButton;
        [SerializeField] internal ButtonComponentView gotItButton;
        [SerializeField] internal ButtonComponentView sendButton;
        [SerializeField] internal Button learnMoreButton1;
        [SerializeField] internal Button learnMoreButton2;
        [SerializeField] internal Slider ratingSlider;
        [SerializeField] internal Button teenAuxButton;
        [SerializeField] internal Button adultAuxButton;
        [SerializeField] internal Button restrictedAuxButton;
        [SerializeField] internal TMP_Text optionsSectionTitle;
        [SerializeField] internal TMP_Text optionsSectionSubtitle;
        [SerializeField] internal Transform optionsContainer;
        [SerializeField] internal ScrollRect optionsScroll;
        [SerializeField] internal GameObject mainModal;
        [SerializeField] internal GameObject mainFormSection;
        [SerializeField] internal GameObject loadingStateSection;
        [SerializeField] internal GameObject reportSentModal;

        [Header("TEEN CONFIGURATION")]
        public string teenOptionTitle;
        public string teenOptionSubtitle;
        public List<string> teenOptions;

        [Header("ADULT CONFIGURATION")]
        public string adultOptionTitle;
        public string adultOptionSubtitle;
        public List<string> adultOptions;

        [Header("RESTRICTED CONFIGURATION")]
        public string restrictedOptionTitle;
        public string restrictedOptionSubtitle;
        public List<string> restrictedOptions;

        [Header("PREFABS")]
        [SerializeField] internal GameObject optionButtonPrefab;
        [SerializeField] internal GameObject commentsPrefab;

        private readonly List<ContentModerationReportingOptionComponentView> teenOptionButtons = new ();
        private readonly List<ContentModerationReportingOptionComponentView> adultOptionButtons = new ();
        private readonly List<ContentModerationReportingOptionComponentView> restrictedOptionButtons = new ();
        private SceneContentCategory currentRating;
        private readonly List<string> selectedOptions = new ();
        private TMP_InputField commentsInput;
        private TMP_Text sendButtonText;
        private bool isLoadingActive;

        public event Action OnPanelClosed;
        public event Action<(SceneContentCategory contentCategory, List<string> issues, string comments)> OnSendClicked;
        public event Action OnLearnMoreClicked;

        public override void Awake()
        {
            base.Awake();
            backgroundButton.onClick.AddListener(HidePanel);
            cancelButton.onClick.AddListener(HidePanel);
            gotItButton.onClick.AddListener(HidePanel);
            sendButtonText = sendButton.GetComponentInChildren<TMP_Text>();
            sendButton.onClick.AddListener(() => SendReport((currentRating, selectedOptions, commentsInput.text)));
            learnMoreButton1.onClick.AddListener(GoToLearnMore);
            learnMoreButton2.onClick.AddListener(GoToLearnMore);
            ratingSlider.onValueChanged.AddListener(OnRatingSliderChanged);
            teenAuxButton.onClick.AddListener(() => { ratingSlider.value = 0; });
            adultAuxButton.onClick.AddListener(() => { ratingSlider.value = 1; } );
            restrictedAuxButton.onClick.AddListener(() => { ratingSlider.value = 2; });

            CreateButtons();
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            gotItButton.onClick.RemoveAllListeners();
            sendButton.onClick.RemoveAllListeners();
            learnMoreButton1.onClick.RemoveAllListeners();
            learnMoreButton2.onClick.RemoveAllListeners();
            ratingSlider.onValueChanged.RemoveAllListeners();
            teenAuxButton.onClick.RemoveAllListeners();
            adultAuxButton.onClick.RemoveAllListeners();
            restrictedAuxButton.onClick.RemoveAllListeners();
            base.Dispose();
        }

        public override void RefreshControl() { }

        public void ShowPanel()
        {
            Show();
            SetLoadingState(false);
            SetPanelAsSent(false);
            ResetOptions();
        }

        public void HidePanel()
        {
            if (isLoadingActive)
                return;

            Hide();
            OnPanelClosed?.Invoke();
        }

        public void SetRating(SceneContentCategory contentCategory)
        {
            currentRating = contentCategory;

            switch (contentCategory)
            {
                default:
                case SceneContentCategory.TEEN:
                    ratingSlider.value = 0;
                    break;
                case SceneContentCategory.ADULT:
                    ratingSlider.value = 1;
                    break;
                case SceneContentCategory.RESTRICTED:
                    ratingSlider.value = 2;
                    break;
            }
        }

        public void SetLoadingState(bool isLoading)
        {
            mainFormSection.SetActive(!isLoading);
            loadingStateSection.SetActive(isLoading);
            isLoadingActive = isLoading;
        }

        public void SetPanelAsSent(bool hasBeenSent)
        {
            mainModal.SetActive(!hasBeenSent);
            reportSentModal.SetActive(hasBeenSent);
        }

        private void CreateButtons()
        {
            foreach (string option in teenOptions)
            {
                ContentModerationReportingOptionComponentView optionComponent = InstantiateOptionButton(option);
                teenOptionButtons.Add(optionComponent);
            }

            foreach (string option in adultOptions)
            {
                ContentModerationReportingOptionComponentView optionComponent = InstantiateOptionButton(option);
                adultOptionButtons.Add(optionComponent);
            }

            foreach (string option in restrictedOptions)
            {
                ContentModerationReportingOptionComponentView optionComponent = InstantiateOptionButton(option);
                restrictedOptionButtons.Add(optionComponent);
            }

            commentsInput = InstantiateCommentsBox();
        }

        private ContentModerationReportingOptionComponentView InstantiateOptionButton(string option)
        {
            ContentModerationReportingOptionComponentView optionComponent = Instantiate(optionButtonPrefab, optionsContainer)
               .GetComponent<ContentModerationReportingOptionComponentView>();
            optionComponent.SetOptionText(option);
            optionComponent.UnselectOption();

            optionComponent.OptionButton.onClick.AddListener(() =>
            {
                if (!selectedOptions.Contains(option))
                {
                    selectedOptions.Add(option);
                    optionComponent.SelectOption();
                }
                else
                {
                    selectedOptions.Remove(option);
                    optionComponent.UnselectOption();
                }

                SetSendButtonInteractable(selectedOptions.Count > 0);
            });

            return optionComponent;
        }

        private TMP_InputField InstantiateCommentsBox()
        {
            GameObject commentsObject = Instantiate(commentsPrefab, optionsContainer);
            return commentsObject.GetComponentInChildren<TMP_InputField>();
        }

        private void OnRatingSliderChanged(float value)
        {
            selectedOptions.Clear();

            switch ((int)value)
            {
                case 0:
                    optionsSectionTitle.text = teenOptionTitle;
                    optionsSectionSubtitle.text = teenOptionSubtitle;
                    break;
                case 1:
                    optionsSectionTitle.text = adultOptionTitle;
                    optionsSectionSubtitle.text = adultOptionSubtitle;
                    break;
                case 2:
                    optionsSectionTitle.text = restrictedOptionTitle;
                    optionsSectionSubtitle.text = restrictedOptionSubtitle;
                    break;
            }

            foreach (ContentModerationReportingOptionComponentView option in teenOptionButtons)
                option.SetActive((int)value == 0);
            foreach (ContentModerationReportingOptionComponentView option in adultOptionButtons)
                option.SetActive((int)value == 1);
            foreach (ContentModerationReportingOptionComponentView option in restrictedOptionButtons)
                option.SetActive((int)value == 2);

            ResetOptions();
        }

        private void ResetOptions()
        {
            foreach (ContentModerationReportingOptionComponentView option in teenOptionButtons)
                option.UnselectOption();
            foreach (ContentModerationReportingOptionComponentView option in adultOptionButtons)
                option.UnselectOption();
            foreach (ContentModerationReportingOptionComponentView option in restrictedOptionButtons)
                option.UnselectOption();

            selectedOptions.Clear();
            optionsScroll.verticalNormalizedPosition = 1f;
            commentsInput.text = string.Empty;
            SetSendButtonInteractable(false);
        }

        private void SetSendButtonInteractable(bool isInteractable)
        {
            sendButton.SetInteractable(isInteractable);
            sendButtonText.text = isInteractable ? "FLAG THIS SCENE" : "SELECT AT LEAST ONE";
        }

        private void SendReport((SceneContentCategory contentCategory, List<string> issues, string comments) report) =>
            OnSendClicked?.Invoke(report);

        private void GoToLearnMore() =>
            OnLearnMoreClicked?.Invoke();
    }
}
