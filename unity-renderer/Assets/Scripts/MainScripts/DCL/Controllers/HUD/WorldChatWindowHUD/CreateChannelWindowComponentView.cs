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
        [SerializeField] private TMP_InputField channelNameInput;

        public event Action<string> OnChannelNameUpdated;
        public event Action OnCreateSubmit;
        public event Action OnClose;

        private void Awake()
        {
            createButton.onClick.AddListener(() => OnCreateSubmit?.Invoke());
            channelNameInput.onValueChanged.AddListener(text => OnChannelNameUpdated?.Invoke(text));
            closeButton.onClick.AddListener(() => OnClose?.Invoke());
            cancelButton.onClick.AddListener(() => OnClose?.Invoke());
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void ShowError(string message)
        {
            throw new NotImplementedException();
        }
    }
}