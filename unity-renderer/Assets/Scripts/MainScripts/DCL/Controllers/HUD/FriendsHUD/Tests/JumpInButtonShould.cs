using DCL.Interface;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Collections;
using DCL;
using DCL.Social.Friends;
using NSubstitute.ReceivedExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class JumpInButtonShould : IntegrationTestSuite_Legacy
{
    private const string TEST_USER_ID = "testFriend";
    private const string TEST_SERVER_NAME = "test server name";
    private const string TEST_LAYER_NAME = "test layer name";

    private FriendsController_Mock friendsController;
    private JumpInButton jumpInButton;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        // This is need to sue the TeleportController
        ServiceLocator serviceLocator = ServiceLocatorTestFactory.CreateMocked();
        DCL.Environment.Setup(serviceLocator);

        Vector2 testCoords = new Vector2(5, 20);

        friendsController = new FriendsController_Mock();
        friendsController.AddFriend(new UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = testCoords,
            presence = PresenceStatus.ONLINE,
            realm = new UserStatus.Realm
            {
                serverName = TEST_SERVER_NAME,
                layer = TEST_LAYER_NAME
            }
        });

        jumpInButton =  Object.Instantiate(
            AssetDatabase.LoadAssetAtPath<JumpInButton>(
                "Assets/Scripts/MainScripts/DCL/Controllers/HUD/SocialBarPrefabs/SocialBarV1/Prefabs/JumpInButton.prefab"));

        jumpInButton.Initialize(friendsController, TEST_USER_ID, Substitute.For<ISocialAnalytics>());

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

        friendsController.RaiseUpdateUserStatus(TEST_USER_ID, new UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = newTestCoords,
            presence = PresenceStatus.ONLINE,
            realm = new UserStatus.Realm
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

        friendsController.RaiseUpdateUserStatus(TEST_USER_ID, new UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = newTestCoords,
            presence = PresenceStatus.ONLINE,
            realm = new UserStatus.Realm
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

        friendsController.RaiseUpdateUserStatus(TEST_USER_ID, new UserStatus
        {
            userId = TEST_USER_ID,
            friendshipStatus = FriendshipStatus.FRIEND,
            position = newTestCoords,
            presence = PresenceStatus.OFFLINE,
            realm = new UserStatus.Realm
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

        Environment.i.world.teleportController.Received().JumpIn(Arg.Any<int>(),Arg.Any<int>(),Arg.Any<string>(),Arg.Any<string>());
    }
}
