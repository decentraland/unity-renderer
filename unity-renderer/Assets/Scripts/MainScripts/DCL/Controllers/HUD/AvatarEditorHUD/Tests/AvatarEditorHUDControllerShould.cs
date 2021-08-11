using AvatarShape_Tests;
using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarEditorHUD_Tests
{
    public class AvatarEditorHUDControllerShould : IntegrationTestSuite_Legacy
    {
        private const string EYEBROWS_ID = "urn:decentraland:off-chain:base-avatars:f_eyebrows_01";
        private const string FEMALE_CATGLASSES_ID = "urn:decentraland:off-chain:base-avatars:f_glasses_cat_style";

        private UserProfile userProfile;
        private AvatarEditorHUDController_Mock controller;
        private BaseDictionary<string, WearableItem> catalog;
        private ColorList skinColorList;
        private ColorList hairColorList;
        private ColorList eyeColorList;

        protected override bool justSceneSetUp => true;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

            if (controller == null)
            {
                skinColorList = Resources.Load<ColorList>("SkinTone");
                hairColorList = Resources.Load<ColorList>("HairColor");
                eyeColorList = Resources.Load<ColorList>("EyeColor");

                userProfile = ScriptableObject.CreateInstance<UserProfile>();
            }

            catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            controller = new AvatarEditorHUDController_Mock();
            controller.Initialize(userProfile, catalog);
            controller.SetVisibility(true);
            DataStore.i.isPlayerRendererLoaded.Set(true);

            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }
            }, false);
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            controller.Dispose();
            yield return base.TearDown();
        }

        [Test]
        public void AutofillMandatoryCategoriesIfNotProvided()
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }
            }, false);

            var categoriesEquiped = controller.myModel.wearables.Select(x => x.data.category).ToArray();
            foreach (string category in controller.myCategoriesThatMustHaveSelection)
            {
                if (category != "body_shape")
                {
                    Assert.Contains(category, categoriesEquiped);
                }
            }
        }

        [Test]
        public void ReplaceNotSupportedWearablesWhenChangingBodyShape()
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                    {
                        FEMALE_CATGLASSES_ID
                    },
                }
            }, false);

            controller.WearableClicked(WearableLiterals.BodyShapes.MALE);

            Assert.False(controller.myModel.wearables.Any(x => x.id == FEMALE_CATGLASSES_ID));
        }

        [Test]
        public void LoadUserProfileByConstructor()
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                    {
                        EYEBROWS_ID,
                        "urn:decentraland:off-chain:base-avatars:f_eyes_00",
                        "urn:decentraland:off-chain:base-avatars:bear_slippers",
                        "urn:decentraland:off-chain:base-avatars:f_african_leggins",
                        "urn:decentraland:off-chain:base-avatars:f_mouth_00",
                        "urn:decentraland:off-chain:base-avatars:blue_bandana",
                        "urn:decentraland:off-chain:base-avatars:bee_t_shirt"
                    },
                    skinColor = skinColorList.colors[0],
                    hairColor = hairColorList.colors[0],
                    eyeColor = eyeColorList.colors[0],
                }
            }, false);

            AssertAvatarModelAgainstAvatarEditorHUDModel(userProfile.avatar, controller.myModel);
        }

        [Test]
        public void ReactToUserProfileUpdate()
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                    {
                        EYEBROWS_ID,
                        "urn:decentraland:off-chain:base-avatars:f_eyes_00",
                        "urn:decentraland:off-chain:base-avatars:bear_slippers",
                        "urn:decentraland:off-chain:base-avatars:f_african_leggins",
                        "urn:decentraland:off-chain:base-avatars:f_mouth_00",
                        "urn:decentraland:off-chain:base-avatars:blue_bandana",
                        "urn:decentraland:off-chain:base-avatars:bee_t_shirt"
                    },
                    skinColor = skinColorList.colors[0],
                    hairColor = hairColorList.colors[0],
                    eyeColor = eyeColorList.colors[0],
                }
            }, false);

            AssertAvatarModelAgainstAvatarEditorHUDModel(userProfile.avatar, controller.myModel);
        }

        [Test]
        public void ProcessClickedBodyShape()
        {
            controller.WearableClicked(WearableLiterals.BodyShapes.MALE);

            Assert.AreEqual(WearableLiterals.BodyShapes.MALE, controller.myModel.bodyShape.id);
        }

        [Test]
        public void ProcessClickedWearables()
        {
            controller.WearableClicked(EYEBROWS_ID);

            Assert.AreEqual(EYEBROWS_ID, controller.myModel.wearables.Last().id);
        }

        [Test]
        public void ProcessSupportedClickedHairColor()
        {
            controller.HairColorClicked(hairColorList.colors[3]);

            Assert.AreEqual(hairColorList.colors[3], controller.myModel.hairColor);
        }

        [Test]
        public void ProcessNotSupportedClickedHairColor()
        {
            var current = controller.myModel.hairColor;
            controller.HairColorClicked(hairColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f)); //Getting an arbitrary/invalid color

            Assert.AreEqual(current, controller.myModel.hairColor);
        }

        [Test]
        public void ProcessSupportedClickedSkinColor()
        {
            controller.SkinColorClicked(skinColorList.colors[3]);

            Assert.AreEqual(skinColorList.colors[3], controller.myModel.skinColor);
        }

        [Test]
        public void ProcessNotSupportedClickedSkinColor()
        {
            var current = controller.myModel.skinColor;
            controller.SkinColorClicked(skinColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f)); //Getting an arbitrary/invalid color

            Assert.AreEqual(current, controller.myModel.skinColor);
        }

        [Test]
        public void ProcessSupportedClickedEyeColor()
        {
            controller.EyesColorClicked(eyeColorList.colors[3]);

            Assert.AreEqual(eyeColorList.colors[3], controller.myModel.eyesColor);
        }

        [Test]
        public void ProcessNotSupportedClickedEyesColor()
        {
            var current = controller.myModel.eyesColor;
            controller.EyesColorClicked(eyeColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f)); //Getting an arbitrary/invalid color

            Assert.AreEqual(current, controller.myModel.eyesColor);
        }

        [Test]
        public void RandomizeOnlyTheSelectedSetOfWearables()
        {
            controller.RandomizeWearables();

            Assert.AreEqual(WearableLiterals.BodyShapes.FEMALE, controller.myModel.bodyShape.id);
            var categoriesEquiped = controller.myModel.wearables.Select(x => x.data.category).ToArray();
            foreach (string category in categoriesEquiped)
            {
                Assert.Contains(category, controller.myCategoriesToRandomize);
            }
        }

        [Test]
        public void SaveAvatarProperly()
        {
            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                    {
                        EYEBROWS_ID,
                        "urn:decentraland:off-chain:base-avatars:f_eyes_00",
                        "urn:decentraland:off-chain:base-avatars:bear_slippers",
                        "urn:decentraland:off-chain:base-avatars:f_african_leggins",
                        "urn:decentraland:off-chain:base-avatars:f_mouth_00",
                        "urn:decentraland:off-chain:base-avatars:blue_bandana",
                        "urn:decentraland:off-chain:base-avatars:bee_t_shirt"
                    },
                    skinColor = skinColorList.colors[0],
                    hairColor = hairColorList.colors[0],
                    eyeColor = eyeColorList.colors[0],
                }
            }, false);

            controller.WearableClicked(WearableLiterals.BodyShapes.MALE);
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:eyebrows_01");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:eyes_00");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:bear_slippers");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:basketball_shorts");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:mouth_00");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:blue_bandana");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:black_jacket");

            controller.SaveAvatar(Texture2D.whiteTexture, Texture2D.whiteTexture, Texture2D.whiteTexture, Texture2D.whiteTexture);

            AssertAvatarModelAgainstAvatarEditorHUDModel(userProfile.avatar, controller.myModel);
        }

        private void AssertAvatarModelAgainstAvatarEditorHUDModel(AvatarModel avatarModel, AvatarEditorHUDModel avatarEditorHUDModel)
        {
            Assert.AreEqual(avatarModel.bodyShape, avatarEditorHUDModel.bodyShape.id);

            Assert.AreEqual(avatarModel.wearables.Count, avatarEditorHUDModel.wearables.Count);
            for (var i = 0; i < avatarModel.wearables.Count; i++)
            {
                Assert.AreEqual(avatarModel.wearables[i], avatarEditorHUDModel.wearables[i].id);
            }

            Assert.AreEqual(avatarModel.skinColor, avatarEditorHUDModel.skinColor);
            Assert.AreEqual(avatarModel.hairColor, avatarEditorHUDModel.hairColor);
            Assert.AreEqual(avatarModel.eyeColor, avatarEditorHUDModel.eyesColor);
        }
    }
}