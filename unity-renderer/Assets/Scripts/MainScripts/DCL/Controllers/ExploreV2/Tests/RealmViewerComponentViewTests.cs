using NUnit.Framework;
using UnityEngine;

public class RealmViewerComponentViewTests
{
    private RealmViewerComponentView realmViewerComponent;

    [SetUp]
    public void SetUp() { realmViewerComponent = BaseComponentView.Create<RealmViewerComponentView>("MainMenu/Realms/RealmViewer"); }

    [TearDown]
    public void TearDown()
    {
        realmViewerComponent.Dispose();
        GameObject.Destroy(realmViewerComponent.gameObject);
    }

    [Test]
    public void ConfigureRealmViewerCorrectly()
    {
        // Arrange
        RealmViewerComponentModel testModel = new RealmViewerComponentModel
        {
            realmName = "Test Realm Name",
            numberOfUsers = 50000
        };

        // Act
        realmViewerComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, realmViewerComponent.model, "The model does not match after configuring the button.");
    }

    [Test]
    public void SetRealmCorrectly()
    {
        // Arrange
        string testRealmName = "Test Realm Name";

        // Act
        realmViewerComponent.SetRealm(testRealmName);

        // Assert
        Assert.AreEqual(testRealmName, realmViewerComponent.model.realmName, "The realm name does not match in the model.");
        Assert.AreEqual(testRealmName, realmViewerComponent.realm.text, "The realm text does not match.");
    }

    [Test]
    [TestCase(50)]
    [TestCase(1000)]
    public void SetNumberOfUsersCorrectly(int numberOfUsers)
    {
        // Act
        realmViewerComponent.SetNumberOfUsers(numberOfUsers);

        // Assert
        Assert.AreEqual(numberOfUsers, realmViewerComponent.model.numberOfUsers, "The number of users does not match in the model.");

        if (numberOfUsers >= 1000)
        {
            float formattedUsersCount = numberOfUsers / 1000f;
            Assert.AreEqual($"{string.Format("{0:0.##}", formattedUsersCount)}k", realmViewerComponent.numberOfusers.text, "The number of users text does not match.");
        }
        else
        {
            Assert.AreEqual($"{numberOfUsers}", realmViewerComponent.numberOfusers.text, "The number of users text does not match.");
        }
    }
}