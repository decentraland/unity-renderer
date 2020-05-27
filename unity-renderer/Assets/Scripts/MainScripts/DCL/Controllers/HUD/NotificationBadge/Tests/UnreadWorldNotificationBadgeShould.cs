using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class UnreadWorldNotificationBadgeShould : TestsBase
{
    private const string UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME = "UnreadWorldNotificationBadge";
    private const string TEST_USER_ID = "testUser";

    private ChatController_Mock chatController;
    private UnreadWorldNotificationBadge unreadWorldNotificationBadge;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        chatController = new ChatController_Mock();

        GameObject go = Object.Instantiate((GameObject)Resources.Load(UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME));
        unreadWorldNotificationBadge = go.GetComponent<UnreadWorldNotificationBadge>();
        unreadWorldNotificationBadge.Initialize(chatController);

        Assert.AreEqual(0, unreadWorldNotificationBadge.currentUnreadMessages, "There shouldn't be any unread notification after initialization");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");

        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(unreadWorldNotificationBadge.gameObject);
        yield break;
    }

    [Test]
    public void ReceiveOneUnreadNotification()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(1, unreadWorldNotificationBadge.currentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be activated");
        Assert.AreEqual("1", unreadWorldNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveSeveralUnreadNotifications()
    {
        unreadWorldNotificationBadge.maxNumberToShow = 9;

        for (int i = 0; i < unreadWorldNotificationBadge.maxNumberToShow + 1; i++)
        {
            chatController.RaiseAddMessage(new ChatMessage
            {
                messageType = ChatMessage.Type.PUBLIC,
                sender = TEST_USER_ID,
                body = string.Format("test body {0}", i + 1),
                recipient = "test recipient",
                timestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        Assert.AreEqual(unreadWorldNotificationBadge.maxNumberToShow + 1, unreadWorldNotificationBadge.currentUnreadMessages, "There should be [unreadWorldNotificationBadge.maxNumberToShow + 1] unread notifications");
        Assert.AreEqual(true, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be activated");
        Assert.AreEqual(string.Format("+{0}", unreadWorldNotificationBadge.maxNumberToShow), unreadWorldNotificationBadge.notificationText.text, "Notification text should be '+[unreadWorldNotificationBadge.maxNumberToShow]'");
    }

    [Test]
    public void NotReceiveUnreadNotificationsBecauseOfPrivateMessage()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            sender = TEST_USER_ID,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(0, unreadWorldNotificationBadge.currentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");
    }

    [Test]
    public void NotReceiveUnreadNotificationsBecauseOfSenderIsThePlayer()
    {
        chatController.RaiseAddMessage(new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            sender = UserProfile.GetOwnUserProfile().userId,
            body = "test body",
            recipient = "test recipient",
            timestamp = (ulong)System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        Assert.AreEqual(0, unreadWorldNotificationBadge.currentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");
    }

    [Test]
    public void CleanAllUnreadNotifications()
    {
        ReceiveOneUnreadNotification();
        ReadLastMessages();

        Assert.AreEqual(0, unreadWorldNotificationBadge.currentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");
    }

    private static void ReadLastMessages()
    {
        CommonScriptableObjects.lastReadWorldChatMessages.Set(System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}
