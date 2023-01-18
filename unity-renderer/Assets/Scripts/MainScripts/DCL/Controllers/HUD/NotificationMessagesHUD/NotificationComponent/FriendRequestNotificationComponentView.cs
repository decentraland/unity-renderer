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
        [SerializeField] internal RectTransform backgroundTransform;
        [SerializeField] internal RectTransform messageContainerTransform;
        [SerializeField] internal GameObject receivedRequestMark;
        [SerializeField] internal GameObject acceptedRequestMark;
        [SerializeField] internal bool shouldAnimateFocus = true;
        [SerializeField] internal RectTransform header;
        [SerializeField] internal RectTransform content;

        [Header("Configuration")]
        [SerializeField] internal FriendRequestNotificationComponentModel model;

        public delegate void ClickedNotificationDelegate(string friendRequestId, string userId, bool isAccepted);
        public event ClickedNotificationDelegate OnClickedNotification;
        private float startingXPosition;

        public void Configure(FriendRequestNotificationComponentModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void Awake()
        {
            base.Awake();
            button?.onClick.AddListener(() => OnClickedNotification?.Invoke(model.FriendRequestId, model.UserId, model.IsAccepted));

            startingXPosition = messageContainerTransform.anchoredPosition.x;
            RefreshControl();
        }

        public override void Show(bool instant = false)
        {
            if (showHideAnimator != null)
                showHideAnimator.animSpeedFactor = 0.7f;

            base.Show(instant);
            ForceUIRefresh();
        }

        public override void Hide(bool instant = false)
        {
            if (showHideAnimator != null)
                showHideAnimator.animSpeedFactor = 0.05f;

            base.Hide(instant);
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

            SetFriendRequestId(model.FriendRequestId);
            SetUser(model.UserId, model.UserName);
            SetMessage(model.Message);
            SetTimestamp(model.Time);
            SetHeader(model.Header);
            SetIsAccepted(model.IsAccepted);
        }

        public void SetFriendRequestId(string friendRequestId) =>
            model.FriendRequestId = friendRequestId;

        public void SetUser(string userId, string userName)
        {
            model.UserId = userId;
            model.UserName = userName;

            notificationSender.text = userName;
            ForceUIRefresh();
        }

        public void SetMessage(string message)
        {
            model.Message = message;
            notificationMessage.text = message;
            ForceUIRefresh();
        }

        public void SetTimestamp(string timestamp)
        {
            model.Time = timestamp;
            notificationTimestamp.text = timestamp;
            ForceUIRefresh();
        }

        public void SetHeader(string header)
        {
            model.Header = header;
            notificationHeader.text = header;
            ForceUIRefresh();
        }

        public void SetIsAccepted(bool isAccepted)
        {
            model.IsAccepted = isAccepted;

            if (receivedRequestMark != null)
                receivedRequestMark.SetActive(!isAccepted);

            if (acceptedRequestMark != null)
                acceptedRequestMark.SetActive(isAccepted);
        }

        private void ForceUIRefresh()
        {
            Utils.ForceRebuildLayoutImmediate(backgroundTransform);
        }
    }
}
