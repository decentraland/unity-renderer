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
    [TestCase(ExploreSection.Backpack, true)]
    [TestCase(ExploreSection.Explore, true)]
    [TestCase(ExploreSection.Map, true)]
    [TestCase(ExploreSection.Quest, true)]
    [TestCase(ExploreSection.Settings, true)]
    [TestCase(ExploreSection.Backpack, false)]
    [TestCase(ExploreSection.Explore, false)]
    [TestCase(ExploreSection.Map, false)]
    [TestCase(ExploreSection.Quest, false)]
    [TestCase(ExploreSection.Settings, false)]
    public void SetSectionActiveCorrectly(ExploreSection section, bool isActive)
    {
        // Act
        exploreV2MenuComponent.SetSectionActive(section, isActive);

        // Assert
        Assert.AreEqual(isActive, exploreV2MenuComponent.sectionSelector.GetSection((int)section).IsActive());
    }

    [Test]
    [TestCase(ExploreSection.Backpack)]
    [TestCase(ExploreSection.Map)]
    [TestCase(ExploreSection.Quest)]
    [TestCase(ExploreSection.Settings)]
    public void ConfigureEncapsulatedSectionCorrectly(ExploreSection section)
    {
        // Arrange
        GameObject testGO = new GameObject();
        BaseVariable<Transform> featureConfiguratorFlag = new BaseVariable<Transform>(testGO.transform);

        // Act
        exploreV2MenuComponent.ConfigureEncapsulatedSection(section, featureConfiguratorFlag);

        // Assert
        Assert.IsTrue(featureConfiguratorFlag.Get());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
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
        Assert.AreEqual(sectionIndex, (int)sectionSelected);
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    public void RemoveSectionSelectorMappingsCorrectly(int sectionIndex)
    {
        // Arrange
        exploreV2MenuComponent.sectionSelector.RefreshControl();
        exploreV2MenuComponent.CreateSectionSelectorMappings();

        if (sectionIndex == 0)
            DataStore.i.exploreV2.currentSectionIndex.Set(1, false);
        else
            DataStore.i.exploreV2.currentSectionIndex.Set(0, false);

        // Act
        exploreV2MenuComponent.RemoveSectionSelectorMappings();
        exploreV2MenuComponent.sectionSelector.GetSection(sectionIndex).onSelect.Invoke(true);

        // Assert
        Assert.AreNotEqual(sectionIndex, DataStore.i.exploreV2.currentSectionIndex.Get());
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

    [Test]
    public void ConfigureRealmSelectorModalCorrectly()
    {
        // Arrange

        // Act
        RealmSelectorComponentView testModal = exploreV2MenuComponent.ConfigureRealmSelectorModal();

        // Assert
        Assert.IsNotNull(testModal);
        Assert.AreEqual(ExploreV2MenuComponentView.REALM_SELECTOR_MODAL_ID, testModal.gameObject.name);
    }
}