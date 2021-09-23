using NUnit.Framework;
using UnityEngine;

public class PlacesAndEventsSectionComponentViewTests
{
    private PlacesAndEventsSectionComponentView placesAndEventsSectionComponent;

    [SetUp]
    public void SetUp() { placesAndEventsSectionComponent = BaseComponentView.Create<PlacesAndEventsSectionComponentView>("Sections/PlacesAndEvents/PlacesAndEventsSection"); }

    [TearDown]
    public void TearDown()
    {
        placesAndEventsSectionComponent.Dispose();
        GameObject.Destroy(placesAndEventsSectionComponent.gameObject);
    }

    [Test]
    public void ConfigurePlacesAndEventsSectionCorrectly()
    {
        // Arrange
        PlacesAndEventsSectionComponentModel testModel = new PlacesAndEventsSectionComponentModel
            { };

        // Act
        placesAndEventsSectionComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, placesAndEventsSectionComponent.model, "The model does not match after configuring the button.");
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
        placesAndEventsSectionComponent.highlightsSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.placesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.favoritesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.myPlacesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.eventsSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.CreateSubSectionSelectorMappings();
        placesAndEventsSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case 0:
                Assert.IsTrue(placesAndEventsSectionComponent.highlightsSubSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsTrue(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsTrue(placesAndEventsSectionComponent.favoritesSubSection.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsTrue(placesAndEventsSectionComponent.myPlacesSubSection.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsTrue(placesAndEventsSectionComponent.eventsSubSection.gameObject.activeSelf);
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
        placesAndEventsSectionComponent.CreateSubSectionSelectorMappings();
        placesAndEventsSectionComponent.highlightsSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.placesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.favoritesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.myPlacesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.eventsSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.RemoveSectionSelectorMappings();
        placesAndEventsSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case 0:
                Assert.IsFalse(placesAndEventsSectionComponent.highlightsSubSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsFalse(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case 2:
                Assert.IsFalse(placesAndEventsSectionComponent.favoritesSubSection.gameObject.activeSelf);
                break;
            case 3:
                Assert.IsFalse(placesAndEventsSectionComponent.myPlacesSubSection.gameObject.activeSelf);
                break;
            case 4:
                Assert.IsFalse(placesAndEventsSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    public void ShowDefaultSubSectionCorrectly()
    {
        // Arrange
        placesAndEventsSectionComponent.highlightsSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.ShowDefaultSubSection();

        // Assert
        Assert.IsTrue(placesAndEventsSectionComponent.highlightsSubSection.gameObject.activeSelf, "The highlights sub-section should be actived.");
    }
}