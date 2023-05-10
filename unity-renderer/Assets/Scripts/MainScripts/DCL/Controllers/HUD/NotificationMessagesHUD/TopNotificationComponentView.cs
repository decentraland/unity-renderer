using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public class TopNotificationComponentView : BaseComponentView, ITopNotificationsComponentView
    {
        private const float X_OFFSET = 32f;
        private const float NORMAL_CONTENT_X_POS = 111;
        private const float NORMAL_HEADER_X_POS = 70;
        private const int NEW_NOTIFICATION_DELAY = 5000;

        public event Action<string> OnClickedChatMessage;
        public event ITopNotificationsComponentView.ClickedNotificationDelegate OnClickedFriendRequest;

        [SerializeField] private ChatNotificationMessageComponentView chatNotificationComponentView;
        [SerializeField] private FriendRequestNotificationComponentView friendRequestNotificationComponentView;

        public event Action<bool> OnResetFade;

        //This structure is temporary for the first integration of the top notification, it will change when further defined
        private float offsetContentXPos;
        private float offsetHeaderXPos;
        private int stackedNotifications;
        private CancellationTokenSource animationCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource waitCancellationToken = new CancellationTokenSource();
        private RectTransform notificationRect;
        private RectTransform friendRequestRect;
        private IShowableNotificationView showableNotification;

        public bool isShowingNotification;

        public void Start()
        {
            offsetContentXPos = NORMAL_CONTENT_X_POS - X_OFFSET;
            offsetHeaderXPos = NORMAL_HEADER_X_POS - X_OFFSET;

            chatNotificationComponentView.OnClickedNotification += ClickedOnNotification;
            notificationRect = chatNotificationComponentView.gameObject.GetComponent<RectTransform>();
            chatNotificationComponentView.shouldAnimateFocus = false;
            chatNotificationComponentView.SetIsPrivate(true);

            friendRequestNotificationComponentView.OnClickedNotification += ClickedOnFriendRequestNotification;
            friendRequestRect = friendRequestNotificationComponentView.gameObject.GetComponent<RectTransform>();
            friendRequestNotificationComponentView.shouldAnimateFocus = false;
        }

        public Transform GetPanelTransform()
        {
            return gameObject.transform;
        }

        public void AddNewChatNotification(PrivateChatMessageNotificationModel model)
        {
            stackedNotifications++;
            if (isShowingNotification && stackedNotifications <= 2)
            {
                waitCancellationToken.Cancel();
                waitCancellationToken = new CancellationTokenSource();
                WaitBeforeShowingNewNotification(model, waitCancellationToken.Token).Forget();
                return;
            }

            isShowingNotification = true;
            animationCancellationToken.Cancel();
            animationCancellationToken = new CancellationTokenSource();
            friendRequestNotificationComponentView.gameObject.SetActive(false);
            chatNotificationComponentView.gameObject.SetActive(true);
            showableNotification = chatNotificationComponentView;
            if (stackedNotifications > 2)
            {
                OnResetFade?.Invoke(true);
                PopulateMultipleNotification();
                AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
                return;
            }

            stackedNotifications--;
            OnResetFade?.Invoke(true);
            PopulatePrivateNotification(model);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
            ShowNotificationCooldown().Forget();
        }

        public void AddNewChatNotification(PublicChannelMessageNotificationModel model)
        {
            stackedNotifications++;
            if (isShowingNotification && stackedNotifications <= 2)
            {
                waitCancellationToken.Cancel();
                waitCancellationToken = new CancellationTokenSource();
                WaitBeforeShowingNewNotification(model, waitCancellationToken.Token).Forget();
                return;
            }

            isShowingNotification = true;
            animationCancellationToken.Cancel();
            animationCancellationToken = new CancellationTokenSource();
            friendRequestNotificationComponentView.gameObject.SetActive(false);
            chatNotificationComponentView.gameObject.SetActive(true);
            showableNotification = chatNotificationComponentView;
            if (stackedNotifications > 2)
            {
                OnResetFade?.Invoke(true);
                PopulateMultipleNotification();
                AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
                return;
            }

            stackedNotifications--;
            OnResetFade?.Invoke(true);
            PopulatePublicNotification(model);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
            ShowNotificationCooldown().Forget();
        }

        public void AddNewFriendRequestNotification(FriendRequestNotificationModel model)
        {
            isShowingNotification = true;
            animationCancellationToken.Cancel();
            animationCancellationToken = new CancellationTokenSource();
            chatNotificationComponentView.gameObject.SetActive(false);
            friendRequestNotificationComponentView.gameObject.SetActive(true);
            showableNotification = friendRequestNotificationComponentView;

            OnResetFade?.Invoke(true);
            PopulateFriendRequestNotification(model);
            AnimateNewEntry(friendRequestRect, animationCancellationToken.Token).Forget();
            ShowNotificationCooldown().Forget();
        }

        private async UniTaskVoid ShowNotificationCooldown()
        {
            await UniTask.Delay(NEW_NOTIFICATION_DELAY);
            stackedNotifications--;
            isShowingNotification = false;
        }

        private async UniTaskVoid WaitBeforeShowingNewNotification(PublicChannelMessageNotificationModel model, CancellationToken cancellationToken)
        {
            while (isShowingNotification)
                await UniTask.NextFrame(cancellationToken).AttachExternalCancellation(cancellationToken);

            AddNewChatNotification(model);
        }

        private async UniTaskVoid WaitBeforeShowingNewNotification(PrivateChatMessageNotificationModel model, CancellationToken cancellationToken)
        {
            while (isShowingNotification)
                await UniTask.NextFrame(cancellationToken).AttachExternalCancellation(cancellationToken);

            AddNewChatNotification(model);
        }

        private async UniTaskVoid WaitBeforeShowingNewNotification(FriendRequestNotificationModel model, CancellationToken cancellationToken)
        {
            while (isShowingNotification)
                await UniTask.NextFrame(cancellationToken).AttachExternalCancellation(cancellationToken);

            AddNewFriendRequestNotification(model);
        }

        private async UniTaskVoid AnimateNewEntry(RectTransform notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mySequence = DOTween.Sequence().AppendInterval(0.2f)
                .Append(notification.DOScale(1, 0.3f).SetEase(Ease.OutBack));
            try
            {
                Vector2 endPosition = new Vector2(0, 0);
                Vector2 currentPosition = notification.anchoredPosition;
                notification.localScale = Vector3.zero;
                DOTween.To(() => currentPosition, x => currentPosition = x, endPosition, 0.8f).SetEase(Ease.OutCubic);
                while (notification.anchoredPosition.y < 0)
                {
                    notification.anchoredPosition = currentPosition;
                    await UniTask.NextFrame(cancellationToken).AttachExternalCancellation(cancellationToken);
                }

                mySequence.Play();
            }
            catch (OperationCanceledException)
            {
                if (!DOTween.IsTweening(notification))
                    notification.DOScale(1, 0.3f).SetEase(Ease.OutBack);
            }
        }

        private void PopulatePrivateNotification(PrivateChatMessageNotificationModel model)
        {
            string senderName = model.ImTheSender ? "You" : model.SenderUsername;

            chatNotificationComponentView.SetIsPrivate(true);
            chatNotificationComponentView.SetMaxContentCharacters(40 - senderName.Length);
            chatNotificationComponentView.SetMessage(model.Body);
            chatNotificationComponentView.SetNotificationHeader($"DM - {model.PeerUsername}");
            chatNotificationComponentView.SetNotificationSender($"{senderName}:");
            chatNotificationComponentView.SetNotificationTargetId(model.TargetId);
            chatNotificationComponentView.SetImageVisibility(true);
            chatNotificationComponentView.SetOwnPlayerMention(model.IsOwnPlayerMentioned);
            chatNotificationComponentView.SetImage(model.ProfilePicture);
        }

        private void PopulatePublicNotification(PublicChannelMessageNotificationModel model)
        {
            string channelName = model.ChannelName == "nearby" ? "~nearby" : $"#{model.ChannelName}";
            string senderName = model.ImTheSender ? "You" : model.Username;

            chatNotificationComponentView.SetIsPrivate(false);
            chatNotificationComponentView.SetMaxContentCharacters(40 - senderName.Length);
            chatNotificationComponentView.SetMessage(model.Body);
            chatNotificationComponentView.SetNotificationTargetId(model.ChannelId);
            chatNotificationComponentView.SetNotificationHeader(channelName);
            chatNotificationComponentView.SetNotificationSender($"{senderName}:");
            chatNotificationComponentView.SetImageVisibility(false);
        }

        private void PopulateFriendRequestNotification(FriendRequestNotificationModel model)
        {
            friendRequestNotificationComponentView.SetFriendRequestId(model.FriendRequestId);
            friendRequestNotificationComponentView.SetUser(model.UserId, model.UserName);
            friendRequestNotificationComponentView.SetHeader(model.Header);
            friendRequestNotificationComponentView.SetMessage(model.Message);
            friendRequestNotificationComponentView.SetTimestamp(Utils.UnixTimeStampToLocalTime(model.Timestamp));
            friendRequestNotificationComponentView.SetIsAccepted(model.IsAccepted);
        }

        private void PopulateMultipleNotification()
        {
            chatNotificationComponentView.SetMessage("");
            chatNotificationComponentView.SetNotificationTargetId("conversationList");
            chatNotificationComponentView.SetNotificationHeader("CHAT NOTIFICATIONS");
            chatNotificationComponentView.SetNotificationSender($"{stackedNotifications} messages");
            chatNotificationComponentView.SetIsMultipleNotifications();
        }

        public override void Show(bool instant = false)
        {
            if (gameObject.activeInHierarchy)
                return;

            friendRequestNotificationComponentView.gameObject.SetActive(false);
            chatNotificationComponentView.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            if (!gameObject.activeInHierarchy)
                return;

            isShowingNotification = false;
            stackedNotifications = 0;
            friendRequestNotificationComponentView.gameObject.SetActive(false);
            chatNotificationComponentView.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void ShowNotification()
        {
            if (showableNotification == null)
                return;

            showableNotification.Show();
        }

        public void HideNotification()
        {
            isShowingNotification = false;
            stackedNotifications = 0;
            friendRequestNotificationComponentView.Hide();
            chatNotificationComponentView.Hide();
        }

        private void ClickedOnNotification(string targetId)
        {
            HideNotification();
            isShowingNotification = false;
            stackedNotifications = 0;
            OnClickedChatMessage?.Invoke(targetId);
        }

        private void ClickedOnFriendRequestNotification(string friendRequestId, string userId, bool isAcceptedFromPeer)
        {
            HideNotification();
            isShowingNotification = false;
            stackedNotifications = 0;
            OnClickedFriendRequest?.Invoke(friendRequestId, userId, isAcceptedFromPeer);
        }

        public override void Dispose()
        {
            base.Dispose();

            chatNotificationComponentView.OnClickedNotification -= ClickedOnNotification;
            friendRequestNotificationComponentView.OnClickedNotification -= ClickedOnFriendRequestNotification;
        }

        public override void RefreshControl()
        {
        }
    }
}
