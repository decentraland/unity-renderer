using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarEditorHUD_Tests
{
    public class AvatarEditorHUDController_Mock : AvatarEditorHUDController
    {
        public AvatarEditorHUDController_Mock(UserProfile userProfile, WearableDictionary catalog) : base(userProfile, catalog) { }

        public void MyOnItemSelected(WearableItem wearableItem) => OnItemSelected(wearableItem);
        public AvatarEditorHUDModel myModel => model;
    }

    public class WearableItemsShould : TestsBase
    {
        private UserProfile userProfile;
        private AvatarEditorHUDController_Mock controller;
        private WearableDictionary catalog;

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            yield return InitScene();

            userProfile = ScriptableObject.CreateInstance<UserProfile>();
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = "dcl://base-avatars/BaseFemale",
                    wearables = new List<string>()
                        { }
                }
            });

            catalog = AvatarTestHelpers.CreateTestCatalog();
            controller = new AvatarEditorHUDController_Mock(userProfile, catalog);
        }

        [Test]
        public void BeAddedWhenEquiped()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);

            controller.MyOnItemSelected(sunglasses);
            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(sunglassesId));
        }

        [Test]
        public void BeReplacedByGeneralReplaces()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.replaces = new [] { sunglasses.category };
            controller.MyOnItemSelected(sunglasses);
            controller.MyOnItemSelected(bandana);

            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(bandanaId));
            Assert.IsFalse(controller.myModel.avatarModel.wearables.Contains(sunglassesId));
        }

        [Test]
        public void NotBeReplacedByWrongGeneralReplaces()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.replaces = new [] { "NonExistentCategory" };
            controller.MyOnItemSelected(sunglasses);
            controller.MyOnItemSelected(bandana);

            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(bandanaId));
            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(sunglassesId));
        }

        [Test]
        public void BeReplacedByOverrideReplaces()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.GetRepresentation(userProfile.avatar.bodyShape).overrideReplaces = new [] { sunglasses.category };
            controller.MyOnItemSelected(sunglasses);
            controller.MyOnItemSelected(bandana);

            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(bandanaId));
            Assert.IsFalse(controller.myModel.avatarModel.wearables.Contains(sunglassesId));
        }

        [Test]
        public void NotBeReplacedByWrongOverrideReplaces()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.GetRepresentation("dcl://base-avatars/BaseMale").overrideReplaces = new [] { sunglasses.category };
            controller.MyOnItemSelected(sunglasses);
            controller.MyOnItemSelected(bandana);

            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(bandanaId));
            Assert.IsTrue(controller.myModel.avatarModel.wearables.Contains(sunglassesId));
        }
    }
}