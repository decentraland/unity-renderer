using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.ProfanityFiltering;
using DCL.Social.Friends;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

public class PlayerInfoCardHUDControllerShould : IntegrationTestSuite_Legacy
{
    private static readonly string[] IDS = new[] { "userId", "userId1", "userId2", "userId3", "userId4", "userId5", "blockedUserId" };

    private PlayerInfoCardHUDController controller;
    private Dictionary<string, UserProfile> userProfiles;
    private DataStore dataStore;
    private IWearablesCatalogService wearablesCatalogService;
    private WearableItem[] wearables;
    private FriendsController_Mock friendsController;
    private IUserProfileBridge userProfileBridge;
    private ISocialAnalytics socialAnalytics;
    private IProfanityFilter profanityFilter;
    private StringVariable currentPlayerIdData;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        PrepareUsers();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        userProfileBridge.GetOwn().Returns(GivenMyOwnUserProfile());
        userProfileBridge.Get(Arg.Any<string>()).Returns(x => userProfiles[x.ArgAt<string>(0)]);

        currentPlayerIdData = ScriptableObject.CreateInstance<StringVariable>();
        currentPlayerIdData.Set(null);

        GivenWearableCatalog();

        dataStore = new DataStore();
        dataStore.featureFlags.flags.Set(new FeatureFlag { flags = { ["friends_enabled"] = true } });
        GivenProfanityFiltering();
        GivenProfanityFilteringAvailability(true);

        friendsController = new FriendsController_Mock();
        socialAnalytics = Substitute.For<ISocialAnalytics>();

        controller = new PlayerInfoCardHUDController(friendsController,
            currentPlayerIdData,
            userProfileBridge,
            wearablesCatalogService,
            socialAnalytics,
            profanityFilter,
            dataStore,
            CommonScriptableObjects.playerInfoCardVisibleState);
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
        currentPlayerIdData.Set(IDS[0]);
        Assert.IsNotNull(controller.currentPlayerId);
        Assert.AreEqual(IDS[0], controller.currentPlayerId);
    }

    [Test]
    public void ReactToCurrentPlayerNameChanges()
    {
        currentPlayerIdData.Set(IDS[0]);
        Assert.AreEqual(controller.currentUserProfile, userProfiles[IDS[0]]);
    }

    [Test]
    public void UpdateNameAndDescription()
    {
        currentPlayerIdData.Set(IDS[0]);
        Assert.AreEqual(controller.view.name.text, userProfiles[IDS[0]].userName);
        Assert.AreEqual(controller.view.description.text, userProfiles[IDS[0]].description);
    }

    [Test]
    public void BlockUser()
    {
        currentPlayerIdData.Set(IDS[1]);
        userProfiles[IDS[1]].UpdateData(new UserProfileModel { userId = IDS[1], name = "blockeduser" });
        Assert.IsFalse(controller.view.unblockPlayerButton.gameObject.activeSelf);
        Assert.IsFalse(controller.view.blockedAvatarOverlay.gameObject.activeSelf);
    }

    [Test]
    public void ShowWearables()
    {
        currentPlayerIdData.Set(IDS[0]);
        Assert.AreEqual(userProfiles[IDS[0]].inventory.Count, controller.view.playerInfoCollectibles.Count);

        Assert.IsTrue(wearables.All(wearable =>
            controller.view.playerInfoCollectibles.Any(item => item.collectible == wearable)));
    }

    [Test]
    public void ShowNoFriendshipStatus()
    {
        GivenFriendshipStatus(IDS[0], FriendshipStatus.NOT_FRIEND);
        currentPlayerIdData.Set(IDS[0]);

        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.addFriendButton.gameObject.activeSelf);
    }

    [Test]
    public void ShowFriendAddedStatus()
    {
        GivenFriendshipStatus(IDS[0], FriendshipStatus.FRIEND);
        currentPlayerIdData.Set(IDS[0]);

        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.alreadyFriendsContainer.gameObject.activeSelf);
    }

    [Test]
    public void ShowFriendRequestedToStatus()
    {
        GivenFriendshipStatus(IDS[0], FriendshipStatus.REQUESTED_TO);
        currentPlayerIdData.Set(IDS[0]);

        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.requestSentButton.gameObject.activeSelf);
    }

    [Test]
    public void ShowFriendRequestedFromStatus()
    {
        GivenFriendshipStatus(IDS[0], FriendshipStatus.REQUESTED_FROM);
        currentPlayerIdData.Set(IDS[0]);

        Assert.IsTrue(controller.view.friendStatusContainer.activeSelf);
        Assert.IsTrue(controller.view.requestReceivedContainer.gameObject.activeSelf);
    }

    [UnityTest]
    [TestCase("fucker123", "****er123", ExpectedResult = (IEnumerator)null)]
    [TestCase("holyshit", "holy****", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityName(string originalName, string filteredName)
    {
        profanityFilter.Filter(originalName).Returns(UniTask.FromResult(filteredName));
        currentPlayerIdData.Set(IDS[0]);
        GivenUserName(IDS[0], originalName);

        // must wait the switch to main thread
        yield return null;

        Assert.AreEqual(filteredName, controller.view.name.text);
    }

    [TestCase("fuckerrrrr", "****errrrr", ExpectedResult = (IEnumerator)null)]
    [TestCase("shit bro thats some nonsense", "**** bro thats some nonsense", ExpectedResult = (IEnumerator)null)]
    public IEnumerator FilterProfanityDescription(string originalDescription, string filteredDescription)
    {
        profanityFilter.Filter(originalDescription).Returns(UniTask.FromResult(filteredDescription));
        currentPlayerIdData.Set(IDS[0]);
        GivenUserDescription(IDS[0], originalDescription);

        // must wait the switch to main thread
        yield return null;

        Assert.AreEqual(filteredDescription, controller.view.description.text);
    }

    [TestCase("fucker123")]
    [TestCase("holyshit")]
    public void DoNotFilterProfanityNameWhenFeatureIsDisabled(string originalName)
    {
        currentPlayerIdData.Set(IDS[0]);
        GivenProfanityFilteringAvailability(false);
        GivenUserName(IDS[0], originalName);

        Assert.AreEqual(originalName, controller.view.name.text);
    }

    [TestCase("fuckerrrrr")]
    [TestCase("shit bro thats some nonsense")]
    public void DoNotFilterProfanityDescriptionWhenFeatureIsDisabled(string description)
    {
        currentPlayerIdData.Set(IDS[0]);
        GivenProfanityFilteringAvailability(false);
        GivenUserDescription(IDS[0], description);

        Assert.AreEqual(description, controller.view.description.text);
    }

    [Test]
    public void SendAnalyticsWhenOpened()
    {
        Environment.i.platform.serviceProviders.analytics.ClearReceivedCalls();

        controller.currentPlayerId.Set(IDS[0]);
        controller.currentPlayerId.Set(null);
        controller.currentPlayerId.Set(IDS[1]);
        controller.currentPlayerId.Set(null);
        controller.currentPlayerId.Set(IDS[2]);
        controller.currentPlayerId.Set(IDS[3]);
        controller.currentPlayerId.Set(null);
        controller.currentPlayerId.Set(IDS[4]);

        socialAnalytics.Received(5).SendPassportOpen();
    }

    private void PrepareUsers()
    {
        var wearables = new[]
        {
            WearableLiterals.ItemRarity.EPIC,
            WearableLiterals.ItemRarity.LEGENDARY,
            WearableLiterals.ItemRarity.MYTHIC,
            WearableLiterals.ItemRarity.RARE,
            WearableLiterals.ItemRarity.UNIQUE
        };

        userProfiles = IDS.ToDictionary(x => x, x => GetUserProfile(x, wearables));
    }

    private UserProfile GetUserProfile(string id, IEnumerable<string> inventory)
    {
        UserProfile userProfile = ScriptableObject.CreateInstance<UserProfile>();

        userProfile.UpdateData(
            new UserProfileModel()
            {
                userId = id,
                name = $"name_{id}",
                description = $"description_{id}",
                email = $"email_{id}"
            });

        userProfile.SetInventory(inventory);
        return userProfile;
    }

    private void GivenFriendshipStatus(string userId, FriendshipStatus status)
    {
        var friendStatus = new UserStatus
        {
            userId = userId,
            friendshipStatus = status
        };

        friendsController.AddFriend(friendStatus);
    }

    private void GivenUserName(string userId, string name)
    {
        userProfiles[userId]
           .UpdateData(new UserProfileModel
            {
                userId = userId,
                name = name,
                description = null
            });
    }

    private void GivenProfanityFiltering()
    {
        profanityFilter = Substitute.For<IProfanityFilter>();
        profanityFilter.Filter(Arg.Any<string>()).Returns(info => UniTask.FromResult(info[0]?.ToString() ?? ""));
    }

    private void GivenWearableCatalog()
    {
        wearables = new[]
        {
            new WearableItem { id = "epic", rarity = WearableLiterals.ItemRarity.EPIC },
            new WearableItem { id = "legendary", rarity = WearableLiterals.ItemRarity.LEGENDARY },
            new WearableItem { id = "mythic", rarity = WearableLiterals.ItemRarity.MYTHIC },
            new WearableItem { id = "rare", rarity = WearableLiterals.ItemRarity.RARE },
            new WearableItem { id = "unique", rarity = WearableLiterals.ItemRarity.UNIQUE }
        };

        wearablesCatalogService = Substitute.For<IWearablesCatalogService>();
        wearablesCatalogService.IsValidWearable(Arg.Any<string>()).Returns(true);

        foreach (string id in IDS)
        {
            wearablesCatalogService
               .RequestOwnedWearablesAsync(id, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
               .Returns(_ =>
                {
                    UniTaskCompletionSource<(IReadOnlyList<WearableItem> wearables, int totalAmount)> mockedResult = new UniTaskCompletionSource<(IReadOnlyList<WearableItem> wearables, int totalAmount)>();
                    mockedResult.TrySetResult((wearables, wearables.Length));
                    return mockedResult.Task;
                });
        }
    }

    private UserProfile GivenMyOwnUserProfile()
    {
        var myUserProfile = ScriptableObject.CreateInstance<UserProfile>();
        myUserProfile.UpdateData(new UserProfileModel { userId = "myUserId" });
        myUserProfile.Block("blockedUserId");
        return myUserProfile;
    }

    private void GivenUserDescription(string userId, string originalDescription)
    {
        userProfiles[userId]
           .UpdateData(new UserProfileModel
            {
                userId = userId,
                name = "test",
                description = originalDescription
            });
    }

    private void GivenProfanityFilteringAvailability(bool enabled)
    {
        dataStore.settings.profanityChatFilteringEnabled.Set(enabled);
    }
}
