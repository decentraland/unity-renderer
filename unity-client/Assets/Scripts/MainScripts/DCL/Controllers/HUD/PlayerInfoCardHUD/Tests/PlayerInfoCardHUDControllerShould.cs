using System.Collections;
using NUnit.Framework;
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
            name = "username",
            description = "description",
            email = "email",
            inventory = new string [] { }
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
        var currentPlayerName = Resources.Load<StringVariable>(PlayerInfoCardHUDController.CURRENT_PLAYER_NAME);
        Assert.IsNotNull(controller.currentPlayerName);
        Assert.AreEqual(currentPlayerName, controller.currentPlayerName);
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

        controller.currentPlayerName.Set(userProfile.userName);
        Assert.AreEqual(controller.currentUserProfile, userProfile);
    }
}