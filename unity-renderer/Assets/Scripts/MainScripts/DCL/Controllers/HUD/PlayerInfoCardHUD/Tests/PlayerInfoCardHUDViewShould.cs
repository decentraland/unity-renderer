using NUnit.Framework;
using System.Collections;
using System.Linq;

public class PlayerInfoCardHUDViewShould : IntegrationTestSuite_Legacy
{
    private PlayerInfoCardHUDView view;
    private UserProfile userProfile;
    protected override bool justSceneSetUp => true;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        view = PlayerInfoCardHUDView.CreateView();
        view.Initialize(null, null, null, null, null, null, null, null);

        CreateMockWearableByRarity(WearableLiterals.ItemRarity.EPIC);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.LEGENDARY);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.MYTHIC);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.RARE);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.UNIQUE);

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel {userId = "userId"});

        userProfile = UserProfileController.userProfilesCatalog.Get("userId");
        userProfile.UpdateData(new UserProfileModel
        {
            userId = "userId",
            name = "username",
            description = "description",
            email = "email",
            inventory = new[]
            {
                WearableLiterals.ItemRarity.EPIC,
                WearableLiterals.ItemRarity.LEGENDARY,
                WearableLiterals.ItemRarity.MYTHIC,
                WearableLiterals.ItemRarity.RARE,
                WearableLiterals.ItemRarity.UNIQUE,
            }
        });
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(view.gameObject);
        yield return base.TearDown();
    }

    [Test]
    public void BeCreatedProperly()
    {
        Assert.IsNotNull(view);
    }

    [Test]
    public void InitializeProperly()
    {
        bool hideCardButtonWasPressed = false;
        bool reportButtonWasPressed = false;
        bool blockButtonWasPressed = false;
        bool unblockButtonWasPressed = false;
        bool addFriendWasPressed = false;
        bool cancelWasPressed = false;
        bool acceptRequestWasPressed = false;
        bool rejectRequestWasPressed = false;

        view.Initialize(() => hideCardButtonWasPressed = true, () => reportButtonWasPressed = true,
            () => blockButtonWasPressed = true, () => unblockButtonWasPressed = true, () => addFriendWasPressed = true,
            () => cancelWasPressed = true, () => acceptRequestWasPressed = true, () => rejectRequestWasPressed = true);
        view.hideCardButton.onClick.Invoke();
        view.reportPlayerButton.onClick.Invoke();
        view.blockPlayerButton.onClick.Invoke();
        view.unblockPlayerButton.onClick.Invoke();
        view.addFriendButton.onClick.Invoke();
        view.requestSentButton.onClick.Invoke();
        view.acceptRequestButton.onClick.Invoke();
        view.rejectRequestButton.onClick.Invoke();

        Assert.IsTrue(hideCardButtonWasPressed);
        Assert.IsTrue(reportButtonWasPressed);
        Assert.IsTrue(blockButtonWasPressed);
        Assert.IsTrue(unblockButtonWasPressed);
        Assert.IsTrue(addFriendWasPressed);
        Assert.IsTrue(cancelWasPressed);
        Assert.IsTrue(acceptRequestWasPressed);
        Assert.IsTrue(rejectRequestWasPressed);
        Assert.IsTrue(GetTabMapping(PlayerInfoCardHUDView.Tabs.Passport).container.activeSelf);
        Assert.IsFalse(GetTabMapping(PlayerInfoCardHUDView.Tabs.Trade).container.activeSelf);
        Assert.IsFalse(GetTabMapping(PlayerInfoCardHUDView.Tabs.Block).container.activeSelf);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void UpdateCanvasActiveProperly(bool cardActive)
    {
        view.SetCardActive(cardActive);
        Assert.AreEqual(cardActive, view.cardCanvas.enabled);
    }

    [Test]
    [TestCase(PlayerInfoCardHUDView.Tabs.Passport)]
    [TestCase(PlayerInfoCardHUDView.Tabs.Block)]
    [TestCase(PlayerInfoCardHUDView.Tabs.Trade)]
    public void UpdateTabsProperly(PlayerInfoCardHUDView.Tabs tab)
    {
        GetTabMapping(tab).toggle.isOn = true;
        foreach (var tabsMapping in view.tabsMapping)
        {
            Assert.AreEqual(tabsMapping.container.activeSelf, tabsMapping.tab == tab,
                $"{tab} Tab was selected and Tab: {tabsMapping.tab} is {tabsMapping.container.activeSelf}");
        }
    }

    [Test]
    public void UpdateProfileDataProperly()
    {
        view.SetUserProfile(userProfile);

        Assert.AreEqual(userProfile, view.currentUserProfile);
        Assert.AreEqual(userProfile.userName, view.name.text);
        Assert.AreEqual(userProfile.description, view.description.text);
    }

    [Test]
    [Ignore("This test never worked as it should be. Another test was overwriting the userProfile with an empty inventory, so failed when run individually but passed when run all. TODO refactor: requesting wearables behaviour should be mocked")]
    public void CreateCollectibles()
    {
        view.SetUserProfile(userProfile);

        Assert.AreEqual(userProfile.inventory.Count, view.playerInfoCollectibles.Count);
        foreach (var keyValuePair in userProfile.inventory)
        {
            var wearable = CatalogController.wearableCatalog.Get(keyValuePair.Key);
            Assert.IsTrue(view.playerInfoCollectibles.Any(x => x.collectible == wearable));
        }
    }

    [TestCase("fucker123", "****er123")]
    [TestCase("holyshit", "holy****")]
    public void FilterProfanityName(string originalName, string filteredName)
    {
        userProfile.UpdateData(new UserProfileModel
        {
            userId = "userId",
            name = originalName,
            description = "description"
        });

        view.SetUserProfile(userProfile);

        Assert.AreEqual(filteredName, view.name.text);
    }
    
    [TestCase("fuckerrrrr", "****errrrr")]
    [TestCase("shit bro thats some nonsense", "**** bro thats some nonsense")]
    public void FilterProfanityDescription(string originalDescription, string filteredDescription)
    {
        userProfile.UpdateData(new UserProfileModel
        {
            userId = "userId",
            name = "test",
            description = originalDescription
        });

        view.SetUserProfile(userProfile);

        Assert.AreEqual(filteredDescription, view.description.text);
    }

    private PlayerInfoCardHUDView.TabsMapping GetTabMapping(PlayerInfoCardHUDView.Tabs tab)
    {
        return view.tabsMapping.First(x => x.tab == tab);
    }

    private WearableItem CreateMockWearableByRarity(string rarity)
    {
        var wearable = new WearableItem()
        {
            id = rarity,
            rarity = rarity,
            data = new WearableItem.Data()
            {
                tags = new string[] { },
                category = WearableLiterals.Categories.UPPER_BODY
            }
        };

        CatalogController.wearableCatalog.Remove(rarity);
        CatalogController.wearableCatalog.Add(rarity, wearable);

        return wearable;
    }
}