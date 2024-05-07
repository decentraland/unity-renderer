using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class UnreadWorldNotificationBadgeShould : IntegrationTestSuite_Legacy
{
    private const string UNREAD_NOTIFICATION_BADGE_RESOURCE_PATH = "Assets/Scripts/MainScripts/DCL/Controllers/HUD/NotificationBadge/Prefabs/UnreadWorldNotificationBadge.prefab";

    private UnreadWorldNotificationBadge unreadWorldNotificationBadge;
    private IChatController chatController;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        GameObject go = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(UNREAD_NOTIFICATION_BADGE_RESOURCE_PATH));
        unreadWorldNotificationBadge = go.GetComponent<UnreadWorldNotificationBadge>();
        chatController = Substitute.For<IChatController>();
        unreadWorldNotificationBadge.Initialize(chatController);

        Assert.AreEqual(0, unreadWorldNotificationBadge.CurrentUnreadMessages,
            "There shouldn't be any unread notification after initialization");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");

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
        chatController.TotalUnseenMessages.Returns(1);
        chatController.OnTotalUnseenMessagesUpdated += Raise.Event<Action<int>>(1);

        Assert.AreEqual(1, unreadWorldNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadWorldNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual("1", unreadWorldNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveSeveralUnreadNotifications()
    {
        unreadWorldNotificationBadge.maxNumberToShow = 9;

        for (var i = 1; i <= 10; i++)
            chatController.OnTotalUnseenMessagesUpdated += Raise.Event<Action<int>>(i);

        Assert.AreEqual(unreadWorldNotificationBadge.maxNumberToShow + 1,
            unreadWorldNotificationBadge.CurrentUnreadMessages,
            "There should be [unreadWorldNotificationBadge.maxNumberToShow + 1] unread notifications");
        Assert.AreEqual(true, unreadWorldNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be activated");
        Assert.AreEqual($"+{unreadWorldNotificationBadge.maxNumberToShow}",
            unreadWorldNotificationBadge.notificationText.text,
            "Notification text should be '+[unreadWorldNotificationBadge.maxNumberToShow]'");
    }

    [Test]
    public void CleanAllUnreadNotifications()
    {
        ReceiveOneUnreadNotification();

        chatController.TotalUnseenMessages.Returns(0);
        chatController.OnTotalUnseenMessagesUpdated += Raise.Event<Action<int>>(0);

        Assert.AreEqual(0, unreadWorldNotificationBadge.CurrentUnreadMessages,
            "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf,
            "Notificaton container should be deactivated");
    }
}
