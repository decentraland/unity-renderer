using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

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
    
    [UnityTest]
    public IEnumerator PopulateSystemChat()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);
    }

    [UnityTest]
    public IEnumerator PopulateReceivedPublicChat()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("<b>user-test:</b> test message", entry.body.text);
    }
    
    [UnityTest]
    public IEnumerator PopulateSentPublicChat()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("test message", entry.body.text);
    }

    [UnityTest]
    public IEnumerator PopulateReceivedWhisperInPublicChannel()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("<b><color=#5EBD3D>From user-test:</color></b> test message", entry.body.text);
    }
    
    [UnityTest]
    public IEnumerator PopulateSentWhisperInPublicChannel()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("<b>To receiver-test:</b> test message", entry.body.text);
    }
    
    [UnityTest]
    public IEnumerator PopulateSentWhisperInPrivateChannel()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("test message", entry.body.text);
    }
    
    [UnityTest]
    public IEnumerator PopulateReceivedWhisperInPrivateChannel()
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
        
        yield return Populate(message);
        
        Assert.AreEqual("test message", entry.body.text);
    }

    IEnumerator Populate(ChatEntryModel message)
    {
        entry.Populate(message);
        
        // To cope with DefaultChatEntry.PopulateTask() inner hack to avoid a client crash due to a TMPro's bug
        yield return null;
        yield return null;
        yield return null;
        yield return null;
    }

    private void GivenEntryChat(string prefabName)
    {
        entry = Object.Instantiate(Resources.Load<DefaultChatEntry>($"SocialBarV1/{prefabName}"),
            canvas.transform, false);
    }
}