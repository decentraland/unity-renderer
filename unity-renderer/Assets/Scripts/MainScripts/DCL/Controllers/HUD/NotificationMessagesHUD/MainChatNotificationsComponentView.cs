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
    [SerializeField] RectTransform chatEntriesContainer;
    [SerializeField] GameObject chatNotification;

    public ChatNotificationController controller;

    private const string NOTIFICATION_POOL_NAME_PREFIX = "NotificationEntriesPool_";
    private const int MAX_NOTIFICATION_ENTRIES = 30;

    internal Queue<PoolableObject> poolableQueue = new Queue<PoolableObject>();
    internal Queue<ChatNotificationMessageComponentView> creationQueue2 = new Queue<ChatNotificationMessageComponentView>();
    private Pool entryPool;
    public event Action<string> OnClickedNotification;

    public static MainChatNotificationsComponentView Create()
    {
        return Instantiate(Resources.Load<MainChatNotificationsComponentView>("SocialBarV1/ChatNotificationHUD"));
    }

    public void Initialize(ChatNotificationController chatController)
    {
        controller = chatController;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public ChatNotificationMessageComponentView AddNewChatNotification(ChatMessage message, string username = null, string profilePicture = null)
    {
        CheckNotificationCountAndRelease();
        entryPool = GetNotificationEntryPool();
        var newNotification = entryPool.Get();
        
        ChatNotificationMessageComponentView chatNotificationComponentView = newNotification.gameObject.GetComponent<ChatNotificationMessageComponentView>();
        poolableQueue.Enqueue(newNotification);
        creationQueue2.Enqueue(chatNotificationComponentView);

        chatNotificationComponentView.OnClickedNotification -= ClickedOnNotification;

        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            chatNotificationComponentView.SetIsPrivate(true);
            chatNotificationComponentView.SetMessage(message.body);
            chatNotificationComponentView.SetNotificationHeader(username);
            chatNotificationComponentView.SetNotificationTargetId(message.sender);
            if (profilePicture != null)
                chatNotificationComponentView.SetImage(profilePicture);
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            chatNotificationComponentView.SetIsPrivate(false);
            chatNotificationComponentView.SetMessage($"{username}: {message.body}");
            chatNotificationComponentView.SetNotificationTargetId("#nearby");
            chatNotificationComponentView.SetNotificationHeader("#nearby");
        }

        chatNotificationComponentView.transform.SetParent(chatEntriesContainer, false);
        chatNotificationComponentView.RefreshControl();
        chatNotificationComponentView.SetTimestamp(UnixTimeStampToLocalTime(message.timestamp));
        chatNotificationComponentView.OnClickedNotification += ClickedOnNotification;
        return chatNotificationComponentView;
    }

    private void ClickedOnNotification(string targetId)
    {
        OnClickedNotification?.Invoke(targetId);
    }

    private void CheckNotificationCountAndRelease()
    {
        if (poolableQueue.Count >= MAX_NOTIFICATION_ENTRIES)
        {
            entryPool.Release(poolableQueue.Dequeue());
            creationQueue2.Dequeue();
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
