using DCl.Social.Friends;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportHUDControllerShould
    {
        private PlayerPassportHUDController controller;
        private IPlayerPassportHUDView view;
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
            Environment.Setup(ServiceLocatorTestFactory.CreateMocked());

            view = Substitute.For<IPlayerPassportHUDView>();

            currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            dataStore = Substitute.For<DataStore>();
            profanityFilter = Substitute.For<IProfanityFilter>();
            friendsController = Substitute.For<IFriendsController>();
            playerInfoController = new PassportPlayerInfoComponentController(
                                currentPlayerInfoCardId,
                                Substitute.For<IPassportPlayerInfoComponentView>(),
                                dataStore,
                                profanityFilter,
                                friendsController,
                                userProfileBridge,
                                socialAnalytics);

            var playerPreviewView = Substitute.For<IPassportPlayerPreviewComponentView>();
            playerPreviewView.PreviewCameraRotation.Returns(new GameObject().AddComponent<PreviewCameraRotation>());

            playerPreviewController = new PassportPlayerPreviewComponentController(playerPreviewView);
            passportNavigationController = new PassportNavigationComponentController(
                                Substitute.For<IPassportNavigationComponentView>(),
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

            view.Received(1).SetVisibility(true);
        }

        [Test]
        public void SetVisibilityFalseCorrectly()
        {
            controller.SetVisibility(false);

            view.Received(1).SetVisibility(false);
        }
    }
    }
