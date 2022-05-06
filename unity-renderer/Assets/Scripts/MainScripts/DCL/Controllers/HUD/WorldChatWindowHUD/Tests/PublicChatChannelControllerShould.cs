using DCL;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics.TestHelpers;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class PublicChatChannelControllerShould : IntegrationTestSuite_Legacy
{
    private PublicChatChannelController controller;
    private IChannelChatWindowView view;
    private IChatHUDComponentView internalChatView;
    private IChatController chatController;
    private UserProfileModel ownProfileModel;
    private UserProfileModel testProfileModel;
    private IUserProfileBridge userProfileBridge;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfileModel = new UserProfileModel
        {
            userId = "my-user-id",
            name = "NO_USER"
        };
        ownProfile.UpdateData(ownProfileModel);

        var testProfile = ScriptableObject.CreateInstance<UserProfile>();
        testProfileModel = new UserProfileModel
        {
            userId = "my-user-id-2",
            name = "TEST_USER"
        };
        testProfile.UpdateData(testProfileModel);
        
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.Get(ownProfileModel.userId).Returns(ownProfile);
        userProfileBridge.Get(testProfileModel.userId).Returns(testProfile);
        userProfileBridge.GetOwn().Returns(ownProfile);

        chatController = Substitute.For<IChatController>();
        controller = new PublicChatChannelController(
            chatController,
            Substitute.For<ILastReadMessagesService>(),
            userProfileBridge,
            ScriptableObject.CreateInstance<InputAction_Trigger>(),
            new DataStore(),
            new RegexProfanityFilter(Substitute.For<IProfanityWordProvider>()),
            SocialAnalyticsTestHelpers.CreateMockedSocialAnalytics());

        view = Substitute.For<IChannelChatWindowView>();
        internalChatView = Substitute.For<IChatHUDComponentView>();
        view.ChatHUD.Returns(internalChatView);
        controller.Initialize(view);

        Assert.IsTrue(view != null, "World chat hud view is null?");
        Assert.IsTrue(controller != null, "World chat hud controller is null?");
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void AddEntryWhenPrivateMessage()
    {
        var sentPM = new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = ownProfileModel.userId,
            recipient = testProfileModel.userId,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(sentPM);

        internalChatView.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model =>
            model.messageType == sentPM.messageType
            && model.bodyText == $"<noparse>{sentPM.body}</noparse>"
            && model.senderId == sentPM.sender
            && model.otherUserId == sentPM.recipient));
    }

    [Test]
    public void AddEntryWhenMessageReceived()
    {
        var chatMessage = new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            body = "test message",
            sender = testProfileModel.userId
        };

        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(chatMessage);

        internalChatView.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model =>
            model.messageType == chatMessage.messageType
            && model.bodyText == $"<noparse>{chatMessage.body}</noparse>"
            && model.senderId == chatMessage.sender));
    }

    [Test]
    public void SendChatMessageProperly()
    {
        controller.SendChatMessage(new ChatMessage {body = "test message"});
        chatController.Received(1).Send(Arg.Is<ChatMessage>(c => c.body == "test message"));
    }

    [Test]
    public void CloseWhenButtonPressed()
    {
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        void HandleControllerClosed() => isViewActive = false;
        controller.OnClosed += HandleControllerClosed;
        view.IsActive.Returns(info => isViewActive);
        
        controller.SetVisibility(true);
        Assert.IsTrue(view.IsActive);
        
        controller.View.OnClose += Raise.Event<Action>();
        Assert.IsFalse(view.IsActive);
        controller.OnClosed -= HandleControllerClosed;
    }

    [UnityTest]
    public IEnumerator ReplyToWhisperingSender()
    {
        var msg = new ChatMessage
        {
            body = "test message",
            sender = testProfileModel.userId,
            recipient = ownProfileModel.userId,
            messageType = ChatMessage.Type.PRIVATE,
            timestamp = (ulong) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };

        yield return null;

        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(msg);

        yield return null;

        Assert.AreEqual(controller.lastPrivateMessageRecipient, testProfileModel.name);

        internalChatView.OnMessageUpdated += Raise.Event<Action<string>>("/r ");

        internalChatView.Received(1).SetInputFieldText($"/w {testProfileModel.name} ");
    }
}