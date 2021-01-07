using NUnit.Framework;
using UnityEngine.TestTools;
using DCL.HelpAndSupportHUD;
using System.Collections;

namespace Tests
{
    public class HelpAndSupportHUDControllerShould : IntegrationTestSuite_Legacy
    {
        private HelpAndSupportHUDController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            controller = new HelpAndSupportHUDController();
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void CreateViewProperly()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
        }

        [Test]
        public void ShowViewProperly()
        {
            controller.view.gameObject.SetActive(false);
            controller.SetVisibility(true);
            Assert.IsTrue(controller.view.isOpen);
        }

        [Test]
        public void HideViewProperly()
        {
            controller.view.gameObject.SetActive(true);
            controller.SetVisibility(false);
            Assert.IsFalse(controller.view.isOpen);
        }
    }
}