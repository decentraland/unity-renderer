using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

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
    public void ConfigureExploreV2MenuCorrectly()
    {
        // Arrange
        ExploreV2MenuComponentModel testModel = new ExploreV2MenuComponentModel
        {
            profileInfo = new ProfileCardComponentModel
            {
                profilePicture = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero),
                profileName = "Test Name",
                profileAddress = "Test Address",
                onClick = new Button.ButtonClickedEvent()
            },
            realmInfo = new RealmViewerComponentModel
            {
                realmName = "Test Realm Name",
                numberOfUsers = 50000
            }
        };

        // Act
        exploreV2MenuComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, exploreV2MenuComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void RefreshExploreV2MenuCorrectly()
    {
        // Arrange
        Sprite testPicture = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero);
        string testProfileName = "Test Name";
        string testAddress = "Test Address";
        Button.ButtonClickedEvent testClickedEvent = new Button.ButtonClickedEvent();

        exploreV2MenuComponent.model.profileInfo = new ProfileCardComponentModel
        {
            profilePicture = testPicture,
            profileName = testProfileName,
            profileAddress = testAddress,
            onClick = testClickedEvent
        };

        string testRealmName = "Test Realm Name";
        int testNumberOfUsers = 50000;

        exploreV2MenuComponent.model.realmInfo = new RealmViewerComponentModel
        {
            realmName = testRealmName,
            numberOfUsers = testNumberOfUsers
        };

        // Act
        exploreV2MenuComponent.RefreshControl();

        // Assert
        Assert.AreEqual(testPicture, exploreV2MenuComponent.model.profileInfo.profilePicture, "The profile picture does not match in the model.");
        Assert.AreEqual(testProfileName, exploreV2MenuComponent.model.profileInfo.profileName, "The profile name does not match in the model.");
        Assert.AreEqual(testAddress, exploreV2MenuComponent.model.profileInfo.profileAddress, "The profile address does not match in the model.");
        Assert.AreEqual(testClickedEvent, exploreV2MenuComponent.model.profileInfo.onClick, "The profile address does not match in the model.");
        Assert.AreEqual(testRealmName, exploreV2MenuComponent.model.realmInfo.realmName, "The realm name does not match in the model.");
        Assert.AreEqual(testNumberOfUsers, exploreV2MenuComponent.model.realmInfo.numberOfUsers, "The number of users does not match in the model.");
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
    public void SetRealmInfoCorrectly()
    {
        // Arrange
        RealmViewerComponentModel testModel = new RealmViewerComponentModel
        {
            realmName = "Test Realm Name",
            numberOfUsers = 50000
        };

        // Act
        exploreV2MenuComponent.SetRealmInfo(testModel);

        // Assert
        Assert.AreEqual(testModel, exploreV2MenuComponent.model.realmInfo, "The realm info model does not match after configuring the button.");
    }

    [Test]
    public void SetProfileInfoCorrectly()
    {
        // Arrange
        ProfileCardComponentModel testModel = new ProfileCardComponentModel
        {
            profilePicture = Sprite.Create(new Texture2D(10, 10), new Rect(), Vector2.zero),
            profileName = "Test Name",
            profileAddress = "Test Address",
            onClick = new Button.ButtonClickedEvent()
        };

        // Act
        exploreV2MenuComponent.SetProfileInfo(testModel);

        // Assert
        Assert.AreEqual(testModel, exploreV2MenuComponent.model.profileInfo, "The profile info model does not match after configuring the button.");
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
        exploreV2MenuComponent.exploreSection.gameObject.SetActive(false);
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
                Assert.IsTrue(exploreV2MenuComponent.exploreSection.gameObject.activeSelf);
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
        exploreV2MenuComponent.CreateSectionSelectorMappings();
        exploreV2MenuComponent.exploreSection.gameObject.SetActive(false);
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
                Assert.IsFalse(exploreV2MenuComponent.exploreSection.gameObject.activeSelf);
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
        exploreV2MenuComponent.exploreSection.gameObject.SetActive(false);

        // Act
        exploreV2MenuComponent.ShowDefaultSection();

        // Assert
        Assert.IsTrue(exploreV2MenuComponent.exploreSection.gameObject.activeSelf, "The explore section should be actived.");
    }
}