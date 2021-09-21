using NUnit.Framework;
using UnityEngine;

public class ExploreSectionComponentViewTests
{
    private ExploreSectionComponentView exploreSectionComponent;

    [SetUp]
    public void SetUp() { exploreSectionComponent = BaseComponentView.Create<ExploreSectionComponentView>("Sections/Explore/ExploreSection"); }

    [TearDown]
    public void TearDown()
    {
        exploreSectionComponent.Dispose();
        GameObject.Destroy(exploreSectionComponent.gameObject);
    }

    [Test]
    public void ConfigureExploreSectionCorrectly()
    {
        // Arrange
        ExploreSectionComponentModel testModel = new ExploreSectionComponentModel
            { };

        // Act
        exploreSectionComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, exploreSectionComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void CreateSubSectionSelectorMappingsCorrectly(int subSectionIndex)
    {
        // Arrange
        exploreSectionComponent.highlightsSubSection.gameObject.SetActive(false);
        exploreSectionComponent.placesSubSection.gameObject.SetActive(false);
        exploreSectionComponent.favoritesSubSection.gameObject.SetActive(false);
        exploreSectionComponent.myPlacesSubSection.gameObject.SetActive(false);
        exploreSectionComponent.eventsSubSection.gameObject.SetActive(false);

        // Act
        exploreSectionComponent.CreateSubSectionSelectorMappings();
        exploreSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case 0:
                Assert.IsTrue(exploreSectionComponent.highlightsSubSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsTrue(exploreSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsTrue(exploreSectionComponent.favoritesSubSection.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsTrue(exploreSectionComponent.myPlacesSubSection.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsTrue(exploreSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void RemoveSectionSelectorMappingsCorrectly(int subSectionIndex)
    {
        // Arrange
        exploreSectionComponent.CreateSubSectionSelectorMappings();
        exploreSectionComponent.highlightsSubSection.gameObject.SetActive(false);
        exploreSectionComponent.placesSubSection.gameObject.SetActive(false);
        exploreSectionComponent.favoritesSubSection.gameObject.SetActive(false);
        exploreSectionComponent.myPlacesSubSection.gameObject.SetActive(false);
        exploreSectionComponent.eventsSubSection.gameObject.SetActive(false);

        // Act
        exploreSectionComponent.RemoveSectionSelectorMappings();
        exploreSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case 0:
                Assert.IsFalse(exploreSectionComponent.highlightsSubSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsFalse(exploreSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsFalse(exploreSectionComponent.favoritesSubSection.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsFalse(exploreSectionComponent.myPlacesSubSection.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsFalse(exploreSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    public void ShowDefaultSubSectionCorrectly()
    {
        // Arrange
        exploreSectionComponent.highlightsSubSection.gameObject.SetActive(false);

        // Act
        exploreSectionComponent.ShowDefaultSubSection();

        // Assert
        Assert.IsTrue(exploreSectionComponent.highlightsSubSection.gameObject.activeSelf, "The highlights sub-section should be actived.");
    }
}