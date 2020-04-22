using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Tests
{
    public class HUDControllerShould : TestsBase
    {
        [UnityTest]
        public IEnumerator NotCreateHUDsInitially()
        {
            yield return InitScene();
            // There must be a hud controller
            HUDController hudController = HUDController.i;
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            // HUD controllers are created
            for (int i = 1; i < (int)HUDController.HUDElementID.COUNT; i++)
            {
                Assert.IsNull(hudController.GetHUDElement((HUDController.HUDElementID)i));
            }
        }

        [UnityTest]
        public IEnumerator CreateHudIfConfigurationIsActive()
        {
            yield return InitScene();
            // There must be a hud controller
            HUDController hudController = HUDController.i;
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            HUDConfiguration config = new HUDConfiguration() { active = true, visible = true };

            for (int i = 1; i < (int)HUDController.HUDElementID.COUNT; i++)
            {
                hudController.ConfigureHUDElement((HUDController.HUDElementID)i, config);
            }

            yield return null;

            // HUD controllers are created
            for (int i = 1; i < (int)HUDController.HUDElementID.COUNT; i++)
            {
                Assert.IsNotNull(hudController.GetHUDElement((HUDController.HUDElementID)i), $"Failed to create {(HUDController.HUDElementID)i}");
            }
        }
    }
}
