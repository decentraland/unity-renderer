using System;
using DCL;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using SignupHUD;

namespace Tests.SignupHUD
{
    public class SignupHUDControllerShould
    {
        private SignupHUDController hudController;
        private ISignupHUDView hudView;
        private IHUD avatarEditorHUD;
        private BaseVariable<bool> signupVisible => DataStore.i.HUDs.signupVisible;

        [SetUp]
        public void SetUp()
        {
            hudView = Substitute.For<ISignupHUDView>();
            avatarEditorHUD = Substitute.For<IHUD>();
            hudController = Substitute.ForPartsOf<SignupHUDController>();
            hudController.Configure().CreateView().Returns(info => hudView);
            hudController.Initialize(avatarEditorHUD);
        }

        [Test]
        public void InitializeProperly()
        {
            Assert.AreEqual(hudView, hudController.view);
            Assert.AreEqual(avatarEditorHUD, hudController.avatarEditorHUD);
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
            hudView.OnEditAvatar += Raise.Event<Action>();
            Assert.IsFalse(signupVisible.Get());
            avatarEditorHUD.Received().SetVisibility(true);
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