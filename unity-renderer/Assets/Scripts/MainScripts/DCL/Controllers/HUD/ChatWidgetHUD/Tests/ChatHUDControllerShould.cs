using System;
using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using DCL;
using NSubstitute;
using UnityEngine;

public class ChatHUDControllerShould : IntegrationTestSuite_Legacy
{
    private const string OWN_USER_ID = "ownUserId";
    
    private ChatHUDController controller;
    private IChatHUDComponentView view;
    private DataStore dataStore;

    protected override IEnumerator SetUp()
    {
        var profanityFilter = GivenProfanityFilter();
        dataStore = new DataStore();
        dataStore.settings.profanityChatFilteringEnabled.Set(true);
        view = Substitute.For<IChatHUDComponentView>();
        var userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        controller = new ChatHUDController(dataStore, userProfileBridge, true, profanityFilter);
        controller.Initialize(view);
        Assert.IsTrue(view != null);
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield break;
    }

    [Test]
    public void SubmitMessageProperly()
    {
        ChatMessage sentMsg = null;
        void SendMessage(ChatMessage msg) => sentMsg = msg;
        controller.OnSendMessage += SendMessage;
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(
            new ChatMessage(ChatMessage.Type.PUBLIC, "idk", "test message"));
        Assert.AreEqual("test message", sentMsg.body);
        Assert.AreEqual(ChatMessage.Type.PUBLIC, sentMsg.messageType);
        Assert.AreEqual(OWN_USER_ID, sentMsg.sender);
        controller.OnSendMessage -= SendMessage;
    }

    [Test]
    public void TrimWhenTooMuchMessagesAreInView()
    {
        view.EntryCount.Returns(ChatHUDController.MAX_CHAT_ENTRIES + 1);

        var msg = new ChatEntryModel
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
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test"
        };

        controller.AddChatMessage(msg);

        view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.messageType == msg.messageType
                                                                  && model.senderName == msg.senderName
                                                                  && model.bodyText ==
                                                                  $"<noparse>{msg.bodyText}</noparse>"));
    }

    [TestCase("ShiT hello shithead", "**** hello shithead")]
    [TestCase("ass hi grass", "*** hi grass")]
    public void FilterProfanityMessageWithExplicitWords(string body, string expected)
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = body
        };

        controller.AddChatMessage(msg);

        view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{expected}</noparse>"));
    }

    [TestCase("fuck1 heh bitch", "****1 heh *****")]
    [TestCase("assfuck bitching", "ass**** *****ing")]
    public void FilterProfanityMessageWithNonExplicitWords(string body, string expected)
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = body
        };

        controller.AddChatMessage(msg);

        view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{expected}</noparse>"));
    }

    [TestCase("fucker123", "****er123")]
    [TestCase("goodname", "goodname")]
    public void FilterProfanitySenderName(string originalName, string filteredName)
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = originalName,
            bodyText = "test"
        };

        controller.AddChatMessage(msg);

        view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.senderName.Equals(filteredName)));
    }

    [TestCase("assholeeee", "*******eee")]
    [TestCase("goodname", "goodname")]
    public void FilterProfanityReceiverName(string originalName, string filteredName)
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            recipientName = originalName,
            bodyText = "test"
        };

        controller.AddChatMessage(msg);

        view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.recipientName.Equals(filteredName)));
    }

    [Test]
    public void DoNotFilterProfanityMessageWhenFeatureFlagIsDisabled()
    {
        dataStore.settings.profanityChatFilteringEnabled.Set(false);

        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "shit"
        };

        controller.AddChatMessage(msg);

        view.Received(1)
            .AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
    }

    [Test]
    public void DoNotFilterProfanityMessageWhenIsPrivate()
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "test",
            bodyText = "shit"
        };

        controller.AddChatMessage(msg);

        view.Received(1)
            .AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
    }

    private RegexProfanityFilter GivenProfanityFilter()
    {
        var wordProvider = Substitute.For<IProfanityWordProvider>();
        wordProvider.GetExplicitWords().Returns(new[] {"ass", "shit"});
        wordProvider.GetNonExplicitWords().Returns(new[] {"fuck", "bitch", "asshole"});
        return new RegexProfanityFilter(wordProvider);
    }
}