using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using DCL.Helpers;
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

        [Header("Name and Email Screen")]
        [SerializeField] internal RectTransform nameAndEmailPanel;

        [SerializeField] internal Button nameAndEmailNextButton;
        [SerializeField] internal TMP_InputField nameInputField;
        [SerializeField] internal GameObject nameInputFieldFullOrInvalid;
        [SerializeField] internal GameObject nameInputInvalidLabel;
        [SerializeField] internal TextMeshProUGUI nameCurrentCharacters;
        [SerializeField] internal GameObject emailInputFieldInvalid;
        [SerializeField] internal TMP_InputField emailInputField;
        [SerializeField] internal GameObject emailInputInvalidLabel;
        [SerializeField] internal Color colorForCharLimit;

        [Header("Terms of Service Screen")]
        [SerializeField] internal RectTransform termsOfServicePanel;

        [SerializeField] internal Button editAvatarButton;
        [SerializeField] internal ScrollRect termsOfServiceScrollView;
        [SerializeField] internal Button termsOfServiceBackButton;
        [SerializeField] internal Button termsOfServiceAgreeButton;
        [SerializeField] internal RawImage avatarPic;

        private ILazyTextureObserver snapshotTextureObserver;

        private void Awake()
        {
            InitNameAndEmailScreen();
            InitTermsOfServicesScreen();
        }

        private void InitNameAndEmailScreen()
        {
            UserProfile userProfile = UserProfile.GetOwnUserProfile();
            snapshotTextureObserver = userProfile.snapshotObserver;
            snapshotTextureObserver.AddListener(OnFaceSnapshotReady);

            nameAndEmailNextButton.interactable = false;
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

            nameAndEmailNextButton.onClick.AddListener(() => OnNameScreenNext?.Invoke(nameInputField.text, emailInputField.text));
            editAvatarButton.onClick.AddListener(() => OnEditAvatar?.Invoke());
        }

        private void InitTermsOfServicesScreen()
        {
            termsOfServiceScrollView.onValueChanged.AddListener(pos =>
            {
                if (pos.y <= 0.1f)
                    termsOfServiceAgreeButton.interactable = true;
            });

            termsOfServiceAgreeButton.interactable = false;
            termsOfServiceBackButton.onClick.AddListener(() => OnTermsOfServiceBack?.Invoke());
            termsOfServiceAgreeButton.onClick.AddListener(() => OnTermsOfServiceAgreed?.Invoke());
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
            snapshotTextureObserver.RemoveListener(OnFaceSnapshotReady);

            if (this != null)
                Destroy(gameObject);
        }

        internal void UpdateNameAndEmailNextButton()
        {
            string name = nameInputField.text;
            string email = emailInputField.text;

            nameAndEmailNextButton.interactable = name.Length >= MIN_NAME_LENGTH && IsValidName(name) && IsValidEmail(email);
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

        private bool IsValidName(string name) { return Regex.IsMatch(name, "^[a-zA-Z0-9]*$"); }
    }
}