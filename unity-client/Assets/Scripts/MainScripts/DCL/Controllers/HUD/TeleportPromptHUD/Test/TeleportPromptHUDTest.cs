using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests
{
    public class TeleportPromptHUDTest : TestsBase
    {
        private TeleportPromptHUDController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new TeleportPromptHUDController();
        }

        [UnityTest]
        public IEnumerator CreateView()
        {
            Assert.NotNull(controller.view);
            Assert.NotNull(controller.view.gameObject);
            yield break;
        }

        [UnityTest]
        public IEnumerator OpenAndCloseCorrectly()
        {
            controller.RequestTeleport("{\"destination\": \"magic\"}");
            Assert.IsTrue(controller.view.content.activeSelf, "teleport dialog should be visible");
            controller.view.contentAnimator.Hide(true);
            Assert.IsFalse(controller.view.content.activeSelf, "teleport dialog should not be visible");
            yield break;
        }
    }
}