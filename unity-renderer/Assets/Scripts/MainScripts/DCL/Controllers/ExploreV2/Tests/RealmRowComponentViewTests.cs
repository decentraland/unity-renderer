using NUnit.Framework;
using System.Linq;
using UnityEngine;

public class RealmRowComponentViewTests
{
    private RealmRowComponentView realmRowComponent;

    [SetUp]
    public void SetUp()
    {
        realmRowComponent = BaseComponentView.Create<RealmRowComponentView>("MainMenu/Realms/RealmRow");
    }

    [TearDown]
    public void TearDown()
    {
        realmRowComponent.Dispose();
    }

    [Test]
    public void ConfigureRealmRowCorrectly()
    {
        // Arrange
        RealmRowComponentModel testModel = new RealmRowComponentModel
        {
            backgroundColor = Color.black,
            isConnected = false,
            name = "Test Name",
            players = 10
        };

        // Act
        realmRowComponent.Configure(testModel);

        // Assert
        Assert.AreEqual(testModel, realmRowComponent.model, "The model does not match after configuring the realm row.");
    }

    [Test]
    [TestCase("Test Name")]
    [TestCase(null)]
    public void SetNameCorrectly(string realmName)
    {
        // Act
        realmRowComponent.SetName(realmName);

        // Assert
        string nameResult = string.IsNullOrEmpty(realmName) ? string.Empty : realmName;
        Assert.AreEqual(nameResult, realmRowComponent.model.name, "The realm name does not match in the model.");
        Assert.AreEqual(nameResult.ToUpper(), realmRowComponent.nameText.text.ToUpper());
    }

    [Test]
    public void SetNumberOfPlayersCorrectly()
    {
        // Arrange
        int testNumPlayers = 10;

        // Act
        realmRowComponent.SetNumberOfPlayers(testNumPlayers);

        // Assert
        Assert.AreEqual(testNumPlayers, realmRowComponent.model.players, "The realm number of players does not match in the model.");

        float formattedPlayersCount = testNumPlayers >= 1000 ? (testNumPlayers / 1000f) : testNumPlayers;
        Assert.AreEqual(testNumPlayers >= 1000 ? $"{string.Format("{0:0.##}", formattedPlayersCount)}k" : $"{formattedPlayersCount}", realmRowComponent.playersText.text.ToLower());
    }

    [Test]
    public void SetAsConnectedCorrectly()
    {
        // Arrange
        bool isConnected = true;

        // Act
        realmRowComponent.SetAsConnected(isConnected);

        // Assert
        Assert.AreEqual(isConnected, realmRowComponent.model.isConnected, "The realm isConnected param does not match in the model.");
        Assert.AreEqual(isConnected, realmRowComponent.connectedMark.activeSelf);
        Assert.AreEqual(!isConnected, realmRowComponent.warpInButton.gameObject.activeSelf);
    }

    [Test]
    public void SetRowColorCorrectly()
    {
        // Arrange
        Color testColor = Color.black;

        // Act
        realmRowComponent.SetRowColor(testColor);

        // Assert
        Assert.AreEqual(testColor, realmRowComponent.model.backgroundColor, "The realm background color does not match in the model.");
        Assert.AreEqual(testColor, realmRowComponent.backgroundImage.color);
        Assert.AreEqual(testColor, realmRowComponent.originalBackgroundColor);
    }

    [Test]
    public void SetOnHoverColorCorrectly()
    {
        // Arrange
        Color testColor = Color.black;

        // Act
        realmRowComponent.SetOnHoverColor(testColor);

        // Assert
        Assert.AreEqual(testColor, realmRowComponent.model.onHoverColor, "The realm on hover color does not match in the model.");
        Assert.AreEqual(testColor, realmRowComponent.onHoverColor);
    }

    [Test]
    public void InitializeFriendsTrackerCorrectly()
    {
        // Arrange
        realmRowComponent.realmInfoHandler = null;
        realmRowComponent.friendsHandler = null;

        // Act
        realmRowComponent.InitializeFriendsTracker();

        // Assert
        Assert.IsNotNull(realmRowComponent.realmInfoHandler, "The realm info handler shouldn't be null");
        Assert.IsNotNull(realmRowComponent.friendsHandler, "The friends handler shouldn't be null");
    }

    [Test]
    public void RaiseOnFriendAddedCorrectly()
    {
        // Arrange
        string testUserId = "123456";
        UserProfile testProfile = new UserProfile();
        testProfile.UpdateData(new UserProfileModel { userId = testUserId });
        Color testColor = Color.green;
        realmRowComponent.currentFriendHeads.Clear();

        // Act
        realmRowComponent.OnFriendAdded(testProfile, testColor);

        // Assert
        Assert.IsTrue(realmRowComponent.friendsGrid.instantiatedItems.Any(x => (x as FriendHeadForPlaceCardComponentView).model.userProfile.userId == testUserId));
        Assert.IsTrue(realmRowComponent.currentFriendHeads.ContainsKey(testUserId));
    }

    [Test]
    public void RaiseOnFriendRemovedCorrectly()
    {
        // Arrange
        string testUserId = "123456";
        UserProfile testProfile = new UserProfile();
        testProfile.UpdateData(new UserProfileModel { userId = testUserId });
        Color testColor = Color.green;
        realmRowComponent.currentFriendHeads.Clear();
        realmRowComponent.OnFriendAdded(testProfile, testColor);

        // Act
        realmRowComponent.OnFriendRemoved(testProfile);

        // Assert
        Assert.IsFalse(realmRowComponent.friendsGrid.instantiatedItems.Any(x => (x as FriendHeadForPlaceCardComponentView).model.userProfile.userId == testUserId));
        Assert.IsFalse(realmRowComponent.currentFriendHeads.ContainsKey(testUserId));
    }

    [Test]
    public void CleanFriendHeadsItemsCorrectly()
    {
        // Arrange
        RaiseOnFriendAddedCorrectly();

        // Act
        realmRowComponent.CleanFriendHeadsItems();

        // Assert
        Assert.AreEqual(0, realmRowComponent.friendsGrid.instantiatedItems.Count, "The friends grid should be empty.");
        Assert.AreEqual(0, realmRowComponent.currentFriendHeads.Count, "The currentFriendHeads list should be empty.");
    }

    [Test]
    public void InstantiateAndConfigureFriendHeadCorrectly()
    {
        // Arrange
        FriendHeadForPlaceCardComponentModel testFriendInfo = new FriendHeadForPlaceCardComponentModel
        {
            userProfile = new UserProfile(),
            backgroundColor = Color.green
        };

        // Act
        BaseComponentView newFriendHead = realmRowComponent.InstantiateAndConfigureFriendHead(testFriendInfo, realmRowComponent.friendHeadPrefab);

        // Assert
        Assert.IsTrue(newFriendHead is FriendHeadForPlaceCardComponentView);
        Assert.AreEqual(testFriendInfo, ((FriendHeadForPlaceCardComponentView)newFriendHead).model, "The new friend head model does not match.");

        realmRowComponent.friendsGrid.Dispose();
    }
}
