using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class DefaultChatEntryShould : IntegrationTestSuite_Legacy
{
    private DefaultChatEntry entry;
    private Canvas canvas;
    
    protected override IEnumerator SetUp()
    {
        var canvasgo = new GameObject("canvas");
        canvas = canvasgo.AddComponent<Canvas>();
        ((RectTransform) canvas.transform).sizeDelta = new Vector2(500, 500);
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(entry.gameObject);
        Object.Destroy(canvas.gameObject);
        yield break;
    }
    
    [Test]
    public void PopulateSystemChat()
    {
        GivenEntryChat("ChatEntrySystem");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.SYSTEM,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.RECEIVED
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);
    }

    [Test]
    public void PopulateReceivedPublicChat()
    {
        GivenEntryChat("PublicChatEntryReceived");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.RECEIVED
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);
    }
    
    [Test]
    public void PopulateSentPublicChat()
    {
        GivenEntryChat("PublicChatEntrySent");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.SENT
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("test message", entry.body.text);
    }

    [Test]
    public void PopulateReceivedWhisperInPublicChannel()
    {
        GivenEntryChat("PublicChatEntryReceivedWhisper");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.RECEIVED
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("<b><color=#5EBD3D>From user-test:</color></b> test message", entry.body.text);
    }
    
    [Test]
    public void PopulateSentWhisperInPublicChannel()
    {
        GivenEntryChat("PublicChatEntrySentWhisper");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.SENT
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("<b>To receiver-test:</b> test message", entry.body.text);
    }
    
    [Test]
    public void PopulateSentWhisperInPrivateChannel()
    {
        GivenEntryChat("PrivateChatEntrySent");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.SENT
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("test message", entry.body.text);
    }
    
    [Test]
    public void PopulateReceivedWhisperInPrivateChannel()
    {
        GivenEntryChat("PrivateChatEntryReceived");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "test message",
            subType = ChatEntryModel.SubType.RECEIVED
        };
        
        entry.Populate(message);
        
        Assert.AreEqual("test message", entry.body.text);
    }

    private void GivenEntryChat(string prefabName)
    {
        entry = Object.Instantiate(Resources.Load<DefaultChatEntry>($"SocialBarV1/{prefabName}"),
            canvas.transform, false);
    }
}