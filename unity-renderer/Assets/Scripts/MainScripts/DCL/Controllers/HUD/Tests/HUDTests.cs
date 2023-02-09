using System.Collections;
using DCL;
using DCL.Chat;
using DCL.Helpers;
using DCL.Social.Chat;
using DCl.Social.Friends;
using DCL.Social.Friends;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;
using UnityEngine.TestTools;

namespace Tests
{
    public class HUDControllerShould : IntegrationTestSuite_Legacy
    {
        private IHUDController hudController;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            FriendsController.CreateSharedInstance(Substitute.For<IFriendsApiBridge>());
            hudController = new HUDController(new DataStore_FeatureFlag(), new HUDFactory());
            hudController.Initialize();
            yield return null;
        }

        protected override IEnumerator TearDown()
        {
            hudController.Dispose();
            yield return base.TearDown();
        }

        protected override ServiceLocator InitializeServiceLocator()
        {
            var serviceLocator = base.InitializeServiceLocator();
            serviceLocator.Register<IWebRequestController>(() => Substitute.For<IWebRequestController>());
            return serviceLocator;
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
