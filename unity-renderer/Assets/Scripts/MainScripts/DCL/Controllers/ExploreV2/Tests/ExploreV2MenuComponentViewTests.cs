using NUnit.Framework;
using UnityEngine;

public class ExploreV2MenuComponentViewTests
{
    private ExploreV2MenuComponentView exploreV2MenuComponent;

    [SetUp]
    public void SetUp() { exploreV2MenuComponent = BaseComponentView.Create<ExploreV2MenuComponentView>("MainMenu/ExploreV2Menu"); }

    [TearDown]
    public void TearDown()
    {
        exploreV2MenuComponent.Dispose();
        GameObject.Destroy(exploreV2MenuComponent.gameObject);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetActiveCorrectly(bool isActive)
    {
        // Arrange
        exploreV2MenuComponent.gameObject.SetActive(!isActive);

        // Act
        exploreV2MenuComponent.SetActive(isActive);

        // Assert
        Assert.AreEqual(isActive, exploreV2MenuComponent.gameObject.activeSelf, "The active property of the explore V2 menu does not match.");
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void CreateSectionSelectorMappingsCorrectly(int sectionIndex)
    {
        // Arrange
        exploreV2MenuComponent.sectionSelector.RefreshControl();
        exploreV2MenuComponent.placesAndEventsSection.gameObject.SetActive(false);
        exploreV2MenuComponent.questSection.gameObject.SetActive(false);
        exploreV2MenuComponent.backpackSection.gameObject.SetActive(false);
        exploreV2MenuComponent.mapSection.gameObject.SetActive(false);
        exploreV2MenuComponent.builderSection.gameObject.SetActive(false);
        exploreV2MenuComponent.marketSection.gameObject.SetActive(false);
        exploreV2MenuComponent.settingsSection.gameObject.SetActive(false);

        // Act
        exploreV2MenuComponent.CreateSectionSelectorMappings();
        exploreV2MenuComponent.sectionSelector.GetSection(sectionIndex).onSelect.Invoke(true);

        // Assert
        switch (sectionIndex)
        {
            case 0:
                Assert.IsTrue(exploreV2MenuComponent.placesAndEventsSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsTrue(exploreV2MenuComponent.questSection.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsTrue(exploreV2MenuComponent.backpackSection.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsTrue(exploreV2MenuComponent.mapSection.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsTrue(exploreV2MenuComponent.builderSection.gameObject.activeSelf);
                break;
            case 5:
                Assert.IsTrue(exploreV2MenuComponent.marketSection.gameObject.activeSelf);
                break;
            case 6:
                Assert.IsTrue(exploreV2MenuComponent.settingsSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    [TestCase(6)]
    public void RemoveSectionSelectorMappingsCorrectly(int sectionIndex)
    {
        // Arrange
        exploreV2MenuComponent.sectionSelector.RefreshControl();
        exploreV2MenuComponent.CreateSectionSelectorMappings();
        exploreV2MenuComponent.placesAndEventsSection.gameObject.SetActive(false);
        exploreV2MenuComponent.questSection.gameObject.SetActive(false);
        exploreV2MenuComponent.backpackSection.gameObject.SetActive(false);
        exploreV2MenuComponent.mapSection.gameObject.SetActive(false);
        exploreV2MenuComponent.builderSection.gameObject.SetActive(false);
        exploreV2MenuComponent.marketSection.gameObject.SetActive(false);
        exploreV2MenuComponent.settingsSection.gameObject.SetActive(false);

        // Act
        exploreV2MenuComponent.RemoveSectionSelectorMappings();
        exploreV2MenuComponent.sectionSelector.GetSection(sectionIndex).onSelect.Invoke(true);

        // Assert
        switch (sectionIndex)
        {
            case 0:
                Assert.IsFalse(exploreV2MenuComponent.placesAndEventsSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsFalse(exploreV2MenuComponent.questSection.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsFalse(exploreV2MenuComponent.backpackSection.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsFalse(exploreV2MenuComponent.mapSection.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsFalse(exploreV2MenuComponent.builderSection.gameObject.activeSelf);
                break;
            case 5:
                Assert.IsFalse(exploreV2MenuComponent.marketSection.gameObject.activeSelf);
                break;
            case 6:
                Assert.IsFalse(exploreV2MenuComponent.settingsSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    public void ConfigureCloseButtonCorrectly()
    {
        // Arrange
        bool closeButtonClicked = false;
        exploreV2MenuComponent.OnCloseButtonPressed += () => closeButtonClicked = true;

        // Act
        exploreV2MenuComponent.ConfigureCloseButton();
        exploreV2MenuComponent.closeMenuButton.onClick.Invoke();

        // Assert
        Assert.IsTrue(closeButtonClicked, "The close button was not clicked.");
    }

    [Test]
    public void ShowDefaultSectionCorrectly()
    {
        // Arrange
        exploreV2MenuComponent.placesAndEventsSection.gameObject.SetActive(false);

        // Act
        exploreV2MenuComponent.ShowDefaultSection();

        // Assert
        Assert.IsTrue(exploreV2MenuComponent.placesAndEventsSection.gameObject.activeSelf, "The explore section should be actived.");
    }
}