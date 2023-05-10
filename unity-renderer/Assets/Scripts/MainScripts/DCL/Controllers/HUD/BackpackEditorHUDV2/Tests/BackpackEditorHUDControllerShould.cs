using DCL.Browser;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DCL.Backpack
{
    public class BackpackEditorHUDControllerShould
    {
        private UserProfile userProfile;
        private IBackpackEditorHUDView view;
        private DataStore dataStore;
        private RendererState rendererState;
        private IUserProfileBridge userProfileBridge;
        private IWearablesCatalogService wearablesCatalogService;
        private IAnalytics analytics;
        private INewUserExperienceAnalytics newUserExperienceAnalytics;
        private IBackpackEmotesSectionController backpackEmotesSectionController;
        private BackpackAnalyticsController backpackAnalyticsController;
        private BackpackEditorHUDController backpackEditorHUDController;
        private WearableGridController wearableGridController;
        private AvatarSlotsHUDController avatarSlotsHUDController;
        private BackpackFiltersController backpackFiltersController;
        private IWearableGridView wearableGridView;
        private IAvatarSlotsView avatarSlotsView;
        private IBackpackFiltersComponentView backpackFiltersComponentView;
        private Texture2D testFace256Texture = new Texture2D(1, 1);
        private Texture2D testBodyTexture = new Texture2D(1, 1);

        [SetUp]
        public void SetUp()
        {
            userProfile = ScriptableObject.CreateInstance<UserProfile>();
            view = Substitute.For<IBackpackEditorHUDView>();
            userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(userProfile);
            wearablesCatalogService = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
            analytics = Substitute.For<IAnalytics>();
            newUserExperienceAnalytics = Substitute.For<INewUserExperienceAnalytics>();
            backpackEmotesSectionController = Substitute.For<IBackpackEmotesSectionController>();
            rendererState = ScriptableObject.CreateInstance<RendererState>();
            dataStore = new DataStore();
            dataStore.HUDs.avatarEditorVisible.Set(false, false);
            wearableGridView = Substitute.For<IWearableGridView>();
            testFace256Texture = new Texture2D(1, 1);
            testBodyTexture = new Texture2D(1, 1);

            backpackAnalyticsController = new BackpackAnalyticsController(
                analytics,
                newUserExperienceAnalytics,
                wearablesCatalogService);

            backpackFiltersComponentView = Substitute.For<IBackpackFiltersComponentView>();
            backpackFiltersController = new BackpackFiltersController(backpackFiltersComponentView, wearablesCatalogService);

            avatarSlotsView = Substitute.For<IAvatarSlotsView>();
            avatarSlotsHUDController = new AvatarSlotsHUDController(avatarSlotsView);

            wearableGridController = new WearableGridController(wearableGridView,
                userProfileBridge,
                wearablesCatalogService,
                dataStore.backpackV2,
                Substitute.For<IBrowserBridge>(),
                backpackFiltersController,
                avatarSlotsHUDController,
                Substitute.For<IBackpackAnalyticsController>());

            backpackEditorHUDController = new BackpackEditorHUDController(
                view,
                dataStore,
                rendererState,
                userProfileBridge,
                wearablesCatalogService,
                backpackEmotesSectionController,
                backpackAnalyticsController,
                wearableGridController,
                avatarSlotsHUDController);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rendererState);
            backpackEditorHUDController.Dispose();
            Object.Destroy(testFace256Texture);
            Object.Destroy(testBodyTexture);
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.IsTrue(dataStore.HUDs.isAvatarEditorInitialized.Get());
            view.Received(1).SetAsFullScreenMenuMode(Arg.Any<Transform>());
            view.Received(1).Hide();
            view.Received(1).ResetPreviewEmote();
            view.Received(1).SetColorPickerVisibility(false);
        }

        [Test]
        public void ShowBackpackCorrectly()
        {
            // Act
            dataStore.HUDs.avatarEditorVisible.Set(true, true);

            // Assert
            backpackEmotesSectionController.Received(1).RestoreEmoteSlots();
            backpackEmotesSectionController.Received(1).LoadEmotes();
            view.Received(1).Show();
        }

        [Test]
        public void EquipAndSaveCorrectly()
        {
            // Arrange
            userProfile.avatar.wearables.Clear();
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel, EquipWearableSource>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:f_eyebrows_01" }, EquipWearableSource.Wearable);
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel, EquipWearableSource>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bear_slippers" }, EquipWearableSource.Wearable);
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel, EquipWearableSource>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bee_t_shirt" }, EquipWearableSource.Wearable);

            view.Configure().TakeSnapshotsAfterStopPreviewAnimation(
                Arg.InvokeDelegate<IBackpackEditorHUDView.OnSnapshotsReady>(testFace256Texture, testBodyTexture),
                Arg.Any<Action>());

            // Act
            dataStore.HUDs.avatarEditorVisible.Set(false, true);

            // Assert
            Assert.IsTrue(userProfile.avatar.wearables.Count > 0);
            Assert.IsTrue(userProfile.avatar.wearables.Contains("urn:decentraland:off-chain:base-avatars:f_eyebrows_01"));
            Assert.IsTrue(userProfile.avatar.wearables.Contains("urn:decentraland:off-chain:base-avatars:bear_slippers"));
            Assert.IsTrue(userProfile.avatar.wearables.Contains("urn:decentraland:off-chain:base-avatars:bee_t_shirt"));
        }

        [Test]
        public void UnEquipAndSaveCorrectly()
        {
            // Arrange
            EquipAndSaveCorrectly();
            wearableGridView.OnWearableUnequipped += Raise.Event<Action<WearableGridItemModel, UnequipWearableSource>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:f_eyebrows_01" }, UnequipWearableSource.None);
            wearableGridView.OnWearableUnequipped += Raise.Event<Action<WearableGridItemModel, UnequipWearableSource>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bear_slippers" }, UnequipWearableSource.None);
            wearableGridView.OnWearableUnequipped += Raise.Event<Action<WearableGridItemModel, UnequipWearableSource>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bee_t_shirt" }, UnequipWearableSource.None);

            view.Configure().TakeSnapshotsAfterStopPreviewAnimation(
                Arg.InvokeDelegate<IBackpackEditorHUDView.OnSnapshotsReady>(testFace256Texture, testBodyTexture),
                Arg.Any<Action>());

            // Act
            dataStore.HUDs.avatarEditorVisible.Set(false, true);

            // Assert
            Assert.IsTrue(userProfile.avatar.wearables.Count == 0);
        }

        [Test]
        public void SelectAndPreVisualizeWearableCorrectly()
        {
            // Arrange
            EquipAndSaveCorrectly();

            // Act
            wearableGridView.OnWearableSelected += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:f_african_leggins" });

            // Assert
            view.Received(1).UpdateAvatarPreview(Arg.Is<AvatarModel>(avatarModel =>
                avatarModel.wearables.Contains("urn:decentraland:off-chain:base-avatars:f_african_leggins")));
        }

        [Test]
        public void UpdateAvatarPreviewCorrectly()
        {
            // Act
            backpackEmotesSectionController.OnNewEmoteAdded += Raise.Event<Action<string>>("testEmoteId");

            // Assert
            view.Received(1).UpdateAvatarPreview(Arg.Any<AvatarModel>());
        }

        [Test]
        public void PreviewEmoteCorrectly()
        {
            // Arrange
            var testEmoteId = "testEmoteId";

            // Act
            backpackEmotesSectionController.OnEmotePreviewed += Raise.Event<Action<string>>(testEmoteId);

            // Assert
            view.Received(1).PlayPreviewEmote(testEmoteId);
        }

        [Test]
        [TestCase("eyes")]
        [TestCase("hair")]
        [TestCase("eyebrows")]
        [TestCase("facial_hair")]
        [TestCase("body_shape")]
        [TestCase("non_existing_category")]
        public void ToggleSlotCorrectly(string slotCategory)
        {
            // Act
            avatarSlotsView.OnToggleAvatarSlot += Raise.Event<IAvatarSlotsView.ToggleAvatarSlotDelegate>(slotCategory, true, true);

            // Assert
            view.Received(1).SetColorPickerVisibility(true);
            view.Received(1).SetColorPickerAsSkinMode(slotCategory == "body_shape");

            if (slotCategory == "non_existing_category")
            {
                view.DidNotReceive().SetColorPickerValue(Arg.Any<Color>());
                return;
            }

            switch (slotCategory)
            {
                case "eyes":
                    view.Received(1).SetColorPickerValue(userProfile.avatar.eyeColor);
                    break;
                case "hair" or "eyebrows" or "facial_hair":
                    view.Received(1).SetColorPickerValue(userProfile.avatar.hairColor);
                    break;
                case "body_shape":
                    view.Received(1).SetColorPickerValue(userProfile.avatar.skinColor);
                    break;
            }
        }

        [Test]
        public void EquipBodyShapeWhenProfileUpdates()
        {
            const string BODY_SHAPE_ID = WearableLiterals.BodyShapes.FEMALE;

            // Arrange
            var testUserProfileModel = GetTestUserProfileModel();

            // Act
            userProfile.UpdateData(testUserProfileModel);

            // Assert
            Assert.IsFalse(userProfile.avatar.wearables.Contains(BODY_SHAPE_ID));
            Assert.IsTrue(dataStore.backpackV2.previewEquippedWearables.Contains(BODY_SHAPE_ID));
            backpackEmotesSectionController.Received(1).SetEquippedBodyShape(BODY_SHAPE_ID);
            avatarSlotsView.Received(1).SetSlotContent(WearableLiterals.Categories.BODY_SHAPE,
                Arg.Is<WearableItem>(w => w.id == BODY_SHAPE_ID),
                BODY_SHAPE_ID);
        }

        [TestCase(WearableLiterals.BodyShapes.FEMALE)]
        [TestCase(WearableLiterals.BodyShapes.MALE)]
        public void EquipBodyShapeFromGrid(string bodyShapeId)
        {
            userProfile.avatar.wearables.Clear();
            view.Configure().TakeSnapshotsAfterStopPreviewAnimation(
                Arg.InvokeDelegate<IBackpackEditorHUDView.OnSnapshotsReady>(testFace256Texture, testBodyTexture),
                Arg.Any<Action>());

            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel, EquipWearableSource>>(new WearableGridItemModel
            {
                WearableId = bodyShapeId,
            }, EquipWearableSource.Wearable);

            Assert.IsFalse(userProfile.avatar.wearables.Contains(bodyShapeId));
            Assert.IsTrue(dataStore.backpackV2.previewEquippedWearables.Contains(bodyShapeId));
            backpackEmotesSectionController.Received(1).SetEquippedBodyShape(bodyShapeId);
            avatarSlotsView.Received(1).SetSlotContent(WearableLiterals.Categories.BODY_SHAPE,
                Arg.Is<WearableItem>(w => w.id == bodyShapeId),
                bodyShapeId);
        }

        [Test]
        public void ReplaceBodyShape()
        {
            EquipBodyShapeFromGrid(WearableLiterals.BodyShapes.FEMALE);
            EquipBodyShapeFromGrid(WearableLiterals.BodyShapes.MALE);
        }

        private static UserProfileModel GetTestUserProfileModel() =>
            new ()
            {
                name = "name",
                email = "mail",
                avatar = new AvatarModel
                {
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>
                    {
                        "urn:decentraland:off-chain:base-avatars:f_eyebrows_01",
                        "urn:decentraland:off-chain:base-avatars:f_eyes_00",
                        "urn:decentraland:off-chain:base-avatars:bear_slippers",
                        "urn:decentraland:off-chain:base-avatars:f_african_leggins",
                        "urn:decentraland:off-chain:base-avatars:f_mouth_00",
                        "urn:decentraland:off-chain:base-avatars:blue_bandana",
                        "urn:decentraland:off-chain:base-avatars:bee_t_shirt",
                    },
                    skinColor = Color.black,
                    hairColor = Color.green,
                    eyeColor = Color.blue,
                },
            };
    }
}
