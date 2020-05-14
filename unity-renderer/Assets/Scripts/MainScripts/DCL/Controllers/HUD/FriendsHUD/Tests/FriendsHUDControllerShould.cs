using DCL.Interface;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

class FriendsController_Mock : IFriendsController
{
    public event Action<string, FriendsController.FriendshipAction> OnUpdateFriendship;
    public event Action<string, FriendsController.UserStatus> OnUpdateUserStatus;
    public event Action<string> OnFriendNotFound;

    Dictionary<string, FriendsController.UserStatus> friends = new Dictionary<string, FriendsController.UserStatus>();

    public int friendCount => friends.Count;

    public Dictionary<string, FriendsController.UserStatus> GetFriends()
    {
        return friends;
    }

    public void RaiseUpdateFriendship(string id, FriendsController.FriendshipAction action)
    {
        if (action == FriendsController.FriendshipAction.NONE)
        {
            if (friends.ContainsKey(id))
                friends.Remove(id);
        }

        if (action == FriendsController.FriendshipAction.APPROVED)
        {
            if (!friends.ContainsKey(id))
                friends.Add(id, new FriendsController.UserStatus());
        }

        OnUpdateFriendship?.Invoke(id, action);
    }

    public void RaiseUpdateUserStatus(string id, FriendsController.UserStatus userStatus)
    {
        OnUpdateUserStatus?.Invoke(id, userStatus);
    }

    public void RaiseOnFriendNotFound(string id)
    {
        OnFriendNotFound?.Invoke(id);
    }
}

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
    public void ReactCorrectlyToJumpInClick()
    {
        var id = "test-id-1";
        var entry = AddFriendThruFriendsController(id);

        bool jumpInCalled = false;

        System.Action<string, string> callback = (name, payload) =>
         {
             if (name == "GoTo")
             {
                 jumpInCalled = true;
             }
         };

        WebInterface.OnMessageFromEngine += callback;

        entry.jumpInButton.onClick.Invoke();

        WebInterface.OnMessageFromEngine -= callback;

        Assert.IsTrue(jumpInCalled);
    }

    [Test]
    public void ReactCorrectlyToWhisperClick()
    {
        var id = "test-id-1";
        var entry = AddFriendThruFriendsController(id);

        bool pressedWhisper = false;
        controller.OnPressWhisper += (x) => { pressedWhisper = x == id; };
        entry.whisperButton.onClick.Invoke();
        Assert.IsTrue(pressedWhisper);
    }

    [Test]
    public void ReactCorrectlyToReportClick()
    {
        var id = "test-id-1";
        var entry = AddFriendThruFriendsController(id);

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
        var entry = AddFriendThruFriendsController(id);

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
            if (msg.action == FriendsController.FriendshipAction.REQUESTED_TO &&
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
        var entry = AddFriendThruFriendsController(id);
        Assert.IsNotNull(entry);

        friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.DELETED);
        entry = controller.view.friendsList.GetEntry(id) as FriendEntry;
        Assert.IsNull(entry);
    }

    [Test]
    public void ReactCorrectlyToFriendRejected()
    {
        var id = "test-id-1";
        var fentry = AddFriendThruFriendsController(id);
        Assert.IsNotNull(fentry);

        friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.REQUESTED_FROM);
        friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.REJECTED);

        var entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNull(entry);
    }

    [Test]
    public void ReactCorrectlyToFriendCancelled()
    {
        var id = "test-id-1";
        AddFriendThruFriendsController(id);

        friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.REQUESTED_TO);
        var entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNotNull(entry);
        friendsController.RaiseUpdateFriendship(id, FriendsController.FriendshipAction.CANCELLED);
        entry = controller.view.friendRequestsList.GetEntry(id);
        Assert.IsNull(entry);
    }

    [Test]
    public void UpdateUserStatusCorrectly()
    {
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

        AddFriendThruFriendsController("friend-1");
        AddFriendThruFriendsController("friend-2");
        AddFriendThruFriendsController("friend-3");
        AddFriendThruFriendsController("friend-4");
        AddFriendThruFriendsController("friend-5", FriendsController.FriendshipAction.REQUESTED_FROM);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(5, friendsTaskbarBadge.finalValue);

        controller.SetVisibility(true);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(1, friendsTaskbarBadge.finalValue);

        AddFriendThruFriendsController("friend-5", FriendsController.FriendshipAction.APPROVED);
        AddFriendThruFriendsController("friend-6", FriendsController.FriendshipAction.REQUESTED_FROM);

        Assert.AreEqual(1, friendsRequestBadge.finalValue);
        Assert.AreEqual(1, friendsTaskbarBadge.finalValue);
    }

    FriendEntry AddFriendThruFriendsController(string id, FriendsController.FriendshipAction action = FriendsController.FriendshipAction.APPROVED)
    {
        UserProfileModel model = new UserProfileModel()
        {
            userId = id,
            name = id,
        };

        UserProfileController.i.AddUserProfileToCatalog(model);
        friendsController.RaiseUpdateFriendship(id, action);
        return view.friendsList.GetEntry(id) as FriendEntry;
    }
}
