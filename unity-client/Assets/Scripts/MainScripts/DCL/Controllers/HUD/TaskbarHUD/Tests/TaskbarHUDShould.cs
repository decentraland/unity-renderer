using DCL.HelpAndSupportHUD;
using DCL.SettingsHUD;
using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class TaskbarHUDShould : TestsBase
{
    private TaskbarHUDController controller;
    private TaskbarHUDView view;

    private FriendsController_Mock friendsController = new FriendsController_Mock();
    private ChatController_Mock chatController = new ChatController_Mock();

    private GameObject userProfileGO;
    private UserProfile userProfile;
    private PrivateChatWindowHUDController privateChatController;
    private FriendsHUDController friendsHudController;
    private WorldChatWindowHUDController worldChatWindowController;
    private SettingsHUDController settingsHudController;
    private HelpAndSupportHUDController helpAndSupportHUDController;
    private ExploreHUDController exploreHUDController;

    protected override bool justSceneSetUp => true;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        UserProfile ownProfile = UserProfile.GetOwnUserProfile();
        CommonScriptableObjects.rendererState.Set(true);

        var ownProfileModel = new UserProfileModel();
        ownProfileModel.userId = "my-user-id";
        ownProfileModel.name = "NO_USER";
        ownProfile.UpdateData(ownProfileModel, false);

        userProfileGO = new GameObject();
        userProfile = userProfileGO.AddComponent<UserProfileController>().ownUserProfile;

        controller = new TaskbarHUDController();
        controller.Initialize(null, chatController, null);
        view = controller.view;

        Assert.IsTrue(view != null, "Taskbar view is null?");
        Assert.IsTrue(view.moreButton.gameObject.activeSelf, "More button is not actived?");
        Assert.IsTrue(CommonScriptableObjects.isTaskbarHUDInitialized, "Taskbar controller is not initialized?");
    }

    protected override IEnumerator TearDown()
    {
        yield return null;

        privateChatController?.Dispose();
        worldChatWindowController?.Dispose();
        friendsHudController?.Dispose();
        settingsHudController?.Dispose();
        helpAndSupportHUDController?.Dispose();
        exploreHUDController?.Dispose();

        controller.Dispose();
        UnityEngine.Object.Destroy(userProfileGO);

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
    public void AddSettingsWindowProperly()
    {
        settingsHudController = new SettingsHUDController();
        controller.AddSettingsWindow(settingsHudController);

        Assert.IsTrue(settingsHudController.view.gameObject.activeSelf, "Settings window is disabled!");
    }

    [Test]
    public void AddHelpAndSupportWindowProperly()
    {
        helpAndSupportHUDController = new HelpAndSupportHUDController();
        controller.AddHelpAndSupportWindow(helpAndSupportHUDController);

        Assert.IsTrue(helpAndSupportHUDController.view.gameObject.activeSelf, "Help and Support window is disabled!");
    }

    [Test]
    public void AddExploreWindowProperly()
    {
        exploreHUDController = new ExploreHUDController();
        exploreHUDController.Initialize(friendsController);
        controller.AddExploreWindow(exploreHUDController);

        Assert.IsTrue(exploreHUDController.view.gameObject.activeSelf, "Explore window is disabled!");
    }

    [Test]
    public void AddControlsMoreButtonProperly()
    {
        controller.AddControlsMoreOption();

        Assert.IsTrue(view.moreMenu.controlsButton.IsActive(), "Controls more button is disabled!");
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

        TestHelpers_Friends.FakeAddFriend(friendsController, friendsHudController.view, "test-1");
        TestHelpers_Chat.FakePrivateChatMessageFrom(chatController, "test-1", "test message!");

        var buttonList = view.GetButtonList();

        Assert.AreEqual(6, buttonList.Count, "Chat head is missing when receiving a private message?");

        Assert.IsFalse(view.chatButton.toggledOn);
        Assert.IsTrue(buttonList[2] is ChatHeadButton);

        ChatHeadButton headButton = buttonList[2] as ChatHeadButton;
        Assert.IsFalse(headButton.toggledOn);

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
    public void ToggleBarVisibilityProperly()
    {
        view.moreMenu.collapseBarButton.onClick.Invoke();

        Assert.IsFalse(view.isBarVisible, "The bar should not be visible!");
        Assert.IsFalse(view.moreMenu.collapseIcon.activeSelf, "The collapse icon should not be actived!");
        Assert.IsFalse(view.moreMenu.collapseText.activeSelf, "The collapse text should not be actived!");
        Assert.IsTrue(view.moreMenu.expandIcon.activeSelf, "The expand icon should be actived!");
        Assert.IsTrue(view.moreMenu.expandText.activeSelf, "The expand text should be actived!");

        view.moreMenu.collapseBarButton.onClick.Invoke();

        Assert.IsTrue(view.isBarVisible, "The bar should be visible!");
        Assert.IsTrue(view.moreMenu.collapseIcon.activeSelf, "The collapse icon should be actived!");
        Assert.IsTrue(view.moreMenu.collapseText.activeSelf, "The collapse text should be actived!");
        Assert.IsFalse(view.moreMenu.expandIcon.activeSelf, "The expand icon should not be actived!");
        Assert.IsFalse(view.moreMenu.expandText.activeSelf, "The expand text should not be actived!");
    }
}