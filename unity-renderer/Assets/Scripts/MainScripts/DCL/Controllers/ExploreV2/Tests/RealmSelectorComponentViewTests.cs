using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RealmSelectorComponentViewTests
{
    private RealmSelectorComponentView realmSelectorComponent;

    [SetUp]
    public void SetUp()
    {
        realmSelectorComponent = BaseComponentView.Create<RealmSelectorComponentView>("MainMenu/Realms/RealmSelector_Modal");
    }

    [TearDown]
    public void TearDown()
    {
        realmSelectorComponent.Dispose();
    }

    [Test]
    public void ConfigureRealmSelectorCorrectly()
    {
        // Arrange
        RealmSelectorComponentModel testModel = new RealmSelectorComponentModel
        {
            currentRealmName = "Test Name"
        };

        // Act
        realmSelectorComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, realmSelectorComponent.model, "The model does not match after configuring the realm selector.");
    }

    [Test]
    public void SetCurrentRealmCorrectly()
    {
        // Arrange
        string testName = "Test Name";

        // Act
        realmSelectorComponent.SetCurrentRealm(testName);

        // Assert
        Assert.AreEqual(testName, realmSelectorComponent.model.currentRealmName, "The realm name does not match in the model.");
        Assert.IsTrue(realmSelectorComponent.currentRealmText.text.ToUpper().Contains(testName.ToUpper()));
    }

    [Test]
    public void SetAvailableRealmsCorrectly()
    {
        // Arrange
        List<RealmRowComponentModel> testRealms = new List<RealmRowComponentModel>
        {
            new RealmRowComponentModel
            {
                backgroundColor = Color.black,
                isConnected = false,
                name = "TestName1",
                players = 10
            },
            new RealmRowComponentModel
            {
                backgroundColor = Color.green,
                isConnected = true,
                name = "TestName2",
                players = 20
            }
        };

        // Act
        realmSelectorComponent.SetAvailableRealms(testRealms);

        // Assert
        Assert.AreEqual(2, realmSelectorComponent.availableRealms.instantiatedItems.Count, "The number of set realms does not match.");
        Assert.IsTrue(realmSelectorComponent.availableRealms.instantiatedItems.Any(x => (x as RealmRowComponentView).model == testRealms[0]), "The realm 1 is not contained in the realms grid");
        Assert.IsTrue(realmSelectorComponent.availableRealms.instantiatedItems.Any(x => (x as RealmRowComponentView).model == testRealms[1]), "The realm 2 is not contained in the realms grid");
    }

    [Test]
    public void CloseModalCorrectly()
    {
        // Arrange
        realmSelectorComponent.Show();

        // Act
        realmSelectorComponent.CloseModal();

        // Assert
        Assert.IsFalse(realmSelectorComponent.isVisible);
    }

    [Test]
    public void RaiseOnCloseActionTriggeredCorrectly()
    {
        // Arrange
        realmSelectorComponent.Show();

        // Act
        realmSelectorComponent.OnCloseActionTriggered(new DCLAction_Trigger());

        // Assert
        Assert.IsFalse(realmSelectorComponent.isVisible);
    }

    [Test]
    public void ConfigureEventCardsPoolCorrectly()
    {
        // Arrange
        realmSelectorComponent.realmsPool = null;

        // Act
        realmSelectorComponent.ConfigureRealmsPool();

        // Assert
        Assert.IsNotNull(realmSelectorComponent.realmsPool);
        Assert.AreEqual(RealmSelectorComponentView.REALMS_POOL_NAME, realmSelectorComponent.realmsPool.id);
    }
}
