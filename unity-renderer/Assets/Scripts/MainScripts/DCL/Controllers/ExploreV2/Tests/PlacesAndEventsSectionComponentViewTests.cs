using NUnit.Framework;
using UnityEngine;

public class PlacesAndEventsSectionComponentViewTests
{
    private PlacesAndEventsSectionComponentView placesAndEventsSectionComponent;

    [SetUp]
    public void SetUp()
    {
        placesAndEventsSectionComponent = Object.Instantiate(Resources.Load<GameObject>("Sections/PlacesAndEventsSection/PlacesAndEventsSection")).GetComponent<PlacesAndEventsSectionComponentView>();
        placesAndEventsSectionComponent.Start();
    }

    [TearDown]
    public void TearDown()
    {
        placesAndEventsSectionComponent.Dispose();
        GameObject.Destroy(placesAndEventsSectionComponent.gameObject);
    }

    [Test]
    [TestCase(PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX)]
    [TestCase(PlacesAndEventsSectionComponentView.PLACES_SUB_SECTION_INDEX)]
    [TestCase(PlacesAndEventsSectionComponentView.HIGHLIGHTS_SUB_SECTION_INDEX)]
    public void CreateSubSectionSelectorMappingsCorrectly(int subSectionIndex)
    {
        // Arrange
        placesAndEventsSectionComponent.highlightsSubSection.GetComponent<Canvas>().enabled = false;
        placesAndEventsSectionComponent.placesSubSection.GetComponent<Canvas>().enabled = false;
        placesAndEventsSectionComponent.eventsSubSection.GetComponent<Canvas>().enabled = false;

        // Act
        placesAndEventsSectionComponent.CreateSubSectionSelectorMappings();
        placesAndEventsSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case PlacesAndEventsSectionComponentView.HIGHLIGHTS_SUB_SECTION_INDEX:
                Assert.IsTrue(placesAndEventsSectionComponent.highlightsSubSection.GetComponent<Canvas>().enabled);
                break;
            case PlacesAndEventsSectionComponentView.PLACES_SUB_SECTION_INDEX:
                Assert.IsTrue(placesAndEventsSectionComponent.placesSubSection.GetComponent<Canvas>().enabled);
                break;
            case PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX:
                Assert.IsTrue(placesAndEventsSectionComponent.eventsSubSection.GetComponent<Canvas>().enabled);
                break;
        }
    }

    [Test]
    [TestCase(PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX)]
    [TestCase(PlacesAndEventsSectionComponentView.PLACES_SUB_SECTION_INDEX)]
    [TestCase(PlacesAndEventsSectionComponentView.HIGHLIGHTS_SUB_SECTION_INDEX)]
    public void RemoveSectionSelectorMappingsCorrectly(int subSectionIndex)
    {
        // Arrange
        placesAndEventsSectionComponent.subSectionSelector.RefreshControl();
        placesAndEventsSectionComponent.CreateSubSectionSelectorMappings();
        placesAndEventsSectionComponent.placesSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.eventsSubSection.gameObject.SetActive(false);
        placesAndEventsSectionComponent.highlightsSubSection.gameObject.SetActive(false);

        // Act
        placesAndEventsSectionComponent.RemoveSectionSelectorMappings();
        placesAndEventsSectionComponent.subSectionSelector.GetSection(subSectionIndex).onSelect.Invoke(true);

        // Assert
        switch (subSectionIndex)
        {
            case PlacesAndEventsSectionComponentView.PLACES_SUB_SECTION_INDEX:
                Assert.IsFalse(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX:
                Assert.IsFalse(placesAndEventsSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
            case PlacesAndEventsSectionComponentView.HIGHLIGHTS_SUB_SECTION_INDEX:
                Assert.IsFalse(placesAndEventsSectionComponent.highlightsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    [TestCase(PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX)]
    [TestCase(PlacesAndEventsSectionComponentView.PLACES_SUB_SECTION_INDEX)]
    [TestCase(PlacesAndEventsSectionComponentView.HIGHLIGHTS_SUB_SECTION_INDEX)]
    public void GoToSubsectionCorrectly(int subSectionIndex)
    {
        // Act
        placesAndEventsSectionComponent.GoToSubsection(subSectionIndex);

        // Assert
        switch (subSectionIndex)
        {
            case PlacesAndEventsSectionComponentView.PLACES_SUB_SECTION_INDEX:
                Assert.IsTrue(placesAndEventsSectionComponent.placesSubSection.gameObject.activeSelf);
                break;
            case PlacesAndEventsSectionComponentView.EVENTS_SUB_SECTION_INDEX:
                Assert.IsTrue(placesAndEventsSectionComponent.eventsSubSection.gameObject.activeSelf);
                break;
            case PlacesAndEventsSectionComponentView.HIGHLIGHTS_SUB_SECTION_INDEX:
                Assert.IsTrue(placesAndEventsSectionComponent.highlightsSubSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetActiveCorrectly(bool isActive)
    {
        // Arrange
        placesAndEventsSectionComponent.GetComponent<Canvas>().enabled = !isActive;

        // Act
        placesAndEventsSectionComponent.SetActive(isActive);

        // Assert
        Assert.AreEqual(isActive, placesAndEventsSectionComponent.GetComponent<Canvas>().enabled);
    }
}