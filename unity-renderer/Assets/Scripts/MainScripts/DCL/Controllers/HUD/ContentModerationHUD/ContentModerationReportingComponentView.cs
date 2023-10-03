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
        [SerializeField] private Button backgroundButton;
        [SerializeField] private ButtonComponentView cancelButton;
        [SerializeField] private ButtonComponentView gotItButton;
        [SerializeField] private ButtonComponentView sendButton;
        [SerializeField] private Button learnMoreButton1;
        [SerializeField] private Button learnMoreButton2;
        [SerializeField] private ContentModerationRatingButtonComponentView teenRatingButton;
        [SerializeField] private ContentModerationRatingButtonComponentView adultRatingButton;
        [SerializeField] private ContentModerationRatingButtonComponentView restrictedRatingButton;
        [SerializeField] private TMP_Text optionsSectionTitle;
        [SerializeField] private TMP_Text optionsSectionSubtitle;
        [SerializeField] private Transform optionsContainer;
        [SerializeField] private ScrollRect optionsScroll;
        [SerializeField] private GameObject mainModal;
        [SerializeField] private GameObject mainFormSection;
        [SerializeField] private GameObject loadingStateSection;
        [SerializeField] private GameObject reportSentModal;
        [SerializeField] private Image modalHeaderImage;

        [Header("TEEN CONFIGURATION")]
        [SerializeField] private Sprite teenHeaderSprite;
        [SerializeField] private string teenOptionTitle;
        [SerializeField] private string teenOptionSubtitle;
        [SerializeField] private List<string> teenOptions;

        [Header("ADULT CONFIGURATION")]
        [SerializeField] private Sprite adultHeaderSprite;
        [SerializeField] private string adultOptionTitle;
        [SerializeField] private string adultOptionSubtitle;
        [SerializeField] private List<string> adultOptions;

        [Header("RESTRICTED CONFIGURATION")]
        [SerializeField] private Sprite restrictedHeaderSprite;
        [SerializeField] private string restrictedOptionTitle;
        [SerializeField] private string restrictedOptionSubtitle;
        [SerializeField] private List<string> restrictedOptions;

        [Header("PREFABS")]
        [SerializeField] private GameObject optionButtonPrefab;
        [SerializeField] private GameObject commentsPrefab;

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
            teenRatingButton.RatingButton.onClick.AddListener(() => OnRatingButtonClicked(SceneContentCategory.TEEN));
            adultRatingButton.RatingButton.onClick.AddListener(() => OnRatingButtonClicked(SceneContentCategory.ADULT));
            restrictedRatingButton.RatingButton.onClick.AddListener(() => OnRatingButtonClicked(SceneContentCategory.RESTRICTED));

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
            teenRatingButton.RatingButton.onClick.RemoveAllListeners();
            adultRatingButton.RatingButton.onClick.RemoveAllListeners();
            restrictedRatingButton.RatingButton.onClick.RemoveAllListeners();
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

            teenRatingButton.Select(contentCategory == SceneContentCategory.TEEN);
            adultRatingButton.Select(contentCategory == SceneContentCategory.ADULT);
            restrictedRatingButton.Select(contentCategory == SceneContentCategory.RESTRICTED);

            teenRatingButton.SetCurrentMarkArctive(contentCategory == SceneContentCategory.TEEN);
            adultRatingButton.SetCurrentMarkArctive(contentCategory == SceneContentCategory.ADULT);
            restrictedRatingButton.SetCurrentMarkArctive(contentCategory == SceneContentCategory.RESTRICTED);

            modalHeaderImage.sprite = contentCategory switch
                                      {
                                          SceneContentCategory.TEEN => teenHeaderSprite,
                                          SceneContentCategory.ADULT => adultHeaderSprite,
                                          SceneContentCategory.RESTRICTED => restrictedHeaderSprite,
                                          _ => teenHeaderSprite,
                                      };
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

        private void OnRatingButtonClicked(SceneContentCategory contentCategory)
        {
            selectedOptions.Clear();
            currentRating = contentCategory;
            teenRatingButton.Select(contentCategory == SceneContentCategory.TEEN);
            adultRatingButton.Select(contentCategory == SceneContentCategory.ADULT);
            restrictedRatingButton.Select(contentCategory == SceneContentCategory.RESTRICTED);

            switch (contentCategory)
            {
                case SceneContentCategory.TEEN:
                    optionsSectionTitle.text = teenOptionTitle;
                    optionsSectionSubtitle.text = teenOptionSubtitle;
                    break;
                case SceneContentCategory.ADULT:
                    optionsSectionTitle.text = adultOptionTitle;
                    optionsSectionSubtitle.text = adultOptionSubtitle;
                    break;
                case SceneContentCategory.RESTRICTED:
                    optionsSectionTitle.text = restrictedOptionTitle;
                    optionsSectionSubtitle.text = restrictedOptionSubtitle;
                    break;
            }

            foreach (ContentModerationReportingOptionComponentView option in teenOptionButtons)
                option.SetActive((int)contentCategory == 0);
            foreach (ContentModerationReportingOptionComponentView option in adultOptionButtons)
                option.SetActive((int)contentCategory == 1);
            foreach (ContentModerationReportingOptionComponentView option in restrictedOptionButtons)
                option.SetActive((int)contentCategory == 2);

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
