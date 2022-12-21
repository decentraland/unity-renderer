using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDComponentView : BaseComponentView, ISendFriendRequestHUDView
    {
        [SerializeField] internal GameObject defaultContainer;
        [SerializeField] internal GameObject failedContainer;
        [SerializeField] internal GameObject pendingToSendContainer;
        [SerializeField] internal GameObject successContainer;
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal TMP_Text successStateLabel;
        [SerializeField] internal Button[] cancelButtons;
        [SerializeField] internal Button sendButton;
        [SerializeField] internal Button retryButton;
        [SerializeField] internal TMP_InputField messageBodyInput;
        [SerializeField] internal TMP_Text messageBodyLengthLabel;
        [SerializeField] internal ImageComponentView profileImage;

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

            messageBodyInput.onValueChanged.AddListener(s =>
            {
                messageBodyLengthLabel.text = $"{s.Length}/140";
                OnMessageBodyChanged?.Invoke(s);
            });
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
            defaultContainer.SetActive(model.State is Model.LayoutState.Default or Model.LayoutState.Pending);
            failedContainer.SetActive(model.State == Model.LayoutState.Failed);
            successContainer.SetActive(model.State == Model.LayoutState.Success);
            pendingToSendContainer.SetActive(model.State == Model.LayoutState.Pending);
            sendButton.gameObject.SetActive(model.State != Model.LayoutState.Pending);
            sendButton.interactable = model.State != Model.LayoutState.Pending;

            foreach (Button cancelButton in cancelButtons)
                cancelButton.interactable = model.State != Model.LayoutState.Pending;

            nameLabel.text = model.Name;
            successStateLabel.text = $"Friend request sent to {model.Name}";
            messageBodyLengthLabel.text = $"{messageBodyInput.text.Length}/140";

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
