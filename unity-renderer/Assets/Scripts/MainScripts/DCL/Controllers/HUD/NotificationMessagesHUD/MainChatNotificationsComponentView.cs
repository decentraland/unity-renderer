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

    private const string NOTIFICATION_POOL_NAME_PREFIX = "NotificationEntriesPool_";
    private const int MAX_NOTIFICATION_ENTRIES = 5;

    Queue<PoolableObject> poolableQueue = new Queue<PoolableObject>();
    Queue<ChatNotificationMessageComponentView> creationQueue2 = new Queue<ChatNotificationMessageComponentView>();
    private Pool entryPool;

    public static MainChatNotificationsComponentView Create()
    {
        return Instantiate(Resources.Load<MainChatNotificationsComponentView>("SocialBarV1/ChatNotificationHUD"));
    }

    public void AddNewChatNotification(ChatMessage message, string profilePicture)
    {
        CheckNotificationCountAndRelease();
        entryPool = GetNotificationEntryPool();
        var newNotification = entryPool.Get();
        
        ChatNotificationMessageComponentView chatNotificationComponentView = newNotification.gameObject.GetComponent<ChatNotificationMessageComponentView>();
        poolableQueue.Enqueue(newNotification);
        creationQueue2.Enqueue(chatNotificationComponentView);


        if (message.messageType == ChatMessage.Type.PRIVATE)
        {
            chatNotificationComponentView.SetIsPrivate(true);
            chatNotificationComponentView.SetMessage(message.body);
            chatNotificationComponentView.SetNotificationHeader(message.sender);
            if (profilePicture != null)
                chatNotificationComponentView.SetImage(profilePicture);
        }
        else if (message.messageType == ChatMessage.Type.PUBLIC)
        {
            chatNotificationComponentView.SetIsPrivate(false);
            chatNotificationComponentView.SetMessage($"{message.sender}: {message.body}");
            chatNotificationComponentView.SetNotificationHeader("#nearby");
        }

        chatNotificationComponentView.transform.SetParent(chatEntriesContainer, false);
        chatNotificationComponentView.RefreshControl();
        //chatNotificationComponentView.SetTimestamp(message.timestamp);
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

    public override void RefreshControl()
    {

    }

}
