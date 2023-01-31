using DCL.Helpers;
using System;
using TMPro;
using UIComponents.Scripts.Components;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Friends
{
    public class SendFriendRequestHUDComponentView : BaseComponentView<SendFriendRequestHUDModel>, ISendFriendRequestHUDView, IFriendRequestHUDView
    {
        [SerializeField] internal ShowHideAnimator showHideAnimatorForDefaultState;
        [SerializeField] internal GameObject pendingToSendContainer;
        [SerializeField] internal ShowHideAnimator showHideAnimatorForSuccessState;
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal TMP_Text successStateLabel;
        [SerializeField] internal Button[] cancelButtons;
        [SerializeField] internal Button sendButton;
        [SerializeField] internal TMP_InputField messageBodyInput;
        [SerializeField] internal TMP_Text messageBodyLengthLabel;
        [SerializeField] internal Image messageBodyMaxLimitMark;
        [SerializeField] internal ImageComponentView profileImage;
        [SerializeField] internal Color bodyMaxLimitColor;

        private ILazyTextureObserver lastProfilePictureObserver;
        private Color messageBodyLengthOriginalColor;

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
                RefreshMessageBodyValidations(s);
                OnMessageBodyChanged?.Invoke(s);
            });
            sendButton.onClick.AddListener(() => OnSend?.Invoke());

            messageBodyLengthOriginalColor = messageBodyLengthLabel.color;
        }

        public override void Dispose()
        {
            base.Dispose();
            model.ProfilePictureObserver?.RemoveListener(profileImage.SetImage);
        }

        public override void RefreshControl()
        {
            sendButton.gameObject.SetActive(model.State != SendFriendRequestHUDModel.LayoutState.Pending);
            sendButton.interactable = model.State != SendFriendRequestHUDModel.LayoutState.Pending;

            foreach (Button cancelButton in cancelButtons)
                cancelButton.interactable = model.State != SendFriendRequestHUDModel.LayoutState.Pending;

            nameLabel.text = model.Name;
            successStateLabel.text = $"Friend request sent to {model.Name}";
            messageBodyLengthLabel.text = $"{messageBodyInput.text.Length}/{messageBodyInput.characterLimit}";
            RefreshMessageBodyValidations(messageBodyInput.text);

            // the load of the profile picture gets stuck if the same listener is registered many times
            if (lastProfilePictureObserver != model.ProfilePictureObserver)
            {
                lastProfilePictureObserver?.RemoveListener(profileImage.SetImage);
                model.ProfilePictureObserver?.AddListener(profileImage.SetImage);
                lastProfilePictureObserver = model.ProfilePictureObserver;
            }
        }

        public void Close() => base.Hide();

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            model.State = SendFriendRequestHUDModel.LayoutState.Default;
            showHideAnimatorForDefaultState.Show(true);
            pendingToSendContainer.SetActive(false);
            showHideAnimatorForSuccessState.Hide(true);
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
            model.State = SendFriendRequestHUDModel.LayoutState.Pending;
            showHideAnimatorForDefaultState.Show(true);
            pendingToSendContainer.SetActive(true);
            showHideAnimatorForSuccessState.Hide(true);
            RefreshControl();
        }

        public void ShowSendSuccess()
        {
            model.State = SendFriendRequestHUDModel.LayoutState.Success;
            showHideAnimatorForDefaultState.Hide(true);
            pendingToSendContainer.SetActive(false);
            showHideAnimatorForSuccessState.Show();
            RefreshControl();
        }

        public void ClearInputField() => messageBodyInput.text = "";

        private void RefreshMessageBodyValidations(string message)
        {
            messageBodyLengthLabel.text = $"{message.Length}/{messageBodyInput.characterLimit}";
            messageBodyMaxLimitMark.gameObject.SetActive(message.Length >= messageBodyInput.characterLimit);
            messageBodyMaxLimitMark.color = messageBodyMaxLimitMark.gameObject.activeSelf ? bodyMaxLimitColor : messageBodyLengthOriginalColor;
            messageBodyLengthLabel.color = messageBodyMaxLimitMark.gameObject.activeSelf ? bodyMaxLimitColor : messageBodyLengthOriginalColor;
        }
    }
}
