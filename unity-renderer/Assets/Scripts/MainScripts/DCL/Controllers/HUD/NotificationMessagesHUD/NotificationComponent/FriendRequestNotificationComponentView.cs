using DCL.Helpers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.Notifications
{
    public class FriendRequestNotificationComponentView :
        BaseComponentView,
        IComponentModelConfig<FriendRequestNotificationComponentModel>,
        IShowableNotificationView
    {
        [Header("Prefab References")]
        [SerializeField] internal Button button;
        [SerializeField] internal TMP_Text notificationMessage;
        [SerializeField] internal TMP_Text notificationHeader;
        [SerializeField] internal TMP_Text notificationSender;
        [SerializeField] internal TMP_Text notificationTimestamp;
        [SerializeField] internal ImageComponentView image;
        [SerializeField] internal RectTransform backgroundTransform;
        [SerializeField] internal RectTransform messageContainerTransform;
        [SerializeField] internal GameObject receivedRequestMark;
        [SerializeField] internal GameObject acceptedRequestMark;
        [SerializeField] internal bool shouldAnimateFocus = true;
        [SerializeField] internal RectTransform header;
        [SerializeField] internal RectTransform content;

        [Header("Configuration")]
        [SerializeField] internal FriendRequestNotificationComponentModel model;

        public event Action<string> OnClickedNotification;
        private float startingXPosition;

        public void Configure(FriendRequestNotificationComponentModel NewModel)
        {
            model = NewModel;
            RefreshControl();
        }

        public override void Awake()
        {
            base.Awake();
            button?.onClick.AddListener(() => OnClickedNotification?.Invoke(model.UserId));
            startingXPosition = messageContainerTransform.anchoredPosition.x;
            RefreshControl();
        }

        public override void Show(bool Instant = false)
        {
            showHideAnimator.animSpeedFactor = 0.7f;
            base.Show(Instant);
            ForceUIRefresh();
        }

        public override void Hide(bool Instant = false)
        {
            showHideAnimator.animSpeedFactor = 0.05f;
            base.Hide(Instant);
        }

        public override void OnFocus()
        {
            base.OnFocus();
            if (shouldAnimateFocus)
                messageContainerTransform.anchoredPosition = new Vector2(startingXPosition + 5, messageContainerTransform.anchoredPosition.y);
        }

        public override void OnLoseFocus()
        {
            base.OnLoseFocus();
            if (shouldAnimateFocus)
                messageContainerTransform.anchoredPosition = new Vector2(startingXPosition, messageContainerTransform.anchoredPosition.y);
        }

        public override void RefreshControl()
        {
            if (model == null)
                return;

            SetUser(model.UserId, model.UserName);
            SetMessage(model.Message);
            SetTimestamp(model.Time);
            SetHeader(model.Header);
            SetImage(model.ImageUri);
            SetIsAccepted(model.IsAccepted);
        }

        public void SetUser(string UserId, string UserName)
        {
            model.UserId = UserId;
            model.UserName = UserName;

            notificationSender.text = UserName;
            ForceUIRefresh();
        }

        public void SetMessage(string Message)
        {
            model.Message = Message;
            notificationMessage.text = Message;
            ForceUIRefresh();
        }

        public void SetTimestamp(string Timestamp)
        {
            model.Time = Timestamp;
            notificationTimestamp.text = Timestamp;
            ForceUIRefresh();
        }

        public void SetHeader(string Header)
        {
            model.Header = Header;
            notificationHeader.text = Header;
            ForceUIRefresh();
        }

        public void SetImage(string ImageUri)
        {
            model.ImageUri = ImageUri;
            image.SetImage(ImageUri);
        }

        public void SetIsAccepted(bool IsAccepted)
        {
            model.IsAccepted = IsAccepted;

            if (receivedRequestMark != null)
                receivedRequestMark.SetActive(!IsAccepted);

            if (acceptedRequestMark != null)
                acceptedRequestMark.SetActive(IsAccepted);
        }

        private void ForceUIRefresh()
        {
            Utils.ForceRebuildLayoutImmediate(backgroundTransform);
        }
    }
}
