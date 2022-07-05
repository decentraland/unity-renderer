using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using DCL.Interface;

public class MainChatNotificationsComponentViewShould : MonoBehaviour
{
    private MainChatNotificationsComponentView view;

    [SetUp]
    public void SetUp()
    {
        view = MainChatNotificationsComponentView.Create();
    }

    [TearDown]
    public void TearDown()
    {
        view.Dispose();
    }

    [Test]
    public void Show()
    {
        view.Show();
        Assert.IsTrue(view.gameObject.activeSelf);
    }

    [Test]
    public void Hide()
    {
        view.Hide();
        Assert.IsFalse(view.gameObject.activeSelf);
    }

    [Test]
    public void CreateWithoutEntries()
    {
        Assert.IsTrue(view.creationQueue2.Count == 0);
        Assert.IsTrue(view.poolableQueue.Count == 0);
    }

    [Test]
    public void AddPrivateNotificationCorrectly()
    {
        ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PRIVATE, "0x00000ba", "This is a test message");
        view.AddNewChatNotification(newMessage, "UsernameTest");

        Assert.IsTrue(view.creationQueue2.Count == 1);
        Assert.IsTrue(view.poolableQueue.Count == 1);
        ChatNotificationMessageComponentView addedNotification = view.creationQueue2.Dequeue();

        Assert.AreEqual(newMessage.body, addedNotification.model.message);
        Assert.AreEqual(newMessage.body, addedNotification.notificationMessage.text);
        Assert.AreEqual("UsernameTest", addedNotification.model.messageHeader);
        Assert.AreEqual("UsernameTest", addedNotification.notificationHeader.text);
        Assert.AreEqual("0x00000ba", addedNotification.model.notificationTargetId);
    }

    [Test]
    public void AddPublicNotificationCorrectly()
    {
        ChatMessage newMessage = new ChatMessage(ChatMessage.Type.PUBLIC, "0x00000ba", "This is a test message");
        view.AddNewChatNotification(newMessage, "UsernameTest");

        Assert.IsTrue(view.creationQueue2.Count == 1);
        Assert.IsTrue(view.poolableQueue.Count == 1);
        ChatNotificationMessageComponentView addedNotification = view.creationQueue2.Dequeue();

        Assert.AreEqual($"UsernameTest: {newMessage.body}", addedNotification.model.message);
        Assert.AreEqual($"UsernameTest: {newMessage.body}", addedNotification.notificationMessage.text);
        Assert.AreEqual("#nearby", addedNotification.model.messageHeader);
        Assert.AreEqual("#nearby", addedNotification.notificationHeader.text);
        Assert.AreEqual("#nearby", addedNotification.model.notificationTargetId);
    }
}
