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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/ChatEntrySystem.prefab");
        
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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/PublicChatEntryReceived.prefab");
        
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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/PublicChatEntrySent.prefab");
        
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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/PublicChatEntryReceivedWhisper.prefab");
        
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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/PublicChatEntrySentWhisper.prefab");
        
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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/PrivateChatEntrySent.prefab");
        
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
        GivenEntryChat("Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/Chat/PrivateChatEntryReceived.prefab");
        
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

    private void GivenEntryChat(string path)
    {
        entry = Object.Instantiate(AssetDatabase.LoadAssetAtPath<DefaultChatEntry>(path),
            canvas.transform, false);
    }
}