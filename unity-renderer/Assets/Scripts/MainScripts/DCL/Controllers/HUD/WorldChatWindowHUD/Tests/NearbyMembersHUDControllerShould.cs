using DCL;
using DCL.Chat.HUD;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

public class NearbyMembersHUDControllerShould
{
    private NearbyMembersHUDController nearbyMembersHUDController;
    private IChannelMembersComponentView channelMembersComponentView;
    private IUserProfileBridge userProfileBridge;
    private DataStore_Player playerDataStore;
    private UserProfile testOwnUserProfile;
    private UserProfile testUserProfile;

    [SetUp]
    public void SetUp()
    {
        channelMembersComponentView = Substitute.For<IChannelMembersComponentView>();
        playerDataStore = new DataStore_Player();
        testOwnUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testOwnUserProfile.UpdateData(new UserProfileModel
        {
            userId = "ownTestId",
            name = "ownTestName",
            snapshots = new UserProfileModel.Snapshots { face256 = "ownTestFace256Url" },
        });
        testUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        testUserProfile.UpdateData(new UserProfileModel
        {
            userId = "testId",
            name = "testName",
            snapshots = new UserProfileModel.Snapshots { face256 = "testFace256Url" },
        });
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.Get(testUserProfile.userId).Returns(testUserProfile);
        userProfileBridge.GetOwn().Returns(testOwnUserProfile);
        nearbyMembersHUDController = new NearbyMembersHUDController(channelMembersComponentView, playerDataStore, userProfileBridge);
    }

    [TearDown]
    public void TearDown()
    {
        nearbyMembersHUDController.Dispose();
        Object.Destroy(testOwnUserProfile);
        Object.Destroy(testUserProfile);
    }

    [Test]
    public void InitializeCorrectly()
    {
        // Assert
        Assert.AreEqual(channelMembersComponentView, nearbyMembersHUDController.View);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibilityCorrectly(bool isVisible)
    {
        // Act
        nearbyMembersHUDController.SetVisibility(isVisible);

        // Assert
        if (isVisible)
        {
            channelMembersComponentView.Received(1).Show();
            channelMembersComponentView.Received(1).ClearSearchInput(false);
            channelMembersComponentView.Received(1).ClearAllEntries();
        }
        else
            channelMembersComponentView.Received(1).Hide();
    }

    [Test]
    public void ClearSearchCorrectly()
    {
        // Act
        nearbyMembersHUDController.ClearSearch();

        // Assert
        channelMembersComponentView.Received(1).ClearSearchInput();
    }

    [Test]
    public void AddPlayerCorrectly()
    {
        // Arrange
        ChannelMemberEntryModel testChannelMemberEntryModel = new ChannelMemberEntryModel
        {
            isOnline = true,
            thumnailUrl = testUserProfile.face256SnapshotURL,
            userId = testUserProfile.userId,
            userName = testUserProfile.userName,
            isOptionsButtonHidden = false,
        };

        // Act
        nearbyMembersHUDController.SetVisibility(true);
        playerDataStore.otherPlayers.Add(testUserProfile.userId, new Player { id = testUserProfile.userId });

        // Assert
        channelMembersComponentView.Set(testChannelMemberEntryModel);
    }

    [Test]
    public void RemovePlayerCorrectly()
    {
        // Arrange
        AddPlayerCorrectly();

        // Act
        nearbyMembersHUDController.SetVisibility(true);
        playerDataStore.otherPlayers.Remove(testUserProfile.userId);

        // Assert
        channelMembersComponentView.Remove(testUserProfile.userId);
    }
}
