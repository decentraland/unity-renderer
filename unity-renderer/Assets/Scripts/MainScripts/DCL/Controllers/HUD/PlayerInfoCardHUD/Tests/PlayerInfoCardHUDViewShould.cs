using NUnit.Framework;
using System.Collections;
using System.Linq;
using DCL;
using DCL.Helpers;
using DCLServices.WearablesCatalogService;

public class PlayerInfoCardHUDViewShould : IntegrationTestSuite_Legacy
{
    private UserProfileController userProfileController;
    private PlayerInfoCardHUDView view;
    private UserProfile userProfile;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        DataStore.i.settings.profanityChatFilteringEnabled.Set(true);

        view = PlayerInfoCardHUDView.CreateView();
        view.Initialize(null, null, null, null, null, null, null, null);

        CreateMockWearableByRarity(WearableLiterals.ItemRarity.EPIC);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.LEGENDARY);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.MYTHIC);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.RARE);
        CreateMockWearableByRarity(WearableLiterals.ItemRarity.UNIQUE);

        userProfileController = TestUtils.CreateComponentWithGameObject<UserProfileController>("UserProfileController");
        userProfileController.AddUserProfileToCatalog(new UserProfileModel { userId = "userid" });

        userProfile = UserProfileController.userProfilesCatalog.Get("userid");
        userProfile.UpdateData(new UserProfileModel
        {
            userId = "userId",
            name = "username",
            description = "description",
            email = "email"
        });
        userProfile.SetInventory(new[]
        {
            WearableLiterals.ItemRarity.EPIC,
            WearableLiterals.ItemRarity.LEGENDARY,
            WearableLiterals.ItemRarity.MYTHIC,
            WearableLiterals.ItemRarity.RARE,
            WearableLiterals.ItemRarity.UNIQUE,
        });
    }

    protected override IEnumerator TearDown()
    {
        UnityEngine.Object.Destroy(userProfileController.gameObject);
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
        CommonScriptableObjects.playerInfoCardVisibleState.Set(cardActive);
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

        var wearablesCatalogService = Environment.i.serviceLocator.Get<IWearablesCatalogService>();
        wearablesCatalogService.WearablesCatalog.Remove(rarity);
        wearablesCatalogService.WearablesCatalog.Add(rarity, wearable);

        return wearable;
    }
}
