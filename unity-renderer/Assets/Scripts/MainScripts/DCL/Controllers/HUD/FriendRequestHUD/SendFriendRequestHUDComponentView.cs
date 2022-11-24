using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDComponentView : BaseComponentView, ISendFriendRequestHUDView
    {
        [SerializeField] private GameObject defaultContainer;
        [SerializeField] private GameObject pendingToSendContainer;
        [SerializeField] private GameObject failedContainer;
        [SerializeField] private GameObject successContainer;
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text pendingStateLabel;
        [SerializeField] private TMP_Text successStateLabel;
        [SerializeField] private Button[] cancelButtons;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private TMP_InputField messageBodyInput;
        [SerializeField] private ImageComponentView profileImage;

        private readonly Model model = new Model();

        public event Action<string> OnMessageBodyChanged;
        public event Action OnSend;
        public event Action OnCancel;

        public static SendFriendRequestHUDComponentView Create()
        {
            return Instantiate(
                Resources.Load<SendFriendRequestHUDComponentView>("FriendRequests/SendFriendRequestHUD"));
        }

        public override void Awake()
        {
            base.Awake();

            foreach (var button in cancelButtons)
                button.onClick.AddListener(() => OnCancel?.Invoke());

            messageBodyInput.onValueChanged.AddListener(s => OnMessageBodyChanged?.Invoke(s));
            sendButton.onClick.AddListener(() => OnSend?.Invoke());
            retryButton.onClick.AddListener(() => OnSend?.Invoke());
        }

        public override void RefreshControl()
        {
            defaultContainer.SetActive(model.state == Model.State.Default);
            pendingToSendContainer.SetActive(model.state == Model.State.Pending);
            failedContainer.SetActive(model.state == Model.State.Failed);
            successContainer.SetActive(model.state == Model.State.Success);
            profileImage.SetImage(model.profilePictureUrl);
            nameLabel.text = model.name;
            pendingStateLabel.text = $"Sending friend request to {model.name}";
            successStateLabel.text = $"Friend request sent to {model.name}";
        }

        public void Close() => gameObject.SetActive(false);

        public void Show()
        {
            base.Show(instant: true);
            gameObject.SetActive(true);
            RefreshControl();
        }

        public void SetName(string name)
        {
            model.name = name;
            RefreshControl();
        }

        public void SetProfilePicture(string imageUrl)
        {
            model.profilePictureUrl = imageUrl;
            RefreshControl();
        }

        public void ShowPendingToSend()
        {
            model.state = Model.State.Pending;
            RefreshControl();
        }

        public void ShowSendSuccess()
        {
            model.state = Model.State.Success;
            RefreshControl();
        }

        public void ShowSendFailed()
        {
            model.state = Model.State.Failed;
            RefreshControl();
        }

        private class Model
        {
            public string name;
            public State state;
            public string profilePictureUrl;

            public enum State
            {
                Default,
                Pending,
                Failed,
                Success
            }
        }
    }
}