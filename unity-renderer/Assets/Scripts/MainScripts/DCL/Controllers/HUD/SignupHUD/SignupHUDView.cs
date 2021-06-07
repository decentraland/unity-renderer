using System;
using System.Net.Mail;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SignupHUD
{
    public interface ISignupHUDView : IDisposable
    {
        delegate void NameScreenDone(string newName, string newEmail);
        event NameScreenDone OnNameScreenNext;
        event Action OnEditAvatar;
        event Action OnTermsOfServiceAgreed;
        event Action OnTermsOfServiceBack;

        void SetVisibility(bool visible);
        void ShowNameScreen();
        void ShowTermsOfServiceScreen();
    }

    public class SignupHUDView : MonoBehaviour, ISignupHUDView
    {
        private const int MIN_NAME_LENGTH = 1;
        private const int MAX_NAME_LENGTH = 15;

        public event ISignupHUDView.NameScreenDone OnNameScreenNext;
        public event Action OnEditAvatar;
        public event Action OnTermsOfServiceAgreed;
        public event Action OnTermsOfServiceBack;

        [SerializeField] internal RectTransform nameAndEmailPanel;
        [SerializeField] internal Button nameAndEmailNextButton;
        [SerializeField] internal TMP_InputField nameInputField;
        [SerializeField] internal TextMeshProUGUI nameCurrentCharacters;
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal RectTransform termsOfServicePanel;
        [SerializeField] internal Button editAvatarButton;
        [SerializeField] internal ScrollRect termsOfServiceScrollView;
        [SerializeField] internal Button termsOfServiceBackButton;
        [SerializeField] internal Button termsOfServiceAgreeButton;
        [SerializeField] internal RawImage avatarPic;

        private void Awake()
        {
            UserProfile userProfile = UserProfile.GetOwnUserProfile();
            OnFaceSnapshotReady(userProfile.faceSnapshot);
            userProfile.OnFaceSnapshotReadyEvent += OnFaceSnapshotReady;

            nameAndEmailNextButton.interactable = false;
            nameCurrentCharacters.text = $"{0} / {MAX_NAME_LENGTH}";
            nameInputField.characterLimit = MAX_NAME_LENGTH;
            nameInputField.onValueChanged.AddListener((text) =>
            {
                UpdateNameAndEmailNextButton();
                nameCurrentCharacters.text = $"{text.Length} / {MAX_NAME_LENGTH}";
            });
            nameInputField.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
            emailInputField.onValueChanged.AddListener((text) =>
            {
                UpdateNameAndEmailNextButton();
            });
            nameAndEmailNextButton.onClick.AddListener(() => OnNameScreenNext?.Invoke(nameInputField.text, emailInputField.text));

            termsOfServiceScrollView.onValueChanged.AddListener(pos =>
            {
                if (pos.y <= 0.1f)
                    termsOfServiceAgreeButton.interactable = true;
            });
            termsOfServiceAgreeButton.interactable = false;
            termsOfServiceBackButton.onClick.AddListener(() => OnTermsOfServiceBack?.Invoke());
            termsOfServiceAgreeButton.onClick.AddListener(() => OnTermsOfServiceAgreed?.Invoke());
            editAvatarButton.onClick.AddListener(() => OnEditAvatar?.Invoke());
        }

        private void OnFaceSnapshotReady(Texture2D texture) { avatarPic.texture = texture; }

        public static SignupHUDView CreateView()
        {
            SignupHUDView view = Instantiate(Resources.Load<GameObject>("SignupHUD")).GetComponent<SignupHUDView>();
            view.gameObject.name = "_SignupHUD";
            return view;
        }

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

        internal void UpdateNameAndEmailNextButton()
        {
            string name = nameInputField.text;
            string email = emailInputField.text;

            nameAndEmailNextButton.interactable = name.Length >= MIN_NAME_LENGTH && (email.Length == 0 || IsValidEmail(email));
        }

        private bool IsValidEmail(string email)
        {
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
    }
}