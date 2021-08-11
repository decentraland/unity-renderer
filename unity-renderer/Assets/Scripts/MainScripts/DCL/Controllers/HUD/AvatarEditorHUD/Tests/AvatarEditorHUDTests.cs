using AvatarShape_Tests;
using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarEditorHUD_Tests
{
    public class AvatarEditorHUDController_Mock : AvatarEditorHUDController
    {
        public AvatarEditorHUDModel myModel => model;
        public AvatarEditorHUDView myView => view;
        public string[] myCategoriesThatMustHaveSelection => categoriesThatMustHaveSelection;
        public string[] myCategoriesToRandomize => categoriesToRandomize;
    }

    public class WearableItemsShould : IntegrationTestSuite_Legacy
    {
        private UserProfile userProfile;
        private AvatarEditorHUDController_Mock controller;
        private BaseDictionary<string, WearableItem> catalog;

        protected override bool justSceneSetUp => true;

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
            }, false);

            catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            controller = new AvatarEditorHUDController_Mock();
            controller.Initialize(userProfile, catalog);
            DataStore.i.isPlayerRendererLoaded.Set(true);
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            controller.CleanUp();
            yield return base.TearDown();
        }

        [Test]
        public void BeAddedWhenEquiped()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);

            controller.WearableClicked(sunglassesId);
            Assert.IsTrue(controller.myModel.wearables.Contains(sunglasses));
        }

        [Test]
        public void BeReplacedByGeneralReplaces()
        {
            var sunglassesId = "urn:decentraland:off-chain:base-avatars:black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = catalog.Get(bandanaId);

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
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = catalog.Get(bandanaId);

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
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = catalog.Get(bandanaId);

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
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "urn:decentraland:off-chain:base-avatars:blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.GetRepresentation(WearableLiterals.BodyShapes.MALE)
                .overrideReplaces = new[] { sunglasses.data.category };
            controller.WearableClicked(sunglassesId);
            controller.WearableClicked(bandanaId);

            Assert.IsTrue(controller.myModel.wearables.Contains(bandana));
            Assert.IsTrue(controller.myModel.wearables.Contains(sunglasses));
        }
    }
}