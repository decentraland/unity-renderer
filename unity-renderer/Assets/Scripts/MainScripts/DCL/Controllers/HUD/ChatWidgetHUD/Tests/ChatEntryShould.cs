using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class ChatEntryShould : IntegrationTestSuite_Legacy
{
    ChatEntry entry;
    Canvas canvas;
    protected override IEnumerator SetUp()
    {
        var canvasgo = new GameObject("canvas");
        canvas = canvasgo.AddComponent<Canvas>();
        (canvas.transform as RectTransform).sizeDelta = new Vector2(500, 500);
        var go = Object.Instantiate(Resources.Load("Chat Entry"), canvas.transform, false) as GameObject;
        entry = go.GetComponent<ChatEntry>();
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(entry.gameObject);
        Object.Destroy(canvas.gameObject);
        yield break;
    }

    [Test]
    public void BePopulatedCorrectly()
    {
        var message = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
        };

        entry.Populate(message);

        Assert.AreEqual("<b>user-test:</b>", entry.username.text);
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);

        message.messageType = ChatMessage.Type.PRIVATE;
        message.subType = ChatEntry.Model.SubType.SENT;

        entry.Populate(message);
        Assert.AreEqual("<b>To receiver-test:</b>", entry.username.text);
        Assert.AreEqual("<b>To receiver-test:</b> test message", entry.body.text);

        message.subType = ChatEntry.Model.SubType.RECEIVED;
        entry.Populate(message);
        Assert.AreEqual("<b>From user-test:</b>", entry.username.text);
        Assert.AreEqual("<b>From user-test:</b> test message", entry.body.text);

        message.messageType = ChatMessage.Type.SYSTEM;
        entry.Populate(message);
        Assert.AreEqual("test message", entry.body.text);
    }
}