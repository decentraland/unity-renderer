using System;
using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using Cysharp.Threading.Tasks;
using DCL;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

public class ChatHUDControllerShould
{
    private const string OWN_USER_ID = "ownUserId";

    private ChatHUDController controller;
    private IChatHUDComponentView view;
    private DataStore dataStore;

    [SetUp]
    public void SetUp()
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
    }

    [TearDown]
    public void TearDown()
    {
        controller.Dispose();
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

    [UnityTest]
    public IEnumerator TrimWhenTooMuchMessagesAreInView() => UniTask.ToCoroutine(async () =>
    {
        view.EntryCount.Returns(ChatHUDController.MAX_CHAT_ENTRIES + 1);

        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test"
        };

        await controller.AddChatMessage(msg);

        view.Received(1).RemoveOldestEntry();
    });

    [UnityTest]
    public IEnumerator AddChatEntryProperly() => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test"
        };

        await controller.AddChatMessage(msg);

        view.Received(1)
            .AddEntry(Arg.Is<ChatEntryModel>(
                model => model.messageType == msg.messageType && model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
    });

    [TestCase("/w usr hello", "usr", "hello")]
    [TestCase("/w usr im goku", "usr", "im goku")]
    public void SetWhisperPropertiesWhenSendMessage(string text, string recipient, string body)
    {
        ChatMessage msg = null;
        controller.OnSendMessage += m => msg = m;
        
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.NONE, "", text));
        
        Assert.AreEqual(OWN_USER_ID, msg.sender);
        Assert.AreEqual(ChatMessage.Type.PRIVATE, msg.messageType);
        Assert.AreEqual(recipient, msg.recipient);
        Assert.AreEqual(body, msg.body);
    }

    [Test]
    public void SetSenderWhenSendingPublicMessage()
    {
        const string body = "how are you?";
        ChatMessage msg = null;
        controller.OnSendMessage += m => msg = m;
        
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.NONE, "", body));
        
        Assert.AreEqual(OWN_USER_ID, msg.sender);
        Assert.AreEqual(body, msg.body);
    }

    [UnityTest]
    [TestCase("ShiT hello shithead", "**** hello shithead", ExpectedResult = (IEnumerator) null)]
    [TestCase("ass hi grass", "*** hi grass", ExpectedResult = (IEnumerator) null)]
    public IEnumerator FilterProfanityMessageWithExplicitWords(string body, string expected) => UniTask.ToCoroutine(
        async () =>
        {
            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test",
                bodyText = body
            };

            await controller.AddChatMessage(msg);

            view.Received(1)
                .AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{expected}</noparse>"));
        });

    [UnityTest]
    [TestCase("fuck1 heh bitch", "****1 heh *****", ExpectedResult = (IEnumerator) null)]
    [TestCase("assfuck bitching", "ass**** *****ing", ExpectedResult = (IEnumerator) null)]
    public IEnumerator FilterProfanityMessageWithNonExplicitWords(string body, string expected) => UniTask.ToCoroutine(
        async () =>
        {
            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test",
                bodyText = body
            };

            await controller.AddChatMessage(msg);

            view.Received(1)
                .AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{expected}</noparse>"));
        });

    [UnityTest]
    [TestCase("fucker123", "****er123", ExpectedResult = (IEnumerator) null)]
    [TestCase("goodname", "goodname", ExpectedResult = (IEnumerator) null)]
    public IEnumerator FilterProfanitySenderName(string originalName, string filteredName) => UniTask.ToCoroutine(
        async () =>
        {
            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = originalName,
                bodyText = "test"
            };

            controller.AddChatMessage(msg);

            view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.senderName.Equals(filteredName)));
        });

    [UnityTest]
    [TestCase("assholeeee", "*******eee", ExpectedResult = (IEnumerator) null)]
    [TestCase("goodname", "goodname", ExpectedResult = (IEnumerator) null)]
    public IEnumerator FilterProfanityReceiverName(string originalName, string filteredName) => UniTask.ToCoroutine(
        async () =>
        {
            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test",
                recipientName = originalName,
                bodyText = "test"
            };

            await controller.AddChatMessage(msg);

            view.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model => model.recipientName.Equals(filteredName)));
        });

    [UnityTest]
    public IEnumerator DoNotFilterProfanityMessageWhenFeatureFlagIsDisabled() => UniTask.ToCoroutine(async () =>
    {
        dataStore.settings.profanityChatFilteringEnabled.Set(false);

        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "shit"
        };

        await controller.AddChatMessage(msg);

        view.Received(1)
            .AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
    });

    [UnityTest]
    public IEnumerator DoNotFilterProfanityMessageWhenIsPrivate() => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntryModel
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "test",
            bodyText = "shit"
        };

        await controller.AddChatMessage(msg);

        view.Received(1)
            .AddEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
    });

    [Test]
    public void DisplayNextMessageInHistory()
    {
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", "hey"));
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", "sup"));
        view.OnNextChatInHistory += Raise.Event<Action>();
        view.OnNextChatInHistory += Raise.Event<Action>();
        
        Received.InOrder(() =>
        {
            view.SetInputFieldText("sup");
            view.SetInputFieldText("hey");
        });
    }
    
    [Test]
    public void DisplayPreviousMessageInHistory()
    {
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", "hey"));
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", "sup"));
        view.OnPreviousChatInHistory += Raise.Event<Action>();
        view.OnPreviousChatInHistory += Raise.Event<Action>();
        
        Received.InOrder(() =>
        {
            view.SetInputFieldText("hey");
            view.SetInputFieldText("sup");
        });
    }
    
    [Test]
    public void DoNotDuplicateMessagesInHistory()
    {
        const string repeatedMessage = "hey";
        
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", repeatedMessage));
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", repeatedMessage));
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", "bleh"));
        view.OnNextChatInHistory += Raise.Event<Action>();
        view.OnNextChatInHistory += Raise.Event<Action>();
        view.OnNextChatInHistory += Raise.Event<Action>();
        
        Received.InOrder(() =>
        {
            view.SetInputFieldText("bleh");
            view.SetInputFieldText(repeatedMessage);
            view.SetInputFieldText("bleh");
        });
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ResetInputField(bool loseFocus)
    {
        controller.ResetInputField(loseFocus);
        
        view.Received(1).ResetInputField(loseFocus);
    }

    [Test]
    public void UnfocusInputField()
    {
        controller.UnfocusInputField();
        
        view.Received(1).UnfocusInputField();
    }

    [TestCase("hehe")]
    [TestCase("jojo")]
    [TestCase("lelleolololohohoho")]
    public void SetInputFieldText(string text)
    {
        controller.SetInputFieldText(text);
        
        view.Received(1).SetInputFieldText(text);
    }

    [Test]
    public void ClearAllEntries()
    {
        controller.ClearAllEntries();
        
        view.Received(1).ClearAllEntries();
    }

    [Test]
    public void SetChannelJoinSourceWhenJoinCommandIsWritten()
    {
        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage(ChatMessage.Type.PUBLIC, "test", "/join my-channel"));
        
        Assert.AreEqual(ChannelJoinedSource.Command, dataStore.channels.channelJoinedSource.Get());
    }

    private RegexProfanityFilter GivenProfanityFilter()
    {
        var wordProvider = Substitute.For<IProfanityWordProvider>();
        wordProvider.GetExplicitWords().Returns(new[] {"ass", "shit"});
        wordProvider.GetNonExplicitWords().Returns(new[] {"fuck", "bitch", "asshole"});
        return new RegexProfanityFilter(wordProvider);
    }
}