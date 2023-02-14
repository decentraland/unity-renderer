using Cysharp.Threading.Tasks;
using System.Collections;
using DCL;
using DCL.Chat;
using DCL.Helpers;
using DCL.Providers;
using DCL.Social.Chat;
using DCl.Social.Friends;
using DCL.Social.Friends;
using DCLServices.WearablesCatalogService;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;
using System;
using System.Collections.Generic;
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
            hudController = new HUDController(Environment.i.serviceLocator.Get<IWearablesCatalogService>(), new DataStore_FeatureFlag(), new HUDFactory(new AddressableResourceProvider()));
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
            foreach (HUDElementID element in Enum.GetValues(typeof(HUDElementID)))
                Assert.IsNull(hudController.GetHUDElement(element));

            yield break;
        }

        [UnityTest]
        public IEnumerator CreateHudIfConfigurationIsActive()
        {
            // There must be a hud controller
            Assert.IsNotNull(hudController, "There must be a HUDController in the scene");

            HUDConfiguration config = new HUDConfiguration() { active = true, visible = true };

            foreach (HUDElementID element in Enum.GetValues(typeof(HUDElementID)))
            {
                if (HUDController.IsHUDElementDeprecated(element) || element == HUDElementID.NONE)
                    continue;

                yield return hudController.ConfigureHUDElement(element, config).ToCoroutine();

                // HUD controllers are created
                for (var i = 0; i < 5; i++)
                    yield return null;

                Assert.IsNotNull(hudController.GetHUDElement(element), $"Failed to create {element}");
            }
        }
    }
}
