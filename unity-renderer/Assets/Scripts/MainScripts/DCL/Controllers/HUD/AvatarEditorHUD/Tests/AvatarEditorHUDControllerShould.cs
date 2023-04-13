using DCL;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
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
        private IWearablesCatalogService wearablesCatalogService;
        private ColorList skinColorList;
        private ColorList hairColorList;
        private ColorList eyeColorList;
        private IAnalytics analytics;

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

            analytics = Substitute.For<IAnalytics>();
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            IUserProfileBridge userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(userProfile);

            controller = new AvatarEditorHUDController_Mock(DataStore.i.featureFlags, analytics, wearablesCatalogService,
                userProfileBridge);

            // TODO: We should convert the WearablesFetchingHelper static class into a non-static one and make it implement an interface. It would allow us to inject it
            //       into AvatarEditorHUDController and we would be able to replace the GetThirdPartyCollections() call by a mocked one in this test, allowing us to avoid
            //       the use of 'collectionsAlreadyLoaded = true'.
            controller.collectionsAlreadyLoaded = true;
            controller.Initialize();
            controller.SetVisibility(true);
            DataStore.i.common.isPlayerRendererLoaded.Set(true);

            userProfile.UpdateData(new UserProfileModel()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel()
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>() { },
                }
            });

            controller.avatarIsDirty = false;
        }

        [UnityTearDown]
        protected override IEnumerator TearDown()
        {
            wearablesCatalogService.Dispose();
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
            });

            var categoriesEquiped = controller.myModel.wearables.Select(x => x.data.category).ToArray();

            foreach (string category in controller.myCategoriesThatMustHaveSelection)
            {
                if (category != "body_shape") { Assert.Contains(category, categoriesEquiped); }
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
            });

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
            });

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
            });

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
            controller.HairColorClicked(hairColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f));

            Assert.AreEqual(hairColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f), controller.myModel.hairColor);
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
            controller.SkinColorClicked(skinColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f));

            Assert.AreEqual(skinColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f), controller.myModel.skinColor);
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
            controller.EyesColorClicked(eyeColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f));

            Assert.AreEqual(eyeColorList.colors[0] * new Color(0.2f, 0.4f, 0.2f), controller.myModel.eyesColor);
        }

        [Test]
        public void RandomizeOnlyTheSelectedSetOfWearables()
        {
            controller.RandomizeWearables();

            Assert.AreEqual(WearableLiterals.BodyShapes.FEMALE, controller.myModel.bodyShape.id);
            var categoriesEquiped = controller.myModel.wearables.Select(x => x.data.category).ToArray();

            foreach (string category in categoriesEquiped) { Assert.Contains(category, controller.myCategoriesToRandomize); }
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
            });

            controller.WearableClicked(WearableLiterals.BodyShapes.MALE);
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:eyebrows_01");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:eyes_00");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:bear_slippers");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:basketball_shorts");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:mouth_00");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:blue_bandana");
            controller.WearableClicked("urn:decentraland:off-chain:base-avatars:black_jacket");

            controller.SaveAvatar(Texture2D.whiteTexture, Texture2D.whiteTexture);

            AssertAvatarModelAgainstAvatarEditorHUDModel(userProfile.avatar, controller.myModel);
        }

        [Test]
        public void SendNewEquippedWearablesAnalyticsProperly()
        {
            // Arrange
            WearableItem alreadyExistingTestWearable = new WearableItem
            {
                id = "testWearableId1",
                rarity = WearableLiterals.ItemRarity.LEGENDARY,
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.EYES,
                    tags = new[] { WearableLiterals.Tags.BASE_WEARABLE },
                    representations = new WearableItem.Representation[] { }
                },
                i18n = new i18n[] { new i18n { code = "en", text = "testWearableId1" } }
            };

            WearableItem newTestWearableequipped1 = new WearableItem
            {
                id = "testWearableIdEquipped1",
                rarity = WearableLiterals.ItemRarity.EPIC,
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.FEET,
                    tags = new[] { WearableLiterals.Tags.BASE_WEARABLE },
                    representations = new WearableItem.Representation[] { }
                },
                i18n = new i18n[] { new i18n { code = "en", text = "testWearableIdEquipped1" } }
            };

            WearableItem newTestWearableequipped2 = new WearableItem
            {
                id = "testWearableIdEquipped2",
                rarity = WearableLiterals.ItemRarity.EPIC,
                data = new WearableItem.Data
                {
                    category = WearableLiterals.Categories.FEET,
                    tags = new[] { WearableLiterals.Tags.BASE_WEARABLE },
                    representations = new WearableItem.Representation[] { }
                },
                i18n = new i18n[] { new i18n { code = "en", text = "testWearableIdEquipped2" } }
            };

            wearablesCatalogService.WearablesCatalog.Add(alreadyExistingTestWearable.id, alreadyExistingTestWearable);
            wearablesCatalogService.WearablesCatalog.Add(newTestWearableequipped1.id, newTestWearableequipped1);
            wearablesCatalogService.WearablesCatalog.Add(newTestWearableequipped2.id, newTestWearableequipped2);

            List<string> oldWearables = new List<string>
            {
                alreadyExistingTestWearable.id
            };

            List<string> newWearables = new List<string>
            {
                alreadyExistingTestWearable.id,
                newTestWearableequipped1.id,
                newTestWearableequipped2.id
            };

            // Act
            controller.SendNewEquippedWearablesAnalytics(oldWearables, newWearables);

            // Assert
            analytics.Received(2).SendAnalytic(AvatarEditorHUDController.EQUIP_WEARABLE_METRIC, Arg.Any<Dictionary<string, string>>());
        }

        private void AssertAvatarModelAgainstAvatarEditorHUDModel(AvatarModel avatarModel, AvatarEditorHUDModel avatarEditorHUDModel)
        {
            Assert.AreEqual(avatarModel.bodyShape, avatarEditorHUDModel.bodyShape.id);

            Assert.AreEqual(avatarModel.wearables.Count, avatarEditorHUDModel.wearables.Count);

            for (var i = 0; i < avatarModel.wearables.Count; i++) { Assert.AreEqual(avatarModel.wearables[i], avatarEditorHUDModel.wearables[i].id); }

            Assert.AreEqual(avatarModel.skinColor, avatarEditorHUDModel.skinColor);
            Assert.AreEqual(avatarModel.hairColor, avatarEditorHUDModel.hairColor);
            Assert.AreEqual(avatarModel.eyeColor, avatarEditorHUDModel.eyesColor);
        }
    }
}
