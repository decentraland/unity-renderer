using AvatarSystem;
using DCL.ProfanityFiltering;
using DCL.Social.Friends;
using NSubstitute;
using NUnit.Framework;
using SocialFeaturesAnalytics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Social.Passports
{
    public class PassportInfoHUDControllerShould
    {
        private PassportPlayerInfoComponentController playerInfoController;
        private StringVariable currentPlayerInfoCardId;
        private IUserProfileBridge userProfileBridge;
        private ISocialAnalytics socialAnalytics;
        private DataStore dataStore;
        private IProfanityFilter profanityFilter;
        private IPassportApiBridge passportApiBridge;
        private IFriendsController friendsController;
        private IPassportPlayerInfoComponentView playerInfoView;

        [SetUp]
        public void Setup()
        {
            currentPlayerInfoCardId = ScriptableObject.CreateInstance<StringVariable>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            socialAnalytics = Substitute.For<ISocialAnalytics>();
            dataStore = Substitute.For<DataStore>();
            profanityFilter = Substitute.For<IProfanityFilter>();
            passportApiBridge = Substitute.For<IPassportApiBridge>();
            friendsController = Substitute.For<IFriendsController>();
            playerInfoView = Substitute.For<IPassportPlayerInfoComponentView>();

            playerInfoController = new PassportPlayerInfoComponentController(
                currentPlayerInfoCardId,
                playerInfoView,
                dataStore,
                profanityFilter,
                friendsController,
                userProfileBridge,
                socialAnalytics,
                Substitute.For<IClipboard>(),
                passportApiBridge);
        }

        [TearDown]
        public void TearDown()
        {
            playerInfoController.Dispose();
        }


        [Test]
        public void OpenGuestPanelWhenAddingFriendAsGuest()
        {
            var ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();
            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = "ownId",
                hasConnectedWeb3 = false
            });
            var connectWalletModalVisibilityWasCalled = false;
            dataStore.HUDs.connectWalletModalVisible.OnChange += (current, previous) => connectWalletModalVisibilityWasCalled = true;

            userProfileBridge.GetOwn().Returns(ownUserProfile);
            playerInfoView.OnAddFriend += Raise.Event<Action>();

            Assert.IsTrue(connectWalletModalVisibilityWasCalled);
        }
    }
}
