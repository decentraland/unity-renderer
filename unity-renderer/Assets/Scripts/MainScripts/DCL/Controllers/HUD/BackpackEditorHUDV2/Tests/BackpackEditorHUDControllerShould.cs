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
        private IWearableGridView wearableGridView;
        private IAvatarSlotsView avatarSlotsView;
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

            wearableGridController = new WearableGridController(wearableGridView,
                userProfileBridge,
                wearablesCatalogService,
                dataStore.backpackV2);

            avatarSlotsView = Substitute.For<IAvatarSlotsView>();

            backpackEditorHUDController = new BackpackEditorHUDController(
                view,
                dataStore,
                rendererState,
                userProfileBridge,
                wearablesCatalogService,
                backpackEmotesSectionController,
                backpackAnalyticsController,
                wearableGridController,
                new AvatarSlotsHUDController(avatarSlotsView));
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
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:f_eyebrows_01" });
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bear_slippers" });
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bee_t_shirt" });

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
            wearableGridView.OnWearableUnequipped += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:f_eyebrows_01" });
            wearableGridView.OnWearableUnequipped += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bear_slippers" });
            wearableGridView.OnWearableUnequipped += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bee_t_shirt" });


            view.Configure().TakeSnapshotsAfterStopPreviewAnimation(
                Arg.InvokeDelegate<IBackpackEditorHUDView.OnSnapshotsReady>(testFace256Texture, testBodyTexture),
                Arg.Any<Action>());

            // Act
            dataStore.HUDs.avatarEditorVisible.Set(false, true);

            // Assert
            Assert.IsTrue(userProfile.avatar.wearables.Count == 0);
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
        public void EquipBodyShapeCorrectly()
        {
            // Arrange
            var testUserProfileModel = GetTestUserProfileModel();

            // Act
            userProfile.UpdateData(testUserProfileModel);

            // Assert
            backpackEmotesSectionController.Received(1).SetEquippedBodyShape(testUserProfileModel.avatar.bodyShape);
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
