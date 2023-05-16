using Cysharp.Threading.Tasks;
using DCL;
using DCL.Chat.HUD;
using DCL.Interface;
using DCL.ProfanityFiltering;
using DCL.Social.Chat;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

public class ChatHUDControllerShould
{
    private const string OWN_USER_ID = "ownUserId";

    private ChatHUDController controller;
    private IChatHUDComponentView view;
    private DataStore dataStore;
    private IProfanityFilter profanityFilter;
    private Func<List<UserProfile>> suggestedProfilesAction;
    private IUserProfileBridge userProfileBridge;

    [SetUp]
    public void SetUp()
    {
        profanityFilter = Substitute.For<IProfanityFilter>();

        profanityFilter.Filter(Arg.Any<string>(), Arg.Any<CancellationToken>())
                       .Returns(info => UniTask.FromResult(info[0].ToString()));

        dataStore = new DataStore();
        dataStore.settings.profanityChatFilteringEnabled.Set(true);
        dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["chat_mentions_enabled"] = true } });
        view = Substitute.For<IChatHUDComponentView>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(new UserProfileModel { userId = OWN_USER_ID });
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        suggestedProfilesAction = () => new List<UserProfile>();

        controller = new ChatHUDController(dataStore, userProfileBridge, true,
            (name, count, token) => UniTask.FromResult(suggestedProfilesAction.Invoke()),
            Substitute.For<ISocialAnalytics>(),
            Substitute.For<IChatController>(),
            profanityFilter);

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

        void SendMessage(ChatMessage msg) =>
            sentMsg = msg;

        controller.OnSendMessage += SendMessage;

        view.OnSendMessage += Raise.Event<Action<ChatMessage>>(
            new ChatMessage(ChatMessage.Type.PUBLIC, "idk", "test message"));

        Assert.AreEqual("test message", sentMsg.body);
        Assert.AreEqual(ChatMessage.Type.PUBLIC, sentMsg.messageType);
        Assert.AreEqual(OWN_USER_ID, sentMsg.sender);
        controller.OnSendMessage -= SendMessage;
    }

    [UnityTest]
    public IEnumerator TrimWhenTooMuchMessagesAreInView() =>
        UniTask.ToCoroutine(async () =>
        {
            view.EntryCount.Returns(ChatHUDController.MAX_CHAT_ENTRIES + 1);

            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test",
                bodyText = "test"
            };

            await controller.SetChatMessage(msg);

            view.Received(1).RemoveOldestEntry();
        });

    [UnityTest]
    public IEnumerator AddChatEntryProperly() =>
        UniTask.ToCoroutine(async () =>
        {
            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test",
                bodyText = "test"
            };

            await controller.SetChatMessage(msg);

            view.Received(1)
                .SetEntry(Arg.Is<ChatEntryModel>(
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
    [TestCase("ShiT hello shithead", "**** hello shithead", ExpectedResult = (IEnumerator)null)]
    [TestCase("ass hi grass", "*** hi grass", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityMessageWithExplicitWords(string body, string expected) =>
        UniTask.ToCoroutine(
            async () =>
            {
                profanityFilter.Filter($"<noparse>{body}</noparse>").Returns(UniTask.FromResult($"<noparse>{expected}</noparse>"));

                var msg = new ChatEntryModel
                {
                    messageType = ChatMessage.Type.PUBLIC,
                    senderName = "test",
                    bodyText = body
                };

                await controller.SetChatMessage(msg);

                view.Received(1)
                    .SetEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{expected}</noparse>"));
            });

    [UnityTest]
    [TestCase("fuck1 heh bitch", "****1 heh *****", ExpectedResult = (IEnumerator)null)]
    [TestCase("assfuck bitching", "ass**** *****ing", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityMessageWithNonExplicitWords(string body, string expected) =>
        UniTask.ToCoroutine(
            async () =>
            {
                profanityFilter.Filter($"<noparse>{body}</noparse>").Returns(UniTask.FromResult($"<noparse>{expected}</noparse>"));

                var msg = new ChatEntryModel
                {
                    messageType = ChatMessage.Type.PUBLIC,
                    senderName = "test",
                    bodyText = body
                };

                await controller.SetChatMessage(msg);

                view.Received(1)
                    .SetEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{expected}</noparse>"));
            });

    [UnityTest]
    [TestCase("fucker123", "****er123", ExpectedResult = (IEnumerator)null)]
    [TestCase("goodname", "goodname", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanitySenderName(string originalName, string filteredName) =>
        UniTask.ToCoroutine(
            async () =>
            {
                profanityFilter.Filter(originalName).Returns(UniTask.FromResult(filteredName));

                var msg = new ChatEntryModel
                {
                    messageType = ChatMessage.Type.PUBLIC,
                    senderName = originalName,
                    bodyText = "test"
                };

                await controller.SetChatMessage(msg);

                view.Received(1).SetEntry(Arg.Is<ChatEntryModel>(model => model.senderName.Equals(filteredName)));
            });

    [UnityTest]
    [TestCase("assholeeee", "*******eee", ExpectedResult = (IEnumerator)null)]
    [TestCase("goodname", "goodname", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityReceiverName(string originalName, string filteredName) =>
        UniTask.ToCoroutine(
            async () =>
            {
                profanityFilter.Filter(originalName).Returns(UniTask.FromResult(filteredName));

                var msg = new ChatEntryModel
                {
                    messageType = ChatMessage.Type.PUBLIC,
                    senderName = "test",
                    recipientName = originalName,
                    bodyText = "test"
                };

                await controller.SetChatMessage(msg);

                view.Received(1).SetEntry(Arg.Is<ChatEntryModel>(model => model.recipientName.Equals(filteredName)));
            });

    [UnityTest]
    public IEnumerator DoNotFilterProfanityMessageWhenFeatureFlagIsDisabled() =>
        UniTask.ToCoroutine(async () =>
        {
            dataStore.settings.profanityChatFilteringEnabled.Set(false);

            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PUBLIC,
                senderName = "test",
                bodyText = "shit"
            };

            await controller.SetChatMessage(msg);

            view.Received(1)
                .SetEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
        });

    [UnityTest]
    public IEnumerator DoNotFilterProfanityMessageWhenIsPrivate() =>
        UniTask.ToCoroutine(async () =>
        {
            var msg = new ChatEntryModel
            {
                messageType = ChatMessage.Type.PRIVATE,
                senderName = "test",
                bodyText = "shit"
            };

            await controller.SetChatMessage(msg);

            view.Received(1)
                .SetEntry(Arg.Is<ChatEntryModel>(model => model.bodyText == $"<noparse>{msg.bodyText}</noparse>"));
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

    [TestCase("@", 1)]
    [TestCase("@u", 2)]
    [TestCase("hey @", 5)]
    public void ShowMentionSuggestions(string text, int cursorPosition)
    {
        UserProfile user1 = GivenProfile("u1", "userName1", "faceU1");
        UserProfile user2 = GivenProfile("u2", "userName2", "faceU2");

        suggestedProfilesAction = () => new List<UserProfile>
        {
            user1,
            user2,
        };

        view.OnMessageUpdated += Raise.Event<Action<string, int>>(text, cursorPosition);

        view.Received(1).ShowMentionSuggestions();

        view.Received(1)
            .SetMentionSuggestions(Arg.Is<List<ChatMentionSuggestionModel>>(l =>
                 l[0].userId == "u1" && l[0].userName == "userName1" && l[0].imageUrl == "faceU1"
                 && l[1].userId == "u2" && l[1].userName == "userName2" && l[1].imageUrl == "faceU2"));
    }

    [TestCase("h", 1)]
    [TestCase("how are you?", 4)]
    [TestCase("my email is something@domain.com", 0)]
    [TestCase("i just wrote an @ ", 18)]
    public void DoNotShowMentionSuggestionsWhenNoPatternIsMatched(string text, int cursorPosition)
    {
        UserProfile user1 = GivenProfile("u1", "userName1", "faceU1");
        UserProfile user2 = GivenProfile("u2", "userName2", "faceU2");

        suggestedProfilesAction = () => new List<UserProfile>
        {
            user1,
            user2,
        };

        view.OnMessageUpdated += Raise.Event<Action<string, int>>(text, cursorPosition);

        view.Received(0).ShowMentionSuggestions();
        view.Received(0).SetMentionSuggestions(Arg.Any<List<ChatMentionSuggestionModel>>());
    }

    [Test]
    public void HideSuggestionsWhenExceptionOccurs()
    {
        view.ClearReceivedCalls();
        suggestedProfilesAction = () => throw new Exception("Intended exception");

        view.OnMessageUpdated += Raise.Event<Action<string, int>>("@", 1);

        view.Received(1).HideMentionSuggestions();
        view.Received(0).ShowMentionSuggestions();
        view.Received(0).SetMentionSuggestions(Arg.Any<List<ChatMentionSuggestionModel>>());
    }

    [UnityTest]
    public IEnumerator FetchRecipientProfileWhenMissing() =>
        UniTask.ToCoroutine(async () =>
        {
            const string SENDER_ID = "senId";
            const string SENDER_NAME = "senderName";
            const string RECIPIENT_ID = "recId";
            const string RECIPIENT_NAME = "recipientName";

            var senderProfile = GivenProfile(SENDER_ID, SENDER_NAME, "");
            userProfileBridge.Get(SENDER_ID).Returns(senderProfile);

            var recipientProfile = GivenProfile(RECIPIENT_ID, RECIPIENT_NAME, "");

            userProfileBridge.RequestFullUserProfileAsync(RECIPIENT_ID, Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(recipientProfile));

            userProfileBridge.Get(RECIPIENT_ID).Returns(null, recipientProfile);

            controller.SetChatMessage(new ChatMessage("msg1", ChatMessage.Type.PRIVATE, SENDER_ID, "hey", 100)
            {
                recipient = RECIPIENT_ID,
            });

            await UniTask.NextFrame();

            userProfileBridge.Received(1).RequestFullUserProfileAsync(RECIPIENT_ID, Arg.Any<CancellationToken>());

            Received.InOrder(() =>
            {
                view.SetEntry(Arg.Is<ChatEntryModel>(c => c.recipientName == RECIPIENT_ID));
                view.SetEntry(Arg.Is<ChatEntryModel>(c => c.recipientName == RECIPIENT_NAME));
            });
        });

    [UnityTest]
    public IEnumerator FetchSenderProfileWhenMissing() =>
        UniTask.ToCoroutine(async () =>
        {
            const string RECIPIENT_ID = "recId";
            const string RECIPIENT_NAME = "recipientName";
            const string SENDER_ID = "senId";
            const string SENDER_NAME = "senderName";

            var recipientProfile = ScriptableObject.CreateInstance<UserProfile>();
            recipientProfile.UpdateData(new UserProfileModel { userId = RECIPIENT_ID, name = RECIPIENT_NAME });
            userProfileBridge.Get(RECIPIENT_ID).Returns(recipientProfile);

            var senderProfile = ScriptableObject.CreateInstance<UserProfile>();
            senderProfile.UpdateData(new UserProfileModel { userId = SENDER_ID, name = SENDER_NAME });

            userProfileBridge.RequestFullUserProfileAsync(SENDER_ID, Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(senderProfile));

            userProfileBridge.Get(SENDER_ID).Returns(null, senderProfile);

            controller.SetChatMessage(new ChatMessage("msg1", ChatMessage.Type.PUBLIC, SENDER_ID, "hey", 100));

            await UniTask.NextFrame();

            userProfileBridge.Received(1).RequestFullUserProfileAsync(SENDER_ID, Arg.Any<CancellationToken>());

            Received.InOrder(() =>
            {
                view.SetEntry(Arg.Is<ChatEntryModel>(c => c.senderName == SENDER_ID));
                view.SetEntry(Arg.Is<ChatEntryModel>(c => c.senderName == SENDER_NAME));
            });
        });

    [UnityTest]
    public IEnumerator ApplyEllipsisFormatWhenProfileIsMissing() =>
        UniTask.ToCoroutine(async () =>
        {
            const string SENDER_ID = "0xfa58d678567fa5678587fd4";
            const string RECIPIENT_ID = "0xfa2d32345f2a5f352af3df";

            userProfileBridge.RequestFullUserProfileAsync(SENDER_ID, Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(GivenProfile(SENDER_ID, "senderName", "")));

            userProfileBridge.Get(SENDER_ID).Returns((UserProfile)null);

            userProfileBridge.RequestFullUserProfileAsync(RECIPIENT_ID, Arg.Any<CancellationToken>())
                             .Returns(UniTask.FromResult(GivenProfile(RECIPIENT_ID, "recipientName", "")));

            userProfileBridge.Get(RECIPIENT_ID).Returns((UserProfile)null);

            controller.SetChatMessage(new ChatMessage("msg1", ChatMessage.Type.PRIVATE, SENDER_ID, "hey", 100)
            {
                recipient = RECIPIENT_ID,
            });

            await UniTask.NextFrame();

            userProfileBridge.Received(1).RequestFullUserProfileAsync(SENDER_ID, Arg.Any<CancellationToken>());

            view.SetEntry(Arg.Is<ChatEntryModel>(c => c.senderName == "0xfa...7fd4" && c.recipientName == "0xfa...f3df"));
        });

    private UserProfile GivenProfile(string userId, string username, string face256)
    {
        UserProfile user1 = ScriptableObject.CreateInstance<UserProfile>();

        user1.UpdateData(new UserProfileModel
        {
            userId = userId,
            name = username,
            snapshots = new UserProfileModel.Snapshots
            {
                face256 = face256,
            },
        });

        return user1;
    }
}
