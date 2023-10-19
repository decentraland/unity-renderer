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
        [SerializeField] internal GameObject emailInputFieldInvalid;
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal GameObject emailInputInvalidLabel;
        [SerializeField] internal Color colorForCharLimit;
        [SerializeField] internal ToggleComponentView agreeTosAndPrivacyPolicyToggle;
        [SerializeField] internal TMP_Text tosAndPrivacyPolicyText;
        [SerializeField] internal Button termsOfServiceAgreeButton;

        public override void Awake()
        {
            InitNameAndEmailScreen();
            InitTermsOfServicesScreen();
        }

        public override void RefreshControl() { }

        public void SetVisibility(bool visible)
        {
            gameObject.SetActive(visible);

            if (!visible)
                return;

            nameInputField.Select();
            CleanForm();
        }

        public void ShowNameScreen() { }

        public void ShowTermsOfServiceScreen() { }

        private void InitNameAndEmailScreen()
        {
            termsOfServiceAgreeButton.interactable = false;
            nameCurrentCharacters.text = $"{0}/{MAX_NAME_LENGTH}";
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            nameInputInvalidLabel.SetActive(false);
            nameInputFieldFullOrInvalid.SetActive(false);
            emailInputFieldInvalid.SetActive(false);
            emailInputInvalidLabel.SetActive(false);

            nameInputField.onValueChanged.AddListener((text) =>
            {
                UpdateNameAndEmailNextButton();
                nameCurrentCharacters.text = $"{text.Length} / {MAX_NAME_LENGTH}";
                nameCurrentCharacters.color = text.Length < MAX_NAME_LENGTH ? Color.black : colorForCharLimit;
                nameInputInvalidLabel.SetActive(!IsValidName(text));
                nameInputFieldFullOrInvalid.SetActive(text.Length >= MAX_NAME_LENGTH || !IsValidName(text));
            });

            emailInputField.onValueChanged.AddListener((text) =>
            {
                emailInputFieldInvalid.SetActive(!IsValidEmail(text));
                emailInputInvalidLabel.SetActive(!IsValidEmail(text));
                UpdateNameAndEmailNextButton();
            });

            agreeTosAndPrivacyPolicyToggle.OnSelectedChanged += (isOn, id, text) =>
                UpdateNameAndEmailNextButton();
        }

        private void InitTermsOfServicesScreen()
        {
            termsOfServiceAgreeButton.interactable = false;
            termsOfServiceAgreeButton.onClick.AddListener(() => OnTermsOfServiceAgreed?.Invoke());
        }

        private void UpdateNameAndEmailNextButton()
        {
            string nameText = nameInputField.text;
            string emailText = emailInputField.text;

            termsOfServiceAgreeButton.interactable =
                nameText.Length >= MIN_NAME_LENGTH &&
                IsValidName(nameText) && IsValidEmail(emailText) &&
                agreeTosAndPrivacyPolicyToggle.isOn;
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

        private void CleanForm()
        {
            nameInputField.text = string.Empty;
            emailInputField.text = string.Empty;
            agreeTosAndPrivacyPolicyToggle.isOn = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(tosAndPrivacyPolicyText, Input.mousePosition, null);
            if (linkIndex == -1)
                return;

            TMP_LinkInfo linkInfo = tosAndPrivacyPolicyText.textInfo.linkInfo[linkIndex];
            OnLinkClicked?.Invoke(linkInfo.GetLinkID());
        }
    }
}
