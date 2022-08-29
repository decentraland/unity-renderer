using System;
using System.Collections.Generic;
using DCL;
using DCL.Chat.Channels;
using DCL.Friends.WebApi;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
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
    private IMouseCatcher mouseCatcher;
    private UserProfile ownUserProfile;
    private ISocialAnalytics socialAnalytics;
    private DataStore dataStore;

    [SetUp]
    public void SetUp()
    {
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownUserProfile.UpdateData(new UserProfileModel {userId = OWN_USER_ID});
        userProfileBridge.GetOwn().Returns(ownUserProfile);
        chatController = Substitute.For<IChatController>();
        mouseCatcher = Substitute.For<IMouseCatcher>();
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());
        chatController.GetAllocatedChannel("nearby").Returns(new Channel("nearby", 0, 0, true, false, "", 0));
        friendsController = Substitute.For<IFriendsController>();
        friendsController.IsInitialized.Returns(true);
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        dataStore = new DataStore();
        controller = new WorldChatWindowController(userProfileBridge,
            friendsController,
            chatController,
            dataStore,
            mouseCatcher,
            socialAnalytics);
        view = Substitute.For<IWorldChatWindowView>();
    }

    [Test]
    public void SetPublicChannelWhenInitialize()
    {
        controller.Initialize(view);

        view.Received(1).SetPublicChat(Arg.Is<PublicChatModel>(p => p.name == "nearby"
                                                                              && p.channelId == "nearby"));
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

    [TestCase(FriendshipStatus.REQUESTED_TO)]
    [TestCase(FriendshipStatus.REQUESTED_FROM)]
    [TestCase(FriendshipStatus.NOT_FRIEND)]
    public void RemovePrivateChatWhenUserIsUpdatedAsNonFriend(FriendshipStatus status)
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
                friendshipStatus = status
            });

        view.Received(1).RemovePrivateChat(FRIEND_ID);
    }

    [TestCase(FriendshipAction.NONE)]
    [TestCase(FriendshipAction.DELETED)]
    [TestCase(FriendshipAction.REJECTED)]
    [TestCase(FriendshipAction.CANCELLED)]
    [TestCase(FriendshipAction.REQUESTED_TO)]
    [TestCase(FriendshipAction.REQUESTED_FROM)]
    public void RemovePrivateChatWhenFriendshipUpdatesAsNonFriend(FriendshipAction action)
    {
        GivenFriend(FRIEND_ID, PresenceStatus.OFFLINE);
        
        controller.Initialize(view);

        friendsController.OnUpdateFriendship += Raise.Event<Action<string, FriendshipAction>>(FRIEND_ID, action);
        
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
    public void TriggerOpenPublicChat()
    {
        var opened = false;
        controller.OnOpenPublicChat += s => opened = s == "nearby";
        controller.Initialize(view);
        view.OnOpenPublicChat += Raise.Event<Action<string>>("nearby");
        
        Assert.IsTrue(opened);
    }
    
    [Test]
    public void TriggerOpenChannel()
    {
        var opened = false;
        controller.OnOpenChannel += s => opened = s == FRIEND_ID;
        controller.Initialize(view);
        view.OnOpenPublicChat += Raise.Event<Action<string>>(FRIEND_ID);
        
        Assert.IsTrue(opened);
    }

    [Test]
    public void ClearChannelFilterWhenSearchIsEmpty()
    {
        controller.Initialize(view);
        view.OnSearchChatRequested += Raise.Event<Action<string>>("");
        
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
        
        view.OnSearchChatRequested += Raise.Event<Action<string>>("near");
        
        view.Received(1).Filter(Arg.Is<Dictionary<string, PrivateChatModel>>(d => d.ContainsKey("nearfr") && d.Count == 1),
            Arg.Is<Dictionary<string, PublicChatModel>>(d => d.ContainsKey("nearby") && d.Count == 1));
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
    public void RequestFriendsWithDirectMessagesForFirstTime()
    {
        controller.Initialize(view);
        controller.SetVisibility(true);

        friendsController.Received(1).GetFriendsWithDirectMessages(50, 0);
        view.Received(1).ShowPrivateChatsLoading();
        view.Received(1).HideMoreChatsToLoadHint();
    }

    [Test]
    public void RequestFriendsWithDirectMessagesWhenViewRequires()
    {
        controller.Initialize(view);
        controller.SetVisibility(true);
        view.PrivateChannelsCount.Returns(42);
        view.ClearReceivedCalls();
        friendsController.ClearReceivedCalls();

        friendsController.OnAddFriendsWithDirectMessages += Raise.Event<Action<List<FriendWithDirectMessages>>>(
            new List<FriendWithDirectMessages>
            {
                new FriendWithDirectMessages {userId = "bleh", lastMessageBody = "hey", lastMessageTimestamp = 6}
            });

        view.OnRequireMorePrivateChats += Raise.Event<Action>();

        friendsController.Received(1).GetFriendsWithDirectMessages(20, 42);
        view.Received(1).ShowMoreChatsLoading();
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
    public void RequestChannelsWhenBecomesVisible()
    {
        controller.Initialize(view);
        controller.SetVisibility(true);
        
        chatController.Received(1).GetJoinedChannels(10, 0);
    }

    [Test]
    public void RequestChannelsWhenFriendsInitializes()
    {
        friendsController.IsInitialized.Returns(false);
        controller.Initialize(view);
        controller.SetVisibility(true);
        view.IsActive.Returns(true);
        friendsController.IsInitialized.Returns(true);
        
        friendsController.OnInitialized += Raise.Event<Action>();
        
        chatController.Received(1).GetJoinedChannels(10, 0);
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

    [Test]
    public void LeaveChannel()
    {
        const string channelId = "channelId";
        
        controller.Initialize(view);

        string channelToLeave = "";
        controller.OnOpenChannelLeave += channelId =>
        {
            channelToLeave = channelId;
        };

        view.OnLeaveChannel += Raise.Event<Action<string>>(channelId);

        Assert.AreEqual(channelToLeave, channelId);
    }

    [Test]
    public void TrackEmptyChannelCreated()
    {
        controller.Initialize(view);

        dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Search);
        chatController.OnChannelJoined +=
            Raise.Event<Action<Channel>>(new Channel("channelId", 0, 1, true, false, "", 0));
        
        socialAnalytics.Received(1).SendEmptyChannelCreated("channelId", ChannelJoinedSource.Search);
    }
    
    [Test]
    public void TrackPopulatedChannelJoined()
    {
        controller.Initialize(view);

        dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Link);
        chatController.OnChannelJoined +=
            Raise.Event<Action<Channel>>(new Channel("channelId", 0, 2, true, false, "", 0));
        
        socialAnalytics.Received(1).SendPopulatedChannelJoined("channelId", ChannelJoinedSource.Link);
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