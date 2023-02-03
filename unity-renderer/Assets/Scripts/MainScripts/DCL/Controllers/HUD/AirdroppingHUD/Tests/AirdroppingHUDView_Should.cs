using Cysharp.Threading.Tasks;
using DCL.Providers;
using System.Collections;
using NUnit.Framework;
using System;
using UnityEngine.TestTools;

public class AirdroppingHUDView_Should : IntegrationTestSuite_Legacy
{
    private AirdroppingHUDController controller;
    private AirdroppingHUDView view;
    private HUDFactory factory;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        ThumbnailsManager.bypassRequests = true;

        factory = new HUDFactory(new AddressableResourceProvider());

        yield return factory.CreateAirdroppingHUDView().ToCoroutine(resultHandler: CreateController);

        void CreateController(AirdroppingHUDView viewAsset)
        {
            controller = new AirdroppingHUDController(viewAsset);
            view = viewAsset;
        }
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        ThumbnailsManager.bypassRequests = false;
        controller.Dispose();
        factory.Dispose();
        return base.TearDown();
    }

    [Test]
    public void CleanStateProperly()
    {
        view.CleanState();
        Assert.IsFalse(view.initialScreen.activeSelf);
        Assert.IsFalse(view.singleItemScreen.activeSelf);
        Assert.IsFalse(view.summaryScreen.activeSelf);
        Assert.IsFalse(view.summaryNoItemsScreen.activeSelf);
        Assert.AreEqual(0, view.singleItemContainer.transform.childCount);
        Assert.AreEqual(0, view.summaryItemsContainer.transform.childCount);
    }

    [Test]
    public void ShowInitialScreenProperly()
    {
        view.ShowInitialScreen("testTitle", "testSubtitle");
        Assert.IsTrue(view.initialScreen.activeSelf);
        Assert.AreEqual("testTitle", view.initialScreenTitle.text);
        Assert.AreEqual("testSubtitle", view.initialScreenSubtitle.text);
    }

    [Test]
    public void ShowItemScreenProperly()
    {
        view.ShowItemScreen(new AirdroppingHUDController.ItemModel()
        {
            name = "item",
            rarity = "rare",
            subtitle = "subtitle",
            type = "collectible",
            thumbnailURL = "url"
        }, 1);

        AirdroppingItemPanel itemPanel = view.singleItemContainer.GetComponentInChildren<AirdroppingItemPanel>();

        Assert.IsTrue(view.singleItemScreen.activeSelf);
        Assert.AreEqual("1", view.itemsLeft.text);
        Assert.NotNull(itemPanel);
        Assert.AreEqual("item", itemPanel.name.text);
        Assert.AreEqual("subtitle", itemPanel.subtitle.text);
    }

    [Test]
    public void ShowSummaryScreenProperly()
    {
        view.ShowSummaryScreen(new[]
        {
            new AirdroppingHUDController.ItemModel()
            {
                name = "item",
                rarity = "rare",
                subtitle = "subtitle",
                type = "collectible",
                thumbnailURL = "url"
            },
            new AirdroppingHUDController.ItemModel()
            {
                name = "item2",
                rarity = "rare",
                subtitle = "subtitle2",
                type = "collectible",
                thumbnailURL = "url"
            }
        });

        AirdroppingItemPanel[] itemPanel = view.summaryItemsContainer.GetComponentsInChildren<AirdroppingItemPanel>();

        Assert.IsTrue(view.summaryScreen.activeSelf);
        Assert.NotNull(itemPanel);
        Assert.AreEqual(2, itemPanel.Length);
        Assert.AreEqual("item", itemPanel[0].name.text);
        Assert.AreEqual("subtitle", itemPanel[0].subtitle.text);
        Assert.AreEqual("item2", itemPanel[1].name.text);
        Assert.AreEqual("subtitle2", itemPanel[1].subtitle.text);
    }
}
