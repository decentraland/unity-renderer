using DCL;
using DCL.EmotesWheel;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace ExpressionsHUD_Test
{
    public class EmotesWheelControllerShould : IntegrationTestSuite_Legacy
    {
        private EmotesWheelController controller;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new EmotesWheelController(null, null, null);
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
            controller.PlayEmote("wave", UserProfile.EmoteSource.EmotesWheel);

            Assert.AreEqual("wave", UserProfile.GetOwnUserProfile().avatar.expressionTriggerId);
        }
    }

    public class ExpressionsHUDViewShould : IntegrationTestSuite_Legacy
    {
        private EmotesWheelController controller;
        private EmotesWheelView view;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new EmotesWheelController(null, null, null);
            view = controller.view;
        }

        protected override IEnumerator TearDown()
        {
            DataStore.Clear();
            controller.Dispose();
            yield return base.TearDown();
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