using System;
using System.Collections.Generic;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class WorldChatWindowControllerShould
{
    private const string OWN_USER_ID = "myId";
    private const string FRIEND_ID = "friendId";

    private IUserProfileBridge userProfileBridge;
    private WorldChatWindowController controller;
    private IWorldChatWindowView view;
    private IChatController chatController;
    private IFriendsController friendsController;
    private UserProfile ownUserProfile;

    [SetUp]
    public void SetUp()
    {
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());
        friendsController = Substitute.For<IFriendsController>();
        friendsController.IsInitialized.Returns(true);
        controller = new WorldChatWindowController(userProfileBridge,
            friendsController,
            chatController);
        view = Substitute.For<IWorldChatWindowView>();
    }

    [Test]
    public void SetPublicChannelWhenInitialize()
    {
        controller.Initialize(view);

        view.Received(1).SetPublicChannel(Arg.Is<PublicChatChannelModel>(p => p.name == "nearby"
                                                                              && p.channelId == "general"));
    }

    [Test]
    public void FillPrivateChatsWhenInitialize()
    {
        GivenFriend(FRIEND_ID, PresenceStatus.ONLINE);
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>
        {
            new ChatMessage(ChatMessage.Type.PUBLIC, "user2", "hey"),
            new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "wow"),
            new ChatMessage(ChatMessage.Type.SYSTEM, "system", "welcome")
        });

        controller.Initialize(view);

        view.Received(1).SetPrivateChat(Arg.Is<PrivateChatModel>(p => !p.isBlocked
                                                                      && p.isOnline
                                                                      && !p.isBlocked
                                                                      && p.recentMessage.body == "wow"));
    }

    [Test]
    public void ShowPrivateChatWhenMessageIsAdded()
    {
        const string messageBody = "wow";
        
        GivenFriend(FRIEND_ID, PresenceStatus.OFFLINE);
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());

        controller.Initialize(view);
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(
            new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, messageBody));

        view.Received(1).SetPrivateChat(Arg.Is<PrivateChatModel>(p => !p.isBlocked
                                                                      && !p.isOnline
                                                                      && !p.isBlocked
                                                                      && p.recentMessage.body == messageBody));
    }

    [Test]
    public void UpdatePresenceStatus()
    {
        GivenFriend(FRIEND_ID, PresenceStatus.OFFLINE);
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>
        {
            new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "wow"),
        });
        
        controller.Initialize(view);
        friendsController.OnUpdateUserStatus += Raise.Event<Action<string, UserStatus>>(
            FRIEND_ID,
            new UserStatus
            {
                userId = FRIEND_ID,
                presence = PresenceStatus.ONLINE,
                friendshipStatus = FriendshipStatus.FRIEND
            });
        
        Received.InOrder(() =>
        {
            view.SetPrivateChat(Arg.Is<PrivateChatModel>(p => !p.isOnline));
            view.SetPrivateChat(Arg.Is<PrivateChatModel>(p => p.isOnline));
        });
    }
    
    [Test]
    public void RemovePrivateChatWhenFriendIsRemoved()
    {
        GivenFriend(FRIEND_ID, PresenceStatus.OFFLINE);
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>
        {
            new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "wow"),
        });
        
        controller.Initialize(view);
        friendsController.OnUpdateUserStatus += Raise.Event<Action<string, UserStatus>>(
            FRIEND_ID,
            new UserStatus
            {
                userId = FRIEND_ID,
                presence = PresenceStatus.ONLINE,
                friendshipStatus = FriendshipStatus.NOT_FRIEND
            });
        
        view.Received(1).RemovePrivateChat(FRIEND_ID);
    }

    [Test]
    public void ShowPrivateChatsLoadingWhenAuthenticatedWithWallet()
    {
        friendsController.IsInitialized.Returns(false);
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID, hasConnectedWeb3 = true});
        
        controller.Initialize(view);
        
        view.Received(1).ShowPrivateChatsLoading();
    }
    
    [Test]
    public void DoNotShowChatsLoadingWhenIsGuestUser()
    {
        friendsController.IsInitialized.Returns(false);
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID, hasConnectedWeb3 = false});
        
        controller.Initialize(view);
        
        view.DidNotReceive().ShowPrivateChatsLoading();
    }

    [Test]
    public void HideChatsLoadingWhenUserUpdatesAsGuest()
    {
        friendsController.IsInitialized.Returns(false);
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID, hasConnectedWeb3 = true});
        
        controller.Initialize(view);
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID, hasConnectedWeb3 = false});
        
        view.Received(1).HidePrivateChatsLoading();
    }
    
    [Test]
    public void HideChatsLoadWhenFriendsIsInitialized()
    {
        friendsController.IsInitialized.Returns(false);
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID, hasConnectedWeb3 = true});
        
        controller.Initialize(view);
        friendsController.OnInitialized += Raise.Event<Action>();
        
        view.Received(1).HidePrivateChatsLoading();
    }

    [Test]
    public void Show()
    {
        var openTriggered = false;
        controller.OnOpen += () => openTriggered = true;
        
        controller.Initialize(view);
        controller.SetVisibility(true);
        
        view.Received(1).Show();
        Assert.IsTrue(openTriggered);
    }
    
    [Test]
    public void Hide()
    {
        controller.Initialize(view);
        controller.SetVisibility(false);
        
        view.Received(1).Hide();
    }
    
    [Test]
    public void UpdatePrivateChatWhenTooManyEntries()
    {
        GivenFriend(FRIEND_ID, PresenceStatus.ONLINE);
        view.PrivateChannelsCount.Returns(999999);
        view.ContainsPrivateChannel(FRIEND_ID).Returns(true);
        
        controller.Initialize(view);
        chatController.OnAddMessage += Raise.Event<Action<ChatMessage>>(
            new ChatMessage(ChatMessage.Type.PRIVATE, FRIEND_ID, "wow"));
        
        view.Received(1).SetPrivateChat(Arg.Is<PrivateChatModel>(p => p.user.userId == FRIEND_ID));
        view.DidNotReceiveWithAnyArgs().ShowMoreChatsToLoadHint(default);
    }

    [Test]
    public void HideWhenRequested()
    {
        controller.Initialize(view);
        view.OnClose += Raise.Event<Action>();
        
        view.Received(1).Hide();
    }

    [Test]
    public void TriggerOpenPrivateChat()
    {
        var opened = false;
        controller.OnOpenPrivateChat += s => opened = s == FRIEND_ID;
        controller.Initialize(view);
        view.OnOpenPrivateChat += Raise.Event<Action<string>>(FRIEND_ID);
        
        Assert.IsTrue(opened);
    }
    
    [Test]
    public void TriggerOpenPublicChannel()
    {
        var opened = false;
        controller.OnOpenPublicChannel += s => opened = s == FRIEND_ID;
        controller.Initialize(view);
        view.OnOpenPublicChannel += Raise.Event<Action<string>>(FRIEND_ID);
        
        Assert.IsTrue(opened);
    }

    [Test]
    public void ClearChannelFilterWhenSearchIsEmpty()
    {
        controller.Initialize(view);
        view.OnSearchChannelRequested += Raise.Event<Action<string>>("");
        
        view.Received(1).ClearFilter();
    }

    [Test]
    public void SearchChannels()
    {
        GivenFriend("nearfr", PresenceStatus.OFFLINE);
        GivenFriend("fr2", PresenceStatus.OFFLINE);
        GivenFriend("fr3", PresenceStatus.OFFLINE);
        GivenFriend("fr4", PresenceStatus.OFFLINE);
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>
        {
            new ChatMessage(ChatMessage.Type.PRIVATE, "nearfr", "wow"),
            new ChatMessage(ChatMessage.Type.PRIVATE, "fr2", "wow"),
            new ChatMessage(ChatMessage.Type.PRIVATE, "fr3", "wow"),
            new ChatMessage(ChatMessage.Type.PRIVATE, "fr4", "wow"),
        });
        
        controller.Initialize(view);
        
        view.OnSearchChannelRequested += Raise.Event<Action<string>>("near");
        
        view.Received(1).Filter(Arg.Is<Dictionary<string, PrivateChatModel>>(d => d.ContainsKey("nearfr") && d.Count == 1),
            Arg.Is<Dictionary<string, PublicChatChannelModel>>(d => d.ContainsKey("general") && d.Count == 1));
    }

    [Test]
    [TestCase(10)]
    [TestCase(5)]
    public void UpdateMoreChannelsToLoadHintCorrectly(int currentPrivateChannelsCount)
    {
        controller.Initialize(view);
        int totalFriends = 10;
        friendsController.TotalFriendsWithDirectMessagesCount.Returns(totalFriends);
        view.PrivateChannelsCount.Returns(currentPrivateChannelsCount);

        controller.UpdateMoreChannelsToLoadHint();

        if (totalFriends - currentPrivateChannelsCount == 0)
            view.Received(1).HideMoreChatsToLoadHint();
        else
            view.Received(1).ShowMoreChatsToLoadHint(totalFriends - currentPrivateChannelsCount);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RequestFriendsWithDirectMessagesCorrectly(bool requestedByFirstTime)
    {
        controller.Initialize(view);
        int limit = 30;
        long timestamp = 500;
        controller.isRequestingFriendsWithDMs = false;
        controller.areDMsRequestedByFirstTime = requestedByFirstTime;

        controller.RequestFriendsWithDirectMessages(limit, timestamp);

        Assert.IsTrue(controller.isRequestingFriendsWithDMs);

        if (!requestedByFirstTime)
        {
            view.Received(1).ShowPrivateChatsLoading();
            view.Received(1).HideMoreChatsToLoadHint();
        }
        else
            view.Received(1).ShowMoreChatsLoading();

        friendsController.Received(1).GetFriendsWithDirectMessages(limit, timestamp);
        Assert.IsTrue(controller.areDMsRequestedByFirstTime);
    }

    [Test]
    public void RequestFriendsWithDirectMessagesFromSearchCorrectly()
    {
        controller.Initialize(view);
        string userName = "test";
        int limit = 30;

        controller.RequestFriendsWithDirectMessagesFromSearch(userName, limit);

        view.Received(1).ShowSearchLoading();
        friendsController.Received(1).GetFriendsWithDirectMessages(userName, limit);
    }

    [Test]
    public void RequestUnreadMessagesWhenIsVisible()
    {
        controller.Initialize(view);
        controller.SetVisibility(true);
        
        chatController.Received(1).GetUnseenMessagesByUser();
    }

    [Test]
    public void RequestUnreadMessagesWhenFriendsInitializes()
    {
        controller.Initialize(view);
        friendsController.IsInitialized.Returns(false);
        controller.SetVisibility(true);
        friendsController.IsInitialized.Returns(true);
        view.IsActive.Returns(true);
        friendsController.OnInitialized += Raise.Event<Action>();
        
        chatController.Received(1).GetUnseenMessagesByUser();
    }

    private void GivenFriend(string friendId, PresenceStatus presence)
    {
        var friendProfile = ScriptableObject.CreateInstance<UserProfile>();
        friendProfile.UpdateData(new UserProfileModel {userId = friendId, name = friendId});
        userProfileBridge.Get(friendId).Returns(friendProfile);
        friendsController.IsFriend(friendId).Returns(true);
        friendsController.GetUserStatus(friendId).Returns(new UserStatus
            {userId = friendId, presence = presence, friendshipStatus = FriendshipStatus.FRIEND});
    }
}