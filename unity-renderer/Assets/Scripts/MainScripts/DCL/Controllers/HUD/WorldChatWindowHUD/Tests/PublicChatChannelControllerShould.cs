using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Chat.Channels;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PublicChatChannelControllerShould
{
    private const string OWN_USER_ID = "my-user-id";
    private const string TEST_USER_ID = "otherUserId";
    private const string TEST_USER_NAME = "otherUserName";

    private PublicChatWindowController controller;
    private IPublicChatWindowView view;
    private IChatHUDComponentView internalChatView;
    private IChatController chatController;
    private IUserProfileBridge userProfileBridge;
    private IMouseCatcher mouseCatcher;

    [SetUp]
    public void SetUp()
    {
        GivenOwnProfile();
        GivenUser(TEST_USER_ID, TEST_USER_NAME);

        chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedChannel("nearby").Returns(new Channel("nearby", "nearby",
            0, 1, true, false, ""));
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());
        mouseCatcher = Substitute.For<IMouseCatcher>();
        controller = new PublicChatWindowController(
            chatController,
            userProfileBridge,
            new DataStore(),
            new RegexProfanityFilter(Substitute.For<IProfanityWordProvider>()),
            mouseCatcher,
            ScriptableObject.CreateInstance<InputAction_Trigger>());

        view = Substitute.For<IPublicChatWindowView>();
        internalChatView = Substitute.For<IChatHUDComponentView>();
        view.ChatHUD.Returns(internalChatView);
        controller.Initialize(view);
    }

    [TearDown]
    public void TearDown()
    {
        controller.Dispose();
    }

    [Test]
    public void AddEntryWhenMessageReceived()
    {
        var msg = new ChatMessage
        {
            messageType = ChatMessage.Type.PUBLIC,
            body = "test message",
            sender = TEST_USER_ID
        };

        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(msg);

        internalChatView.Received(1).AddEntry(Arg.Is<ChatEntryModel>(model =>
            model.messageType == msg.messageType
            && model.bodyText == $"<noparse>{msg.body}</noparse>"
            && model.senderId == msg.sender));
    }

    [Test]
    public void FilterMessageWhenIsTooOld()
    {
        var msg = new ChatMessage
        {
            messageType = ChatMessage.Type.PRIVATE,
            body = "test message",
            sender = TEST_USER_ID,
            timestamp = 100
        };

        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(msg);

        internalChatView.DidNotReceiveWithAnyArgs().AddEntry(default);
    }

    [Test]
    public void SendPublicMessage()
    {
        internalChatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage
            {body = "test message", messageType = ChatMessage.Type.PUBLIC});
        chatController.Received(1).Send(Arg.Is<ChatMessage>(c => c.body == "test message"
                                                                 && c.sender == OWN_USER_ID
                                                                 && c.messageType == ChatMessage.Type.PUBLIC));
        internalChatView.Received(1).ResetInputField();
        internalChatView.Received(1).FocusInputField();
    }

    [Test]
    public void SendPrivateMessage()
    {
        internalChatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage
            {body = "test message", messageType = ChatMessage.Type.PRIVATE, recipient = TEST_USER_ID});
        chatController.Received(1).Send(Arg.Is<ChatMessage>(c => c.body == $"/w {TEST_USER_ID} test message"
                                                                 && c.sender == OWN_USER_ID
                                                                 && c.messageType == ChatMessage.Type.PRIVATE
                                                                 && c.recipient == TEST_USER_ID));
        internalChatView.Received(1).ResetInputField();
        internalChatView.Received(1).FocusInputField();
    }

    [Test]
    public void CloseWhenButtonPressed()
    {
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        controller.OnClosed += () => isViewActive = false;
        view.IsActive.Returns(info => isViewActive);

        controller.SetVisibility(true);
        Assert.IsTrue(view.IsActive);

        controller.View.OnClose += Raise.Event<Action>();
        Assert.IsFalse(view.IsActive);
    }

    [Test]
    public void ClosePanelWhenMouseIsLocked()
    {
        controller.SetVisibility(true);

        mouseCatcher.OnMouseLock += Raise.Event<Action>();

        view.Received(1).Hide();
    }

    [Test]
    public void ShowPanel()
    {
        controller.SetVisibility(true);

        view.Received(1).Show();
    }

    [Test]
    public void MarkChannelMessagesAsReadCorrectly()
    {
        controller.MarkChannelMessagesAsRead();

        chatController.Received(1).MarkChannelMessagesAsSeen(Arg.Any<string>());
    }

    [Test]
    public void MuteChannel()
    {
        controller.Setup("nearby");
        view.OnMuteChanged += Raise.Event<Action<bool>>(true);

        chatController.Received(1).MuteChannel("nearby");
    }

    [Test]
    public void UnmuteChannel()
    {
        controller.Setup("nearby");
        view.OnMuteChanged += Raise.Event<Action<bool>>(false);

        chatController.Received(1).UnmuteChannel("nearby");
    }

    [Test]
    public void RefreshChannelInformationWhenChannelUpdates()
    {
        controller.Setup("nearby");
        view.ClearReceivedCalls();

        chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(new Channel("nearby", "nearby",
            0, 1, true, true, ""));

        view.Received(1).Configure(Arg.Is<PublicChatModel>(p => p.channelId == "nearby"
                                                                && p.name == "nearby"
                                                                && p.joined == true
                                                                && p.muted == true));
    }

    private void GivenOwnProfile()
    {
        var ownProfileModel = new UserProfileModel
        {
            userId = OWN_USER_ID,
            name = "NO_USER"
        };

        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(ownProfileModel);

        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        userProfileBridge.Get(ownProfileModel.userId).Returns(ownUserProfile);
    }

    private void GivenUser(string userId, string name)
    {
        var testUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testUserProfile.UpdateData(new UserProfileModel
        {
            userId = userId,
            name = name
        });
        userProfileBridge.Get(userId).Returns(testUserProfile);
    }
}