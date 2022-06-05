using System;
using System.Collections;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class UnreadWorldNotificationBadgeShould : IntegrationTestSuite_Legacy
{
    private const string UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME = "UnreadWorldNotificationBadge";

    private UnreadWorldNotificationBadge unreadWorldNotificationBadge;
    private ILastReadMessagesService lastReadMessagesService;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        GameObject go = Object.Instantiate((GameObject)Resources.Load(UNREAD_NOTIFICATION_BADGE_RESOURCE_NAME));
        unreadWorldNotificationBadge = go.GetComponent<UnreadWorldNotificationBadge>();
        lastReadMessagesService = Substitute.For<ILastReadMessagesService>();
        unreadWorldNotificationBadge.Initialize(lastReadMessagesService);

        Assert.AreEqual(0, unreadWorldNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification after initialization");
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
        lastReadMessagesService.GetAllUnreadCount().Returns(1);
        lastReadMessagesService.OnUpdated += Raise.Event<Action<string>>("general");

        Assert.AreEqual(1, unreadWorldNotificationBadge.CurrentUnreadMessages, "There should be 1 unread notification");
        Assert.AreEqual(true, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be activated");
        Assert.AreEqual("1", unreadWorldNotificationBadge.notificationText.text, "Notification text should be 1");
    }

    [Test]
    public void ReceiveSeveralUnreadNotifications()
    {
        unreadWorldNotificationBadge.maxNumberToShow = 9;
        
        lastReadMessagesService.GetAllUnreadCount().Returns(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);

        for (var i = 0; i < 10; i++)
        {
            lastReadMessagesService.OnUpdated += Raise.Event<Action<string>>("general");
        }

        Assert.AreEqual(unreadWorldNotificationBadge.maxNumberToShow + 1, unreadWorldNotificationBadge.CurrentUnreadMessages, "There should be [unreadWorldNotificationBadge.maxNumberToShow + 1] unread notifications");
        Assert.AreEqual(true, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be activated");
        Assert.AreEqual($"+{unreadWorldNotificationBadge.maxNumberToShow}", unreadWorldNotificationBadge.notificationText.text, "Notification text should be '+[unreadWorldNotificationBadge.maxNumberToShow]'");
    }

    [Test]
    public void CleanAllUnreadNotifications()
    {
        ReceiveOneUnreadNotification();
        ReadLastMessages();
        
        lastReadMessagesService.OnUpdated += Raise.Event<Action<string>>("general");

        Assert.AreEqual(0, unreadWorldNotificationBadge.CurrentUnreadMessages, "There shouldn't be any unread notification");
        Assert.AreEqual(false, unreadWorldNotificationBadge.notificationContainer.activeSelf, "Notificaton container should be deactivated");
    }

    private void ReadLastMessages() { lastReadMessagesService.GetAllUnreadCount().Returns(0); }
}