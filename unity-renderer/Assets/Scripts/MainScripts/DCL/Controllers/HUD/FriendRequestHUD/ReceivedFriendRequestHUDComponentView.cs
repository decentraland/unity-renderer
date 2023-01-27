using System;
using System.Globalization;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class ReceivedFriendRequestHUDComponentView : BaseComponentView<ReceivedFriendRequestHUDModel>,
        IReceivedFriendRequestHUDView, IFriendRequestHUDView
    {
        [SerializeField] internal GameObject bodyMessageContainer;
        [SerializeField] internal TMP_InputField bodyMessageInput;
        [SerializeField] internal TMP_Text dateLabel;
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal ImageComponentView otherProfileImageInConfirmDefaultState;
        [SerializeField] internal ImageComponentView otherProfileImageInConfirmSuccessState;
        [SerializeField] internal ImageComponentView otherProfileImageInConfirmRejectState;
        [SerializeField] internal ImageComponentView ownProfileImageInConfirmSuccessState;
        [SerializeField] internal Button[] closeButtons;
        [SerializeField] internal Button openPassportButton;
        [SerializeField] internal Button rejectButton;
        [SerializeField] internal GameObject pendingToRejectContainer;
        [SerializeField] internal Button confirmButton;
        [SerializeField] internal GameObject pendingToConfirmContainer;
        [SerializeField] internal ShowHideAnimator showHideAnimatorForDefaultState;
        [SerializeField] internal ShowHideAnimator showHideAnimatorForConfirmSuccessState;
        [SerializeField] internal ShowHideAnimator showHideAnimatorForRejectSuccessState;
        [SerializeField] internal Button[] buttonsToDisableOnPendingState;
        [SerializeField] internal TMP_Text rejectSuccessLabel;
        [SerializeField] internal TMP_Text confirmSuccessLabel;
        [SerializeField] internal Canvas currentCanvas;

        public event Action OnClose;
        public event Action OnOpenProfile;
        public event Action OnRejectFriendRequest;
        public event Action OnConfirmFriendRequest;

        private bool lastTryWasConfirm;

        public static ReceivedFriendRequestHUDComponentView Create() =>
            Instantiate(
                Resources.Load<ReceivedFriendRequestHUDComponentView>("FriendRequests/ReceivedFriendRequestHUD"));

        public override void Awake()
        {
            base.Awake();

            foreach (var button in closeButtons)
                button.onClick.AddListener(() => OnClose?.Invoke());

            openPassportButton.onClick.AddListener(() => OnOpenProfile?.Invoke());

            rejectButton.onClick.AddListener(() =>
            {
                lastTryWasConfirm = false;
                OnRejectFriendRequest?.Invoke();
            });

            confirmButton.onClick.AddListener(() =>
            {
                lastTryWasConfirm = true;
                OnConfirmFriendRequest?.Invoke();
            });
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var button in closeButtons)
                button.onClick.RemoveAllListeners();

            openPassportButton.onClick.RemoveAllListeners();
            rejectButton.onClick.RemoveAllListeners();
            confirmButton.onClick.RemoveAllListeners();
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetBodyMessage(model.BodyMessage);
            SetTimestamp(model.RequestDate);
            SetSenderName(model.UserName);
            SetSenderProfilePicture(model.UserProfilePictureUri);
            SetRecipientProfilePicture(model.OwnProfilePictureUri);
            SetState(model.State);
        }

        public void SetBodyMessage(string messageBody)
        {
            model.BodyMessage = messageBody;
            bodyMessageInput.text = !string.IsNullOrEmpty(model.UserName)
                ? $"<b>{model.UserName}:</b> {messageBody}"
                : messageBody;
            bodyMessageContainer.SetActive(!string.IsNullOrEmpty(messageBody));
        }

        public void SetTimestamp(DateTime timestamp)
        {
            model.RequestDate = timestamp;
            dateLabel.text = timestamp.Date.ToString("MMM dd", new CultureInfo("en-US")).ToUpper();
        }

        public void SetSenderName(string userName)
        {
            model.UserName = userName;
            nameLabel.text = userName;
            rejectSuccessLabel.text = $"{userName} request rejected!";
            confirmSuccessLabel.text = $"{userName} and you are friends now!";

            if (!string.IsNullOrEmpty(model.BodyMessage))
                bodyMessageInput.text = $"<b>{model.UserName}:</b> {model.BodyMessage}";
        }

        public void SetSenderProfilePicture(string uri)
        {
            model.UserProfilePictureUri = uri;
            otherProfileImageInConfirmDefaultState.SetImage(uri);
            otherProfileImageInConfirmSuccessState.SetImage(uri);
            otherProfileImageInConfirmRejectState.SetImage(uri);
        }

        public void SetRecipientProfilePicture(string uri)
        {
            model.OwnProfilePictureUri = uri;
            ownProfileImageInConfirmSuccessState.SetImage(uri);
        }

        public void SetState(ReceivedFriendRequestHUDModel.LayoutState state)
        {
            model.State = state;

            switch (state)
            {
                case ReceivedFriendRequestHUDModel.LayoutState.Default:
                    SetDefaultState();
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.Pending:
                    SetPendingState();
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.ConfirmSuccess:
                    SetConfirmSuccessState();
                    break;
                case ReceivedFriendRequestHUDModel.LayoutState.RejectSuccess:
                    SetRejectSuccessState();
                    break;
                default:
                    break;
            }
        }

        public override void Show(bool instant = false)
        {
            SetState(ReceivedFriendRequestHUDModel.LayoutState.Default);
            base.Show(instant);
        }

        public void Close() => base.Hide();

        public void SetSortingOrder(int sortingOrder) => currentCanvas.sortingOrder = sortingOrder;

        private void SetDefaultState()
        {
            showHideAnimatorForDefaultState.Show();
            showHideAnimatorForRejectSuccessState.Hide(true);
            showHideAnimatorForConfirmSuccessState.Hide(true);

            confirmButton.gameObject.SetActive(true);
            pendingToConfirmContainer.SetActive(false);
            rejectButton.gameObject.SetActive(true);
            pendingToRejectContainer.SetActive(false);

            foreach (var button in buttonsToDisableOnPendingState)
                button.interactable = true;
        }

        private void SetPendingState()
        {
            confirmButton.gameObject.SetActive(!lastTryWasConfirm);
            pendingToConfirmContainer.SetActive(lastTryWasConfirm);
            rejectButton.gameObject.SetActive(lastTryWasConfirm);
            pendingToRejectContainer.SetActive(!lastTryWasConfirm);

            foreach (var button in buttonsToDisableOnPendingState)
                button.interactable = false;
        }

        private void SetConfirmSuccessState()
        {
            showHideAnimatorForDefaultState.Hide();
            showHideAnimatorForRejectSuccessState.Hide(true);
            showHideAnimatorForConfirmSuccessState.Show();
        }

        private void SetRejectSuccessState()
        {
            showHideAnimatorForDefaultState.Hide();
            showHideAnimatorForRejectSuccessState.Show();
            showHideAnimatorForConfirmSuccessState.Hide(true);
        }
    }
}
