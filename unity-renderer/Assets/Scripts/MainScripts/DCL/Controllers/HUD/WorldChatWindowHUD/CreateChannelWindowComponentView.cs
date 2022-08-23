using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class CreateChannelWindowComponentView : BaseComponentView, ICreateChannelWindowView
    {
        [SerializeField] internal Button createButton;
        [SerializeField] internal Button[] closeButtons;
        [SerializeField] internal Button joinButton;
        [SerializeField] internal TMP_InputField channelNameInput;
        [SerializeField] internal GameObject channelExistsContainer;
        [SerializeField] internal GameObject channelExistsWithJoinOptionContainer;
        [SerializeField] internal TMP_Text genericErrorLabel;
        [SerializeField] internal TMP_Text channelNameLengthLabel;
        [SerializeField] internal GameObject inputFieldErrorBevel;
        [SerializeField] private Color errorColor;
        
        private Color lengthLabelOriginalColor;

        public event Action<string> OnChannelNameUpdated;
        public event Action OnCreateSubmit;
        public event Action OnClose;
        public event Action OnJoinChannel;

        public static CreateChannelWindowComponentView Create()
        {
            return Instantiate(Resources.Load<CreateChannelWindowComponentView>("SocialBarV1/ChannelCreationHUD"));
        }

        private void Awake()
        {
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

        public void ShowError(string message)
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            genericErrorLabel.gameObject.SetActive(true);
            inputFieldErrorBevel.SetActive(true);
            channelNameLengthLabel.color = errorColor;
            genericErrorLabel.text = message;
        }

        public void ShowChannelExistsError(bool showJoinChannelOption)
        {
            channelExistsContainer.SetActive(!showJoinChannelOption);
            channelExistsWithJoinOptionContainer.SetActive(showJoinChannelOption);
            inputFieldErrorBevel.SetActive(true);
            genericErrorLabel.gameObject.SetActive(false);
            channelNameLengthLabel.color = errorColor;
        }

        public void ClearError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            genericErrorLabel.gameObject.SetActive(false);
            inputFieldErrorBevel.SetActive(false);
            channelNameLengthLabel.color = lengthLabelOriginalColor;
        }

        public void DisableCreateButton() => createButton.interactable = false;

        public void EnableCreateButton() => createButton.interactable = true;

        public void ClearInputText() => channelNameInput.text = "";
        
        public void FocusInputField() => channelNameInput.Select();
    }
}