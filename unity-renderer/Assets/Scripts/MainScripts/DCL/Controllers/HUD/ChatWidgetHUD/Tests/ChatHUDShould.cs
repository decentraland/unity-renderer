using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using Cysharp.Threading.Tasks;
using DCL;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;

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

    [UnityTest]
    public IEnumerator TrimWhenTooMuchMessagesAreInView() => UniTask.ToCoroutine(async () =>
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

            await controller.AddChatMessage(msg);
        }

        ChatHUDController.MAX_CHAT_ENTRIES = cacheMaxEntries;
        Assert.AreEqual(newMaxEntries, controller.view.entries.Count);
        Assert.AreEqual(ChatUtils.AddNoParse("test5"), controller.view.entries[0].model.bodyText);
    });

    [UnityTest]
    public IEnumerator AddAndClearChatEntriesProperly() => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntry.Model()
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "test",
        };

        await controller.AddChatMessage(msg);

        Assert.AreEqual(1, controller.view.entries.Count);
        msg.bodyText = ChatUtils.AddNoParse(msg.bodyText);
        Assert.AreEqual(msg, controller.view.entries[0].model);

        controller.view.CleanAllEntries();

        Assert.AreEqual(0, controller.view.entries.Count);
    });

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

    [UnityTest]
    [TestCase("ShiT hello shithead", "**** hello shithead", ExpectedResult = (IEnumerator)null)]
    [TestCase("ass hi grass", "*** hi grass", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityMessageWithExplicitWords(string body, string expected) => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = body
        };

        await controller.AddChatMessage(msg);
        expected = ChatUtils.AddNoParse(expected);
        Assert.AreEqual(expected, controller.view.entries[0].model.bodyText);
    });

    [UnityTest]
    [TestCase("fuck1 heh bitch", "****1 heh *****", ExpectedResult = (IEnumerator)null)]
    [TestCase("assfuck bitching", "ass**** *****ing", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityMessageWithNonExplicitWords(string body, string expected) => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = body
        };

        await controller.AddChatMessage(msg);
        expected = ChatUtils.AddNoParse(expected);
        Assert.AreEqual(expected, controller.view.entries[0].model.bodyText);
    });

    [UnityTest]
    [TestCase("fucker123", "****er123", ExpectedResult = (IEnumerator)null)]
    [TestCase("goodname", "goodname", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanitySenderName(string originalName, string filteredName) => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = originalName,
            bodyText = "test"
        };

        await controller.AddChatMessage(msg);
        msg.bodyText = ChatUtils.AddNoParse(msg.bodyText);
        Assert.AreEqual(filteredName, controller.view.entries[0].model.senderName);
    });

    [UnityTest]
    [TestCase("assholeeee", "*******eee", ExpectedResult = (IEnumerator)null)]
    [TestCase("goodname", "goodname", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityReceiverName(string originalName, string filteredName) => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            recipientName = originalName,
            bodyText = "test"
        };

        await controller.AddChatMessage(msg);
        msg.bodyText = ChatUtils.AddNoParse(msg.bodyText);
        Assert.AreEqual(filteredName, controller.view.entries[0].model.recipientName);
    });

    [UnityTest]
    public IEnumerator DoNotFilterProfanityMessageWhenFeatureFlagIsDisabled() => UniTask.ToCoroutine(async () =>
    {
        dataStore.settings.profanityChatFilteringEnabled.Set(false);

        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PUBLIC,
            senderName = "test",
            bodyText = "shit"
        };

        await controller.AddChatMessage(msg);
        msg.bodyText = ChatUtils.AddNoParse(msg.bodyText);
        Assert.AreEqual(msg.bodyText, controller.view.entries[0].model.bodyText);
    });

    [UnityTest]
    public IEnumerator DoNotFilterProfanityMessageWhenIsPrivate() => UniTask.ToCoroutine(async () =>
    {
        var msg = new ChatEntry.Model
        {
            messageType = ChatMessage.Type.PRIVATE,
            senderName = "test",
            bodyText = "shit"
        };

        await controller.AddChatMessage(msg);
        msg.bodyText = ChatUtils.AddNoParse(msg.bodyText);
        Assert.AreEqual(msg.bodyText, controller.view.entries[0].model.bodyText);
    });
    
    private RegexProfanityFilter GivenProfanityFilter()
    {
        var wordProvider = Substitute.For<IProfanityWordProvider>();
        wordProvider.GetExplicitWords().Returns(new[] {"ass", "shit"});
        wordProvider.GetNonExplicitWords().Returns(new[] {"fuck", "bitch", "asshole"});
        return new RegexProfanityFilter(wordProvider);
    }
}