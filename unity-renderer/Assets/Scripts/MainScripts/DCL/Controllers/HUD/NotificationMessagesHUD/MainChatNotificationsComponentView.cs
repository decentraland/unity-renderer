using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

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
    public bool areOtherPanelsOpen = false;

    internal Queue<PoolableObject> poolableQueue = new Queue<PoolableObject>();
    internal Queue<ChatNotificationMessageComponentView> notificationQueue = new Queue<ChatNotificationMessageComponentView>();

    private Pool entryPool;
    private bool isOverMessage = false;
    private bool isOverPanel = false;
    private int notificationCount = 1;
    private Vector2 notificationOffset = new Vector2(0, -56);
    private TMP_Text notificationMessage;
    private CancellationTokenSource animationCancellationToken = new CancellationTokenSource();

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
        if(newValue.y <= 0.0)
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

    public void AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        entryPool = GetNotificationEntryPool();
        var newNotification = entryPool.Get();
        
        ChatNotificationMessageComponentView entry = newNotification.gameObject.GetComponent<ChatNotificationMessageComponentView>();
        poolableQueue.Enqueue(newNotification);
        notificationQueue.Enqueue(entry);

        entry.OnClickedNotification -= ClickedOnNotification;
        entry.onFocused -= FocusedOnNotification;
        entry.showHideAnimator.OnWillFinishHide -= SetScrollToEnd;

        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            PopulatePrivateNotification(entry, message, username, profilePicture);
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            PopulatePublicNotification(entry, message, username);
        }

        entry.transform.SetParent(chatEntriesContainer, false);
        entry.RefreshControl();
        entry.SetTimestamp(UnixTimeStampToLocalTime(message.timestamp));
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
        Sequence mySequence = DOTween.Sequence().AppendInterval(0.2f).Append(notification.DOScale(1, 0.3f).SetEase(Ease.OutBack));
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
        catch (OperationCanceledException ex)
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

    private void PopulatePrivateNotification(ChatNotificationMessageComponentView chatNotificationComponentView, ChatMessage message, string username = null, string profilePicture = null)
    {
        chatNotificationComponentView.SetIsPrivate(true);
        chatNotificationComponentView.SetMessage(message.body);
        chatNotificationComponentView.SetNotificationHeader("Private message");
        chatNotificationComponentView.SetNotificationSender(username);
        chatNotificationComponentView.SetNotificationTargetId(message.sender);
        if (profilePicture != null)
            chatNotificationComponentView.SetImage(profilePicture);
    }

    private void PopulatePublicNotification(ChatNotificationMessageComponentView chatNotificationComponentView, ChatMessage message, string username = null)
    {
        chatNotificationComponentView.SetIsPrivate(false);
        chatNotificationComponentView.SetMessage(message.body);
        chatNotificationComponentView.SetNotificationTargetId("~nearby");
        chatNotificationComponentView.SetNotificationHeader("~nearby");
        chatNotificationComponentView.SetNotificationSender(username);
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

    private static string UnixTimeStampToLocalTime(ulong unixTimeStampMilliseconds)
    {
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddMilliseconds(unixTimeStampMilliseconds).ToLocalTime();
        return $"{dtDateTime.Hour}:{dtDateTime.Minute.ToString("D2")}";
    }

    public override void RefreshControl()
    {

    }

}
