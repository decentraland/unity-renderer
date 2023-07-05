using DCL.Browser;
using NSubstitute;
using NUnit.Framework;
using System;
using UnityEngine;

namespace DCL.MyAccount
{
    public class MyAccountCardControllerShould
    {
        private const string OWN_USER_ID = "ownUserId";

        private MyAccountCardController controller;
        private IMyAccountCardComponentView view;
        private DataStore dataStore;
        private IBrowserBridge browserBridge;
        private IUserProfileBridge userProfileBridge;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IMyAccountCardComponentView>();
            dataStore = new DataStore();

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            UserProfile userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
            });

            userProfileBridge.GetOwn().Returns(userProfile);

            browserBridge = Substitute.For<IBrowserBridge>();

            controller = new MyAccountCardController(view,
                dataStore,
                userProfileBridge,
                null,
                browserBridge);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void OpenOwnProfile()
        {
            view.OnPreviewProfileClicked += Raise.Event<Action>();

            Assert.AreEqual(OWN_USER_ID, dataStore.HUDs.currentPlayerId.Get().playerId);
            Assert.AreEqual("ProfileHUD", dataStore.HUDs.currentPlayerId.Get().source);
            Assert.IsFalse(dataStore.exploreV2.profileCardIsOpen.Get());
            view.Received(1).Hide();
        }

        [Test]
        public void OpenAccountSettings()
        {
            view.OnAccountSettingsClicked += Raise.Event<Action>();

            Assert.AreEqual(true, dataStore.myAccount.myAccountSectionOpenFromProfileHUD.Get());
        }

        [Test]
        public void SignOut()
        {
            view.OnSignOutClicked += Raise.Event<Action>();

            userProfileBridge.Received(1).LogOut();
        }

        [Test]
        public void OpenTermsOfService()
        {
            view.OnTermsOfServiceClicked += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("https://decentraland.org/terms");
        }

        [Test]
        public void OpenPrivacyPolicy()
        {
            view.OnPrivacyPolicyClicked += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("https://decentraland.org/privacy");
        }
    }
}
