using DCL;
using NUnit.Framework;
using UnityEngine;

public class ExploreV2MenuComponentViewTests
{
    private ExploreV2MenuComponentView exploreV2MenuComponent;

    [SetUp]
    public void SetUp()
    {
        exploreV2MenuComponent = Object.Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>();
        exploreV2MenuComponent.Start();
    }

    [TearDown]
    public void TearDown()
    {
        exploreV2MenuComponent.Dispose();
        GameObject.Destroy(exploreV2MenuComponent.gameObject);
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void SetVisibleCorrectly(bool isVisible)
    {
        // Arrange
        exploreV2MenuComponent.placesAndEventsSection.gameObject.SetActive(!isVisible);

        // Act
        exploreV2MenuComponent.SetVisible(isVisible);

        // Assert
        Assert.AreEqual(isVisible, exploreV2MenuComponent.isVisible, "exploreV2MenuComponent isVisible property does not match.");
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void CreateSectionSelectorMappingsCorrectly(int sectionIndex)
    {
        // Arrange
        exploreV2MenuComponent.sectionSelector.RefreshControl();
        ExploreSection sectionSelected = sectionIndex == 0 ? ExploreSection.Backpack : ExploreSection.Explore;
        exploreV2MenuComponent.OnSectionOpen += (section) => sectionSelected = section;

        // Act
        exploreV2MenuComponent.CreateSectionSelectorMappings();
        exploreV2MenuComponent.sectionSelector.GetSection(sectionIndex).onSelect.Invoke(true);

        // Assert
        Assert.AreEqual(sectionIndex, (int)DataStore.i.exploreV2.currentSectionIndex.Get());
        Assert.AreEqual(sectionIndex, (int)sectionSelected);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void RemoveSectionSelectorMappingsCorrectly(int sectionIndex)
    {
        // Arrange
        exploreV2MenuComponent.sectionSelector.RefreshControl();
        exploreV2MenuComponent.CreateSectionSelectorMappings();
        exploreV2MenuComponent.placesAndEventsSection.gameObject.SetActive(false);

        // Act
        exploreV2MenuComponent.RemoveSectionSelectorMappings();
        exploreV2MenuComponent.sectionSelector.GetSection(sectionIndex).onSelect.Invoke(true);

        // Assert
        switch (sectionIndex)
        {
            case 0:
                Assert.IsFalse(exploreV2MenuComponent.placesAndEventsSection.gameObject.activeSelf);
                break;
        }
    }

    [Test]
    [TestCase("TestFromCloseButton")]
    [TestCase("TestFromCloseAction")]
    public void ConfigureCloseButtonCorrectly(string fromAction)
    {
        // Arrange
        bool closeButtonClicked = false;
        exploreV2MenuComponent.OnCloseButtonPressed += (fromShortcut) => closeButtonClicked = true;

        // Act
        exploreV2MenuComponent.ConfigureCloseButton();
        if (fromAction == "TestFromCloseButton")
            exploreV2MenuComponent.closeMenuButton.onClick.Invoke();
        else
            exploreV2MenuComponent.closeAction.RaiseOnTriggered();

        // Assert
        Assert.IsTrue(closeButtonClicked, "The close button was not clicked.");
    }
}