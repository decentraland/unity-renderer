using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Interface;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.Notifications
{
    public class MainChatNotificationsComponentView : BaseComponentView, IMainChatNotificationsComponentView
    {
        [SerializeField] private RectTransform chatEntriesContainer;
        [SerializeField] private GameObject chatNotification;
        [SerializeField] private ScrollRect scrollRectangle;
        [SerializeField] private Button notificationButton;
        [SerializeField] private ShowHideAnimator panelAnimator;
        [SerializeField] private ShowHideAnimator scrollbarAnimator;

        private const string NOTIFICATION_POOL_NAME_PREFIX = "NotificationEntriesPool_";
        private const int MAX_NOTIFICATION_ENTRIES = 30;

        public event Action<string> OnClickedNotification;
        public event Action<bool> OnResetFade;
        public event Action<bool> OnPanelFocus;

        internal readonly Queue<PoolableObject> poolableQueue = new Queue<PoolableObject>();

        internal readonly Queue<ChatNotificationMessageComponentView> notificationQueue =
            new Queue<ChatNotificationMessageComponentView>();

        private readonly Vector2 notificationOffset = new Vector2(0, -56);

        private Pool entryPool;
        private bool isOverMessage;
        private bool isOverPanel;
        private int notificationCount = 1;
        private TMP_Text notificationMessage;
        private CancellationTokenSource animationCancellationToken = new CancellationTokenSource();
        private BaseVariable<string> openedChat => DataStore.i.HUDs.openedChat;

        public static MainChatNotificationsComponentView Create()
        {
            return Instantiate(Resources.Load<MainChatNotificationsComponentView>("SocialBarV1/ChatNotificationHUD"));
        }

        public void Awake()
        {
            onFocused += FocusedOnPanel;
            notificationMessage = notificationButton.GetComponentInChildren<TMP_Text>();
            notificationButton?.onClick.RemoveAllListeners();
            notificationButton?.onClick.AddListener(() => SetScrollToEnd());
            scrollRectangle.onValueChanged.AddListener(ResetNotificationButtonFromScroll);
        }

        public override void Show(bool instant = false)
        {
            openedChat.Set("");
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            SetScrollToEnd();
            gameObject.SetActive(false);
        }

        public Transform GetPanelTransform()
        {
            return gameObject.transform;
        }

        private void ResetNotificationButtonFromScroll(Vector2 newValue)
        {
            if (newValue.y <= 0.0)
            {
                ResetNotificationButton();
            }
        }

        public void ShowPanel()
        {
            panelAnimator?.Show();
            scrollbarAnimator?.Show();
        }

        public void HidePanel()
        {
            panelAnimator?.Hide();
            scrollbarAnimator?.Hide();
        }

        public void ShowNotifications()
        {
            foreach (ChatNotificationMessageComponentView notification in notificationQueue)
            {
                notification.Show();
            }
        }

        public void HideNotifications()
        {
            foreach (ChatNotificationMessageComponentView notification in notificationQueue)
            {
                notification.Hide();
            }
        }

        public void AddNewChatNotification(PrivateChatMessageNotificationModel model)
        {
            entryPool = GetNotificationEntryPool();
            var newNotification = entryPool.Get();

            var entry = newNotification.gameObject.GetComponent<ChatNotificationMessageComponentView>();
            poolableQueue.Enqueue(newNotification);
            notificationQueue.Enqueue(entry);

            entry.OnClickedNotification -= ClickedOnNotification;
            entry.onFocused -= FocusedOnNotification;
            entry.showHideAnimator.OnWillFinishHide -= SetScrollToEnd;

            PopulatePrivateNotification(entry, model);

            entry.transform.SetParent(chatEntriesContainer, false);
            entry.RefreshControl();
            entry.SetTimestamp(Utils.UnixTimeStampToLocalTime(model.Timestamp));
            entry.OnClickedNotification += ClickedOnNotification;
            entry.onFocused += FocusedOnNotification;
            entry.showHideAnimator.OnWillFinishHide += SetScrollToEnd;

            chatEntriesContainer.anchoredPosition += notificationOffset;
            if (isOverPanel)
            {
                notificationButton.gameObject.SetActive(true);
                IncreaseNotificationCount();
            }
            else
            {
                ResetNotificationButton();
                animationCancellationToken.Cancel();
                animationCancellationToken = new CancellationTokenSource();
                AnimateNewEntry(entry.gameObject.transform, animationCancellationToken.Token).Forget();
            }

            OnResetFade?.Invoke(!isOverMessage && !isOverPanel);
            CheckNotificationCountAndRelease();
        }

        public void AddNewChatNotification(PublicChannelMessageNotificationModel model)
        {
            entryPool = GetNotificationEntryPool();
            var newNotification = entryPool.Get();

            var entry = newNotification.gameObject.GetComponent<ChatNotificationMessageComponentView>();
            poolableQueue.Enqueue(newNotification);
            notificationQueue.Enqueue(entry);

            entry.OnClickedNotification -= ClickedOnNotification;
            entry.onFocused -= FocusedOnNotification;
            entry.showHideAnimator.OnWillFinishHide -= SetScrollToEnd;

            PopulatePublicNotification(entry, model);

            entry.transform.SetParent(chatEntriesContainer, false);
            entry.RefreshControl();
            entry.SetTimestamp(Utils.UnixTimeStampToLocalTime(model.Timestamp));
            entry.OnClickedNotification += ClickedOnNotification;
            entry.onFocused += FocusedOnNotification;
            entry.showHideAnimator.OnWillFinishHide += SetScrollToEnd;

            chatEntriesContainer.anchoredPosition += notificationOffset;
            if (isOverPanel)
            {
                notificationButton.gameObject.SetActive(true);
                IncreaseNotificationCount();
            }
            else
            {
                ResetNotificationButton();
                animationCancellationToken.Cancel();
                animationCancellationToken = new CancellationTokenSource();
                AnimateNewEntry(entry.gameObject.transform, animationCancellationToken.Token).Forget();
            }

            OnResetFade?.Invoke(!isOverMessage && !isOverPanel);
            CheckNotificationCountAndRelease();
        }

        private async UniTaskVoid AnimateNewEntry(Transform notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Sequence mySequence = DOTween.Sequence().AppendInterval(0.2f)
                .Append(notification.DOScale(1, 0.3f).SetEase(Ease.OutBack));
            try
            {
                Vector2 endPosition = new Vector2(0, 0);
                Vector2 currentPosition = chatEntriesContainer.anchoredPosition;
                notification.localScale = Vector3.zero;
                DOTween.To(() => currentPosition, x => currentPosition = x, endPosition, 0.8f).SetEase(Ease.OutCubic);
                while (chatEntriesContainer.anchoredPosition.y < 0)
                {
                    chatEntriesContainer.anchoredPosition = currentPosition;
                    await UniTask.NextFrame(cancellationToken);
                }

                mySequence.Play();
            }
            catch (OperationCanceledException)
            {
                if (!DOTween.IsTweening(notification))
                    notification.DOScale(1, 0.3f).SetEase(Ease.OutBack);
            }
        }

        private void ResetNotificationButton()
        {
            notificationButton.gameObject.SetActive(false);
            notificationCount = 0;

            if (notificationMessage != null)
                notificationMessage.text = notificationCount.ToString();
        }

        private void IncreaseNotificationCount()
        {
            notificationCount++;
            if (notificationMessage != null)
                notificationMessage.text = notificationCount <= 9 ? notificationCount.ToString() : "9+";
        }

        private void SetScrollToEnd(ShowHideAnimator animator = null)
        {
            scrollRectangle.normalizedPosition = new Vector2(0, 0);
            ResetNotificationButton();
        }

        private void PopulatePrivateNotification(ChatNotificationMessageComponentView chatNotificationComponentView,
            PrivateChatMessageNotificationModel model)
        {
            chatNotificationComponentView.SetIsPrivate(true);
            chatNotificationComponentView.SetMessage(model.Body);
            chatNotificationComponentView.SetNotificationHeader("Private message");
            chatNotificationComponentView.SetNotificationSender($"{model.Username}:");
            chatNotificationComponentView.SetNotificationTargetId(model.SenderId);
            if (!string.IsNullOrEmpty(model.ProfilePicture))
                chatNotificationComponentView.SetImage(model.ProfilePicture);
        }

        private void PopulatePublicNotification(ChatNotificationMessageComponentView chatNotificationComponentView,
            PublicChannelMessageNotificationModel model)
        {
            chatNotificationComponentView.SetIsPrivate(false);
            chatNotificationComponentView.SetMessage(model.Body);

            var channelId = model.ChannelId;
            var channelName = model.ChannelName == "nearby" ? "~nearby" : $"#{model.ChannelName}";

            chatNotificationComponentView.SetNotificationTargetId(channelId);
            chatNotificationComponentView.SetNotificationHeader(channelName);
            chatNotificationComponentView.SetNotificationSender($"{model.Username}:");
        }

        private void ClickedOnNotification(string targetId)
        {
            OnClickedNotification?.Invoke(targetId);
        }

        private void FocusedOnNotification(bool isInFocus)
        {
            isOverMessage = isInFocus;
            OnResetFade?.Invoke(!isOverMessage && !isOverPanel);
        }

        private void FocusedOnPanel(bool isInFocus)
        {
            isOverPanel = isInFocus;
            OnPanelFocus?.Invoke(isOverPanel);
            OnResetFade?.Invoke(!isOverMessage && !isOverPanel);
        }

        private void CheckNotificationCountAndRelease()
        {
            if (poolableQueue.Count >= MAX_NOTIFICATION_ENTRIES)
            {
                ChatNotificationMessageComponentView notificationToDequeue = notificationQueue.Dequeue();
                notificationToDequeue.onFocused -= FocusedOnNotification;
                entryPool.Release(poolableQueue.Dequeue());
            }
        }

        private Pool GetNotificationEntryPool()
        {
            var entryPool = PoolManager.i.GetPool(NOTIFICATION_POOL_NAME_PREFIX + name + GetInstanceID());
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                NOTIFICATION_POOL_NAME_PREFIX + name + GetInstanceID(),
                Instantiate(chatNotification).gameObject,
                maxPrewarmCount: MAX_NOTIFICATION_ENTRIES,
                isPersistent: true);
            entryPool.ForcePrewarm();

            return entryPool;
        }

        public override void RefreshControl()
        {
        }
    }
}