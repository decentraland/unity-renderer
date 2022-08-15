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

        public event Action<string> OnChannelNameUpdated;
        public event Action OnCreateSubmit;
        public event Action OnClose;
        public event Action OnOpenChannel;

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
            foreach (var button in closeButtons)
                button.onClick.AddListener(() => OnClose?.Invoke());
            joinButton.onClick.AddListener(() => OnOpenChannel?.Invoke());
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
            genericErrorLabel.text = message;
        }

        public void ShowChannelExistsError(bool showJoinChannelOption)
        {
            channelExistsContainer.SetActive(!showJoinChannelOption);
            channelExistsWithJoinOptionContainer.SetActive(showJoinChannelOption);
            genericErrorLabel.gameObject.SetActive(false);
        }

        public void ClearError()
        {
            channelExistsContainer.SetActive(false);
            channelExistsWithJoinOptionContainer.SetActive(false);
            genericErrorLabel.gameObject.SetActive(false);
        }

        public void DisableCreateButton() => createButton.interactable = false;

        public void EnableCreateButton() => createButton.interactable = true;

        public void ClearInputText() => channelNameInput.text = "";
    }
}