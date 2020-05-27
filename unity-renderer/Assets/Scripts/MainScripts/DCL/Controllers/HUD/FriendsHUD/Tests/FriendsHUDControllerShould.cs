using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendsHUDControllerShould : TestsBase
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
        yield break;
    }

    protected override IEnumerator TearDown()
    {
        yield return base.TearDown();
        controller.Dispose();
    }

    [Test]
    public void ReactCorrectlyToWhisperClick()
    {
        var id = "test-id-1";
        var entry = TestHelpers_Friends.FakeAddFriend(friendsController, view, id);

        bool pressedWhisper = false;
        controller.OnPressWhisper += (x) => { pressedWhisper = x == id; };
        entry.whisperButton.onClick.Invoke();
        Assert.IsTrue(pressedWhisper);
    }

    [Test]
    public void ReactCorrectlyToReportClick()
    {
        var id = "test-id-1";
        var entry = TestHelpers_Friends.FakeAddFriend(friendsController, view, id);

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
        Assert.AreEqual(entry, controller.view.friendsList.contextMenuPanel.targetEntry);

        controller.view.friendsList.contextMenuPanel.reportButton.onClick.Invoke();

        Assert.IsTrue(reportPlayerSent);

        WebInterface.OnMessageFromEngine -= callback;
    }

    [Test]
    public void ReactCorrectlyToPassportClick()
    {
        var id = "test-id-1";
        var entry = TestHelpers_Friends.FakeAddFriend(friendsController, view, id);

        var currentPlayerId = Resources.Load<StringVariable>(FriendsHUDController.CURRENT_PLAYER_ID);

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


    [Test]
    public void ReactCorrectlyToFriendApproved()
    {
        var id = "test-id-1";
        var entry = TestHelpers_Friends.FakeAddFriend(friendsController, view, id);
        Assert.IsNotNull(entry);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.DELETED);
        entry = controller.view.friendsList.GetEntry(id) as FriendEntry;
        Assert.IsNull(entry);
    }

    [Test]
    public void ReactCorrectlyToFriendRejected()
    {
        var id = "test-id-1";
        var fentry = TestHelpers_Friends.FakeAddFriend(friendsController, view, id);
        Assert.IsNotNull(fentry);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REQUESTED_FROM);
        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REJECTED);

        var entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNull(entry);
    }

    [Test]
    public void ReactCorrectlyToFriendCancelled()
    {
        var id = "test-id-1";
        TestHelpers_Friends.FakeAddFriend(friendsController, view, id);

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
        GameObject go = UnityEngine.Object.Instantiate(prefab);
        Assert.IsTrue(go != null);

        var noti = go.GetComponent<NotificationBadge>();
        noti.Initialize();

        return noti;
    }

    [Test]
    public void TaskbarNotificationBadgeHasCorrectValue()
    {
        PlayerPrefs.SetInt(FriendsHUDController.PLAYER_PREFS_SEEN_FRIEND_COUNT, 0);

        var friendsRequestBadge = GetBadge("NotificationBadge_FriendsRequestTab");
        var friendsTaskbarBadge = GetBadge("NotificationBadge_FriendsButton");

        controller.SetVisibility(false);

        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-1");
        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-2");
        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-3");
        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-4");
        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-5", FriendshipAction.REQUESTED_FROM);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(5, friendsTaskbarBadge.finalValue);

        controller.SetVisibility(true);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(1, friendsTaskbarBadge.finalValue);

        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-5", FriendshipAction.APPROVED);
        TestHelpers_Friends.FakeAddFriend(friendsController, view, "friend-6", FriendshipAction.REQUESTED_FROM);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(1, friendsTaskbarBadge.finalValue);
    }
}
