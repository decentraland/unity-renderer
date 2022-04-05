using System;
using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using DCL;
using NSubstitute;

public class ChatHUDControllerShould : IntegrationTestSuite_Legacy
{
    private ChatHUDController controller;
    private IChatHUDComponentView view;
    private ChatMessage lastMsgSent;
    private DataStore dataStore;

    protected override IEnumerator SetUp()
    {
        var profanityFilter = GivenProfanityFilter();
        dataStore = new DataStore();
        dataStore.settings.profanityChatFilteringEnabled.Set(true);
        view = Substitute.For<IChatHUDComponentView>();
        controller = new ChatHUDController(dataStore, Substitute.For<IUserProfileBridge>(), true, profanityFilter);
        controller.OnSendMessage += OnSendMessage;
        Assert.IsTrue(view != null);
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        controller.OnSendMessage -= OnSendMessage;
        controller.Dispose();
        yield break;
    }

    [Test]
    public void SubmitMessageProperly()
    {
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(
            new ChatMessage(ChatMessage.Type.PUBLIC, "idk", "test message"));
        Assert.AreEqual("test message", lastMsgSent.body);
    }

    [Test]
    public void TrimWhenTooMuchMessagesAreInView()
    {
        view.EntryCount.Returns(ChatHUDController.MAX_CHAT_ENTRIES + 1);
        
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test"
        };
        
        controller.AddChatMessage(msg);
        
        view.Received(1).RemoveFirstEntry();
    }

    [Test]
    public void AddChatEntryProperly()
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test"
        };
        
        controller.AddChatMessage(msg);
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.Equals(msg)));
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
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText.Equals(expected)));
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
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText.Equals(expected)));
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
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.senderName.Equals(filteredName)));
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
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.recipientName.Equals(filteredName)));
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
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText.Equals(msg.bodyText)));
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
        
        view.Received(1).AddEntry(Arg.Is<ChatEntry.Model>(model => model.bodyText.Equals(msg.bodyText)));
    }
    
    private void OnSendMessage(ChatMessage msg) => lastMsgSent = msg;
    
    private RegexProfanityFilter GivenProfanityFilter()
    {
        var wordProvider = Substitute.For<IProfanityWordProvider>();
        wordProvider.GetExplicitWords().Returns(new[] {"ass", "shit"});
        wordProvider.GetNonExplicitWords().Returns(new[] {"fuck", "bitch", "asshole"});
        return new RegexProfanityFilter(wordProvider);
    }
}