using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class CreateChannelWindowComponentView : MonoBehaviour, ICreateChannelWindowView
    {
        [SerializeField] private Button createButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_InputField channelNameInput;
        [SerializeField] private GameObject channelExistsContainer;
        [SerializeField] private GameObject channelExistsWithJoinOptionContainer;
        [SerializeField] private TMP_Text genericErrorLabel;

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
            channelNameInput.onValueChanged.AddListener(text => OnChannelNameUpdated?.Invoke(text));
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            cancelButton.onClick.AddListener(() => OnClose?.Invoke());
            joinButton.onClick.AddListener(() => OnOpenChannel?.Invoke());
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