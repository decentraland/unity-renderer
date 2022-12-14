using AvatarSystem;
using DCL.Social.Friends;
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
        private IWearableItemResolver wearableItemResolver;
        private DataStore dataStore;
        private IProfanityFilter profanityFilter;
        private IPassportApiBridge passportApiBridge;
        private IFriendsController friendsController;

        [SetUp]
        public void SetUp()
        {
            Environment.Setup(ServiceLocatorTestFactory.CreateMocked());

            view = Substitute.For<IPlayerPassportHUDView>();

            currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            wearableItemResolver = Substitute.For<IWearableItemResolver>();
            dataStore = Substitute.For<DataStore>();
            profanityFilter = Substitute.For<IProfanityFilter>();
            passportApiBridge = Substitute.For<IPassportApiBridge>();
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
                                wearableItemResolver,
                                Substitute.For<IWearableCatalogBridge>(),
                                Substitute.For<IEmotesCatalogService>(),
                                dataStore);

            controller = new PlayerPassportHUDController(
                view,
                playerInfoController,
                playerPreviewController,
                passportNavigationController,
                currentPlayerInfoCardId,
                userProfileBridge,
                passportApiBridge,
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
