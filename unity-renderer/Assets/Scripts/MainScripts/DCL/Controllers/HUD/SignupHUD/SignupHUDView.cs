using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SignupHUD
{
    public class SignupHUDView : MonoBehaviour, ISignupHUDView
    {
        private const int MIN_NAME_LENGTH = 15;
        public event ISignupHUDView.NameScreenDone OnNameScreenNext;
        public event Action OnTermsOfServiceAgreed;
        public event Action OnTermsOfServiceBack;

        [SerializeField] private RectTransform nameAndEmailPanel;
        [SerializeField] private Button nameAndEmailNextButton;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TextMeshProUGUI nameCurrentCharacters;
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private RectTransform termsOfServicePanel;
        [SerializeField] private Button termsOfServiceBackButton;
        [SerializeField] private Button termsOfServiceAgreeButton;

        private void Awake()
        {
            nameAndEmailNextButton.enabled = false;
            nameCurrentCharacters.text = $"{0} / {MIN_NAME_LENGTH}";
            nameInputField.onValueChanged.AddListener((text) =>
            {
                nameAndEmailNextButton.enabled = text.Length >= MIN_NAME_LENGTH;
                nameCurrentCharacters.text = $"{text.Length} / {MIN_NAME_LENGTH}";
            });
            nameAndEmailNextButton.onClick.AddListener(() => OnNameScreenNext?.Invoke(nameInputField.text, emailInputField.text));
            termsOfServiceBackButton.onClick.AddListener(() => OnTermsOfServiceBack?.Invoke());
            termsOfServiceAgreeButton.onClick.AddListener(() => OnTermsOfServiceAgreed?.Invoke());
        }

        public static SignupHUDView CreateView() { return null; }

        public void SetVisibility(bool visible) { gameObject.SetActive(visible); }

        public void ShowNameScreen()
        {
            nameAndEmailPanel.gameObject.SetActive(true);
            termsOfServicePanel.gameObject.SetActive(false);
        }
        public void ShowTermsOfServiceScreen()
        {
            nameAndEmailPanel.gameObject.SetActive(false);
            termsOfServicePanel.gameObject.SetActive(true);
        }

        public void Dispose()
        {
            if (this != null)
                Destroy(gameObject);
        }
    }
}