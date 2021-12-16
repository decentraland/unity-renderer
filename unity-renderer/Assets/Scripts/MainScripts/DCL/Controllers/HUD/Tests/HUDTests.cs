using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Helpers;
using UnityEngine.TestTools;

namespace Tests
{
    public class HUDControllerShould : IntegrationTestSuite_Legacy
    {
        private IHUDController hudController = null;
        private FriendsController friendsController;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            friendsController = TestUtils.CreateComponentWithGameObject<FriendsController>("FriendsController");
            hudController = DCL.Environment.i.hud.controller;
            hudController.Cleanup();
            yield return null;
        }

        protected override IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(friendsController.gameObject);
            hudController.Cleanup();
            yield return base.TearDown();
        }

        [UnityTest]
        public IEnumerator NotCreateHUDsInitially()
        {
            // There must be a hud controller
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            hudController.Cleanup();
            // HUD controllers are created
            for (int i = 1; i < (int) HUDElementID.COUNT; i++)
            {
                Assert.IsNull(hudController.GetHUDElement((HUDElementID) i));
            }

            yield break;
        }

        [UnityTest]
        public IEnumerator CreateHudIfConfigurationIsActive()
        {
            // There must be a hud controller
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            HUDConfiguration config = new HUDConfiguration() { active = true, visible = true };

            for (int i = 1; i < (int) HUDElementID.COUNT; i++)
            {
                hudController.ConfigureHUDElement((HUDElementID) i, config, null);
            }

            yield return null;

            // HUD controllers are created
            for (int i = 1; i < (int) HUDElementID.COUNT; i++)
            {
                HUDElementID elementID = (HUDElementID) i;
                if (HUDController.IsHUDElementDeprecated(elementID))
                    continue;

                Assert.IsNotNull(hudController.GetHUDElement(elementID), $"Failed to create {elementID}");
            }
        }
    }
}