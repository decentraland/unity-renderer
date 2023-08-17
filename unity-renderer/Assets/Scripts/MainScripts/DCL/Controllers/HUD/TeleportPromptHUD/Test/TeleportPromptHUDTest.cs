using DCL;
using DCL.Map;
using System.Collections;
using NUnit.Framework;
using NSubstitute;
using RPC.Context;
using UnityEngine.TestTools;

namespace Tests
{
    public class TeleportPromptHUDTest : IntegrationTestSuite_Legacy
    {
        private TeleportPromptHUDController controller;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            controller = new TeleportPromptHUDController(Substitute.For<DataStore>(),
                Substitute.For<IMinimapApiBridge>(),
                Substitute.For<RestrictedActionsContext>(),
                Substitute.For<ITeleportController>());
        }

        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
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
            controller.RequestJSONTeleport("{\"destination\": \"magic\"}");
            yield return null;

            Assert.IsTrue(controller.view.content.activeSelf, "teleport dialog should be visible");
            Assert.IsTrue(controller.view.imageGotoMagic.gameObject.activeSelf, "magic should be visible");

            controller.view.Reset();
            Assert.IsFalse(controller.view.imageGotoMagic.gameObject.activeSelf, "magic should be visible");
        }
    }
}
