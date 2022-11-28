using DCL.Helpers;
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
        private ILazyTextureObserver lastProfilePictureObserver;

        public event Action<string> OnMessageBodyChanged;
        public event Action OnSend;
        public event Action OnCancel;

        public static SendFriendRequestHUDComponentView Create() =>
            Instantiate(
                Resources.Load<SendFriendRequestHUDComponentView>("FriendRequests/SendFriendRequestHUD"));

        public override void Awake()
        {
            base.Awake();

            foreach (var button in cancelButtons)
                button.onClick.AddListener(() => OnCancel?.Invoke());

            messageBodyInput.onValueChanged.AddListener(s => OnMessageBodyChanged?.Invoke(s));
            sendButton.onClick.AddListener(() => OnSend?.Invoke());
            retryButton.onClick.AddListener(() => OnSend?.Invoke());
        }

        public override void Dispose()
        {
            base.Dispose();
            model.ProfilePictureObserver?.RemoveListener(profileImage.SetImage);
        }

        public override void RefreshControl()
        {
            defaultContainer.SetActive(model.State == Model.LayoutState.Default);
            pendingToSendContainer.SetActive(model.State == Model.LayoutState.Pending);
            failedContainer.SetActive(model.State == Model.LayoutState.Failed);
            successContainer.SetActive(model.State == Model.LayoutState.Success);
            nameLabel.text = model.Name;
            pendingStateLabel.text = $"Sending friend request to {model.Name}";
            successStateLabel.text = $"Friend request sent to {model.Name}";

            // the load of the profile picture gets stuck if the same listener is registered many times
            if (lastProfilePictureObserver != model.ProfilePictureObserver)
            {
                lastProfilePictureObserver?.RemoveListener(profileImage.SetImage);
                model.ProfilePictureObserver?.AddListener(profileImage.SetImage);
                lastProfilePictureObserver = model.ProfilePictureObserver;
            }
        }

        public void Close()
        {
            base.Hide(instant: true);
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            base.Show(instant: true);
            model.State = Model.LayoutState.Default;
            RefreshControl();
        }

        public void SetName(string name)
        {
            model.Name = name;
            RefreshControl();
        }

        public void SetProfilePicture(ILazyTextureObserver textureObserver)
        {
            model.ProfilePictureObserver = textureObserver;
            RefreshControl();
        }

        public void ShowPendingToSend()
        {
            model.State = Model.LayoutState.Pending;
            RefreshControl();
        }

        public void ShowSendSuccess()
        {
            model.State = Model.LayoutState.Success;
            RefreshControl();
        }

        public void ShowSendFailed()
        {
            model.State = Model.LayoutState.Failed;
            RefreshControl();
        }

        public void ClearInputField() => messageBodyInput.text = "";

        private class Model
        {
            public string Name;
            public LayoutState State;
            public ILazyTextureObserver ProfilePictureObserver;

            public enum LayoutState
            {
                Default,
                Pending,
                Failed,
                Success
            }
        }
    }
}
