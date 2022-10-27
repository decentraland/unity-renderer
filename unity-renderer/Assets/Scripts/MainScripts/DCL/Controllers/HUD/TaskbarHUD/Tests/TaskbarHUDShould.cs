using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Browser;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Chat.HUD;
using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using UnityEngine;

public class TaskbarHUDShould : IntegrationTestSuite_Legacy
{
    private TaskbarHUDController controller;
    private TaskbarHUDView view;

    private readonly FriendsController_Mock friendsController = new FriendsController_Mock();
    private readonly ChatController_Mock chatController = new ChatController_Mock();

    private PrivateChatWindowController privateChatController;
    private FriendsHUDController friendsHudController;
    private WorldChatWindowController worldChatWindowController;
    private ISocialAnalytics socialAnalytics;
    private IUserProfileBridge userProfileBridge;

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateMouseCatcher());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel{name = "myself", userId = "myUserId"});
        userProfileBridge.GetOwn().Returns(ownProfile);

        controller = new TaskbarHUDController(chatController, Substitute.For<IFriendsController>());
        controller.Initialize(null);
        view = controller.view;

        socialAnalytics = Substitute.For<ISocialAnalytics>();

        Assert.IsTrue(view != null, "Taskbar view is null?");
        Assert.IsTrue(CommonScriptableObjects.isTaskbarHUDInitialized, "Taskbar controller is not initialized?");
    }

    protected override IEnumerator TearDown()
    {
        yield return null;

        privateChatController?.Dispose();
        worldChatWindowController?.Dispose();
        friendsHudController?.Dispose();
        controller.Dispose();

        yield return base.TearDown();
    }

    [Test]
    public void AddWorldChatWindowProperly()
    {
        var chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedChannel("nearby").Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());
        worldChatWindowController = new WorldChatWindowController(
            Substitute.For<IUserProfileBridge>(),
            Substitute.For<IFriendsController>(),
            chatController,
            new DataStore(),
            Substitute.For<IMouseCatcher>(),
            Substitute.For<ISocialAnalytics>(),
            Substitute.For<IChannelsFeatureFlagService>(),
            Substitute.For<IBrowserBridge>());
        worldChatWindowController.Initialize(new GameObject("WorldChatWindowViewMock").AddComponent<WorldChatWindowViewMock>());
        controller.AddWorldChatWindow(worldChatWindowController);

        Assert.IsTrue(worldChatWindowController.View.Transform.parent == view.leftWindowContainer,
            "Chat window isn't inside taskbar window container!");
        Assert.IsTrue(worldChatWindowController.View.IsActive, "Chat window is disabled!");
    }

    [Test]
    public void AddFriendWindowProperly()
    {
        friendsHudController = new FriendsHUDController(new DataStore(), friendsController, userProfileBridge,
            socialAnalytics, chatController,
            Substitute.For<IMouseCatcher>());
        friendsHudController.Initialize(new GameObject("FriendsHUDWindowMock").AddComponent<FriendsHUDWindowMock>());
        controller.AddFriendsWindow(friendsHudController);

        Assert.IsTrue(friendsHudController.View.Transform.parent == view.leftWindowContainer,
            "Friends window isn't inside taskbar window container!");
        Assert.IsTrue(friendsHudController.View.IsActive(), "Friends window is disabled!");
    }

    [Test]
    public void ToggleWindowsProperly()
    {
        privateChatController = new PrivateChatWindowController(
            new DataStore(),
            userProfileBridge,
            this.chatController,
            Substitute.For<IFriendsController>(),
            socialAnalytics,
            Substitute.For<IMouseCatcher>(),
            ScriptableObject.CreateInstance<InputAction_Trigger>());
        privateChatController.Initialize(new GameObject("PrivateChatWindowMock").AddComponent<PrivateChatWindowMock>());
        controller.AddPrivateChatWindow(privateChatController);

        var chatController = Substitute.For<IChatController>();
        chatController.GetAllocatedChannel("nearby").Returns(new Channel("nearby", "nearby", 0, 0, true, false, ""));
        chatController.GetAllocatedEntries().Returns(new List<ChatMessage>());
        worldChatWindowController = new WorldChatWindowController(
            userProfileBridge,
            Substitute.For<IFriendsController>(),
            chatController,
            new DataStore(),
            Substitute.For<IMouseCatcher>(),
            Substitute.For<ISocialAnalytics>(),
            Substitute.For<IChannelsFeatureFlagService>(),
            Substitute.For<IBrowserBridge>());
        worldChatWindowController.Initialize(new GameObject("WorldChatWindowViewMock").AddComponent<WorldChatWindowViewMock>());
        controller.AddWorldChatWindow(worldChatWindowController);

        var publicChatChannelController = new PublicChatWindowController(
            this.chatController,  
            userProfileBridge,
            new DataStore(),
            new RegexProfanityFilter(Substitute.For<IProfanityWordProvider>()),
            Substitute.For<IMouseCatcher>(),
            ScriptableObject.CreateInstance<InputAction_Trigger>());
        publicChatChannelController.Initialize(new GameObject("PublicChatChannelWindowMock").AddComponent<PublicChatPublicWindowMock>());
        controller.AddPublicChatChannel(publicChatChannelController);

        friendsHudController = new FriendsHUDController(new DataStore(), friendsController, userProfileBridge,
            socialAnalytics, this.chatController,
            Substitute.For<IMouseCatcher>());
        friendsHudController.Initialize(new GameObject("FriendsHUDWindowMock").AddComponent<FriendsHUDWindowMock>());
        controller.AddFriendsWindow(friendsHudController);

        var channelSearchController = new SearchChannelsWindowController(this.chatController, Substitute.For<IMouseCatcher>(), new DataStore(),
            Substitute.For<ISocialAnalytics>(), Substitute.For<IUserProfileBridge>(), Substitute.For<IChannelsFeatureFlagService>());
        channelSearchController.Initialize(new GameObject("SearchChannelsWindowMock").AddComponent<SearchChannelsWindowMock>());
        controller.AddChannelSearch(channelSearchController);

        Assert.IsFalse(view.chatButton.toggledOn);

        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);
        Assert.IsTrue(controller.privateChatWindow.View.IsActive);

        //NOTE(Brian): Toggle friends window on and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindow.View.IsActive);
        Assert.IsTrue(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends window off and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindow.View.IsActive);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends on, and then chat button on. Then check if world chat window is hidden and private chat is showing up.
        view.friendsButton.toggleButton.onClick.Invoke();
        view.chatButton.toggleButton.onClick.Invoke();

        Assert.IsTrue(controller.publicChatWindow.View.IsActive);
        Assert.IsFalse(controller.worldChatWindowHud.View.IsActive);
        Assert.IsFalse(controller.friendsHud.View.IsActive());
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
    }
}