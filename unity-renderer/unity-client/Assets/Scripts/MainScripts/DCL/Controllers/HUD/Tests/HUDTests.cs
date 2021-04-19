using NUnit.Framework;
using System.Collections;
using DCL;
using UnityEngine.TestTools;

namespace Tests
{
    public class HUDControllerShould : IntegrationTestSuite_Legacy
    {
        protected override bool justSceneSetUp => true;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            HUDController.i.Cleanup();
        }

        protected override IEnumerator TearDown()
        {
            HUDController.i.Cleanup();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator NotCreateHUDsInitially()
        {
            // There must be a hud controller
            HUDController hudController = HUDController.i;
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            HUDController.i.Cleanup();
            // HUD controllers are created
            for (int i = 1; i < (int) HUDController.HUDElementID.COUNT; i++)
            {
                Assert.IsNull(hudController.GetHUDElement((HUDController.HUDElementID) i));
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator CreateHudIfConfigurationIsActive()
        {
            // There must be a hud controller
            HUDController hudController = HUDController.i;
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            HUDConfiguration config = new HUDConfiguration() {active = true, visible = true};

            for (int i = 1; i < (int) HUDController.HUDElementID.COUNT; i++)
            {
                hudController.ConfigureHUDElement((HUDController.HUDElementID) i, config, null);
            }

            yield return null;

            // HUD controllers are created
            for (int i = 1; i < (int) HUDController.HUDElementID.COUNT; i++)
            {
                Assert.IsNotNull(hudController.GetHUDElement((HUDController.HUDElementID) i), $"Failed to create {(HUDController.HUDElementID) i}");
            }
        }
    }
}