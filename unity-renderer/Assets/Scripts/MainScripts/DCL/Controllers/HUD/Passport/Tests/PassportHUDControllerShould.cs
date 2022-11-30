using AvatarSystem;
using DCl.Social.Friends;
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
        private IFriendsController friendsController;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IPlayerPassportHUDView>();

            currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            wearableItemResolver = Substitute.For<IWearableItemResolver>();
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

            playerPreviewController = new PassportPlayerPreviewComponentController(Substitute.For<IPassportPlayerPreviewComponentView>());
            passportNavigationController = new PassportNavigationComponentController(
                                Substitute.For<IPassportNavigationComponentView>(),
                                profanityFilter,
                                wearableItemResolver,
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
