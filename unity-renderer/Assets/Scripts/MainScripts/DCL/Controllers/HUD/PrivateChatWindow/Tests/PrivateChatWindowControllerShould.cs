using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using UnityEngine;
using UnityEngine.TestTools;

public class PrivateChatWindowControllerShould
{
    private const string BLOCKED_FRIEND_ID = "blocked-friend-id";
    private const string FRIEND_ID = "my-user-id-2";
    private const string FRIEND_NAME = "myFriendName";
    private const string OWN_USER_ID = "my-user-id";

    private PrivateChatWindowController controller;
    private IPrivateChatComponentView view;
    private IChatHUDComponentView internalChatView;
    private IUserProfileBridge userProfileBridge;
    private ISocialAnalytics socialAnalytics;
    private IChatController chatController;
    private IFriendsController friendsController;
    private IMouseCatcher mouseCatcher;

    [SetUp]
    public void SetUp()
    {
        view = Substitute.For<IPrivateChatComponentView>();
        internalChatView = Substitute.For<IChatHUDComponentView>();
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        view.ChatHUD.Returns(internalChatView);
        
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        friendsController = Substitute.For<IFriendsController>();

        GivenOwnProfile();
        GivenFriend(FRIEND_ID, FRIEND_NAME, PresenceStatus.ONLINE);
        GivenFriend(BLOCKED_FRIEND_ID, "blockedFriendName", PresenceStatus.OFFLINE);

        chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedEntries().ReturnsForAnyArgs(new List<ChatMessage>());
        chatController.GetPrivateAllocatedEntriesByUser(Arg.Any<string>()).ReturnsForAnyArgs(new List<ChatMessage>());

        mouseCatcher = Substitute.For<IMouseCatcher>();
        controller = new PrivateChatWindowController(
            new DataStore(),
            userProfileBridge,
            chatController,
            friendsController,
            socialAnalytics,
            mouseCatcher,
            ScriptableObject.CreateInstance<InputAction_Trigger>());
    }

    [TearDown]
    public void TearDown()
    {
        controller.Dispose();
    }

    [Test]
    public void ClearAllMessagesWhenInitialize()
    {
        WhenControllerInitializes(FRIEND_ID);
        
        internalChatView.Received(1).ClearAllEntries();
    }

    [Test]
    public void ReceivesOneMessageProperly()
    {
        WhenControllerInitializes(FRIEND_ID);

        var msg1 = new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "message1");
        var msg2 = new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "message2");
        var msg3 = new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "message3");

        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(msg1);
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(msg2);
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(msg3);

        internalChatView.Received(3).AddEntry(Arg.Is<ChatEntryModel>(model =>
            model.messageType == ChatMessage.Type.PRIVATE
            && model.senderId == FRIEND_ID));
    }

    [Test]
    public void SendChatMessageWhenViewTriggers()
    {
        WhenControllerInitializes(FRIEND_ID);

        internalChatView.OnSendMessage += Raise.Event<Action<ChatMessage>>(new ChatMessage {body = "test message"});

        chatController.Received(1).Send(Arg.Is<ChatMessage>(message =>
            message.body == $"/w {FRIEND_NAME} test message"
            && message.recipient == FRIEND_NAME));
    }

    [Test]
    public void CloseWhenCloseButtonIsPressed()
    {
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        view.IsActive.Returns(info => isViewActive);

        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        Assert.IsTrue(isViewActive);

        view.OnClose += Raise.Event<Action>();
        Assert.IsFalse(isViewActive);
    }

    [Test]
    public void TriggerBackWhenViewPressesBack()
    {
        WhenControllerInitializes(FRIEND_ID);

        var eventCalled = false;
        controller.OnBack += () => eventCalled = true;

        controller.SetVisibility(true);
        view.OnPressBack += Raise.Event<Action>();

        Assert.AreEqual(true, eventCalled);
    }

    [Test]
    public void SetupViewCorrectly()
    {
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);

        view.Received(1).Setup(Arg.Is<UserProfile>(u => u.userId == FRIEND_ID), true, false);
    }

    [Test]
    public void SetUpViewAsBlocked()
    {
        WhenControllerInitializes(BLOCKED_FRIEND_ID);
        controller.SetVisibility(true);

        view.Received(1).Setup(Arg.Is<UserProfile>(u => u.userId == BLOCKED_FRIEND_ID), false, true);
    }

    [Test]
    public void AvoidReloadingChatsWhenIsTheSameUser()
    {
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        controller.SetVisibility(false);
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        
        chatController.ReceivedWithAnyArgs(1).GetPrivateMessages(default, default, default);
    }

    [Test]
    public void Show()
    {
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        view.IsActive.Returns(info => isViewActive);
        
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        
        internalChatView.Received(1).FocusInputField();
        view.Received().Setup(Arg.Is<UserProfile>(u => u.userId == FRIEND_ID), true, false);
        view.Received(1).Show();
        Assert.IsTrue(isViewActive);
        chatController.Received(1).MarkMessagesAsSeen(FRIEND_ID);
    }
    
    [Test]
    public void Hide()
    {
        var isViewActive = false;
        view.When(v => v.Show()).Do(info => isViewActive = true);
        view.When(v => v.Hide()).Do(info => isViewActive = false);
        view.IsActive.Returns(info => isViewActive);
        
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        controller.SetVisibility(false);
        
        internalChatView.Received(1).UnfocusInputField();
        view.Received(1).Hide();
        Assert.IsFalse(isViewActive);
    }

    [Test]
    public void HideViewWhenMouseIsLocked()
    {
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        view.IsActive.Returns(true);

        mouseCatcher.OnMouseLock += Raise.Event<Action>();
        
        view.Received(1).Hide();
        internalChatView.Received(1).UnfocusInputField();
    }

    [Test]
    public void ActivatePanel()
    {
        WhenControllerInitializes(FRIEND_ID);
        controller.SetVisibility(true);
        
        view.Received(1).Show();
    }

    [Test]
    public void RequestPrivateMessagesCorrectly()
    {
        controller.Initialize(view);
        string userId = "testId";
        int limit = 30;
        string testMessageId = "testId";

        controller.RequestPrivateMessages(userId, limit, testMessageId);

        view.Received(1).SetLoadingMessagesActive(true);
        chatController.Received(1).GetPrivateMessages(userId, limit, testMessageId);
    }

    [Test]
    public void RequestOldConversationsCorrectly()
    {
        WhenControllerInitializes(FRIEND_ID);
        controller.ConversationUserId = FRIEND_ID;
        GivenPrivateMessages(FRIEND_ID, 3);

        controller.RequestOldConversations();

        view.Received(1).SetOldMessagesLoadingActive(true);
        chatController.Received(1).GetPrivateMessages(
            FRIEND_ID,
            PrivateChatWindowController.USER_PRIVATE_MESSAGES_TO_REQUEST_FOR_SHOW_MORE,
            Arg.Any<string>());
    }

    private void WhenControllerInitializes(string friendId)
    {
        controller.Initialize(view);
        controller.Setup(friendId);
    }

    private void GivenOwnProfile()
    {
        var ownProfileModel = new UserProfileModel
        {
            userId = OWN_USER_ID,
            name = "NO_USER",
            blocked = new List<string> {BLOCKED_FRIEND_ID}
        };

        var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(ownProfileModel);
        
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        userProfileBridge.Get(ownProfileModel.userId).Returns(ownUserProfile);
    }

    private void GivenFriend(string friendId, string name, PresenceStatus presence)
    {
        var testUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testUserProfile.UpdateData(new UserProfileModel
        {
            userId = friendId,
            name = name
        });
        userProfileBridge.Get(friendId).Returns(testUserProfile);
        friendsController.GetUserStatus(testUserProfile.userId).Returns(new UserStatus
        {
            presence = presence,
            friendshipStatus = FriendshipStatus.FRIEND,
        });
    }
    
    private void GivenPrivateMessages(string friendId, int count)
    {
        var messages = new List<ChatMessage>();
        for (var i = 0; i < count; i++)
            messages.Add(new ChatMessage(Guid.NewGuid().ToString(), ChatMessage.Type.PRIVATE, friendId, $"message{i}")
                {recipient = friendId});
        
        chatController.GetAllocatedEntries().ReturnsForAnyArgs(messages);
        chatController.GetPrivateAllocatedEntriesByUser(friendId).ReturnsForAnyArgs(messages);
    }
}