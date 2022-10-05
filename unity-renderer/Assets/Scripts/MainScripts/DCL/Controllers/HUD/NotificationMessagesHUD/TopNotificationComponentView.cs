using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace DCL.Chat.Notifications
{
    public class TopNotificationComponentView : BaseComponentView, ITopNotificationsComponentView
    {
        private const float X_OFFSET = 32f;
        private const float NORMAL_CONTENT_X_POS = 111;
        private const float NORMAL_HEADER_X_POS = 70;
        private const int NEW_NOTIFICATION_DELAY = 5000;

        public event Action<string> OnClickedNotification;

        [SerializeField] private ChatNotificationMessageComponentView chatNotificationComponentView;

        public event Action<bool> OnResetFade;

        //This structure is temporary for the first integration of the top notification, it will change when further defined
        private float offsetContentXPos;
        private float offsetHeaderXPos;
        private int stackedNotifications;
        private CancellationTokenSource animationCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource waitCancellationToken = new CancellationTokenSource();
        private RectTransform notificationRect;

        public bool isShowingNotification;

        public static TopNotificationComponentView Create()
        {
            return Instantiate(Resources.Load<TopNotificationComponentView>("SocialBarV1/TopNotificationHUD"));
        }

        public override void Start()
        {
            chatNotificationComponentView.OnClickedNotification += ClickedOnNotification;
            offsetContentXPos = NORMAL_CONTENT_X_POS - X_OFFSET;
            offsetHeaderXPos = NORMAL_HEADER_X_POS - X_OFFSET;
            chatNotificationComponentView.SetPositionOffset(NORMAL_HEADER_X_POS, NORMAL_CONTENT_X_POS);
            notificationRect = chatNotificationComponentView.gameObject.GetComponent<RectTransform>();
            chatNotificationComponentView.shouldAnimateFocus = false;
            chatNotificationComponentView.SetIsPrivate(true);
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
            chatNotificationComponentView.gameObject.SetActive(true);
            if (stackedNotifications > 2)
            {
                OnResetFade?.Invoke(true);
                PopulateMultipleNotification();
                chatNotificationComponentView.SetPositionOffset(NORMAL_HEADER_X_POS, NORMAL_CONTENT_X_POS);
                AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
                return;
            }

            stackedNotifications--;
            OnResetFade?.Invoke(true);
            PopulatePrivateNotification(model);
            chatNotificationComponentView.SetPositionOffset(NORMAL_HEADER_X_POS, NORMAL_CONTENT_X_POS);
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
            chatNotificationComponentView.gameObject.SetActive(true);
            if (stackedNotifications > 2)
            {
                OnResetFade?.Invoke(true);
                PopulateMultipleNotification();
                chatNotificationComponentView.SetPositionOffset(NORMAL_HEADER_X_POS, NORMAL_CONTENT_X_POS);
                AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
                return;
            }

            stackedNotifications--;
            OnResetFade?.Invoke(true);
            PopulatePublicNotification(model);
            chatNotificationComponentView.SetPositionOffset(offsetHeaderXPos, offsetContentXPos);
            AnimateNewEntry(notificationRect, animationCancellationToken.Token).Forget();
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
            chatNotificationComponentView.SetIsPrivate(true);
            chatNotificationComponentView.SetMessage(model.Body);
            chatNotificationComponentView.SetNotificationHeader("Private message");
            chatNotificationComponentView.SetNotificationSender($"{model.Username}:");
            chatNotificationComponentView.SetNotificationTargetId(model.SenderId);
        }

        private void PopulatePublicNotification(PublicChannelMessageNotificationModel model)
        {
            chatNotificationComponentView.SetIsPrivate(false);
            chatNotificationComponentView.SetMessage(model.Body);

            var channelName = model.ChannelName == "nearby" ? "~nearby" : $"#{model.ChannelName}";

            chatNotificationComponentView.SetNotificationTargetId(model.ChannelId);
            chatNotificationComponentView.SetNotificationHeader(channelName);
            chatNotificationComponentView.SetNotificationSender($"{model.Username}:");
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

            chatNotificationComponentView.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            if (!gameObject.activeInHierarchy)
                return;

            isShowingNotification = false;
            stackedNotifications = 0;
            chatNotificationComponentView.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public void ShowNotification()
        {
            chatNotificationComponentView.Show();
        }

        public void HideNotification()
        {
            isShowingNotification = false;
            stackedNotifications = 0;
            chatNotificationComponentView.Hide();
        }

        private void ClickedOnNotification(string targetId)
        {
            HideNotification();
            isShowingNotification = false;
            stackedNotifications = 0;
            OnClickedNotification?.Invoke(targetId);
        }

        public override void Dispose()
        {
            base.Dispose();

            chatNotificationComponentView.OnClickedNotification -= ClickedOnNotification;
        }

        public override void RefreshControl()
        {
        }
    }
}