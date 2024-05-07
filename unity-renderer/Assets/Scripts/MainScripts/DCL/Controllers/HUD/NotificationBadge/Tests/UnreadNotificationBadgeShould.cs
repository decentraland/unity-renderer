using DCL;
using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class UnreadNotificationBadgeShould : IntegrationTestSuite_Legacy
{
    private const string UNREAD_NOTIFICATION_BADGE_RESOURCE_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/HUD/NotificationBadge/Prefabs/UnreadNotificationBadge.prefab";
    private const string TEST_USER_ID = "testFriend";
    private const string INVALID_TEST_USER_ID = "invalidTestFriend";
    private const string TEST_CHANNEL_ID = "testChannel";
    private const string INVALID_TEST_CHANNEL_ID = "invalidTestChannel";

    private IChatController chatController;
    private UnreadNotificationBadge unreadNotificationBadge;
    private DataStore_Mentions mentionDataStore;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        var go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(UNREAD_NOTIFICATION_BADGE_RESOURCE_PATH));
        unreadNotificationBadge = go.GetComponent<UnreadNotificationBadge>();
        chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedUnseenMessages(TEST_USER_ID).Returns(0);
        mentionDataStore = new DataStore_Mentions();
        unreadNotificationBadge.Initialize(chatController, TEST_USER_ID, mentionDataStore);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages,
            "There shouldn't be any unread notification after initialization");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");

        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(unreadNotificationBadge.gameObject);
        yield break;
    }

    [Test]
    public void ReceiveOneUnreadNotification()
    {
        chatController.GetAllocatedUnseenMessages(TEST_USER_ID).Returns(1);
        chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_USER_ID, 1);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveSeveralUnreadNotifications()
    {
        const int unreadNotificationCount = 10;
        chatController.GetAllocatedUnseenMessages(TEST_USER_ID).Returns(unreadNotificationCount);
        unreadNotificationBadge.maxNumberToShow = 9;

        for (var i = 1; i <= unreadNotificationCount; i++)
            chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_USER_ID, i);

        Assert.AreEqual(unreadNotificationCount, unreadNotificationBadge.CurrentUnreadMessages,
            "There should be [unreadNotificationBadge.maxNumberToShow + 1] unread notifications");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual($"+{unreadNotificationBadge.maxNumberToShow}",
            unreadNotificationBadge.notificationText.text,
            "Notification text should be '+[unreadNotificationBadge.maxNumberToShow]'");
    }

    [Test]
    public void NotReceiveUnreadNotificationsBecauseOfDifferentUser()
    {
        chatController.GetAllocatedUnseenMessages(INVALID_TEST_USER_ID).Returns(1);
        chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(INVALID_TEST_USER_ID, 1);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void CleanAllUnreadNotifications()
    {
        chatController.GetAllocatedUnseenMessages(TEST_USER_ID).Returns(1);
        chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_USER_ID, 1);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be one notification");

        chatController.GetAllocatedUnseenMessages(TEST_USER_ID).Returns(0);
        chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_USER_ID, 0);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void NotCleanUnreadNotificationsBecauseOfDifferentUser()
    {
        chatController.GetAllocatedUnseenMessages(TEST_USER_ID).Returns(1);
        chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_USER_ID, 1);

        chatController.GetAllocatedUnseenMessages(INVALID_TEST_USER_ID).Returns(0);
        chatController.OnUserUnseenMessagesUpdated += Raise.Event<Action<string, int>>(INVALID_TEST_USER_ID, 0);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveOneUnreadChannelNotification()
    {
        unreadNotificationBadge.Initialize(chatController, TEST_CHANNEL_ID, mentionDataStore);
        chatController.GetAllocatedUnseenChannelMessages(TEST_CHANNEL_ID).Returns(1);
        chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_CHANNEL_ID, 1);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveSeveralUnreadChannelNotifications()
    {
        const int unreadNotificationCount = 10;
        unreadNotificationBadge.Initialize(chatController, TEST_CHANNEL_ID, mentionDataStore);
        chatController.GetAllocatedUnseenMessages(TEST_CHANNEL_ID).Returns(unreadNotificationCount);
        unreadNotificationBadge.maxNumberToShow = 9;

        for (var i = 1; i <= unreadNotificationCount; i++)
            chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_CHANNEL_ID, i);

        Assert.AreEqual(unreadNotificationCount, unreadNotificationBadge.CurrentUnreadMessages,
            "There should be [unreadNotificationBadge.maxNumberToShow + 1] unread notifications");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual($"+{unreadNotificationBadge.maxNumberToShow}",
            unreadNotificationBadge.notificationText.text,
            "Notification text should be '+[unreadNotificationBadge.maxNumberToShow]'");
    }

    [Test]
    public void NotReceiveUnreadChannelNotificationsBecauseOfDifferentUser()
    {
        unreadNotificationBadge.Initialize(chatController, TEST_CHANNEL_ID, mentionDataStore);
        chatController.GetAllocatedUnseenChannelMessages(INVALID_TEST_CHANNEL_ID).Returns(1);
        chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(INVALID_TEST_CHANNEL_ID, 1);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void CleanAllUnreadChannelNotifications()
    {
        unreadNotificationBadge.Initialize(chatController, TEST_CHANNEL_ID, mentionDataStore);
        chatController.GetAllocatedUnseenChannelMessages(TEST_CHANNEL_ID).Returns(1);
        chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_CHANNEL_ID, 1);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be one notification");

        chatController.GetAllocatedUnseenChannelMessages(TEST_CHANNEL_ID).Returns(0);
        chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_CHANNEL_ID, 0);

        Assert.AreEqual(0, unreadNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }

    [Test]
    public void NotCleanUnreadChannelNotificationsBecauseOfDifferentUser()
    {
        unreadNotificationBadge.Initialize(chatController, TEST_CHANNEL_ID, mentionDataStore);
        chatController.GetAllocatedUnseenChannelMessages(TEST_CHANNEL_ID).Returns(1);
        chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(TEST_CHANNEL_ID, 1);

        chatController.GetAllocatedUnseenChannelMessages(INVALID_TEST_CHANNEL_ID).Returns(0);
        chatController.OnChannelUnseenMessagesUpdated += Raise.Event<Action<string, int>>(INVALID_TEST_CHANNEL_ID, 0);

        Assert.AreEqual(1, unreadNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadNotificationBadge.notificationText.text, "Notification text should be 1");
    }
}
