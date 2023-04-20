using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NUnit.Framework;
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
        private IWearableGridView wearableGridView;

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

            backpackAnalyticsController = new BackpackAnalyticsController(
                analytics,
                newUserExperienceAnalytics,
                wearablesCatalogService);

            backpackEditorHUDController = new BackpackEditorHUDController(
                view,
                dataStore,
                rendererState,
                userProfileBridge,
                wearablesCatalogService,
                backpackEmotesSectionController,
                backpackAnalyticsController,
                new WearableGridController(wearableGridView,
                    userProfileBridge,
                    wearablesCatalogService));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rendererState);
            backpackEditorHUDController.Dispose();
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
        public void LoadUserProfileProperly()
        {
            // Arrange
            backpackEditorHUDController.model.wearables.Clear();
            dataStore.common.isPlayerRendererLoaded.Set(true, false);

            // Act
            UpdateUserProfileWithTestData();

            // Assert
            AssertAvatarModelAgainstBackpackModel(userProfile.avatar, backpackEditorHUDController.model);

        }

        [Test]
        public void SaveAvatarProperly()
        {
            // Arrange
            UpdateUserProfileWithTestData();

            // Act
            backpackEditorHUDController.SaveAvatar(Texture2D.whiteTexture, Texture2D.whiteTexture);

            // Assert
            AssertAvatarModelAgainstBackpackModel(userProfile.avatar, backpackEditorHUDController.model);

        }

        private void UpdateUserProfileWithTestData()
        {
            userProfile.UpdateData(new UserProfileModel()
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
            });
        }

        private void AssertAvatarModelAgainstBackpackModel(AvatarModel avatarModel, BackpackEditorHUDModel backpackEditorHUDModel)
        {
            Assert.AreEqual(avatarModel.bodyShape, backpackEditorHUDModel.bodyShape.id);
            Assert.AreEqual(avatarModel.wearables.Count, backpackEditorHUDModel.wearables.Count);
            for (var i = 0; i < avatarModel.wearables.Count; i++)
                Assert.AreEqual(avatarModel.wearables[i], backpackEditorHUDModel.wearables[i].id);
            Assert.AreEqual(avatarModel.skinColor, backpackEditorHUDModel.skinColor);
            Assert.AreEqual(avatarModel.hairColor, backpackEditorHUDModel.hairColor);
            Assert.AreEqual(avatarModel.eyeColor, backpackEditorHUDModel.eyesColor);
        }
    }
}
