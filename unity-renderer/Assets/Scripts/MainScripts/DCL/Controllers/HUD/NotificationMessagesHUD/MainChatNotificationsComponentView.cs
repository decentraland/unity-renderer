using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.Notifications
{
    public class MainChatNotificationsComponentView : BaseComponentView, IMainChatNotificationsComponentView
    {
        [SerializeField] private RectTransform chatEntriesContainer;
        [SerializeField] private GameObject chatNotification;
        [SerializeField] private GameObject friendRequestNotification;
        [SerializeField] private ScrollRect scrollRectangle;
        [SerializeField] private Button notificationButton;
        [SerializeField] private ShowHideAnimator panelAnimator;
        [SerializeField] private ShowHideAnimator scrollbarAnimator;

        private const string CHAT_NOTIFICATION_POOL_NAME_PREFIX = "ChatNotificationEntriesPool_";
        private const string FRIEND_REQUEST_NOTIFICATION_POOL_NAME_PREFIX = "FriendRequestNotificationEntriesPool_";
        private const int MAX_NOTIFICATION_ENTRIES = 30;

        public event Action<string> OnClickedChatMessage;
        public event IMainChatNotificationsComponentView.ClickedNotificationDelegate OnClickedFriendRequest;
        public event Action<bool> OnResetFade;
        public event Action<bool> OnPanelFocus;

        internal readonly Queue<PoolableObject> poolableQueue = new ();
        internal readonly Queue<BaseComponentView> notificationQueue = new ();

        private readonly Vector2 notificationOffset = new (0, -56);

        private bool isOverMessage;
        private bool isOverPanel;
        private int notificationCount = 1;
        private TMP_Text notificationMessage;
        private CancellationTokenSource animationCancellationToken = new ();

        public override void Awake()
        {
            onFocused += FocusedOnPanel;
            notificationMessage = notificationButton.GetComponentInChildren<TMP_Text>();
            notificationButton?.onClick.RemoveAllListeners();
            notificationButton?.onClick.AddListener(() => SetScrollToEnd());
            scrollRectangle.onValueChanged.AddListener(ResetNotificationButtonFromScroll);
        }

        public override void Show(bool instant = false) =>
            gameObject.SetActive(true);

        public override void Hide(bool instant = false)
        {
            SetScrollToEnd();
            gameObject.SetActive(false);
        }

        public Transform GetPanelTransform() =>
            gameObject.transform;

        private void ResetNotificationButtonFromScroll(Vector2 newValue)
        {
            if (newValue.y <= 0.0) { ResetNotificationButton(); }
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

        public int GetNotificationsCount() =>
            notificationQueue.Count;

        public void ShowNotifications()
        {
            foreach (BaseComponentView notification in notificationQueue)
                notification.Show();
        }

        public void HideNotifications()
        {
            foreach (BaseComponentView notification in notificationQueue)
                notification.Hide();
        }

        public void AddNewChatNotification(PrivateChatMessageNotificationModel model)
        {
            Pool entryPool = GetChatNotificationEntryPool();
            PoolableObject newNotification = entryPool.Get();

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
            Pool entryPool = GetChatNotificationEntryPool();
            PoolableObject newNotification = entryPool.Get();

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

            if (model.ShouldPlayMentionSfx)
                AudioScriptableObjects.ChatReceiveMentionEvent.Play(true);

            OnResetFade?.Invoke(!isOverMessage && !isOverPanel);
            CheckNotificationCountAndRelease();
        }

        public void AddNewFriendRequestNotification(FriendRequestNotificationModel model)
        {
            Pool entryPool = GetFriendRequestNotificationEntryPool();
            PoolableObject newNotification = entryPool.Get();

            var entry = newNotification.gameObject.GetComponent<FriendRequestNotificationComponentView>();
            poolableQueue.Enqueue(newNotification);
            notificationQueue.Enqueue(entry);

            entry.OnClickedNotification -= ClickedOnFriendRequest;
            entry.onFocused -= FocusedOnNotification;
            entry.showHideAnimator.OnWillFinishHide -= SetScrollToEnd;

            PopulateFriendRequestNotification(entry, model);

            entry.transform.SetParent(chatEntriesContainer, false);
            entry.RefreshControl();
            entry.OnClickedNotification += ClickedOnFriendRequest;
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

            // TODO: refactor sfx usage into non-static
            AudioScriptableObjects.FriendRequestEvent.Play();
        }

        private async UniTaskVoid AnimateNewEntry(Transform notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Sequence mySequence = DOTween.Sequence()
                                         .AppendInterval(0.2f)
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

        private void PopulatePrivateNotification(ChatNotificationMessageComponentView view,
            PrivateChatMessageNotificationModel model)
        {
            string senderName = model.ImTheSender ? "You" : model.SenderUsername;

            view.SetIsPrivate(true);
            view.SetMaxContentCharacters(40 - senderName.Length);
            view.SetMessage(model.Body);
            view.SetNotificationHeader($"DM - {model.PeerUsername}");
            view.SetNotificationSender($"{senderName}:");
            view.SetNotificationTargetId(model.TargetId);
            view.SetImageVisibility(!model.ImTheSender);
            view.SetOwnPlayerMention(model.IsOwnPlayerMentioned);

            if (!string.IsNullOrEmpty(model.ProfilePicture))
                view.SetImage(model.ProfilePicture);

            if (model.ImTheSender)
                view.DockRight();
            else
                view.DockLeft();
        }

        private void PopulatePublicNotification(ChatNotificationMessageComponentView view,
            PublicChannelMessageNotificationModel model)
        {
            string channelId = model.ChannelId;
            string channelName = model.ChannelName == "nearby" ? "~nearby" : $"#{model.ChannelName}";
            string senderName = model.ImTheSender ? "You" : model.Username;

            view.SetIsPrivate(false);
            view.SetMaxContentCharacters(40 - senderName.Length);
            view.SetMessage(model.Body);
            view.SetNotificationTargetId(channelId);
            view.SetNotificationHeader(channelName);
            view.SetNotificationSender($"{senderName}:");
            view.SetImageVisibility(false);
            view.SetOwnPlayerMention(model.IsOwnPlayerMentioned);

            if (model.ImTheSender)
                view.DockRight();
            else
                view.DockLeft();
        }

        private void ClickedOnNotification(string targetId)
        {
            OnClickedChatMessage?.Invoke(targetId);
        }

        private void PopulateFriendRequestNotification(FriendRequestNotificationComponentView friendRequestNotificationComponentView,
            FriendRequestNotificationModel model)
        {
            friendRequestNotificationComponentView.SetFriendRequestId(model.FriendRequestId);
            friendRequestNotificationComponentView.SetUser(model.UserId, model.UserName);
            friendRequestNotificationComponentView.SetHeader(model.Header);
            friendRequestNotificationComponentView.SetMessage(model.Message);
            DateTime localTime = model.Timestamp.ToLocalTime();
            friendRequestNotificationComponentView.SetTimestamp($"{localTime.Hour}:{localTime.Minute:D2}");
            friendRequestNotificationComponentView.SetIsAccepted(model.IsAccepted);
        }

        private void ClickedOnFriendRequest(string friendRequestId, string userId, bool isAcceptedFromPeer) =>
            OnClickedFriendRequest?.Invoke(friendRequestId, userId, isAcceptedFromPeer);

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
            if (poolableQueue.Count < MAX_NOTIFICATION_ENTRIES) return;

            BaseComponentView notificationToDequeue = notificationQueue.Dequeue();
            notificationToDequeue.onFocused -= FocusedOnNotification;

            PoolableObject pooledObj = poolableQueue.Dequeue();

            switch (notificationToDequeue)
            {
                case FriendRequestNotificationComponentView:
                    GetFriendRequestNotificationEntryPool().Release(pooledObj);
                    break;
                case ChatNotificationMessageComponentView:
                    GetChatNotificationEntryPool().Release(pooledObj);
                    break;
                default:
                    Debug.LogError($"Failed to release notification of type {notificationToDequeue.GetType()}. No object pool found.");
                    break;
            }
        }

        private Pool GetChatNotificationEntryPool()
        {
            var entryPool = PoolManager.i.GetPool(CHAT_NOTIFICATION_POOL_NAME_PREFIX + name + GetInstanceID());
            if (entryPool != null) return entryPool;

            entryPool = PoolManager.i.AddPool(
                CHAT_NOTIFICATION_POOL_NAME_PREFIX + name + GetInstanceID(),
                Instantiate(chatNotification),
                maxPrewarmCount: MAX_NOTIFICATION_ENTRIES,
                isPersistent: true);

            entryPool.ForcePrewarm();

            return entryPool;
        }

        private Pool GetFriendRequestNotificationEntryPool()
        {
            var entryPool = PoolManager.i.GetPool(FRIEND_REQUEST_NOTIFICATION_POOL_NAME_PREFIX + name + GetInstanceID());

            if (entryPool != null)
                return entryPool;

            entryPool = PoolManager.i.AddPool(
                FRIEND_REQUEST_NOTIFICATION_POOL_NAME_PREFIX + name + GetInstanceID(),
                Instantiate(friendRequestNotification),
                maxPrewarmCount: MAX_NOTIFICATION_ENTRIES,
                isPersistent: true);

            entryPool.ForcePrewarm();

            return entryPool;
        }

        public override void RefreshControl() { }
    }
}
