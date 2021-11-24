using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using DCL;
using NSubstitute;

public class ChatHUDShould : IntegrationTestSuite_Legacy
{
    ChatHUDController controller;
    ChatHUDView view;
    ChatMessage lastMsgSent;
    private DataStore dataStore;

    protected override IEnumerator SetUp()
    {
        var profanityFilter = GivenProfanityFilter();
        dataStore = new DataStore();
        dataStore.settings.profanityChatFilteringEnabled.Set(true);
        controller = new ChatHUDController(dataStore, profanityFilter);
        controller.Initialize(null, OnSendMessage);
        view = controller.view;
        Assert.IsTrue(view != null);
        yield break;
    }

    void OnSendMessage(ChatMessage msg) { lastMsgSent = msg; }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield break;
    }

    [Test]
    public void SubmitMessageProperly()
    {
        controller.view.inputField.onSubmit.Invoke("test message");
        Assert.AreEqual("test message", lastMsgSent.body);
    }

    [Test]
    public void TrimWhenTooMuchMessagesAreInView()
    {
        int cacheMaxEntries = ChatHUDController.MAX_CHAT_ENTRIES;
        const int newMaxEntries = 10;
        ChatHUDController.MAX_CHAT_ENTRIES = newMaxEntries;
        for (int i = 0; i < ChatHUDController.MAX_CHAT_ENTRIES + 5; i++)
        {
            var msg = new ChatEntry.Model()
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test" + i,
                bodyText = "test" + i,
            };

            controller.AddChatMessage(msg);
        }

        ChatHUDController.MAX_CHAT_ENTRIES = cacheMaxEntries;
        Assert.AreEqual(newMaxEntries, controller.view.entries.Count);
        Assert.AreEqual("test5", controller.view.entries[0].model.bodyText);
    }

    [Test]
    public void AddAndClearChatEntriesProperly()
    {
        var msg = new ChatEntry.Model()
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test",
        };

        controller.AddChatMessage(msg);

        Assert.AreEqual(1, controller.view.entries.Count);
        Assert.AreEqual(msg, controller.view.entries[0].model);

        controller.view.CleanAllEntries();

        Assert.AreEqual(0, controller.view.entries.Count);

    }

    [Test]
    public void CancelMessageSubmitionByEscapeKey()
    {
        string testMessage = "test message";

        controller.view.FocusInputField();
        controller.view.inputField.text = testMessage;
        controller.view.inputField.ProcessEvent(new UnityEngine.Event { keyCode = UnityEngine.KeyCode.Escape });
        controller.view.inputField.onSubmit.Invoke(testMessage);

        Assert.AreEqual("", lastMsgSent.body);
        Assert.AreEqual(testMessage, controller.view.inputField.text);
    }

    [TestCase("ShiT hello shithead", "**** hello shithead")]
    [TestCase("ass hi grass", "*** hi grass")]
    public void FilterProfanityMessageWithExplicitWords(string body, string expected)
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = body
        };

        controller.AddChatMessage(msg);

        Assert.AreEqual(expected, controller.view.entries[0].model.bodyText);
    }
    
    [TestCase("fuck1 heh bitch", "****1 heh *****")]
    [TestCase("assfuck bitching", "ass**** *****ing")]
    public void FilterProfanityMessageWithNonExplicitWords(string body, string expected)
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = body
        };

        controller.AddChatMessage(msg);

        Assert.AreEqual(expected, controller.view.entries[0].model.bodyText);
    }

    [TestCase("fucker123", "****er123")]
    [TestCase("goodname", "goodname")]
    public void FilterProfanitySenderName(string originalName, string filteredName)
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = originalName,
            bodyText = "test"
        };

        controller.AddChatMessage(msg);

        Assert.AreEqual(filteredName, controller.view.entries[0].model.senderName);
    }
    
    [TestCase("assholeeee", "*******eee")]
    [TestCase("goodname", "goodname")]
    public void FilterProfanityReceiverName(string originalName, string filteredName)
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            recipientName = originalName,
            bodyText = "test"
        };

        controller.AddChatMessage(msg);

        Assert.AreEqual(filteredName, controller.view.entries[0].model.recipientName);
    }

    [Test]
    public void DoNotFilterProfanityMessageWhenFeatureFlagIsDisabled()
    {
        dataStore.settings.profanityChatFilteringEnabled.Set(false);
        
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "shit"
        };
        
        controller.AddChatMessage(msg);
        
        Assert.AreEqual(msg.bodyText, controller.view.entries[0].model.bodyText);
    }

    [Test]
    public void DoNotFilterProfanityMessageWhenIsPrivate()
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "test",
            bodyText = "shit"
        };
        
        controller.AddChatMessage(msg);
        
        Assert.AreEqual(msg.bodyText, controller.view.entries[0].model.bodyText);
    }
    
    private RegexProfanityFilter GivenProfanityFilter()
    {
        var wordProvider = Substitute.For<IProfanityWordProvider>();
        wordProvider.GetExplicitWords().Returns(new[] {"ass", "shit"});
        wordProvider.GetNonExplicitWords().Returns(new[] {"fuck", "bitch", "asshole"});
        return new RegexProfanityFilter(wordProvider);
    }
}