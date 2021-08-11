using NUnit.Framework;
using SignupHUD;
using UnityEngine;

namespace Tests.SignupHUD
{
    public class SignupHUDViewShould
    {
        private SignupHUDView hudView;

        [SetUp]
        public void SetUp() { hudView = Object.Instantiate(Resources.Load<GameObject>("SignupHUD")).GetComponent<SignupHUDView>(); }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void SetVisibilityProperly(bool visibility)
        {
            hudView.SetVisibility(visibility);
            Assert.AreEqual(visibility, hudView.gameObject.activeSelf);
        }

        [Test]
        public void ShowNameScreenProperly()
        {
            hudView.nameAndEmailPanel.gameObject.SetActive(false);
            hudView.termsOfServicePanel.gameObject.SetActive(true);

            hudView.ShowNameScreen();

            Assert.AreEqual(true, hudView.nameAndEmailPanel.gameObject.activeSelf);
            Assert.AreEqual(false, hudView.termsOfServicePanel.gameObject.activeSelf);
        }

        [Test]
        public void ShowTermsOfServiceScreenProperly()
        {
            hudView.nameAndEmailPanel.gameObject.SetActive(true);
            hudView.termsOfServicePanel.gameObject.SetActive(false);

            hudView.ShowTermsOfServiceScreen();

            Assert.AreEqual(false, hudView.nameAndEmailPanel.gameObject.activeSelf);
            Assert.AreEqual(true, hudView.termsOfServicePanel.gameObject.activeSelf);
        }

        [Test]
        public void DisableNextButtonWithShortName()
        {
            hudView.nameAndEmailNextButton.interactable = true;
            hudView.nameInputField.text = "";
            hudView.emailInputField.text = "";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsFalse(hudView.nameAndEmailNextButton.interactable);
        }

        [Test]
        public void EnableNextButtonWithValidName()
        {
            hudView.nameAndEmailNextButton.interactable = false;
            hudView.nameInputField.text = "ValidName";
            hudView.emailInputField.text = "";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsTrue(hudView.nameAndEmailNextButton.interactable);
        }

        [Test]
        public void DisableNextButtonWithInvalidEmail()
        {
            hudView.nameAndEmailNextButton.interactable = true;
            hudView.nameInputField.text = "ValidName";
            hudView.emailInputField.text = "this_is_not_an_email";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsFalse(hudView.nameAndEmailNextButton.interactable);
        }

        [Test]
        public void EnableNextButtonWithValidEmail()
        {
            hudView.nameAndEmailNextButton.interactable = true;
            hudView.nameInputField.text = "ValidName";
            hudView.emailInputField.text = "myvalid@email.com";

            hudView.UpdateNameAndEmailNextButton();

            Assert.IsTrue(hudView.nameAndEmailNextButton.interactable);
        }

        [TearDown]
        public void TearDown() { Object.Destroy(hudView.gameObject); }
    }
}