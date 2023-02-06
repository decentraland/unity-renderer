using DCL.Providers;
using NUnit.Framework;
using SignupHUD;
using System.Threading.Tasks;
using UnityEngine;

namespace Tests.SignupHUD
{
    public class SignupHUDViewShould
    {
        private SignupHUDView hudView;
        private HUDFactory factory;

        [SetUp]
        public async Task SetUp()
        {
            factory = new HUDFactory(new AddressableResourceProvider());
            hudView = (SignupHUDView)await factory.CreateSignupHUDView();
        }

        [TearDown]
        public void TearDown()
        {
            factory.Dispose();

            if (hudView != null)
                Object.Destroy(hudView.gameObject);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task SetVisibilityProperly(bool visibility)
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.SetVisibility(visibility);
            Assert.AreEqual(visibility, hudView.gameObject.activeSelf);
        }

        [Test]
        public async Task ShowNameScreenProperly()
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.nameAndEmailPanel.gameObject.SetActive(false);
            hudView.termsOfServicePanel.gameObject.SetActive(true);

            hudView.ShowNameScreen();

            Assert.AreEqual(true, hudView.nameAndEmailPanel.gameObject.activeSelf);
            Assert.AreEqual(false, hudView.termsOfServicePanel.gameObject.activeSelf);
        }

        [Test]
        public async Task ShowTermsOfServiceScreenProperly()
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.nameAndEmailPanel.gameObject.SetActive(true);
            hudView.termsOfServicePanel.gameObject.SetActive(false);

            hudView.ShowTermsOfServiceScreen();

            Assert.AreEqual(false, hudView.nameAndEmailPanel.gameObject.activeSelf);
            Assert.AreEqual(true, hudView.termsOfServicePanel.gameObject.activeSelf);
        }

        [Test]
        public async Task DisableNextButtonWithShortName()
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.nameAndEmailNextButton.interactable = true;
            hudView.nameInputField.text = "";
            hudView.emailInputField.text = "";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsFalse(hudView.nameAndEmailNextButton.interactable);
        }

        [Test]
        public async Task EnableNextButtonWithValidName()
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.nameAndEmailNextButton.interactable = false;
            hudView.nameInputField.text = "ValidName";
            hudView.emailInputField.text = "";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsTrue(hudView.nameAndEmailNextButton.interactable);
        }

        [Test]
        public async Task DisableNextButtonWithInvalidEmail()
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.nameAndEmailNextButton.interactable = true;
            hudView.nameInputField.text = "ValidName";
            hudView.emailInputField.text = "this_is_not_an_email";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsFalse(hudView.nameAndEmailNextButton.interactable);
        }

        [Test]
        public async Task EnableNextButtonWithValidEmail()
        {
            if(hudView == null)
                hudView = (SignupHUDView)await factory.CreateSignupHUDView();

            hudView.nameAndEmailNextButton.interactable = true;
            hudView.nameInputField.text = "ValidName";
            hudView.emailInputField.text = "myvalid@email.com";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsTrue(hudView.nameAndEmailNextButton.interactable);
        }
    }
}
