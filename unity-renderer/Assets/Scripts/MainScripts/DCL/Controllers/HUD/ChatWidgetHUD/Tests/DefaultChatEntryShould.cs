using System.Collections;
using DCL.Interface;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class DefaultChatEntryShould
{
    private DefaultChatEntry entry;
    private Canvas canvas;
    
    [SetUp]
    public void SetUp()
    {
        var canvasgo = new GameObject("canvas");
        canvas = canvasgo.AddComponent<Canvas>();
        ((RectTransform) canvas.transform).sizeDelta = new Vector2(500, 500);
    }

    [TearDown]
    public void TearDown()
    {
        Object.Destroy(entry.gameObject);
        Object.Destroy(canvas.gameObject);
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

    [UnityTest]
    [TestCase("im at 100,100", "100,100", "im at ", ExpectedResult = null)]
    [TestCase("nah we should go to 0,-122", "0,-122", "nah we should go to ", ExpectedResult = null)]
    public IEnumerator AddParcelCoordinates(string body, string coordinates, string bodyWithoutCoordinates)
    {
        GivenEntryChat("PublicChatEntryReceivedWhisper");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = body,
            subType = ChatEntryModel.SubType.SENT
        };
        
        yield return Populate(message);
        
        Assert.AreEqual($"<b>To receiver-test:</b> {bodyWithoutCoordinates}</noparse><link={coordinates}><color=#4886E3><u>{coordinates}</u></color></link><noparse>", entry.body.text);
    }

    [UnityTest]
    public IEnumerator AddManyParcelCoordinates()
    {
        GivenEntryChat("PublicChatEntryReceivedWhisper");
        
        var message = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "user-test",
            recipientName = "receiver-test",
            bodyText = "perhaps 73,94 then -5,42 and after -36,72",
            subType = ChatEntryModel.SubType.SENT
        };
        
        yield return Populate(message);
        
        Assert.AreEqual("<b>To receiver-test:</b> perhaps </noparse><link=73,94><color=#4886E3><u>73,94</u></color></link><noparse> then </noparse><link=-5,42><color=#4886E3><u>-5,42</u></color></link><noparse> and after </noparse><link=-36,72><color=#4886E3><u>-36,72</u></color></link><noparse>", entry.body.text);
    }

    [UnityTest]
    public IEnumerator DoNotShowUserName()
    {
        GivenEntryChat("PrivateChatEntryReceived");
        entry.showUserName = false;
        
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
    
    [UnityTest]
    public IEnumerator ShowUserName()
    {
        GivenEntryChat("PrivateChatEntryReceived");
        entry.showUserName = true;
        
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