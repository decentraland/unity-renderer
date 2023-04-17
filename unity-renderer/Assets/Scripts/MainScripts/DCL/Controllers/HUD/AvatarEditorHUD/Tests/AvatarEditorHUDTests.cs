using DCL;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarEditorHUD_Tests
{
    public class AvatarEditorHUDController_Mock : AvatarEditorHUDController
    {
        public AvatarEditorHUDController_Mock(DataStore_FeatureFlag featureFlags,
            IAnalytics analytics,
            IWearablesCatalogService wearablesCatalogService,
            IUserProfileBridge userProfileBridge)
            : base(featureFlags, analytics, wearablesCatalogService, userProfileBridge) { }

        public AvatarEditorHUDModel myModel => model;
        public AvatarEditorHUDView myView => view;
        public string[] myCategoriesThatMustHaveSelection => categoriesThatMustHaveSelection;
        public string[] myCategoriesToRandomize => categoriesToRandomize;
    }

    public class WearableItemsShould : IntegrationTestSuite_Legacy
    {
        private UserProfile userProfile;
        private AvatarEditorHUDController_Mock controller;
        private IAnalytics analytics;
        private IWearablesCatalogService wearablesCatalogService;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            userProfile = ScriptableObject.CreateInstance<UserProfile>();

            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                        { }
                }
            });

            analytics = Substitute.For<IAnalytics>();
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            IUserProfileBridge userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(userProfile);

            controller = new AvatarEditorHUDController_Mock(DataStore.i.featureFlags, analytics, wearablesCatalogService,
                userProfileBridge);

            controller.collectionsAlreadyLoaded = true;
            controller.Initialize();
            DataStore.i.common.isPlayerRendererLoaded.Set(true);
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            wearablesCatalogService.Dispose();
            controller.CleanUp();
            Object.Destroy(userProfile);
            yield return base.TearDown();
        }

        [Test]
        public void BeAddedWhenEquiped()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = wearablesCatalogService.WearablesCatalog.Get(sunglassesId);

            controller.WearableClicked(sunglassesId);
            Assert.IsTrue(controller.myModel.wearables.Contains(sunglasses));
        }

        [Test]
        public void BeReplacedByGeneralReplaces()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = wearablesCatalogService.WearablesCatalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = wearablesCatalogService.WearablesCatalog.Get(bandanaId);

            bandana.data.replaces = new[] { sunglasses.data.category };
            controller.WearableClicked(sunglassesId);
            controller.WearableClicked(bandanaId);

            Assert.IsTrue(controller.myModel.wearables.Contains(bandana));
            Assert.IsFalse(controller.myModel.wearables.Contains(sunglasses));
        }

        [Test]
        public void NotBeReplacedByWrongGeneralReplaces()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = wearablesCatalogService.WearablesCatalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = wearablesCatalogService.WearablesCatalog.Get(bandanaId);

            bandana.data.replaces = new[] { "NonExistentCategory" };
            controller.WearableClicked(sunglassesId);
            controller.WearableClicked(bandanaId);

            Assert.IsTrue(controller.myModel.wearables.Contains(bandana));
            Assert.IsTrue(controller.myModel.wearables.Contains(sunglasses));
        }

        [Test]
        public void BeReplacedByOverrideReplaces()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = wearablesCatalogService.WearablesCatalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = wearablesCatalogService.WearablesCatalog.Get(bandanaId);

            bandana.GetRepresentation(userProfile.avatar.bodyShape).overrideReplaces = new[] { sunglasses.data.category };
            controller.WearableClicked(sunglassesId);
            controller.WearableClicked(bandanaId);

            Assert.IsTrue(controller.myModel.wearables.Contains(bandana));
            Assert.IsFalse(controller.myModel.wearables.Contains(sunglasses));
        }

        [Test]
        public void NotBeReplacedByWrongOverrideReplaces()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = wearablesCatalogService.WearablesCatalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = wearablesCatalogService.WearablesCatalog.Get(bandanaId);

            bandana.GetRepresentation(WearableLiterals.BodyShapes.MALE)
                   .overrideReplaces = new[] { sunglasses.data.category };

            controller.WearableClicked(sunglassesId);
            controller.WearableClicked(bandanaId);

            Assert.IsTrue(controller.myModel.wearables.Contains(bandana));
            Assert.IsTrue(controller.myModel.wearables.Contains(sunglasses));
        }
    }
}
