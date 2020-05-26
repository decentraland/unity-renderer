using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class AirdroppingHUDController_Should : TestsBase
{
    protected override bool justSceneSetUp => true;

    private AirdroppingHUDController controller;
    private AirdroppingHUDController.Model model_2Item;
    private AirdroppingHUDController.Model model_0Item;

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        model_2Item = new AirdroppingHUDController.Model()
        {
            id = "id",
            title = "title",
            subtitle = "subtitle",
            items = new[]
            {
                new AirdroppingHUDController.ItemModel()
                {
                    name = "itemName1",
                    rarity = "rare",
                    subtitle = "itemSubtitle",
                    type = "collectible",
                    thumbnailURL = "theUrl"
                },
                new AirdroppingHUDController.ItemModel()
                {
                    name = "itemName2",
                    rarity = "rare",
                    subtitle = "itemSubtitle",
                    type = "collectible",
                    thumbnailURL = "theUrl"
                },
            }
        };

        model_0Item = new AirdroppingHUDController.Model()
        {
            id = "id",
            title = "title",
            subtitle = "subtitle",
            items = new AirdroppingHUDController.ItemModel[0]
        };

        controller = new AirdroppingHUDController();
        ThumbnailsManager.bypassRequests = true;
    }

    [Test]
    public void ReactToAirdropRequest()
    {
        controller.AirdroppingRequested(model_2Item);

        Assert.AreEqual(model_2Item, controller.model);
        Assert.AreEqual(AirdroppingHUDController.State.Initial, controller.currentState);
        Assert.AreEqual(0, controller.currentItemShown);
        Assert.AreEqual(2, controller.totalItems);
    }

    [Test]
    public void MoveThroughStatesProperly_WithItems()
    {
        controller.AirdroppingRequested(model_2Item);

        Assert.AreEqual(AirdroppingHUDController.State.Initial, controller.currentState);
        controller.MoveToNextState();
        Assert.AreEqual(AirdroppingHUDController.State.SingleItem, controller.currentState);
        Assert.AreEqual(0, controller.currentItemShown);
        controller.MoveToNextState();
        Assert.AreEqual(AirdroppingHUDController.State.SingleItem, controller.currentState);
        Assert.AreEqual(1, controller.currentItemShown);
        controller.MoveToNextState();
        Assert.AreEqual(AirdroppingHUDController.State.Summary, controller.currentState);
        controller.MoveToNextState();
        Assert.AreEqual(AirdroppingHUDController.State.Hidden, controller.currentState);
    }

    [Test]
    public void MoveThroughStatesProperly_WithNoItems()
    {
        controller.AirdroppingRequested(model_0Item);

        Assert.AreEqual(AirdroppingHUDController.State.Initial, controller.currentState);
        controller.MoveToNextState();
        Assert.AreEqual(AirdroppingHUDController.State.Summary_NoItems, controller.currentState);
        controller.MoveToNextState();
        Assert.AreEqual(AirdroppingHUDController.State.Hidden, controller.currentState);
    }

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        ThumbnailsManager.bypassRequests = false;
        controller.Dispose();
        return base.TearDown();
    }
}