using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class PlayerInfoCardHUDControllerShould : IntegrationTestSuite_Legacy
{
    private const string USER_ID = "userId";
    private PlayerInfoCardHUDController controller;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel {userId = USER_ID});
        var userProfile = UserProfileController.userProfilesCatalog.Get(USER_ID);
        userProfile.UpdateData(new UserProfileModel
        {
            userId = USER_ID,
            name = "username",
            description = "description",
            email = "email",
            inventory = new string[] { }
        });
        Resources.Load<StringVariable>(PlayerInfoCardHUDController.CURRENT_PLAYER_ID)
            .Set(USER_ID);

        controller = new PlayerInfoCardHUDController();
    }

    protected override IEnumerator TearDown()
    {
        controller.Dispose();
        yield return base.TearDown();
    }

    [Test]
    public void CreateTheView()
    {
        Assert.IsNotNull(controller.view);
        Assert.IsNotNull(controller.view.gameObject);
    }

    [Test]
    public void CurrentPlayerNameIsFound()
    {
        var currentPlayerName = Resources.Load<StringVariable>(PlayerInfoCardHUDController.CURRENT_PLAYER_ID);
        Assert.IsNotNull(controller.currentPlayerId);
        Assert.AreEqual(currentPlayerName, controller.currentPlayerId);
    }

    [Test]
    public void ReactToCurrentPlayerNameChanges()
    {
        UserProfile userProfile;
        using (var iterator = UserProfileController.userProfilesCatalog.GetEnumerator())
        {
            iterator.MoveNext();
            userProfile = iterator.Current.Value;
        }

        controller.currentPlayerId.Set(userProfile.userId);
        Assert.AreEqual(controller.currentUserProfile, userProfile);
    }
}