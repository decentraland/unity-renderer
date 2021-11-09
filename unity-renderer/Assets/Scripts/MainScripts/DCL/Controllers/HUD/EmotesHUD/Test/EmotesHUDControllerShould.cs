using System;
using System.Collections;
using DCL;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ExpressionsHUD_Test
{
    public class EmotesHUDControllerShould : IntegrationTestSuite_Legacy
    {
        private EmotesHUDController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new EmotesHUDController();
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
            controller.EmoteCalled("wave");

            Assert.AreEqual("wave", UserProfile.GetOwnUserProfile().avatar.expressionTriggerId);
        }
    }

    public class ExpressionsHUDViewShould : IntegrationTestSuite_Legacy
    {
        private EmotesHUDController controller;
        private EmotesHUDView view;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new EmotesHUDController();
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
            EmotesHUDView.ExpressionClicked callback = (x) => expressionCalled = x;
            view.Initialize(callback);

            view.buttonToEmotesMap[0].button.OnPointerDown(null);

            Assert.AreEqual(view.buttonToEmotesMap[0].expressionId, expressionCalled);
        }

        [Test]
        public void ReactToExpressionsVisibleChange()
        {
            Assert.True(!view.gameObject.activeSelf);
            DataStore.i.HUDs.emotesVisible.Set(true);
            Assert.True(view.gameObject.activeSelf);
            DataStore.i.HUDs.emotesVisible.Set(false);
            Assert.False(view.gameObject.activeSelf);
        }
    }
}