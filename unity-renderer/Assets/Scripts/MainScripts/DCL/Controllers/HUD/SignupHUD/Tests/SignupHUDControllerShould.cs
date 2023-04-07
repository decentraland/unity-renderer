using DCL;
using NSubstitute;
using NUnit.Framework;
using SignupHUD;
using System;

namespace Tests.SignupHUD
{
    public class SignupHUDControllerShould
    {
        private SignupHUDController hudController;
        private ISignupHUDView hudView;
        private DataStore_HUDs dataStoreHUDs;
        private BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<ISignupHUDView>();
            dataStoreHUDs = new DataStore_HUDs();

            hudController = new SignupHUDController(Substitute.For<IAnalytics>(),
                hudView,
                new DataStore_LoadingScreen(),
                dataStoreHUDs);
            hudController.Initialize();
        }

        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView, hudController.view);
            Assert.IsFalse(signupVisible.Get());
        }

        [Test]
        public void ReactToSignupVisibleTrue()
        {
            hudController.name = "this_will_be_null";
            hudController.email = "this_will_be_null";

            signupVisible.Set(true, true); //Force event notification

            hudView.Received().SetVisibility(true);
            Assert.IsNull(hudController.name);
            Assert.IsNull(hudController.email);
            hudView.Received().ShowNameScreen();
        }

        [Test]
        public void ReactToSignupVisibleFalse()
        {
            signupVisible.Set(false, true); //Force event notification
            hudView.Received().SetVisibility(false);
        }

        [Test]
        public void StartSignupProcessProperly()
        {
            hudController.name = "this_will_be_null";
            hudController.email = "this_will_be_null";

            hudController.StartSignupProcess();

            Assert.IsNull(hudController.name);
            Assert.IsNull(hudController.email);
            hudView.Received().ShowNameScreen();
        }

        [Test]
        public void ReactsToNameScreenNextProperly()
        {
            hudView.OnNameScreenNext += Raise.Event<ISignupHUDView.NameScreenDone>("new_name", "new_email");
            Assert.AreEqual(hudController.name, "new_name");
            Assert.AreEqual(hudController.email, "new_email");
            hudView.Received().ShowTermsOfServiceScreen();
        }

        [Test]
        public void ReactsToEditAvatarProperly()
        {
            var isAvatarEditorVisible = false;
            dataStoreHUDs.avatarEditorVisible.OnChange += (current, previous) => isAvatarEditorVisible = current;

            hudView.OnEditAvatar += Raise.Event<Action>();
            Assert.IsFalse(signupVisible.Get());
            Assert.IsTrue(isAvatarEditorVisible);
        }

        [Test]
        public void ReactsToTermsOfServiceAgreed()
        {
            hudView.OnTermsOfServiceAgreed += Raise.Event<Action>();
            //TODO assert webinterface interaction
            Assert.IsFalse(signupVisible.Get());
            Assert.IsFalse(DataStore.i.common.isSignUpFlow.Get());
        }

        [Test]
        public void ReactsToTermsOfServiceBack()
        {
            hudController.name = "this_will_be_null";
            hudController.email = "this_will_be_null";

            hudView.OnTermsOfServiceBack += Raise.Event<Action>();

            Assert.IsNull(hudController.name);
            Assert.IsNull(hudController.email);
            hudView.Received().ShowNameScreen();
        }

        [TearDown]
        public void TearDown() { DataStore.Clear(); }
    }
}
