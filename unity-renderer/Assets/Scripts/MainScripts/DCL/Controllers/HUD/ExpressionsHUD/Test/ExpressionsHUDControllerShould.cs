using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace ExpressionsHUD_Test
{
    public class ExpressionsHUDControllerShould : TestsBase
    {
        private ExpressionsHUDController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new ExpressionsHUDController();
        }

        [Test]
        [Explicit("Expressions are disabled")]
        public void CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
        }

        [Test]
        [Explicit("Expressions are disabled")]
        public void UpdateOwnUserProfileWhenExpressionIsCalled()
        {
            controller.ExpressionCalled("wave");

            Assert.AreEqual("wave", UserProfile.GetOwnUserProfile().avatar.expressionTriggerId);
        }
    }

    public class ExpressionsHUDViewShould : TestsBase
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

        [Test]
        [Explicit("Expressions are disabled")]
        public void BeInitializedProperly()
        {
            view.content.gameObject.SetActive(true);
            view.Initialize(null);
            Assert.IsFalse(view.content.gameObject.activeSelf);
        }

        [Test]
        [Explicit("Expressions are disabled")]
        public void RegisterButtonsCallbackProperly()
        {
            string expressionCalled = null;
            ExpressionsHUDView.ExpressionClicked callback = (x) => expressionCalled = x;
            view.Initialize(callback);

            view.buttonToExpressionMap[0].button.OnPointerDown(null);

            Assert.AreEqual(view.buttonToExpressionMap[0].expressionId, expressionCalled);
        }

        [Test]
        [Explicit("Expressions are disabled")]
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
        [Explicit("Expressions are disabled")]
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
