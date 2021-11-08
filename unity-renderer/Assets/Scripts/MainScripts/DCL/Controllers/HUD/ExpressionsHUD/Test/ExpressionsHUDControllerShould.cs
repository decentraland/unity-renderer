using System.Collections;
using DCL;
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
            DataStore.Clear();
            controller.Dispose();
            yield return base.TearDown();
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
        public void ReactToExpressionsVisibleChange()
        {
            Assert.True(!view.gameObject.activeSelf);
            DataStore.i.HUDs.expressionsVisible.Set(true);
            Assert.True(view.gameObject.activeSelf);
            DataStore.i.HUDs.expressionsVisible.Set(false);
            Assert.False(view.gameObject.activeSelf);
        }
    }
}