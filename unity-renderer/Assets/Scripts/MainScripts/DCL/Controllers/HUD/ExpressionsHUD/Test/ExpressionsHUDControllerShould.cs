using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ExpressionsHUD_Test
{
    public class ExpressionsHUDControllerShould : IntegrationTestSuite_Legacy
    {
        private ExpressionsHUDController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new ExpressionsHUDController();
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
        }

        [Test]
        public void UpdateOwnUserProfileWhenExpressionIsCalled()
        {
            controller.ExpressionCalled("wave");

            Assert.AreEqual("wave", UserProfile.GetOwnUserProfile().avatar.expressionTriggerId);
        }
    }

    public class ExpressionsHUDViewShould : IntegrationTestSuite_Legacy
    {
        private ExpressionsHUDController controller;
        private ExpressionsHUDView view;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new ExpressionsHUDController();
            view = controller.view;
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void BeInitializedProperly()
        {
            view.content.gameObject.SetActive(true);
            view.Initialize(null);
            Assert.IsFalse(view.content.gameObject.activeSelf);
        }

        [Test]
        public void RegisterButtonsCallbackProperly()
        {
            string expressionCalled = null;
            ExpressionsHUDView.ExpressionClicked callback = (x) => expressionCalled = x;
            view.Initialize(callback);

            view.buttonToExpressionMap[0].button.OnPointerDown(null);

            Assert.AreEqual(view.buttonToExpressionMap[0].expressionId, expressionCalled);
        }

        [Test]
        public void ToggleContentProperly()
        {
            var currentActive = view.content.gameObject.activeSelf;
            view.ToggleContent();
            Assert.AreNotEqual(currentActive, view.content.gameObject.activeSelf);

            currentActive = view.content.gameObject.activeSelf;
            view.ToggleContent();
            Assert.AreNotEqual(currentActive, view.content.gameObject.activeSelf);

            currentActive = view.content.gameObject.activeSelf;
            view.ToggleContent();
            Assert.AreNotEqual(currentActive, view.content.gameObject.activeSelf);
        }

        [Test]
        public void ReactToOpenExpressionsInputAction()
        {
            var inputAction = view.openExpressionsAction;

            var currentActive = view.content.gameObject.activeSelf;
            inputAction.RaiseOnTriggered();
            Assert.AreNotEqual(currentActive, view.content.gameObject.activeSelf);

            currentActive = view.content.gameObject.activeSelf;
            inputAction.RaiseOnTriggered();
            Assert.AreNotEqual(currentActive, view.content.gameObject.activeSelf);
        }
    }
}