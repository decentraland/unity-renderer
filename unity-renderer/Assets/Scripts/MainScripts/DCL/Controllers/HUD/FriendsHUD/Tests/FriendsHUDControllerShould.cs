using DCL.Helpers;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Collections;
using DCL;
using UnityEngine;
using UnityEngine.TestTools;

public class FriendsHUDControllerShould : IntegrationTestSuite_Legacy
{
    UserProfileController userProfileController;
    private NotificationsController notificationsController;
    FriendsHUDController controller;
    IFriendsHUDComponentView view;
    FriendsController_Mock friendsController;
    ISocialAnalytics socialAnalytics;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        notificationsController = TestUtils.CreateComponentWithGameObject<NotificationsController>("NotificationsController");
        notificationsController.Initialize(new NotificationHUDController());

        userProfileController = TestUtils.CreateComponentWithGameObject<UserProfileController>("UserProfileController");
        controller = new FriendsHUDController(new DataStore());
        friendsController = new FriendsController_Mock();
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        controller.Initialize(friendsController, UserProfile.GetOwnUserProfile(), socialAnalytics);
        view = controller.View;

        Assert.IsTrue(view != null, "Friends hud view is null?");
        Assert.IsTrue(controller != null, "Friends hud controller is null?");
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(userProfileController.gameObject);
        notificationsController.Dispose();
        UnityEngine.Object.Destroy(notificationsController.gameObject);

        controller.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToWhisperClick()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, id);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        bool pressedWhisper = false;
        controller.OnPressWhisper += (x) => { pressedWhisper = x == id; };
        entry.whisperButton.onClick.Invoke();
        Assert.IsTrue(pressedWhisper);
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToPassportClick()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, id);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        var currentPlayerId = Resources.Load<StringVariable>(UserContextMenu.CURRENT_PLAYER_ID);

        Assert.AreNotEqual(id, currentPlayerId.Get());
        entry.passportButton.onClick.Invoke();
        Assert.AreEqual(id, currentPlayerId.Get());
    }

    [Test]
    public void HandleUsernameErrorCorrectly() { friendsController.RaiseOnFriendNotFound("test"); }

    // TODO: redo this test.. needs FriendsHUDController to be refactored first
    // [Test]
    // public void SendFriendRequestCorrectly()
    // {
    //     bool messageSent = false;
    //
    //     string id = "user test";
    //     Action<string, string> callback = (name, payload) =>
    //     {
    //         var msg = JsonUtility.FromJson<FriendsController.FriendshipUpdateStatusMessage>(payload);
    //         if (msg.action == FriendshipAction.REQUESTED_TO &&
    //             msg.userId == id)
    //         {
    //             messageSent = true;
    //         }
    //     };
    //
    //     WebInterface.OnMessageFromEngine += callback;
    //
    //     view.Search(id);
    //
    //     Assert.IsTrue(messageSent);
    //
    //     WebInterface.OnMessageFromEngine -= callback;
    // }

    [UnityTest]
    public IEnumerator ReactCorrectlyToFriendApproved()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, id, FriendshipAction.APPROVED);
        var entry = TestHelpers_Friends.GetEntry(view, id);
        Assert.IsNotNull(entry);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.DELETED);
        entry = controller.View.GetEntry(id) as FriendEntry;
        Assert.IsNull(entry);
    }

    [UnityTest]
    public IEnumerator ReactCorrectlyToFriendRejected()
    {
        var id = "test-id-1";
        yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, id, FriendshipAction.NONE);

        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REQUESTED_FROM);
        friendsController.RaiseUpdateFriendship(id, FriendshipAction.REJECTED);

        var entry = controller.View.GetEntry(id);
        Assert.IsNull(entry);
    }

    // TODO: the view should be mocked to test this correctly 
    // [UnityTest]
    // public IEnumerator ReactCorrectlyToFriendCancelled()
    // {
    //     var id = "test-id-1";
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, id, FriendshipAction.NONE);
    //
    //     friendsController.RaiseUpdateFriendship(id, FriendshipAction.REQUESTED_TO);
    //     var entry = controller.View.GetEntry(id);
    //     Assert.IsNotNull(entry);
    //     friendsController.RaiseUpdateFriendship(id, FriendshipAction.CANCELLED);
    //     yield return null;
    //     entry = controller.View.GetEntry(id);
    //     Assert.IsNull(entry);
    // }

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

    // TODO: redo this test.. needs FriendsHUDController to be refactored first
    // [UnityTest]
    // public IEnumerator TaskbarNotificationBadgeHasCorrectValue()
    // {
    //     PlayerPrefsUtils.SetInt(FriendsHUDController.PLAYER_PREFS_SEEN_FRIEND_COUNT, 0);
    //
    //     var friendsRequestBadge = GetBadge("NotificationBadge_FriendsRequestTab");
    //     var friendsTaskbarBadge = GetBadge("NotificationBadge_FriendsButton");
    //
    //     controller.SetVisibility(false);
    //
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-1");
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-2");
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-3");
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-4");
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-5", FriendshipAction.REQUESTED_FROM);
    //
    //     Assert.AreEqual(1, friendsRequestBadge.finalValue);
    //     Assert.AreEqual(5, friendsTaskbarBadge.finalValue);
    //
    //     controller.SetVisibility(true);
    //
    //     Assert.AreEqual(1, friendsRequestBadge.finalValue);
    //     Assert.AreEqual(1, friendsTaskbarBadge.finalValue);
    //
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-5", FriendshipAction.APPROVED);
    //     yield return TestHelpers_Friends.FakeAddFriend(userProfileController, friendsController, view, "friend-6", FriendshipAction.REQUESTED_FROM);
    //
    //     Assert.AreEqual(1, friendsRequestBadge.finalValue);
    //     Assert.AreEqual(1, friendsTaskbarBadge.finalValue);
    // }
}