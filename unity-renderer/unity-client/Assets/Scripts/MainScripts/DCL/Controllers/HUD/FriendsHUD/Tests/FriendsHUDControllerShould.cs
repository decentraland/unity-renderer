using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendsHUDControllerShould : IntegrationTestSuite_Legacy
{
    FriendsHUDController controller;
    FriendsHUDView view;
    FriendsController_Mock friendsController;

    protected override bool justSceneSetUp => true;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        NotificationsController.i.Initialize(new NotificationHUDController());

        controller = new FriendsHUDController();
        friendsController = new FriendsController_Mock();
        controller.Initialize(friendsController, UserProfile.GetOwnUserProfile());
        this.view = controller.view;

        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");
    }

    protected override IEnumerator TearDown()
    {
        NotificationsController.i.Dispose();
        controller.Dispose();

        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToWhisperClick()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, id);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        bool pressedWhisper = false;
        controller.OnPressWhisper += (x) => { pressedWhisper = x == id; };
        entry.whisperButton.onClick.Invoke();
        Assert.IsTrue(pressedWhisper);
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToReportClick()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, id);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        bool reportPlayerSent = false;

        Action<string, string> callback =
            (name, payload) =>
            {
                if (name == "ReportPlayer")
                {
                    reportPlayerSent = true;
                }
            };

        WebInterface.OnMessageFromEngine += callback;

        entry.menuButton.onClick.Invoke();

        Assert.IsTrue(controller.view.friendsList.contextMenuPanel.gameObject.activeSelf);

        controller.view.friendsList.contextMenuPanel.reportButton.onClick.Invoke();

        Assert.IsTrue(reportPlayerSent);

        WebInterface.OnMessageFromEngine -= callback;
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToPassportClick()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, id);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        var currentPlayerId = Resources.Load<StringVariable>(UserContextMenu.CURRENT_PLAYER_ID);

        entry.menuButton.onClick.Invoke();
        Assert.AreNotEqual(id, currentPlayerId.Get());

        view.friendsList.contextMenuPanel.passportButton.onClick.Invoke();

        Assert.AreEqual(id, currentPlayerId.Get());
    }

    [Test]
    public void HandleUsernameErrorCorrectly()
    {
        friendsController.RaiseOnFriendNotFound("test");
    }

    [Test]
    public void SendFriendRequestCorrectly()
    {
        bool messageSent = false;

        string id = "user test";
        Action<string, string> callback = (name, payload) =>
        {
            var msg = JsonUtility.FromJson<FriendsController.FriendshipUpdateStatusMessage>(payload);
            if (msg.action == FriendshipAction.REQUESTED_TO &&
                msg.userId == id)
            {
                messageSent = true;
            }
        };

        WebInterface.OnMessageFromEngine += callback;

        view.friendRequestsList.friendSearchInputField.onSubmit.Invoke(id);

        Assert.IsTrue(messageSent);

        WebInterface.OnMessageFromEngine -= callback;
    }


    [UnityTest]
    public IEnumerator ReactCorrectlyToFriendApproved()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, id, FriendshipAction.APPROVED);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.DELETED);
        entry = controller.view.friendsList.GetEntry(id) as FriendEntry;
        Assert.IsNull(entry);
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToFriendRejected()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, id);
        var fentry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(fentry);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REQUESTED_FROM);
        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REJECTED);

        var entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNull(entry);
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToFriendCancelled()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, id);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REQUESTED_TO);
        var entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNotNull(entry);
        friendsController.RaiseUpdateFriendship(id, FriendshipAction.CANCELLED);
        entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNull(entry);
    }

    NotificationBadge GetBadge(string path)
    {
        GameObject prefab = Resources.Load(path) as GameObject;
        Assert.IsTrue(prefab != null);
        GameObject go = this.InstantiateTestGameObject(prefab);
        Assert.IsTrue(go != null);

        var noti = go.GetComponent<NotificationBadge>();
        noti.Initialize();

        return noti;
    }

    [UnityTest]
    public IEnumerator TaskbarNotificationBadgeHasCorrectValue()
    {
        PlayerPrefsUtils.SetInt(FriendsHUDController.PLAYER_PREFS_SEEN_FRIEND_COUNT, 0);

        var friendsRequestBadge = GetBadge("NotificationBadge_FriendsRequestTab");
        var friendsTaskbarBadge = GetBadge("NotificationBadge_FriendsButton");

        controller.SetVisibility(false);

        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-1");
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-2");
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-3");
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-4");
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-5", FriendshipAction.REQUESTED_FROM);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(5, friendsTaskbarBadge.finalValue);

        controller.SetVisibility(true);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(1, friendsTaskbarBadge.finalValue);

        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-5", FriendshipAction.APPROVED);
        yield return TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-6", FriendshipAction.REQUESTED_FROM);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(1, friendsTaskbarBadge.finalValue);
    }
}