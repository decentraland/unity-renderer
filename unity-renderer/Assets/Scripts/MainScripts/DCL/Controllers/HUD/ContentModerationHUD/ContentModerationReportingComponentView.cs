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
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Button backgroundButton;
        [SerializeField] private ButtonComponentView cancelButton;
        [SerializeField] private ButtonComponentView gotItButton1;
        [SerializeField] private ButtonComponentView gotItButton2;
        [SerializeField] private ButtonComponentView sendButton;
        [SerializeField] private Button learnMoreButton;
        [SerializeField] private ContentModerationRatingButtonComponentView teenRatingButton;
        [SerializeField] private ContentModerationRatingButtonComponentView adultRatingButton;
        [SerializeField] private ContentModerationRatingButtonComponentView restrictedRatingButton;
        [SerializeField] private GameObject optionsSectionContainer;
        [SerializeField] private TMP_Text optionsSectionTitle;
        [SerializeField] private TMP_Text optionsSectionSubtitle;
        [SerializeField] private GameObject markedRatingInfoContainer;
        [SerializeField] private TMP_Text markedRatingInfoTitle;
        [SerializeField] private TMP_Text markedRatingInfoDescription;
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
        [SerializeField] private string teenDescription;

        [Header("ADULT CONFIGURATION")]
        [SerializeField] private Sprite adultHeaderSprite;
        [SerializeField] private string adultOptionTitle;
        [SerializeField] private string adultOptionSubtitle;
        [SerializeField] private List<string> adultOptions;
        [SerializeField] private string adultDescription;

        [Header("RESTRICTED CONFIGURATION")]
        [SerializeField] private Sprite restrictedHeaderSprite;
        [SerializeField] private string restrictedOptionTitle;
        [SerializeField] private string restrictedOptionSubtitle;
        [SerializeField] private List<string> restrictedOptions;
        [SerializeField] private string restrictedDescription;

        [Header("PREFABS")]
        [SerializeField] private GameObject optionButtonPrefab;
        [SerializeField] private GameObject commentsPrefab;

        private readonly List<ContentModerationReportingOptionComponentView> teenOptionButtons = new ();
        private readonly List<ContentModerationReportingOptionComponentView> adultOptionButtons = new ();
        private readonly List<ContentModerationReportingOptionComponentView> restrictedOptionButtons = new ();
        private SceneContentCategory currentMarkedRating;
        private SceneContentCategory currentRating;
        private readonly List<string> selectedOptions = new ();
        private TMP_InputField commentsInput;
        private bool isLoadingActive;
        private string currentSceneName;

        public event Action<bool> OnPanelClosed;
        public event Action<(SceneContentCategory contentCategory, List<string> issues, string comments)> OnSendClicked;
        public event Action OnLearnMoreClicked;

        public override void Awake()
        {
            base.Awake();
            backgroundButton.onClick.AddListener(() => HidePanel(!reportSentModal.activeSelf));
            cancelButton.onClick.AddListener(() => HidePanel(true));
            gotItButton1.onClick.AddListener(() => HidePanel(false));
            gotItButton2.onClick.AddListener(() => HidePanel(false));
            sendButton.onClick.AddListener(() => SendReport((currentRating, selectedOptions, commentsInput.text)));
            learnMoreButton.onClick.AddListener(GoToLearnMore);
            teenRatingButton.RatingButton.onClick.AddListener(() => SetRating(SceneContentCategory.TEEN));
            adultRatingButton.RatingButton.onClick.AddListener(() => SetRating(SceneContentCategory.ADULT));
            restrictedRatingButton.RatingButton.onClick.AddListener(() => SetRating(SceneContentCategory.RESTRICTED));

            CreateButtons();
        }

        public override void Dispose()
        {
            backgroundButton.onClick.RemoveAllListeners();
            cancelButton.onClick.RemoveAllListeners();
            gotItButton1.onClick.RemoveAllListeners();
            gotItButton2.onClick.RemoveAllListeners();
            sendButton.onClick.RemoveAllListeners();
            learnMoreButton.onClick.RemoveAllListeners();
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

        public void HidePanel(bool isCancelled)
        {
            if (isLoadingActive || !isVisible)
                return;

            Hide();
            OnPanelClosed?.Invoke(isCancelled);
        }

        public void SetScene(string sceneName) =>
            currentSceneName = sceneName;

        public void SetRatingAsMarked(SceneContentCategory contentCategory)
        {
            currentMarkedRating = contentCategory;

            modalHeaderImage.sprite = contentCategory switch
                                      {
                                          SceneContentCategory.TEEN => teenHeaderSprite,
                                          SceneContentCategory.ADULT => adultHeaderSprite,
                                          SceneContentCategory.RESTRICTED => restrictedHeaderSprite,
                                          _ => teenHeaderSprite,
                                      };

            teenRatingButton.SetCurrentMarkActive(contentCategory == SceneContentCategory.TEEN);
            adultRatingButton.SetCurrentMarkActive(contentCategory == SceneContentCategory.ADULT);
            restrictedRatingButton.SetCurrentMarkActive(contentCategory == SceneContentCategory.RESTRICTED);
        }

        public void SetRating(SceneContentCategory contentCategory)
        {
            currentRating = contentCategory;

            selectedOptions.Clear();

            teenRatingButton.Select(contentCategory == SceneContentCategory.TEEN);
            adultRatingButton.Select(contentCategory == SceneContentCategory.ADULT);
            restrictedRatingButton.Select(contentCategory == SceneContentCategory.RESTRICTED);

            markedRatingInfoContainer.SetActive((contentCategory == SceneContentCategory.TEEN && teenRatingButton.IsMarked) ||
                                                (contentCategory == SceneContentCategory.ADULT && adultRatingButton.IsMarked) ||
                                                (contentCategory == SceneContentCategory.RESTRICTED && restrictedRatingButton.IsMarked));
            optionsSectionContainer.SetActive((contentCategory == SceneContentCategory.TEEN && !teenRatingButton.IsMarked) ||
                                              (contentCategory == SceneContentCategory.ADULT && !adultRatingButton.IsMarked) ||
                                              (contentCategory == SceneContentCategory.RESTRICTED && !restrictedRatingButton.IsMarked));

            gotItButton1.gameObject.SetActive(markedRatingInfoContainer.activeSelf);
            sendButton.gameObject.SetActive(!markedRatingInfoContainer.activeSelf);

            switch (contentCategory)
            {
                default:
                case SceneContentCategory.TEEN:
                    optionsSectionTitle.text = teenOptionTitle;
                    optionsSectionSubtitle.text = teenOptionSubtitle;
                    markedRatingInfoTitle.text = teenOptionTitle;
                    markedRatingInfoDescription.text = teenDescription;
                    break;
                case SceneContentCategory.ADULT:
                    optionsSectionTitle.text = adultOptionTitle;
                    optionsSectionSubtitle.text = adultOptionSubtitle;
                    markedRatingInfoTitle.text = adultOptionTitle;
                    markedRatingInfoDescription.text = adultDescription;
                    break;
                case SceneContentCategory.RESTRICTED:
                    optionsSectionTitle.text = restrictedOptionTitle;
                    optionsSectionSubtitle.text = restrictedOptionSubtitle;
                    markedRatingInfoTitle.text = restrictedOptionTitle;
                    markedRatingInfoDescription.text = restrictedDescription;
                    break;
            }

            foreach (ContentModerationReportingOptionComponentView option in teenOptionButtons)
                option.SetActive((int)contentCategory == 0);
            foreach (ContentModerationReportingOptionComponentView option in adultOptionButtons)
                option.SetActive((int)contentCategory == 1);
            foreach (ContentModerationReportingOptionComponentView option in restrictedOptionButtons)
                option.SetActive((int)contentCategory == 2);

            ResetOptions();
            SetTitle();
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

        public void ResetPanelState()
        {
            mainFormSection.SetActive(true);
            loadingStateSection.SetActive(false);
            isLoadingActive = false;
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

        private void SetTitle()
        {
            titleText.text = currentMarkedRating == currentRating
                ? $"{currentSceneName} has been rated as"
                : $"Flag {currentSceneName} for Rating Review";
        }

        private void SetSendButtonInteractable(bool isInteractable) =>
            sendButton.SetInteractable(isInteractable);

        private void SendReport((SceneContentCategory contentCategory, List<string> issues, string comments) report) =>
            OnSendClicked?.Invoke(report);

        private void GoToLearnMore() =>
            OnLearnMoreClicked?.Invoke();
    }
}
