using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class PlayerInfoCardHUDControllerShould : TestsBase
{
    private PlayerInfoCardHUDController controller;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        controller = new PlayerInfoCardHUDController();
        UserProfileController.i.AddUserProfileToCatalog(new UserProfileModel()
        {
            userId = "userId",
            name = "username",
            description = "description",
            email = "email",
            inventory = new string[] { }
        });
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
