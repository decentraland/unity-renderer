using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class TaskbarHUDShould : IntegrationTestSuite_Legacy
{
    private TaskbarHUDController controller;
    private TaskbarHUDView view;

    private readonly FriendsController_Mock friendsController = new FriendsController_Mock();
    private readonly ChatController_Mock chatController = new ChatController_Mock();

    private GameObject userProfileGO;
    private PrivateChatWindowController privateChatController;
    private FriendsHUDController friendsHudController;
    private WorldChatWindowController worldChatWindowController;
    private UserProfileController userProfileController;

    protected override List<GameObject> SetUp_LegacySystems()
    {
        List<GameObject> result = new List<GameObject>();
        result.Add(MainSceneFactory.CreateMouseCatcher());
        return result;
    }

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        userProfileGO = new GameObject();
        userProfileController = TestUtils.CreateComponentWithGameObject<UserProfileController>("UserProfileController");

        controller = new TaskbarHUDController();
        controller.Initialize(null, chatController, null);
        view = controller.view;

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
        Object.Destroy(userProfileGO);
        Object.Destroy(userProfileController.gameObject);

        yield return base.TearDown();
    }

    [Test]
    public void AddWorldChatWindowProperly()
    {
        worldChatWindowController = new WorldChatWindowController(
            Substitute.For<IUserProfileBridge>(),
            Substitute.For<IFriendsController>(),
            Substitute.For<IChatController>());
        worldChatWindowController.Initialize(Substitute.For<IWorldChatWindowView>());
        controller.AddWorldChatWindow(worldChatWindowController);

        Assert.IsTrue(worldChatWindowController.View.Transform.parent == view.leftWindowContainer,
            "Chat window isn't inside taskbar window container!");
        Assert.IsTrue(worldChatWindowController.View.IsActive, "Chat window is disabled!");
    }

    [Test]
    public void AddFriendWindowProperly()
    {
        friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(null, UserProfile.GetOwnUserProfile(), chatController);
        controller.AddFriendsWindow(friendsHudController);

        Assert.IsTrue(friendsHudController.view.Transform.parent == view.leftWindowContainer,
            "Friends window isn't inside taskbar window container!");
        Assert.IsTrue(friendsHudController.view.IsActive(), "Friends window is disabled!");
    }

    [Test]
    public void ToggleWindowsProperly()
    {
        privateChatController = new PrivateChatWindowController(new DataStore(),
            Substitute.For<IUserProfileBridge>(),
            chatController,
            Substitute.For<IFriendsController>());
        privateChatController.Initialize(Substitute.For<IPrivateChatComponentView>());
        controller.AddPrivateChatWindow(privateChatController);

        const string badPositionMsg =
            "Anchored position should be zero or it won't be correctly placed inside the taskbar";
        const string badPivotMsg = "Pivot should be zero or it won't be correctly placed inside the taskbar";

        RectTransform rt = privateChatController.view.Transform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        worldChatWindowController = new WorldChatWindowController(
            Substitute.For<IUserProfileBridge>(),
            Substitute.For<IFriendsController>(),
            chatController);
        worldChatWindowController.Initialize(Substitute.For<IWorldChatWindowView>());
        controller.AddWorldChatWindow(worldChatWindowController);

        rt = worldChatWindowController.View.Transform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(friendsController, UserProfile.GetOwnUserProfile(), chatController);
        controller.AddFriendsWindow(friendsHudController);

        rt = friendsHudController.view.Transform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, friendsHudController.view, "test-1");
        TestHelpers_Chat.FakePrivateChatMessageFrom(userProfileController, chatController, "test-1", "test message!");

        var buttonList = view.GetButtonList();

        Assert.AreEqual(4, buttonList.Count, "Chat head is missing when receiving a private message?");

        Assert.IsFalse(view.chatButton.toggledOn);
        Assert.IsTrue(buttonList[2] is ChatHeadButton);

        ChatHeadButton headButton = buttonList[2] as ChatHeadButton;
        Assert.IsFalse(headButton.toggledOn);
        Assert.IsTrue(headButton.toggleButton.interactable);

        //NOTE(Brian): Toggle chat head on and test it works as intended
        headButton.toggleButton.onClick.Invoke();

        Assert.IsTrue(headButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);
        Assert.IsTrue(controller.privateChatWindow.view.IsActive);

        //NOTE(Brian): Toggle friends window on and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindow.view.IsActive);
        Assert.IsFalse(headButton.lineOnIndicator.isVisible);
        Assert.IsTrue(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends window off and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindow.view.IsActive);
        Assert.IsFalse(headButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends on, and then chat button on. Then check if world chat window is showing up.
        view.friendsButton.toggleButton.onClick.Invoke();
        view.chatButton.toggleButton.onClick.Invoke();

        Assert.IsTrue(controller.worldChatWindowHud.View.IsActive);
        Assert.IsFalse(controller.friendsHud.view.IsActive());
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
    }
}