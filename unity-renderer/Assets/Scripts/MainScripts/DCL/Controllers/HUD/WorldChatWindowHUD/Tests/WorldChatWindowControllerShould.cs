using System;
using System.Collections.Generic;
using DCL;
using DCL.Browser;
using DCL.Chat;
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
    private IChannelsFeatureFlagService channelsFeatureFlagService;
    private DataStore dataStore;
    private IBrowserBridge browserBridge;

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
        chatController.GetAllocatedChannel("nearby").Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));
        friendsController = Substitute.For<IFriendsController>();
        friendsController.IsInitialized.Returns(true);
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        channelsFeatureFlagService = Substitute.For<IChannelsFeatureFlagService>();
        channelsFeatureFlagService.IsChannelsFeatureEnabled().Returns(true);
        dataStore = new DataStore();
        browserBridge = Substitute.For<IBrowserBridge>();
        controller = new WorldChatWindowController(userProfileBridge,
            friendsController,
            chatController,
            dataStore,
            mouseCatcher,
            socialAnalytics,
            channelsFeatureFlagService,
            browserBridge);
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
                friendshipStatus = FriendshipStatus.NOT_FRIEND
            });

        Received.InOrder(() => view.RemovePrivateChat(FRIEND_ID));
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
        
        view.Received(1).DisableSearchMode();
    }

    [Test]
    public void SearchChannels()
    {
        controller.Initialize(view);
        chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(
            new Channel("channelId", "channelName", 0, 1, true, false, ""));
        view.ClearReceivedCalls();

        view.OnSearchChatRequested += Raise.Event<Action<string>>("nam");

        view.Received(1).EnableSearchMode();
        view.Received(1).SetPublicChat(Arg.Is<PublicChatModel>(p => p.name == "channelName"));
        friendsController.Received(1).GetFriendsWithDirectMessages("nam", 20);
    }

    [Test]
    public void ShowMoreChannelsToLoadHintCorrectly()
    {
        controller.Initialize(view);
        friendsController.TotalFriendsWithDirectMessagesCount.Returns(40);

        controller.SetVisibility(true);
        friendsController.OnAddFriendsWithDirectMessages += Raise.Event<Action<List<FriendWithDirectMessages>>>(
            new List<FriendWithDirectMessages>
            {
                new FriendWithDirectMessages {userId = "bleh", lastMessageBody = "hey", lastMessageTimestamp = 6}
            });
        
        view.Received(1).ShowMoreChatsToLoadHint(10);
    }

    [Test]
    public void HideMoreChannelsToLoadHintCorrectly()
    {
        controller.Initialize(view);
        friendsController.TotalFriendsWithDirectMessagesCount.Returns(26);
        controller.SetVisibility(true);
        view.ClearReceivedCalls();
        
        friendsController.OnAddFriendsWithDirectMessages += Raise.Event<Action<List<FriendWithDirectMessages>>>(
            new List<FriendWithDirectMessages>
            {
                new FriendWithDirectMessages {userId = "bleh", lastMessageBody = "hey", lastMessageTimestamp = 6}
            });
        
        view.Received(1).HideMoreChatsToLoadHint();
    }

    [Test]
    public void RequestFriendsWithDirectMessagesForFirstTime()
    {
        controller.Initialize(view);
        controller.SetVisibility(true);

        friendsController.Received(1).GetFriendsWithDirectMessages(30, 0);
        view.Received(1).ShowPrivateChatsLoading();
        view.Received(1).HideMoreChatsToLoadHint();
    }

    [Test]
    public void RequestFriendsWithDirectMessagesWhenViewRequires()
    {
        controller.Initialize(view);
        controller.SetVisibility(true);
        friendsController.TotalFriendsWithDirectMessagesCount.Returns(42);
        view.ClearReceivedCalls();
        friendsController.ClearReceivedCalls();

        friendsController.OnAddFriendsWithDirectMessages += Raise.Event<Action<List<FriendWithDirectMessages>>>(
            new List<FriendWithDirectMessages>
            {
                new FriendWithDirectMessages {userId = "bleh", lastMessageBody = "hey", lastMessageTimestamp = 6}
            });

        view.OnRequireMorePrivateChats += Raise.Event<Action>();

        friendsController.Received(1).GetFriendsWithDirectMessages(30, 30);
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
            Raise.Event<Action<Channel>>(new Channel("channelId", "channelName", 0, 1, true, false, ""));
        
        socialAnalytics.Received(1).SendEmptyChannelCreated("channelName", ChannelJoinedSource.Search);
    }
    
    [Test]
    public void TrackPopulatedChannelJoined()
    {
        controller.Initialize(view);

        dataStore.channels.channelJoinedSource.Set(ChannelJoinedSource.Link);
        chatController.OnChannelJoined +=
            Raise.Event<Action<Channel>>(new Channel("channelId", "channelName", 0, 2, true, false, ""));
        
        socialAnalytics.Received(1).SendPopulatedChannelJoined("channelName", ChannelJoinedSource.Link);
    }

    [Test]
    public void RemoveChannelWhenLeaveIsConfirmed()
    {
        controller.Initialize(view);
        chatController.GetAllocatedChannel("channelId")
            .Returns(new Channel("channelId", "channelName", 0, 0, true, false, ""));
        
        dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.Command);

        chatController.OnChannelLeft += Raise.Event<Action<string>>("channelId");
        
        socialAnalytics.Received(1).SendLeaveChannel("channelName", ChannelLeaveSource.Command);
        view.Received(1).RemovePublicChat("channelId");
    }

    [Test]
    public void RequestJoinedChannelsWhenChatInitializes()
    {
        controller.Initialize(view);

        chatController.OnInitialized += Raise.Event<Action>();
        
        chatController.Received(1).GetJoinedChannels(10, 0);
    }

    [Test]
    public void ShowConnectWalletWhenIsGuest()
    {
        ownUserProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID, hasConnectedWeb3 = false});
        
        controller.Initialize(view);
        controller.SetVisibility(true);
        
        view.Received(1).ShowConnectWallet();
        view.DidNotReceive().HideConnectWallet();
    }
    
    [Test]
    public void ShowConnectWalletWhenIsGuestAndTheProfileUpdates()
    {
        ownUserProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID, hasConnectedWeb3 = true});
        
        controller.Initialize(view);
        controller.SetVisibility(true);
        view.ClearReceivedCalls();
        
        ownUserProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID, hasConnectedWeb3 = false});
        
        view.Received(1).ShowConnectWallet();
        view.DidNotReceive().HideConnectWallet();
    }

    [Test]
    public void HideConnectWalletWhenIsAuthenticatedUser()
    {
        ownUserProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID, hasConnectedWeb3 = true});
        
        controller.Initialize(view);
        controller.SetVisibility(true);
        
        view.Received(1).HideConnectWallet();
        view.DidNotReceive().ShowConnectWallet();
    }
    
    [Test]
    public void HideConnectWalletWhenIsAuthenticatedUserAndProfileUpdates()
    {
        ownUserProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID, hasConnectedWeb3 = true});
        
        controller.Initialize(view);
        controller.SetVisibility(true);
        view.ClearReceivedCalls();
        
        ownUserProfile.UpdateData(new UserProfileModel{userId = OWN_USER_ID, name = "bleh"});
        
        view.Received(1).HideConnectWallet();
        view.DidNotReceive().ShowConnectWallet();
    }

    [Test]
    public void SignUpWhenViewRequires()
    {
        controller.Initialize(view);
        
        view.OnSignUp += Raise.Event<Action>();
        
        userProfileBridge.Received(1).SignUp();
    }

    [Test]
    public void OpenWalletWebsite()
    {
        controller.Initialize(view);
        
        view.OnRequireWalletReadme += Raise.Event<Action>();
        
        browserBridge.Received(1).OpenUrl("https://docs.decentraland.org/player/blockchain-integration/get-a-wallet/");
    }

    [Test]
    public void ClearOfflineMessagesOnlyTheFirstTime()
    {
        controller.Initialize(view);
        chatController.ClearReceivedCalls();

        chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(
            new Channel("channelId", "channelName", 0, 1, true, false, ""));
        chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(
            new Channel("otherChannelId", "otherChannelName", 0, 1, true, false, ""));
        chatController.OnChannelUpdated += Raise.Event<Action<Channel>>(
            new Channel("channelId", "channelName", 0, 1, true, false, ""));
        
        chatController.Received(1).MarkChannelMessagesAsSeen("channelId");
        chatController.Received(1).MarkChannelMessagesAsSeen("otherChannelId");
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