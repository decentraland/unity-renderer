using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;
using static UnityEngine.Object;

public class WorldsSubSectionComponentViewTests
{
    private static int[] spritesAmounts = { 0, 1, 5, 6 };

    private WorldsSubSectionComponentView worldsSubSectionComponent;
    private Texture2D testTexture;
    private Sprite testSprite;

    [SetUp]
    public void SetUp()
    {
        worldsSubSectionComponent = Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/WorldsSubSection/WorldsSubSection")).GetComponent<WorldsSubSectionComponentView>();
        worldsSubSectionComponent.ConfigurePools();
        worldsSubSectionComponent.Start();
        testTexture = new Texture2D(20, 20);
        testSprite = Sprite.Create(testTexture, new Rect(), Vector2.zero);
    }

    [TearDown]
    public void TearDown()
    {
        worldsSubSectionComponent.worlds.ExtractItems();
        worldsSubSectionComponent.worldCardsPool.ReleaseAll();
        worldsSubSectionComponent.Dispose();

        Destroy(testTexture);
        Destroy(testSprite);
        Destroy(worldsSubSectionComponent.gameObject);

        if (worldsSubSectionComponent.worldModal != null)
            Destroy(worldsSubSectionComponent.worldModal.gameObject);
    }

    [UnityTest]
    public IEnumerator SetWorldsCorrectly([ValueSource(nameof(spritesAmounts))] int spritesAmount)
    {
        // Arrange
        worldsSubSectionComponent.worlds.RemoveItems();
        worldsSubSectionComponent.worlds.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)

        List<PlaceCardComponentModel> testWorlds = ExplorePlacesTestHelpers.CreateTestWorlds(testSprite, spritesAmount);

        // Act
        worldsSubSectionComponent.SetWorlds(testWorlds);

        for (int i = 0; i < spritesAmount - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(spritesAmount, worldsSubSectionComponent.worlds.instantiatedItems.Count, "The number of set worlds does not match.");

        for (int i = 0; i < spritesAmount; i++)
            Assert.IsTrue(worldsSubSectionComponent.worlds.instantiatedItems.Any(x => (x as PlaceCardComponentView)?.model == testWorlds[i]), $"The world {i} is not contained in the worlds grid");

        Assert.AreEqual(spritesAmount == 0, worldsSubSectionComponent.worldsNoDataContainer.activeSelf, "The worldsNoDataText should not be visible when there are worlds.");
    }

    [UnityTest]
    public IEnumerator AddWorldsCorrectly([ValueSource(nameof(spritesAmounts))] int spritesAmount)
    {
        // Arrange
        worldsSubSectionComponent.worlds.RemoveItems();
        worldsSubSectionComponent.worlds.gameObject.SetActive(false); // hack needed for fix AABB canvas fails on tests (happens only in the tests)
        List<PlaceCardComponentModel> testWorlds = ExplorePlacesTestHelpers.CreateTestWorlds(testSprite, spritesAmount);

        // Act
        worldsSubSectionComponent.AddWorlds(testWorlds);

        for (int i = 0; i < spritesAmount - 1; i++)
            yield return null;

        // Assert
        Assert.AreEqual(spritesAmount, worldsSubSectionComponent.worlds.instantiatedItems.Count, "The number of set worlds does not match.");

        for (int i = 0; i < spritesAmount; i++)
            Assert.IsTrue(worldsSubSectionComponent.worlds.instantiatedItems.Any(x => (x as PlaceCardComponentView)?.model == testWorlds[i]), "The world 1 is not contained in the worlds grid");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SetWorldsAsLoadingCorrectly(bool isVisible)
    {
        // Arrange
        worldsSubSectionComponent.worlds.gameObject.GetComponent<Canvas>().enabled = isVisible;
        worldsSubSectionComponent.worldsLoading.SetActive(!isVisible);
        worldsSubSectionComponent.worldsNoDataContainer.SetActive(true);

        // Act
        worldsSubSectionComponent.SetWorldsAsLoading(isVisible);

        // Assert
        Assert.AreEqual(!isVisible, worldsSubSectionComponent.worlds.GetComponent<Canvas>().enabled);
        Assert.AreEqual(isVisible, worldsSubSectionComponent.worldsLoading.activeSelf);

        if (isVisible)
            Assert.IsFalse(worldsSubSectionComponent.worldsNoDataContainer.activeSelf);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void SetShowMoreWorldsButtonActiveCorrectly(bool isVisible)
    {
        // Arrange
        worldsSubSectionComponent.showMoreWorldsButtonContainer.gameObject.SetActive(!isVisible);

        // Act
        worldsSubSectionComponent.SetShowMoreWorldsButtonActive(isVisible);

        // Assert
        Assert.AreEqual(isVisible, worldsSubSectionComponent.showMoreWorldsButtonContainer.gameObject.activeSelf);
    }
}
