using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainChatNotificationsComponentView : BaseComponentView
{
    [SerializeField] private RectTransform chatEntriesContainer;
    [SerializeField] private GameObject chatNotification;
    [SerializeField] private ScrollRect scrollRectangle;
    [SerializeField] private Button notificationButton;
    
    private TMP_Text notificationMessage;
    public ChatNotificationController controller;

    private const string NOTIFICATION_POOL_NAME_PREFIX = "NotificationEntriesPool_";
    private const int MAX_NOTIFICATION_ENTRIES = 30;

    public event Action<string> OnClickedNotification;
    public bool areOtherPanelsOpen = false;

    internal Queue<PoolableObject> poolableQueue = new Queue<PoolableObject>();
    internal Queue<ChatNotificationMessageComponentView> notificationQueue = new Queue<ChatNotificationMessageComponentView>();

    private Pool entryPool;
    private bool isOverMessage = false;
    private bool isOverPanel = false;
    private int notificationCount = 1;

    public static MainChatNotificationsComponentView Create()
    {
        return Instantiate(Resources.Load<MainChatNotificationsComponentView>("SocialBarV1/ChatNotificationHUD"));
    }

    public void Initialize(ChatNotificationController chatController)
    {
        controller = chatController;
        onFocused += FocusedOnPanel;
        notificationMessage = notificationButton.GetComponentInChildren<TMP_Text>();
        notificationButton?.onClick.RemoveAllListeners();
        notificationButton?.onClick.AddListener(() => SetScrollToEnd());

        scrollRectangle.onValueChanged.AddListener(CheckScrollValue);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        SetScrollToEnd();
        gameObject.SetActive(false);
    }

    private void CheckScrollValue(Vector2 newValue)
    { 
        if(scrollRectangle.normalizedPosition == new Vector2(0, 0))
        {
            ResetNotificationButton();
        }
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

    public ChatNotificationMessageComponentView AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        entryPool = GetNotificationEntryPool();
        var newNotification = entryPool.Get();
        
        ChatNotificationMessageComponentView entry = newNotification.gameObject.GetComponent<ChatNotificationMessageComponentView>();
        poolableQueue.Enqueue(newNotification);
        notificationQueue.Enqueue(entry);

        entry.OnClickedNotification -= ClickedOnNotification;
        entry.onFocused -= FocusedOnNotification;
        entry.showHideAnimator.OnWillFinishHide -= _ => SetScrollToEnd();

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
        entry.showHideAnimator.OnWillFinishHide += _ => SetScrollToEnd();

        if (isOverMessage)
        {
            notificationButton.gameObject.SetActive(true);
            IncreaseNotificationCount();
        }
        else
        {
            ResetNotificationButton();
            SetScrollToEnd();
        }

        controller?.ResetFadeout(!isOverMessage && !isOverPanel);
        CheckNotificationCountAndRelease();
        return entry;
    }

    private void ResetNotificationButton()
    {
        if (notificationMessage == null) return;

        notificationButton.gameObject.SetActive(false);
        notificationCount = 0;
        notificationMessage.text = notificationCount.ToString();
    }

    private void IncreaseNotificationCount()
    {
        if (notificationMessage == null) return;

        notificationCount++;
        notificationMessage.text = notificationCount.ToString();
    }

    private void SetScrollToEnd()
    {
        scrollRectangle.normalizedPosition = new Vector2(0, 0);
    }

    private void PopulatePrivateNotification(ChatNotificationMessageComponentView chatNotificationComponentView, ChatMessage message, string username = null, string profilePicture = null)
    {
        chatNotificationComponentView.SetIsPrivate(true);
        chatNotificationComponentView.SetMessage(message.body);
        chatNotificationComponentView.SetNotificationHeader(username);
        chatNotificationComponentView.SetNotificationTargetId(message.sender);
        if (profilePicture != null)
            chatNotificationComponentView.SetImage(profilePicture);
    }

    private void PopulatePublicNotification(ChatNotificationMessageComponentView chatNotificationComponentView, ChatMessage message, string username = null)
    {
        chatNotificationComponentView.SetIsPrivate(false);
        chatNotificationComponentView.SetMessage($"{username}: {message.body}");
        chatNotificationComponentView.SetNotificationTargetId("#nearby");
        chatNotificationComponentView.SetNotificationHeader("#nearby");
    }

    private void ClickedOnNotification(string targetId)
    {
        OnClickedNotification?.Invoke(targetId);
    }

    private void FocusedOnNotification(bool isInFocus)
    {
        isOverMessage = isInFocus;
        controller?.ResetFadeout(!isOverMessage && !isOverPanel);
    }

    private void FocusedOnPanel(bool isInFocus)
    {
        isOverPanel = isInFocus;
        controller?.ResetFadeout(!isOverMessage && !isOverPanel);
    }

    private void CheckNotificationCountAndRelease()
    {
        if (poolableQueue.Count >= MAX_NOTIFICATION_ENTRIES)
        {
            entryPool.Release(poolableQueue.Dequeue());
            notificationQueue.Dequeue();
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
