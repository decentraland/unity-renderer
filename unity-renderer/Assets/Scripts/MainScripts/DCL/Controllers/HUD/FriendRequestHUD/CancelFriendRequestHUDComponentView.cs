using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class CancelFriendRequestHUDComponentView : BaseComponentView, ICancelFriendRequestHUDView
    {
        [SerializeField] internal GameObject defaultContainer;
        [SerializeField] internal GameObject failedContainer;
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal Button[] closeButtons;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button retryButton;
        [SerializeField] internal Button openPassportButton;
        [SerializeField] internal TMP_InputField messageBodyInput;
        [SerializeField] internal ImageComponentView profileImage;
        [SerializeField] internal ImageComponentView senderProfileImage;
        [SerializeField] internal TMP_Text dateLabel;
        [SerializeField] internal GameObject bodyMessageContainer;

        private readonly Model model = new Model();
        private ILazyTextureObserver lastRecipientProfilePictureObserver;
        private ILazyTextureObserver lastSenderProfilePictureObserver;

        public event Action OnCancel;
        public event Action OnClose;
        public event Action OnOpenProfile;

        public static CancelFriendRequestHUDComponentView Create() =>
            Instantiate(
                Resources.Load<CancelFriendRequestHUDComponentView>("FriendRequests/CancelFriendRequestHUD"));

        public override void Awake()
        {
            base.Awake();

            foreach (var button in closeButtons)
                button.onClick.AddListener(() => OnClose?.Invoke());

            cancelButton.onClick.AddListener(() => OnCancel?.Invoke());
            retryButton.onClick.AddListener(() => OnCancel?.Invoke());
            openPassportButton.onClick.AddListener(() => OnOpenProfile?.Invoke());
        }

        public override void Dispose()
        {
            base.Dispose();
            model.RecipientProfilePictureObserver?.RemoveListener(profileImage.SetImage);
        }

        public override void RefreshControl()
        {
            defaultContainer.SetActive(model.State is Model.LayoutState.Default or Model.LayoutState.Pending);
            failedContainer.SetActive(model.State == Model.LayoutState.Failed);
            cancelButton.interactable = model.State != Model.LayoutState.Pending;

            foreach (Button button in closeButtons)
                button.interactable = model.State != Model.LayoutState.Pending;

            nameLabel.text = model.Name;
            messageBodyInput.text = model.BodyMessage;
            bodyMessageContainer.SetActive(!string.IsNullOrEmpty(model.BodyMessage));

            // the load of the profile picture gets stuck if the same listener is registered many times
            if (lastRecipientProfilePictureObserver != model.RecipientProfilePictureObserver)
            {
                lastRecipientProfilePictureObserver?.RemoveListener(profileImage.SetImage);
                model.RecipientProfilePictureObserver?.AddListener(profileImage.SetImage);
                lastRecipientProfilePictureObserver = model.RecipientProfilePictureObserver;
            }

            // the load of the profile picture gets stuck if the same listener is registered many times
            if (lastSenderProfilePictureObserver != model.SenderProfilePictureObserver)
            {
                lastSenderProfilePictureObserver?.RemoveListener(profileImage.SetImage);
                model.SenderProfilePictureObserver?.AddListener(senderProfileImage.SetImage);
                lastSenderProfilePictureObserver = model.SenderProfilePictureObserver;
            }
        }

        public void Close()
        {
            base.Hide(instant: true);
            gameObject.SetActive(false);
        }

        public void SetSenderProfilePicture(ILazyTextureObserver textureObserver)
        {
            model.SenderProfilePictureObserver = textureObserver;
            RefreshControl();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            base.Show(instant: true);
            model.State = Model.LayoutState.Default;
            RefreshControl();
        }

        public void SetRecipientName(string name)
        {
            model.Name = name;
            RefreshControl();
        }

        public void SetRecipientProfilePicture(ILazyTextureObserver textureObserver)
        {
            model.RecipientProfilePictureObserver = textureObserver;
            RefreshControl();
        }

        public void ShowPendingToCancel()
        {
            model.State = Model.LayoutState.Pending;
            RefreshControl();
        }

        public void ShowCancelFailed()
        {
            model.State = Model.LayoutState.Failed;
            RefreshControl();
        }

        public void SetBodyMessage(string messageBody)
        {
            model.BodyMessage = messageBody;
            RefreshControl();
        }

        public void SetTimestamp(DateTime date)
        {
            dateLabel.text = date.Date.ToString("M");
        }

        private class Model
        {
            public string Name;
            public LayoutState State;
            public ILazyTextureObserver RecipientProfilePictureObserver;
            public ILazyTextureObserver SenderProfilePictureObserver;
            public string BodyMessage;

            public enum LayoutState
            {
                Default,
                Pending,
                Failed
            }
        }
    }
}
