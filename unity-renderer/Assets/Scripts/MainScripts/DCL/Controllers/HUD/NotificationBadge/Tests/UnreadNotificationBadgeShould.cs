using System;
using System.Collections;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class UnreadNotificationBadgeShould : IntegrationTestSuite_Legacy
{
    private const string UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME = "UnreadNotificationBadge";
    private const string TEST_USER_ID = "testFriend";
    private const string INVALID_TEST_USER_ID = "invalidTestFriend";

    private ChatController_Mock chatController;
    private UnreadNotificationBadge unreadNotificationBadge;
    private ILastReadMessagesService lastReadMessagesService;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        chatController = new ChatController_Mock();

        GameObject go = Object.Instantiate((GameObject) Resources.Load(UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME));
        unreadNotificationBadge = go.GetComponent<UnreadNotificationBadge>();
        lastReadMessagesService = Substitute.For<ILastReadMessagesService>();
        lastReadMessagesService.GetUnreadCount(TEST_USER_ID).Returns(0);
        unreadNotificationBadge.Initialize(chatController, TEST_USER_ID, lastReadMessagesService);

        CommonScriptableObjects.lastReadChatMessages.Remove(TEST_USER_ID);
        CommonScriptableObjects.lastReadChatMessages.Remove(INVALID_TEST_USER_ID);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages,
            "There shouldn't be any unread notification after initialization");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");

        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(unreadNotificationBadge.gameObject);
        CommonScriptableObjects.lastReadChatMessages.Remove(TEST_USER_ID);
        CommonScriptableObjects.lastReadChatMessages.Remove(INVALID_TEST_USER_ID);
        yield break;
    }

    [Test]
    public void ReceiveOneUnreadNotification()
    {
        lastReadMessagesService.GetUnreadCount(TEST_USER_ID).Returns(1);
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveSeveralUnreadNotifications()
    {
        const int unreadNotificationCount = 10;
        lastReadMessagesService.GetUnreadCount(TEST_USER_ID).Returns(unreadNotificationCount);
        unreadNotificationBadge.maxNumberToShow = 9;
        for (int i = 0; i < unreadNotificationCount; i++)
        {
            chatController.RaiseAddMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.PRIVATE,
                sender = TEST_USER_ID,
                body = $"test body {i + 1}",
                recipient = "test recipient",
                timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        Assert.AreEqual(unreadNotificationCount, unreadNotificationBadge.CurrentUnreadMessages,
            "There should be [unreadNotificationBadge.maxNumberToShow + 1] unread notifications");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual(string.Format("+{0}", unreadNotificationBadge.maxNumberToShow),
            unreadNotificationBadge.notificationText.text,
            "Notification text should be '+[unreadNotificationBadge.maxNumberToShow]'");
    }

    [Test]
    public void NotReceiveUnreadNotificationsBecauseOfPublicMessage()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void NotReceiveUnreadNotificationsBecauseOfDifferentUser()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            sender = INVALID_TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void CleanAllUnreadNotifications()
    {
        lastReadMessagesService.GetUnreadCount(TEST_USER_ID).Returns(1);
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });
        
        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be one notification");
        
        ReadLastMessages(TEST_USER_ID);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void NotCleanUnreadNotificationsBecauseOfFifferentUser()
    {
        ReceiveOneUnreadNotification();
        ReadLastMessages(INVALID_TEST_USER_ID);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    private void ReadLastMessages(string userId)
    {
        lastReadMessagesService.GetUnreadCount(userId).Returns(0);
        lastReadMessagesService.OnUpdated += Raise.Event<Action<string>>(userId);
    }
}