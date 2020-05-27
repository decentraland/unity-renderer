using NUnit.Framework;
using System.Collections;
using System.Linq;

public class PlayerInfoCardHUDViewShould : TestsBase
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

        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = "userId",
            name = "username",
            description = "description",
            email = "email",
            inventory = new string[]
            {
                WearableLiterals.ItemRarity.EPIC,
                WearableLiterals.ItemRarity.LEGENDARY,
                WearableLiterals.ItemRarity.MYTHIC,
                WearableLiterals.ItemRarity.RARE,
                WearableLiterals.ItemRarity.UNIQUE,
            }
        });
        userProfile = UserProfileController.userProfilesCatalog.Get("userId");
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

        view.Initialize(() => hideCardButtonWasPressed = true, () => reportButtonWasPressed = true, () => blockButtonWasPressed = true, () => unblockButtonWasPressed = true, () => addFriendWasPressed = true, () => cancelWasPressed = true, () => acceptRequestWasPressed = true, () => rejectRequestWasPressed = true);
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
            Assert.AreEqual(tabsMapping.container.activeSelf, tabsMapping.tab == tab, $"{tab} Tab was selected and Tab: {tabsMapping.tab} is {tabsMapping.container.activeSelf}");
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
            tags = new string[] { },
            category = WearableLiterals.Categories.UPPER_BODY
        };
        CatalogController.wearableCatalog.Add(rarity, wearable);
        return wearable;
    }
}
