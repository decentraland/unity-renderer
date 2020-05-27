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

    protected override IEnumerator SetUp()
    {
        UserProfile ownProfile = UserProfile.GetOwnUserProfile();

        var ownProfileModel = new UserProfileModel();
        ownProfileModel.userId = "my-user-id";
        ownProfileModel.name = "NO_USER";
        ownProfile.UpdateData(ownProfileModel, false);

        userProfileGO = new GameObject();
        userProfileGO.AddComponent<UserProfileController>();

        controller = new TaskbarHUDController();
        controller.Initialize(null, chatController, null);
        view = controller.view;

        Assert.IsTrue(view != null, "Taskbar view is null?");
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(view);
        UnityEngine.Object.Destroy(userProfileGO);
        yield return null;
        yield break;
    }

    [Test]
    public void AddWindowsProperly()
    {
        WorldChatWindowHUDController chatWindowController = new WorldChatWindowHUDController();
        chatWindowController.Initialize(null, null);

        controller.AddWorldChatWindow(chatWindowController);

        Assert.IsTrue(chatWindowController.view.transform.parent == view.windowContainer,
            "Chat window isn't inside taskbar window container!");
        Assert.IsTrue(chatWindowController.view.gameObject.activeSelf, "Chat window is disabled!");
    }

    [Test]
    public void ToggleWindowsProperly()
    {
        var privateChatController = new PrivateChatWindowHUDController();
        privateChatController.Initialize(chatController);
        controller.AddPrivateChatWindow(privateChatController);

        const string badPositionMsg =
            "Anchored position should be zero or it won't be correctly placed inside the taskbar";
        const string badPivotMsg = "Pivot should be zero or it won't be correctly placed inside the taskbar";

        RectTransform rt = privateChatController.view.transform as RectTransform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        var chatWindowController = new WorldChatWindowHUDController();
        chatWindowController.Initialize(chatController, null);
        controller.AddWorldChatWindow(chatWindowController);

        rt = chatWindowController.view.transform as RectTransform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        var friendsHUDController = new FriendsHUDController();
        friendsHUDController.Initialize(friendsController, UserProfile.GetOwnUserProfile());
        controller.AddFriendsWindow(friendsHUDController);

        rt = friendsHUDController.view.transform as RectTransform;
        Assert.AreEqual(Vector2.zero, rt.anchoredPosition, badPositionMsg);
        Assert.AreEqual(Vector2.zero, rt.pivot, badPivotMsg);

        TestHelpers_Friends.FakeAddFriend(friendsController, friendsHUDController.view, "test-1");
        TestHelpers_Chat.FakePrivateChatMessageFrom(chatController, "test-1", "test message!");

        var buttonList = view.GetButtonList();

        Assert.AreEqual(3, buttonList.Count, "Chat head is missing when receiving a private message?");

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
}