using DCL.Social.Chat;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Chat
{
    public class CreateChannelWindowComponentView : BaseComponentView, ICreateChannelWindowView
    {
        [SerializeField] internal Button createButton;
        [SerializeField] internal Button[] closeButtons;
        [SerializeField] internal Button joinButton;
        [SerializeField] internal TMP_InputField channelNameInput;
        [SerializeField] internal GameObject channelExistsContainer;
        [SerializeField] internal GameObject channelExistsWithJoinOptionContainer;
        [SerializeField] internal GameObject specialCharactersErrorContainer;
        [SerializeField] internal GameObject tooShortErrorContainer;
        [SerializeField] internal GameObject exceededLimitErrorContainer;
        [SerializeField] internal GameObject unknownErrorContainer;
        [SerializeField] internal TMP_Text channelNameLengthLabel;
        [SerializeField] internal GameObject inputFieldErrorBevel;
        [SerializeField] internal Color errorColor;

        private Color lengthLabelOriginalColor;

        public event Action<string> OnChannelNameUpdated;
        public event Action OnCreateSubmit;
        public event Action OnClose;
        public event Action OnJoinChannel;

        public override void Awake()
        {
            base.Awake();

            createButton.onClick.AddListener(() => OnCreateSubmit?.Invoke());
            channelNameInput.onValueChanged.AddListener(text =>
            {
                channelNameLengthLabel.text = $"{Mathf.Min(20, text.Length)}/20";
                OnChannelNameUpdated?.Invoke(text);
            });
            channelNameInput.onSubmit.AddListener(text =>
            {
                OnCreateSubmit?.Invoke();
            });
            foreach (var button in closeButtons)
                button.onClick.AddListener(() => OnClose?.Invoke());
            joinButton.onClick.AddListener(() => OnJoinChannel?.Invoke());
            lengthLabelOriginalColor = channelNameLengthLabel.color;
        }

        public override void RefreshControl()
        {
            throw new NotImplementedException();
        }

        public RectTransform Transform => (RectTransform) transform;

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void ShowChannelExistsError(bool showJoinChannelOption)
        {
            channelExistsContainer.SetActive(!showJoinChannelOption);
            channelExistsWithJoinOptionContainer.SetActive(showJoinChannelOption);
            inputFieldErrorBevel.SetActive(true);
            specialCharactersErrorContainer.SetActive(false);
            tooShortErrorContainer.SetActive(false);
            exceededLimitErrorContainer.SetActive(false);
            unknownErrorContainer.SetActive(false);
            channelNameLengthLabel.color = errorColor;
        }

        public void ClearError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            inputFieldErrorBevel.SetActive(false);
            specialCharactersErrorContainer.SetActive(false);
            tooShortErrorContainer.SetActive(false);
            exceededLimitErrorContainer.SetActive(false);
            unknownErrorContainer.SetActive(false);
            channelNameLengthLabel.color = lengthLabelOriginalColor;
        }

        public void DisableCreateButton() => createButton.interactable = false;

        public void EnableCreateButton() => createButton.interactable = true;

        public void ClearInputText() => channelNameInput.text = "";

        public void FocusInputField() => channelNameInput.Select();

        public void ShowWrongFormatError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            inputFieldErrorBevel.SetActive(true);
            specialCharactersErrorContainer.SetActive(true);
            tooShortErrorContainer.SetActive(false);
            exceededLimitErrorContainer.SetActive(false);
            unknownErrorContainer.SetActive(false);
            channelNameLengthLabel.color = errorColor;
        }

        public void ShowTooShortError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            inputFieldErrorBevel.SetActive(true);
            specialCharactersErrorContainer.SetActive(false);
            tooShortErrorContainer.SetActive(true);
            exceededLimitErrorContainer.SetActive(false);
            unknownErrorContainer.SetActive(false);
            channelNameLengthLabel.color = errorColor;
        }

        public void ShowChannelsExceededError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            inputFieldErrorBevel.SetActive(true);
            specialCharactersErrorContainer.SetActive(false);
            tooShortErrorContainer.SetActive(false);
            exceededLimitErrorContainer.SetActive(true);
            unknownErrorContainer.SetActive(false);
            channelNameLengthLabel.color = errorColor;
        }

        public void ShowUnknownError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            inputFieldErrorBevel.SetActive(true);
            specialCharactersErrorContainer.SetActive(false);
            tooShortErrorContainer.SetActive(false);
            exceededLimitErrorContainer.SetActive(false);
            unknownErrorContainer.SetActive(true);
            channelNameLengthLabel.color = errorColor;
        }
    }
}
