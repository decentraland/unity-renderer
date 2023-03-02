using DCL.Helpers;
using System;
using System.Globalization;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class SentFriendRequestHUDComponentView : BaseComponentView<SentFriendRequestHUDViewModel>, ISentFriendRequestHUDView
    {
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal Button[] closeButtons;
        [SerializeField] internal GameObject cancelButtonsContainer;
        [SerializeField] internal Button cancelButton;
        [SerializeField] internal Button tryCancelButton;
        [SerializeField] internal GameObject pendingToCancelContainer;
        [SerializeField] internal Button openPassportButton;
        [SerializeField] internal TMP_InputField messageBodyInput;
        [SerializeField] internal ImageComponentView profileImage;
        [SerializeField] internal ImageComponentView senderProfileImage;
        [SerializeField] internal TMP_Text dateLabel;
        [SerializeField] internal GameObject bodyMessageContainer;
        [SerializeField] internal ShowHideAnimator confirmationToast;
        [SerializeField] internal Button rejectOperationButton;
        [SerializeField] internal Canvas currentCanvas;

        private ILazyTextureObserver lastRecipientProfilePictureObserver;
        private ILazyTextureObserver lastSenderProfilePictureObserver;

        public event Action OnCancel;
        public event Action OnClose;
        public event Action OnOpenProfile;

        public static SentFriendRequestHUDComponentView Create() =>
            Instantiate(
                Resources.Load<SentFriendRequestHUDComponentView>("FriendRequests/SentFriendRequestHUD"));

        public override void Awake()
        {
            base.Awake();

            foreach (var button in closeButtons)
                button.onClick.AddListener(() => OnClose?.Invoke());

            cancelButton.onClick.AddListener(() => OnCancel?.Invoke());

            tryCancelButton.onClick.AddListener(() =>
            {
                ShowConfirmationToast();
                SwitchToCancelButton();
            });

            rejectOperationButton.onClick.AddListener(() =>
            {
                HideConfirmationToast();
                SwitchToTryCancelButton();
            });

            openPassportButton.onClick.AddListener(() => OnOpenProfile?.Invoke());
        }

        public override void Dispose()
        {
            base.Dispose();
            model.RecipientProfilePictureObserver?.RemoveListener(profileImage.SetImage);
        }

        public override void RefreshControl()
        {
            cancelButton.interactable = model.State != SentFriendRequestHUDViewModel.LayoutState.Pending;
            tryCancelButton.interactable = model.State != SentFriendRequestHUDViewModel.LayoutState.Pending;
            cancelButtonsContainer.SetActive(model.State != SentFriendRequestHUDViewModel.LayoutState.Pending);
            pendingToCancelContainer.SetActive(model.State == SentFriendRequestHUDViewModel.LayoutState.Pending);

            foreach (Button button in closeButtons)
                button.interactable = model.State != SentFriendRequestHUDViewModel.LayoutState.Pending;

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

        public void Close() => base.Hide();

        public void SetSenderProfilePicture(ILazyTextureObserver textureObserver)
        {
            model.SenderProfilePictureObserver = textureObserver;
            RefreshControl();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            HideConfirmationToast();
            SwitchToTryCancelButton();
            model.State = SentFriendRequestHUDViewModel.LayoutState.Default;
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
            model.State = SentFriendRequestHUDViewModel.LayoutState.Pending;
            HideConfirmationToast();
            RefreshControl();
        }

        public void SetBodyMessage(string messageBody)
        {
            model.BodyMessage = messageBody;
            RefreshControl();
        }

        public void SetTimestamp(DateTime date)
        {
            dateLabel.text = date.Date.ToString("MMM dd", new CultureInfo("en-US")).ToUpper();
        }

        public void SetSortingOrder(int sortingOrder) => currentCanvas.sortingOrder = sortingOrder;

        private void ShowConfirmationToast()
        {
            rejectOperationButton.gameObject.SetActive(true);
            confirmationToast.Show();
        }

        private void HideConfirmationToast()
        {
            rejectOperationButton.gameObject.SetActive(false);
            confirmationToast.Hide();
        }

        private void SwitchToCancelButton()
        {
            tryCancelButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);
        }

        private void SwitchToTryCancelButton()
        {
            tryCancelButton.gameObject.SetActive(true);
            cancelButton.gameObject.SetActive(false);
        }
    }
}
