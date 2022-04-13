using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using UnityEngine;
using Environment = DCL.Environment;

public class PlayerInfoCardHUDControllerShould : IntegrationTestSuite_Legacy
{
    private const string USER_ID = "userId";
    private const string USER_ID_1 = "userId1";
    private const string USER_ID_2 = "userId2";
    private const string USER_ID_3 = "userId3";
    private const string USER_ID_4 = "userId4";
    private const string USER_ID_5 = "userId5";
    private const string BLOCKED_USER_ID = "blockedUserId";

    private PlayerInfoCardHUDController controller;
    private UserProfile viewingUserProfile;
    private DataStore dataStore;
    private IWearableCatalogBridge wearableCatalogBridge;
    private WearableItem[] wearables;
    private FriendsController_Mock friendsController;
    private IUserProfileBridge userProfileBridge;
    private RegexProfanityFilter profanityFilter;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        viewingUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        WhenViewingUserUpdates();

        var currentPlayerIdData = ScriptableObject.CreateInstance<StringVariable>();
        currentPlayerIdData.Set(USER_ID);

        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.GetOwn().Returns(GivenMyOwnUserProfile());
        userProfileBridge.Get(USER_ID).Returns(viewingUserProfile);
        userProfileBridge.Get(USER_ID_1).Returns(viewingUserProfile);
        userProfileBridge.Get(USER_ID_2).Returns(viewingUserProfile);
        userProfileBridge.Get(USER_ID_3).Returns(viewingUserProfile);
        userProfileBridge.Get(USER_ID_4).Returns(viewingUserProfile);
        userProfileBridge.Get(USER_ID_5).Returns(viewingUserProfile);

        GivenWearableCatalog();

        dataStore = new DataStore();
        GivenProfanityFiltering();
        GivenProfanityFilteringAvailability(true);

        friendsController = new FriendsController_Mock();

        controller = new PlayerInfoCardHUDController(friendsController,
            currentPlayerIdData,
            userProfileBridge,
            wearableCatalogBridge,
            profanityFilter,
            dataStore);
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void CreateTheView()
    {
        Assert.IsNotNull(controller.view);
        Assert.IsNotNull(controller.view.gameObject);
    }

    [Test]
    public void CurrentPlayerNameIsFound()
    {
        Assert.IsNotNull(controller.currentPlayerId);
        Assert.AreEqual(USER_ID, controller.currentPlayerId);
    }

    [Test]
    public void ReactToCurrentPlayerNameChanges()
    {
        controller.currentPlayerId.Set(USER_ID);
        Assert.AreEqual(controller.currentUserProfile, viewingUserProfile);
    }

    [Test]
    public void UpdateNameAndDescription()
    {
        Assert.AreEqual(viewingUserProfile.userName, controller.view.name.text);
        Assert.AreEqual(viewingUserProfile.description, controller.view.description.text);
    }

    [Test]
    public void BlockUser()
    {
        viewingUserProfile.UpdateData(new UserProfileModel {userId = BLOCKED_USER_ID, name = "blockeduser"});
        Assert.IsFalse(controller.view.unblockPlayerButton.gameObject.activeSelf);
        Assert.IsFalse(controller.view.blockedAvatarOverlay.gameObject.activeSelf);
    }

    [Test]
    public void ShowWearables()
    {
        Assert.AreEqual(viewingUserProfile.inventory.Count, controller.view.playerInfoCollectibles.Count);
        Assert.IsTrue(wearables.All(wearable =>
            controller.view.playerInfoCollectibles.Any(item => item.collectible == wearable)));
    }

    [Test]
    public void ShowNoFriendshipStatus()
    {
        GivenFriendshipStatus(FriendshipStatus.NOT_FRIEND);

        WhenViewingUserUpdates();
        
        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.addFriendButton.gameObject.activeSelf);
    }

    [Test]
    public void ShowFriendAddedStatus()
    {
        GivenFriendshipStatus(FriendshipStatus.FRIEND);

        WhenViewingUserUpdates();
        
        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.alreadyFriendsContainer.gameObject.activeSelf);
    }

    [Test]
    public void ShowFriendRequestedToStatus()
    {
        GivenFriendshipStatus(FriendshipStatus.REQUESTED_TO);

        WhenViewingUserUpdates();
        
        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.requestSentButton.gameObject.activeSelf);
    }

    [Test]
    public void ShowFriendRequestedFromStatus()
    {
        GivenFriendshipStatus(FriendshipStatus.REQUESTED_FROM);

        WhenViewingUserUpdates();
        
        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.requestReceivedContainer.gameObject.activeSelf);
    }

    [TestCase("fucker123", "****er123")]
    [TestCase("holyshit", "holy****")]
    public void FilterProfanityName(string originalName, string filteredName)
    {
        GivenUserName(originalName);

        Assert.AreEqual(filteredName, controller.view.name.text);
    }

    [TestCase("fuckerrrrr", "****errrrr")]
    [TestCase("shit bro thats some nonsense", "**** bro thats some nonsense")]
    public void FilterProfanityDescription(string originalDescription, string filteredDescription)
    {
        GivenUserDescription(originalDescription);

        Assert.AreEqual(filteredDescription, controller.view.description.text);
    }

    [TestCase("fucker123")]
    [TestCase("holyshit")]
    public void DoNotFilterProfanityNameWhenFeatureIsDisabled(string originalName)
    {
        GivenProfanityFilteringAvailability(false);
        GivenUserName(originalName);

        Assert.AreEqual(originalName, controller.view.name.text);
    }

    [TestCase("fuckerrrrr")]
    [TestCase("shit bro thats some nonsense")]
    public void DoNotFilterProfanityDescriptionWhenFeatureIsDisabled(string description)
    {
        GivenProfanityFilteringAvailability(false);
        GivenUserDescription(description);

        Assert.AreEqual(description, controller.view.description.text);
    }

    [Test]
    public void SendAnalyticsWhenOpened()
    {
        Environment.i.platform.serviceProviders.analytics.ClearReceivedCalls();

        controller.currentPlayerId.Set(USER_ID_1);
        controller.currentPlayerId.Set(USER_ID_2);
        controller.currentPlayerId.Set(null);
        controller.currentPlayerId.Set(USER_ID_3);
        controller.currentPlayerId.Set(null);
        controller.currentPlayerId.Set(USER_ID_4);
        controller.currentPlayerId.Set(USER_ID_5);

        Environment.i.platform.serviceProviders.analytics.Received(5).SendAnalytic(PlayerInfoCardHUDController.PASSPORT_OPENED_EVENT, Arg.Any<Dictionary<string, string>>());
    }

    private void WhenViewingUserUpdates()
    {
        var wearables = new[]
        {
            WearableLiterals.ItemRarity.EPIC,
            WearableLiterals.ItemRarity.LEGENDARY,
            WearableLiterals.ItemRarity.MYTHIC,
            WearableLiterals.ItemRarity.RARE,
            WearableLiterals.ItemRarity.UNIQUE
        };

        viewingUserProfile.UpdateData(GetUserProfileModel(USER_ID, "username", "description", "email", wearables));
        viewingUserProfile.UpdateData(GetUserProfileModel(USER_ID_1, "username", "description", "email", wearables));
        viewingUserProfile.UpdateData(GetUserProfileModel(USER_ID_2, "username", "description", "email", wearables));
        viewingUserProfile.UpdateData(GetUserProfileModel(USER_ID_3, "username", "description", "email", wearables));
        viewingUserProfile.UpdateData(GetUserProfileModel(USER_ID_4, "username", "description", "email", wearables));
        viewingUserProfile.UpdateData(GetUserProfileModel(USER_ID_5, "username", "description", "email", wearables));
    }

    private UserProfileModel GetUserProfileModel(string id, string name, string description, string email, string[] inventory)
    {
        return new UserProfileModel()
        {
            userId = id,
            name = name,
            description = description,
            email = email,
            inventory = inventory
        };
    }

    private void GivenFriendshipStatus(FriendshipStatus status)
    {
        var friendStatus = new FriendsController.UserStatus
        {
            userId = USER_ID,
            friendshipStatus = status
        };
        friendsController.AddFriend(friendStatus);
    }

    private void GivenUserName(string name)
    {
        viewingUserProfile.UpdateData(new UserProfileModel
        {
            userId = USER_ID,
            name = name,
            description = null
        });
    }

    private void GivenProfanityFiltering()
    {
        var profanityWordProvider = Substitute.For<IProfanityWordProvider>();
        profanityWordProvider.GetNonExplicitWords().Returns(new[] {"fuck", "shit"});
        profanityWordProvider.GetExplicitWords().Returns(new[] {"ass"});
        profanityFilter = new RegexProfanityFilter(profanityWordProvider);
    }

    private void GivenWearableCatalog()
    {
        wearables = new[]
        {
            new WearableItem {id = "epic", rarity = WearableLiterals.ItemRarity.EPIC},
            new WearableItem {id = "legendary", rarity = WearableLiterals.ItemRarity.LEGENDARY},
            new WearableItem {id = "mythic", rarity = WearableLiterals.ItemRarity.MYTHIC},
            new WearableItem {id = "rare", rarity = WearableLiterals.ItemRarity.RARE},
            new WearableItem {id = "unique", rarity = WearableLiterals.ItemRarity.UNIQUE}
        };

        wearableCatalogBridge = Substitute.For<IWearableCatalogBridge>();
        wearableCatalogBridge.IsValidWearable(Arg.Any<string>()).Returns(true);
        Func<CallInfo, Promise<WearableItem[]>> requestOwnedWearables = info =>
        {
            var promise = new Promise<WearableItem[]>();
            promise.Resolve(wearables);
            return promise;
        };
        wearableCatalogBridge.RequestOwnedWearables(USER_ID).Returns(requestOwnedWearables);
        wearableCatalogBridge.RequestOwnedWearables(USER_ID_1).Returns(requestOwnedWearables);
        wearableCatalogBridge.RequestOwnedWearables(USER_ID_2).Returns(requestOwnedWearables);
        wearableCatalogBridge.RequestOwnedWearables(USER_ID_3).Returns(requestOwnedWearables);
        wearableCatalogBridge.RequestOwnedWearables(USER_ID_4).Returns(requestOwnedWearables);
        wearableCatalogBridge.RequestOwnedWearables(USER_ID_5).Returns(requestOwnedWearables);
        wearableCatalogBridge.RequestOwnedWearables(BLOCKED_USER_ID)
            .Returns(info =>
            {
                var promise = new Promise<WearableItem[]>();
                promise.Resolve(new WearableItem[0]);
                return promise;
            });
    }

    private UserProfile GivenMyOwnUserProfile()
    {
        var myUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        myUserProfile.UpdateData(new UserProfileModel {userId = "myUserId"});
        myUserProfile.Block(BLOCKED_USER_ID);
        return myUserProfile;
    }

    private void GivenUserDescription(string originalDescription)
    {
        viewingUserProfile.UpdateData(new UserProfileModel
        {
            userId = USER_ID,
            name = "test",
            description = originalDescription
        });
    }

    private void GivenProfanityFilteringAvailability(bool enabled)
    {
        dataStore.settings.profanityChatFilteringEnabled.Set(enabled);
    }
}