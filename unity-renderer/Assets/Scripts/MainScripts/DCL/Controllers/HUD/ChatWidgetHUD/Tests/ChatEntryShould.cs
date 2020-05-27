using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
public class ChatEntryShould : TestsBase
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
        var message = new ChatEntry.Model()
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "user-test",
            recipientName = "",
            bodyText = "test message",
        };

        entry.Populate(message);

        Assert.AreEqual(entry.worldMessageColor, entry.body.color);
        Assert.AreEqual("<b>user-test:</b>", entry.username.text);
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);

        message.messageType = ChatMessage.Type.PRIVATE;
        message.subType = ChatEntry.Model.SubType.PRIVATE_TO;

        entry.Populate(message);
        Assert.AreEqual(entry.privateToMessageColor, entry.username.color);

        message.subType = ChatEntry.Model.SubType.PRIVATE_FROM;
        entry.Populate(message);
        Assert.AreEqual(entry.privateFromMessageColor, entry.username.color);

        message.messageType = ChatMessage.Type.SYSTEM;
        entry.Populate(message);
        Assert.AreEqual(entry.systemColor, entry.body.color);
    }
}
