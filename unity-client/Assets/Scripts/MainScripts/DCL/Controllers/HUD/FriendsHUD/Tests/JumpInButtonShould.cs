using DCL.Interface;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class JumpInButtonShould : TestsBase
{
    private const string JUMP_IN_BUTTON_RESOURCE_NAME = "JumpInButton";
    private const string TEST_USER_ID = "testFriend";
    private const string TEST_SERVER_NAME = "test server name";
    private const string TEST_LAYER_NAME = "test layer name";

    private FriendsController_Mock friendsController;
    private JumpInButton jumpInButton;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        Vector2 testCoords = new Vector2(5, 20);

        friendsController = new FriendsController_Mock();
        friendsController.AddFriend(new FriendsController.UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = testCoords,
            presence = PresenceStatus.ONLINE,
            realm = new FriendsController.UserStatus.Realm
            {
                serverName = TEST_SERVER_NAME,
                layer = TEST_LAYER_NAME
            }
        });

        GameObject go = Object.Instantiate((GameObject)Resources.Load(JUMP_IN_BUTTON_RESOURCE_NAME));
        jumpInButton = go.GetComponent<JumpInButton>();
        jumpInButton.Initialize(friendsController, TEST_USER_ID);

        Assert.AreEqual(testCoords, jumpInButton.currentCoords, "Position coords should match with [testCoords]");
        Assert.AreEqual(PresenceStatus.ONLINE, jumpInButton.currentPresenceStatus, "Presence status should be ONLINE");
        Assert.AreEqual(TEST_SERVER_NAME, jumpInButton.currentRealmServerName, "Server name should match with [TEST_SERVER_NAME]");
        Assert.AreEqual(TEST_LAYER_NAME, jumpInButton.currentRealmLayerName, "Server layer should match with [TEST_LAYER_NAME]");
        Assert.AreEqual(true, jumpInButton.gameObject.activeSelf, "JumpInButton game object should be actived");

        yield break;
    }

    protected override IEnumerator TearDown()
    {
        Object.Destroy(jumpInButton.gameObject);
        yield break;
    }

    [Test]
    public void FriendChangesHisPosition()
    {
        Vector2 newTestCoords = new Vector2(10, 20);

        friendsController.RaiseUpdateUserStatus(TEST_USER_ID, new FriendsController.UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = newTestCoords,
            presence = PresenceStatus.ONLINE,
            realm = new FriendsController.UserStatus.Realm
            {
                serverName = TEST_SERVER_NAME,
                layer = TEST_LAYER_NAME
            }
        });

        Assert.AreEqual(newTestCoords, jumpInButton.currentCoords, "Position coords should match with [newTestCoords]");
        Assert.AreEqual(PresenceStatus.ONLINE, jumpInButton.currentPresenceStatus, "Presence status should be ONLINE");
        Assert.AreEqual(TEST_SERVER_NAME, jumpInButton.currentRealmServerName, "Server name should match with [TEST_SERVER_NAME]");
        Assert.AreEqual(TEST_LAYER_NAME, jumpInButton.currentRealmLayerName, "Server layer should match with [TEST_LAYER_NAME]");
        Assert.AreEqual(true, jumpInButton.gameObject.activeSelf, "JumpInButton game object should be actived");
    }

    [Test]
    public void FriendChangesHisRealm()
    {
        Vector2 newTestCoords = new Vector2(10, 20);
        string newRealmServerName = "test server name 2";
        string newRealmLayerName = "test layer name 2";

        friendsController.RaiseUpdateUserStatus(TEST_USER_ID, new FriendsController.UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = newTestCoords,
            presence = PresenceStatus.ONLINE,
            realm = new FriendsController.UserStatus.Realm
            {
                serverName = newRealmServerName,
                layer = newRealmLayerName
            }
        });

        Assert.AreEqual(newTestCoords, jumpInButton.currentCoords, "Position coords should match with [newTestCoords]");
        Assert.AreEqual(PresenceStatus.ONLINE, jumpInButton.currentPresenceStatus, "Presence status should be ONLINE");
        Assert.AreEqual(newRealmServerName, jumpInButton.currentRealmServerName, "Server name should match with [newRealmServerName]");
        Assert.AreEqual(newRealmLayerName, jumpInButton.currentRealmLayerName, "Server layer should match with [newRealmLayerName]");
        Assert.AreEqual(true, jumpInButton.gameObject.activeSelf, "JumpInButton game object should be actived");
    }

    [Test]
    public void FriendChangesHisPresence()
    {
        Vector2 newTestCoords = new Vector2(10, 20);

        friendsController.RaiseUpdateUserStatus(TEST_USER_ID, new FriendsController.UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = newTestCoords,
            presence = PresenceStatus.OFFLINE,
            realm = new FriendsController.UserStatus.Realm
            {
                serverName = TEST_SERVER_NAME,
                layer = TEST_LAYER_NAME
            }
        });

        Assert.AreEqual(newTestCoords, jumpInButton.currentCoords, "Position coords should match with [testCoords]");
        Assert.AreEqual(PresenceStatus.OFFLINE, jumpInButton.currentPresenceStatus, "Presence status should be OFFLINE");
        Assert.AreEqual(TEST_SERVER_NAME, jumpInButton.currentRealmServerName, "Server name should match with [TEST_SERVER_NAME]");
        Assert.AreEqual(TEST_LAYER_NAME, jumpInButton.currentRealmLayerName, "Server layer should match with [TEST_LAYER_NAME]");
        Assert.AreEqual(false, jumpInButton.gameObject.activeSelf, "JumpInButton game object should be deactived");
    }

    [Test]
    public void ReactCorrectlyToJumpInClick()
    {
        bool jumpInCalled = false;

        System.Action<string, string> callback = (type, message) =>
        {
            if (type == "JumpIn")
            {
                jumpInCalled = true;
            }
        };

        WebInterface.OnMessageFromEngine += callback;

        jumpInButton.button.onClick.Invoke();

        WebInterface.OnMessageFromEngine -= callback;

        Assert.IsTrue(jumpInCalled);
    }
}
