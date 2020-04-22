using DCL.Helpers;
using NUnit.Framework;
using System.Collections;

namespace Tests
{
    public class WelcomeHUDShould : TestsBase
    {
        WelcomeHUDController controller;
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new WelcomeHUDController();
            controller.Initialize(hasWallet: true);
        }

        [Test]
        [Explicit("Feature disabled in prod")]
        [Category("Explicit")]
        public void BeCreatedProperly()
        {
            Assert.IsTrue(controller.view != null);
        }


        [Test]
        [Explicit("Feature disabled in prod")]
        [Category("Explicit")]
        public void BehaveCorrectlyAfterCloseButtonIsPressed()
        {
            Assert.IsFalse(Utils.isCursorLocked);

            controller.view.closeButton.onClick.Invoke();

            Assert.IsTrue(Utils.isCursorLocked);
            Assert.IsFalse(controller.view.gameObject.activeSelf);
        }

        [Test]
        [Explicit("Feature disabled in prod")]
        [Category("Explicit")]
        public void BehaveCorrectlyAfterConfirmButtonIsPressed()
        {
            Assert.IsFalse(Utils.isCursorLocked);

            controller.view.confirmButton.onClick.Invoke();

            Assert.IsTrue(Utils.isCursorLocked);
            Assert.IsFalse(controller.view.gameObject.activeSelf);
        }

    }
}
