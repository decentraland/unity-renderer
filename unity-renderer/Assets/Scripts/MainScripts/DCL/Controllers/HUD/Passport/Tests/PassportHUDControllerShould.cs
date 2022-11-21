using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSubstitute;
using NUnit.Framework;
using DCL.Social.Passports;
using DCL;
using SocialFeaturesAnalytics;

public class PassportHUDControllerShould
{
    private PlayerPassportHUDController controller;
    private PlayerPassportHUDView view;
    private PassportPlayerInfoComponentController playerInfoController;
    private PassportPlayerPreviewComponentController playerPreviewController;
    private PassportNavigationComponentController passportNavigationController;
    private StringVariable currentPlayerInfoCardId;
    private IUserProfileBridge userProfileBridge;
    private ISocialAnalytics socialAnalytics;
    private DataStore dataStore;
    private IProfanityFilter profanityFilter;
    private IFriendsController friendsController;

    [SetUp]
    public void SetUp()
    {
        view = PlayerPassportHUDView.CreateView();
        
        currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
        userProfileBridge = Substitute.For<IUserProfileBridge>();
        socialAnalytics = Substitute.For<ISocialAnalytics>();
        dataStore = Substitute.For<DataStore>();
        profanityFilter = Substitute.For<IProfanityFilter>();
        friendsController = Substitute.For<IFriendsController>();
        playerInfoController = new PassportPlayerInfoComponentController(
                            currentPlayerInfoCardId, 
                            view.PlayerInfoView, 
                            dataStore, 
                            profanityFilter, 
                            friendsController, 
                            userProfileBridge);

        playerPreviewController = new PassportPlayerPreviewComponentController(view.PlayerPreviewView);
        passportNavigationController = new PassportNavigationComponentController(
                            view.PassportNavigationView,
                            profanityFilter,
                            dataStore);

        controller = new PlayerPassportHUDController(
            view,
            playerInfoController,
            playerPreviewController,
            passportNavigationController,
            currentPlayerInfoCardId,
            userProfileBridge,
            socialAnalytics
        );
    }

    [TearDown]    
    public void TearDown()
    {
        controller.Dispose();
    }

    [Test]
    public void SetVisibilityTrueCorrectly()
    {
        controller.SetVisibility(true);

        Assert.IsTrue(view.gameObject.activeSelf);
    }

    [Test]
    public void SetVisibilityFalseCorrectly()
    {
        controller.SetVisibility(false);

        Assert.IsFalse(view.gameObject.activeSelf);
    }
}
