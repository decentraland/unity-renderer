using DG.Tweening;
using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SignupHUD
{
    public class SignupHUDV2View : BaseComponentView, ISignupHUDView, IPointerClickHandler
    {
        private const int MIN_NAME_LENGTH = 1;
        private const int MAX_NAME_LENGTH = 15;

        public event ISignupHUDView.NameScreenDone OnNameScreenNext;
        public event Action OnEditAvatar;
        public event Action OnTermsOfServiceAgreed;
        public event Action OnTermsOfServiceBack;
        public event Action<string> OnLinkClicked;

        [SerializeField] internal TMP_InputField nameInputField;
        [SerializeField] internal GameObject nameInputFieldFullOrInvalid;
        [SerializeField] internal GameObject nameInputInvalidLabel;
        [SerializeField] internal TextMeshProUGUI nameCurrentCharacters;
        [SerializeField] internal GameObject nameCurrentCharactersReachedLimit;
        [SerializeField] internal GameObject emailInputFieldInvalid;
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal GameObject emailInputInvalidLabel;
        [SerializeField] internal Color colorForCharLimit;
        [SerializeField] internal ToggleComponentView agreeTosAndPrivacyPolicyToggle;
        [SerializeField] internal TMP_Text tosAndPrivacyPolicyText;
        [SerializeField] internal Button termsOfServiceAgreeButton;
        [SerializeField] internal Image termsOfServiceAgreeButtonIcon;

        [Header("SignUp Mode Transitions")]
        [SerializeField] internal RectTransform signUpRectTransform;
        [SerializeField] internal CanvasGroup signUpCanvasGroup;
        [SerializeField] internal Ease transitionEase = Ease.InOutExpo;
        [SerializeField] internal float transitionDuration = 0.5f;
        [SerializeField] internal float transitionDistance = 200f;

        private Vector2 originalAnchorPositionOfSignUp;

        public override void Awake()
        {
            originalAnchorPositionOfSignUp = signUpRectTransform.anchoredPosition;

            InitNameAndEmailScreen();
            InitTermsOfServicesScreen();
        }

        public override void RefreshControl() { }

        public void SetVisibility(bool visible)
        {
            PlayTransitionAnimation(visible);

            if (!visible)
                return;

            nameInputField.Select();
        }

        public void ShowNameScreen() { }

        public void ShowTermsOfServiceScreen() { }

        private void InitNameAndEmailScreen()
        {
            SetTermsOfServiceAgreeButtonInteractable(false);
            nameCurrentCharacters.text = $"{0}/{MAX_NAME_LENGTH}";
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            nameInputInvalidLabel.SetActive(false);
            nameInputFieldFullOrInvalid.SetActive(false);
            emailInputFieldInvalid.SetActive(false);
            emailInputInvalidLabel.SetActive(false);
            nameCurrentCharactersReachedLimit.SetActive(false);

            nameInputField.onValueChanged.AddListener((text) =>
            {
                UpdateNameAndEmailNextButton();
                nameCurrentCharacters.text = $"{text.Length} / {MAX_NAME_LENGTH}";
                nameCurrentCharacters.color = text.Length < MAX_NAME_LENGTH ? Color.black : colorForCharLimit;
                nameCurrentCharactersReachedLimit.SetActive(text.Length >= MAX_NAME_LENGTH);
                nameInputInvalidLabel.SetActive(!IsValidName(text));
                nameInputFieldFullOrInvalid.SetActive(text.Length >= MAX_NAME_LENGTH || !IsValidName(text));
            });

            emailInputField.onValueChanged.AddListener((text) =>
            {
                emailInputFieldInvalid.SetActive(!IsValidEmail(text));
                emailInputInvalidLabel.SetActive(!IsValidEmail(text));
                UpdateNameAndEmailNextButton();
            });

            agreeTosAndPrivacyPolicyToggle.OnSelectedChanged += (_, _, _) =>
                UpdateNameAndEmailNextButton();
        }

        private void InitTermsOfServicesScreen()
        {
            SetTermsOfServiceAgreeButtonInteractable(false);
            termsOfServiceAgreeButton.onClick.AddListener(() =>
            {
                OnNameScreenNext?.Invoke(nameInputField.text, emailInputField.text);
                OnTermsOfServiceAgreed?.Invoke();
            });
        }

        private void UpdateNameAndEmailNextButton()
        {
            string nameText = nameInputField.text;
            string emailText = emailInputField.text;

            SetTermsOfServiceAgreeButtonInteractable(
                nameText.Length >= MIN_NAME_LENGTH &&
                IsValidName(nameText) && IsValidEmail(emailText) &&
                agreeTosAndPrivacyPolicyToggle.isOn);
        }

        private bool IsValidEmail(string email)
        {
            if (email.Length == 0)
                return true;

            try
            {
                MailAddress mailAddress = new MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidName(string nameText) =>
            Regex.IsMatch(nameText, "^[a-zA-Z0-9]*$");

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(tosAndPrivacyPolicyText, Input.mousePosition, null);
            if (linkIndex == -1)
                return;

            TMP_LinkInfo linkInfo = tosAndPrivacyPolicyText.textInfo.linkInfo[linkIndex];
            OnLinkClicked?.Invoke(linkInfo.GetLinkID());
        }

        private void SetTermsOfServiceAgreeButtonInteractable(bool isInteractable)
        {
            termsOfServiceAgreeButton.interactable = isInteractable;
            termsOfServiceAgreeButtonIcon.color = new Color(termsOfServiceAgreeButtonIcon.color.r, termsOfServiceAgreeButtonIcon.color.g, termsOfServiceAgreeButtonIcon.color.b, isInteractable ? 1f : 0.1f);
        }

        private void PlayTransitionAnimation(bool visible)
        {
            Vector2 signUpEndPosition = originalAnchorPositionOfSignUp;
            if (!visible)
                signUpEndPosition.x -= transitionDistance;
            signUpRectTransform.DOAnchorPos(signUpEndPosition, transitionDuration).SetEase(transitionEase);
            signUpCanvasGroup.DOFade(visible ? 1f : 0f, transitionDuration).SetEase(transitionEase);
            signUpCanvasGroup.blocksRaycasts = visible;
        }
    }
}
