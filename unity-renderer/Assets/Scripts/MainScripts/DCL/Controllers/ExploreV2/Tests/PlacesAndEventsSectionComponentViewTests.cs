using NUnit.Framework;
using UnityEngine;

public class PlacesAndEventsSectionComponentViewTests
{
    private PlacesAndEventsSectionComponentView placesAndEventsSectionComponent;

    [SetUp]
    public void SetUp() { placesAndEventsSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesAndEventsSection")).GetComponent<PlacesAndEventsSectionComponentView>(); }

    [TearDown]
    public void TearDown() { GameObject.Destroy(placesAndEventsSectionComponent.gameObject); }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public void CreateSubSectionSelectorMappingsCorrectly(int subSectionIndex)
    {
        // Arrange
        placesAndEventsSectionComponent.subSectionSelector.RefreshControl();
        placesAndEventsSectionComponent.placesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.eventsSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.CreateSubSectionSelectorMappings();
        placesAndEventsSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case 0:
                Assert.IsTrue(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsTrue(placesAndEventsSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    public void RemoveSectionSelectorMappingsCorrectly(int subSectionIndex)
    {
        // Arrange
        placesAndEventsSectionComponent.subSectionSelector.RefreshControl();
        placesAndEventsSectionComponent.CreateSubSectionSelectorMappings();
        placesAndEventsSectionComponent.placesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.eventsSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.RemoveSectionSelectorMappings();
        placesAndEventsSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case 0:
                Assert.IsFalse(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case 1:
                Assert.IsFalse(placesAndEventsSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    public void ShowDefaultSubSectionCorrectly()
    {
        // Arrange
        placesAndEventsSectionComponent.placesSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.ShowDefaultSubSection();

        // Assert
        Assert.IsTrue(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf, "The places sub-section should be actived.");
    }
}