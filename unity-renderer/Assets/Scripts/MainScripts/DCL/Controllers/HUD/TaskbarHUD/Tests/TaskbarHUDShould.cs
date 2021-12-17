using DCL.SettingsPanelHUD;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine;

public class TaskbarHUDShould : IntegrationTestSuite_Legacy
{
    private TaskbarHUDController controller;
    private TaskbarHUDView view;

    private readonly FriendsController_Mock friendsController = new FriendsController_Mock();
    private readonly ChatController_Mock chatController = new ChatController_Mock();

    private GameObject userProfileGO;
    private PrivateChatWindowHUDController privateChatController;
    private FriendsHUDController friendsHudController;
    private WorldChatWindowHUDController worldChatWindowController;
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
        controller.Initialize(null, chatController, null, null, null);
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
        worldChatWindowController = new WorldChatWindowHUDController();
        worldChatWindowController.Initialize(null, null);
        controller.AddWorldChatWindow(worldChatWindowController);

        Assert.IsTrue(worldChatWindowController.view.transform.parent == view.leftWindowContainer,
            "Chat window isn't inside taskbar window container!");
        Assert.IsTrue(worldChatWindowController.view.gameObject.activeSelf, "Chat window is disabled!");
    }

    [Test]
    public void AddFriendWindowProperly()
    {
        friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(null, null);
        controller.AddFriendsWindow(friendsHudController);

        Assert.IsTrue(friendsHudController.view.transform.parent == view.leftWindowContainer,
            "Friends window isn't inside taskbar window container!");
        Assert.IsTrue(friendsHudController.view.gameObject.activeSelf, "Friends window is disabled!");
    }

    [Test]
    public void ToggleWindowsProperly()
    {
        privateChatController = new PrivateChatWindowHUDController();
        privateChatController.Initialize(chatController);
        controller.AddPrivateChatWindow(privateChatController);

        const string badPositionMsg =
            "Anchored position should be zero or it won't be correctly placed inside the taskbar";
        const string badPivotMsg = "Pivot should be zero or it won't be correctly placed inside the taskbar";

        RectTransform rt = privateChatController.view.transform as RectTransform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        worldChatWindowController = new WorldChatWindowHUDController();
        worldChatWindowController.Initialize(chatController, null);
        controller.AddWorldChatWindow(worldChatWindowController);

        rt = worldChatWindowController.view.transform as RectTransform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        friendsHudController = new FriendsHUDController();
        friendsHudController.Initialize(friendsController, UserProfile.GetOwnUserProfile());
        controller.AddFriendsWindow(friendsHudController);

        rt = friendsHudController.view.transform as RectTransform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, friendsHudController.view, "test-1");
        TestHelpers_Chat.FakePrivateChatMessageFrom(userProfileController, chatController, "test-1", "test message!");

        var buttonList = view.GetButtonList();

        Assert.AreEqual(3, buttonList.Count, "Chat head is missing when receiving a private message?");

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
        Assert.IsTrue(controller.privateChatWindowHud.view.gameObject.activeInHierarchy);

        //NOTE(Brian): Toggle friends window on and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindowHud.view.gameObject.activeInHierarchy);
        Assert.IsFalse(headButton.lineOnIndicator.isVisible);
        Assert.IsTrue(view.friendsButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.chatButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends window off and test all other windows are untoggled
        view.friendsButton.toggleButton.onClick.Invoke();

        Assert.IsFalse(controller.privateChatWindowHud.view.gameObject.activeInHierarchy);
        Assert.IsFalse(headButton.lineOnIndicator.isVisible);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);

        //NOTE(Brian): Toggle friends on, and then chat button on. Then check if world chat window is showing up.
        view.friendsButton.toggleButton.onClick.Invoke();
        view.chatButton.toggleButton.onClick.Invoke();

        Assert.IsTrue(controller.worldChatWindowHud.view.gameObject.activeInHierarchy);
        Assert.IsFalse(controller.friendsHud.view.gameObject.activeInHierarchy);
        Assert.IsFalse(view.friendsButton.lineOnIndicator.isVisible);
    }

    [Test]
    public void AddPortableExperienceItemProperly()
    {
        // Arrange
        string testPEId = "test-pe";

        // Act
        view.AddPortableExperienceElement(testPEId, "Test PE", "");

        // Assert
        var newPE = view.rightButtonsContainer.GetComponentInChildren<PortableExperienceTaskbarItem>();
        Assert.IsNotNull(newPE, "There should exists a PortableExperienceTaskbarItem as child!");
        Assert.AreEqual(0, newPE.gameObject.transform.GetSiblingIndex(), "The sibling index for the new Portable Experience should be 0!");
        Assert.IsTrue(view.activePortableExperienceItems.ContainsKey(testPEId), "The activePortableExperienceItems dictionary should contains the new PE added!");
        Assert.IsTrue(view.activePortableExperiencesPoolables.ContainsKey(testPEId), "The activePortableExperiencesPoolables dictionary should contains the new PE added!");
    }

    [Test]
    public void RemovePortableExperienceItemProperly()
    {
        // Arrange
        string testPEId = "test-pe";

        // Act
        view.AddPortableExperienceElement(testPEId, "Test PE", "");
        view.RemovePortableExperienceElement(testPEId);

        // Assert
        var newPE = view.rightButtonsContainer.GetComponentInChildren<PortableExperienceTaskbarItem>();
        Assert.IsNull(newPE, "There should not exists a PortableExperienceTaskbarItem as child!");
        Assert.IsFalse(view.activePortableExperienceItems.ContainsKey(testPEId), "The activePortableExperienceItems dictionary should not contains the new PE added!");
        Assert.IsFalse(view.activePortableExperiencesPoolables.ContainsKey(testPEId), "The activePortableExperiencesPoolables dictionary should not contains the new PE added!");
    }
}