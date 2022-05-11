using System.Collections;
using System.Collections.Generic;
using DCL;
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

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateMouseCatcher());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        controller = new TaskbarHUDController();
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
        worldChatWindowController = new WorldChatWindowController(
            Substitute.For<IUserProfileBridge>(),
            Substitute.For<IFriendsController>(),
            chatController,
            Substitute.For<ILastReadMessagesService>());
        worldChatWindowController.Initialize(new GameObject("WorldChatWindowViewMock").AddComponent<WorldChatWindowViewMock>());
        controller.AddWorldChatWindow(worldChatWindowController);

        Assert.IsTrue(worldChatWindowController.View.Transform.parent == view.leftWindowContainer,
            "Chat window isn't inside taskbar window container!");
        Assert.IsTrue(worldChatWindowController.View.IsActive, "Chat window is disabled!");
    }

    [Test]
    public void AddFriendWindowProperly()
    {
        friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(null, UserProfile.GetOwnUserProfile(), socialAnalytics,
            new GameObject("FriendsHUDWindowMock").AddComponent<FriendsHUDWindowMock>());
        controller.AddFriendsWindow(friendsHudController);

        Assert.IsTrue(friendsHudController.view.Transform.parent == view.leftWindowContainer,
            "Friends window isn't inside taskbar window container!");
        Assert.IsTrue(friendsHudController.view.IsActive(), "Friends window is disabled!");
    }

    [Test]
    public void ToggleWindowsProperly()
    {
        var userProfileBridge = Substitute.For<IUserProfileBridge>();
        var ownProfile = ScriptableObject.CreateInstance<UserProfile>();
        ownProfile.UpdateData(new UserProfileModel{name = "myself", userId = "myUserId"});
        userProfileBridge.GetOwn().Returns(ownProfile);
        var lastReadMessagesService = Substitute.For<ILastReadMessagesService>();
        privateChatController = new PrivateChatWindowController(
            new DataStore(),
            userProfileBridge,
            chatController,
            Substitute.For<IFriendsController>(),
            ScriptableObject.CreateInstance<InputAction_Trigger>(),
            lastReadMessagesService,
            socialAnalytics);
        privateChatController.Initialize(new GameObject("PrivateChatWindowMock").AddComponent<PrivateChatWindowMock>());
        controller.AddPrivateChatWindow(privateChatController);

        worldChatWindowController = new WorldChatWindowController(
            userProfileBridge,
            Substitute.For<IFriendsController>(),
            chatController,
            lastReadMessagesService);
        worldChatWindowController.Initialize(new GameObject("WorldChatWindowViewMock").AddComponent<WorldChatWindowViewMock>());
        controller.AddWorldChatWindow(worldChatWindowController);

        var publicChatChannelController = new PublicChatChannelController(
            chatController, 
            lastReadMessagesService, 
            userProfileBridge,
            ScriptableObject.CreateInstance<InputAction_Trigger>(),
            new DataStore(),
            new RegexProfanityFilter(Substitute.For<IProfanityWordProvider>()),
            socialAnalytics);
        publicChatChannelController.Initialize(new GameObject("PublicChatChannelWindowMock").AddComponent<PublicChatChannelWindowMock>());
        controller.AddPublicChatChannel(publicChatChannelController);

        friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(friendsController, UserProfile.GetOwnUserProfile(), socialAnalytics);
        controller.AddFriendsWindow(friendsHudController);

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

        //NOTE(Brian): Toggle friends on, and then chat button on. Then check if world chat window is showing up.
        view.friendsButton.toggleButton.onClick.Invoke();
        view.chatButton.toggleButton.onClick.Invoke();

        Assert.IsTrue(controller.worldChatWindowHud.View.IsActive);
        Assert.IsFalse(controller.friendsHud.view.IsActive());
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
    }
}