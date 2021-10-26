using NUnit.Framework;
using UnityEngine;

public class ExploreV2MenuComponentViewTests
{
    private ExploreV2MenuComponentView exploreV2MenuComponent;

    [SetUp]
    public void SetUp() { exploreV2MenuComponent = Object.Instantiate(Resources.Load<GameObject>("MainMenu/ExploreV2Menu")).GetComponent<ExploreV2MenuComponentView>(); }

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
        Assert.AreEqual(isVisible, exploreV2MenuComponent.placesAndEventsSection.gameObject.activeSelf, "placesAndEventsSection active property does not match.");
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

        // Act
        exploreV2MenuComponent.CreateSectionSelectorMappings();
        exploreV2MenuComponent.sectionSelector.GetSection(sectionIndex).onSelect.Invoke(true);

        // Assert
        switch (sectionIndex)
        {
            case 0:
                Assert.IsTrue(exploreV2MenuComponent.placesAndEventsSection.gameObject.activeSelf);
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
        exploreV2MenuComponent.OnCloseButtonPressed += () => closeButtonClicked = true;

        // Act
        exploreV2MenuComponent.ConfigureCloseButton();
        if (fromAction == "TestFromCloseButton")
            exploreV2MenuComponent.closeMenuButton.onClick.Invoke();
        else
            exploreV2MenuComponent.closeAction.RaiseOnTriggered();

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