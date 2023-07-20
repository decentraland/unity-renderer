using DCL.Browser;
using DCLServices.DCLFileBrowser;
using DCLServices.Lambdas;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Components.Avatar.VRMExporter;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using MainScripts.DCL.Models.AvatarAssets.Tests.Helpers;
using NSubstitute;
using NSubstitute.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private BackpackAnalyticsService backpackAnalyticsService;
        private BackpackEditorHUDController backpackEditorHUDController;
        private WearableGridController wearableGridController;
        private AvatarSlotsHUDController avatarSlotsHUDController;
        private BackpackFiltersController backpackFiltersController;
        private IWearableGridView wearableGridView;
        private IAvatarSlotsView avatarSlotsView;
        private IBackpackFiltersComponentView backpackFiltersComponentView;
        private Texture2D testFace256Texture = new Texture2D(1, 1);
        private Texture2D testBodyTexture = new Texture2D(1, 1);
        private IVRMExporter vrmExporter;
        private IDCLFileBrowserService fileBrowserService;

        GameObject audioHandler;

        [SetUp]
        public void SetUp()
        {
            audioHandler = MainSceneFactory.CreateAudioHandler();
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
            vrmExporter = Substitute.For<IVRMExporter>();
            fileBrowserService = Substitute.For<IDCLFileBrowserService>();

            backpackAnalyticsService = new BackpackAnalyticsService(
                analytics,
                newUserExperienceAnalytics);

            backpackFiltersComponentView = Substitute.For<IBackpackFiltersComponentView>();
            backpackFiltersController = new BackpackFiltersController(backpackFiltersComponentView, wearablesCatalogService);

            avatarSlotsView = Substitute.For<IAvatarSlotsView>();
            var ffBaseVariable = Substitute.For<IBaseVariable<FeatureFlag>>();
            var featureFlags = new FeatureFlag();
            ffBaseVariable.Get().Returns(featureFlags);
            avatarSlotsHUDController = new AvatarSlotsHUDController(avatarSlotsView, backpackAnalyticsService, ffBaseVariable);

            wearableGridController = new WearableGridController(wearableGridView,
                userProfileBridge,
                wearablesCatalogService,
                dataStore.backpackV2,
                Substitute.For<IBrowserBridge>(),
                backpackFiltersController,
                avatarSlotsHUDController,
                Substitute.For<IBackpackAnalyticsService>());

            backpackEditorHUDController = new BackpackEditorHUDController(
                view,
                dataStore,
                rendererState,
                userProfileBridge,
                wearablesCatalogService,
                backpackEmotesSectionController,
                backpackAnalyticsService,
                wearableGridController,
                avatarSlotsHUDController,
                new OutfitsController(Substitute.For<IOutfitsSectionComponentView>(), new LambdaOutfitsService(Substitute.For<ILambdasService>(), Substitute.For<IServiceProviders>()), userProfileBridge, Substitute.For<DataStore>(), Substitute.For<IBackpackAnalyticsService>()),
                vrmExporter,
                fileBrowserService);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(rendererState);
            backpackEditorHUDController.Dispose();
            Object.Destroy(testFace256Texture);
            Object.Destroy(testBodyTexture);
            Object.Destroy(audioHandler);
        }

        [Test]
        public void InitializeCorrectly()
        {
            // Assert
            Assert.IsTrue(dataStore.HUDs.isAvatarEditorInitialized.Get());
            view.Received(1).SetAsFullScreenMenuMode(Arg.Any<Transform>());
            view.Received(1).Hide();
            view.Received(1).ResetPreviewPanel();
            view.Received(1).SetColorPickerVisibility(false);
        }

        [Test]
        public void ShowBackpackCorrectly()
        {
            // Arrange
            dataStore.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InWorld, false);

            // Act
            dataStore.HUDs.avatarEditorVisible.Set(true, true);

            // Assert
            Assert.AreEqual(AvatarMaterialProfile.InEditor, dataStore.skyboxConfig.avatarMatProfile.Get());
            backpackEmotesSectionController.Received(1).RestoreEmoteSlots();
            backpackEmotesSectionController.Received(1).LoadEmotes();
            view.Received(1).Show();
        }

        [Test]
        public void EquipAndSaveCorrectly()
        {
            // Arrange
            dataStore.skyboxConfig.avatarMatProfile.Set(AvatarMaterialProfile.InEditor, false);
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
            Assert.AreEqual(AvatarMaterialProfile.InWorld, dataStore.skyboxConfig.avatarMatProfile.Get());
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
        [TestCase("eyes", PreviewCameraFocus.FaceEditing)]
        [TestCase("body_shape", PreviewCameraFocus.DefaultEditing)]
        [TestCase("non_existing_category", PreviewCameraFocus.DefaultEditing)]
        public void ToggleSlotCorrectly(string slotCategory, PreviewCameraFocus previewCameraFocus)
        {
            // Act
            avatarSlotsView.OnToggleAvatarSlot += Raise.Event<IAvatarSlotsView.ToggleAvatarSlotDelegate>(slotCategory, true, previewCameraFocus, true);

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

            view.Received(1).SetAvatarPreviewFocus(previewCameraFocus);
        }

        [Test]
        public void EquipBodyShapeWhenProfileUpdates()
        {
            const string BODY_SHAPE_ID = WearableLiterals.BodyShapes.FEMALE;

            // Arrange
            var testUserProfileModel = GetTestUserProfileModel();
            view.isVisible.Returns(true);
            rendererState.Set(true);

            // Act
            userProfile.UpdateData(testUserProfileModel);

            // Assert
            Assert.IsFalse(userProfile.avatar.wearables.Contains(BODY_SHAPE_ID));
            Assert.IsFalse(dataStore.backpackV2.previewEquippedWearables.Contains(BODY_SHAPE_ID));
            Assert.AreEqual(dataStore.backpackV2.previewBodyShape.Get(), BODY_SHAPE_ID);
            backpackEmotesSectionController.Received(1).SetEquippedBodyShape(BODY_SHAPE_ID);
            avatarSlotsView.Received(1).SetSlotContent(WearableLiterals.Categories.BODY_SHAPE,
                Arg.Is<WearableItem>(w => w.id == BODY_SHAPE_ID),
                BODY_SHAPE_ID,
                Arg.Any<HashSet<string>>());
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
            Assert.AreEqual(dataStore.backpackV2.previewBodyShape.Get(), bodyShapeId);
            Assert.IsFalse(dataStore.backpackV2.previewEquippedWearables.Contains(bodyShapeId));
            backpackEmotesSectionController.Received(1).SetEquippedBodyShape(bodyShapeId);
            avatarSlotsView.Received(1).SetSlotContent(WearableLiterals.Categories.BODY_SHAPE,
                Arg.Is<WearableItem>(w => w.id == bodyShapeId),
                bodyShapeId,
                Arg.Any<HashSet<string>>());
        }

        [Test]
        public void ReplaceBodyShape()
        {
            EquipBodyShapeFromGrid(WearableLiterals.BodyShapes.FEMALE);
            EquipBodyShapeFromGrid(WearableLiterals.BodyShapes.MALE);
        }

        [Test]
        public void SaveAvatarAndCloseViewWhenContinueSignup()
        {
            view.Configure().TakeSnapshotsAfterStopPreviewAnimation(
                Arg.InvokeDelegate<IBackpackEditorHUDView.OnSnapshotsReady>(testFace256Texture, testBodyTexture),
                Arg.Any<Action>());

            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel, EquipWearableSource>>(
                new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:f_eyebrows_01" },
                EquipWearableSource.Wearable);
            wearableGridView.OnWearableEquipped += Raise.Event<Action<WearableGridItemModel, EquipWearableSource>>(
                new WearableGridItemModel { WearableId = "urn:decentraland:off-chain:base-avatars:bear_slippers" },
                EquipWearableSource.Wearable);
            view.ClearReceivedCalls();

            view.OnContinueSignup += Raise.Event<Action>();

            Assert.IsTrue(userProfile.avatar.wearables.Count > 0);
            Assert.IsTrue(userProfile.avatar.wearables.Contains("urn:decentraland:off-chain:base-avatars:f_eyebrows_01"));
            Assert.IsTrue(userProfile.avatar.wearables.Contains("urn:decentraland:off-chain:base-avatars:bear_slippers"));
            view.Received(1).Hide();
            view.ReceivedWithAnyArgs(1).TakeSnapshotsAfterStopPreviewAnimation(default(IBackpackEditorHUDView.OnSnapshotsReady), default(Action));
        }

        [Test]
        public void ShowSignup()
        {
            dataStore.common.isSignUpFlow.Set(true);

            dataStore.HUDs.avatarEditorVisible.Set(true, true);

            view.Received(1).ShowContinueSignup();
        }

        [Test]
        public void HideSignup()
        {
            dataStore.common.isSignUpFlow.Set(false);

            dataStore.HUDs.avatarEditorVisible.Set(true, true);

            view.Received(1).HideContinueSignup();
        }

        [Test]
        public void SelectBodyShapeCategoryWhenSignupFlow()
        {
            dataStore.common.isSignUpFlow.Set(true);

            dataStore.HUDs.avatarEditorVisible.Set(true, true);

            avatarSlotsView.Received(1).Select(WearableLiterals.Categories.BODY_SHAPE, true);
        }

        [Test]
        public async Task ExportVrm()
        {
            List<SkinnedMeshRenderer> smrs = new List<SkinnedMeshRenderer>()
            {
                new GameObject("go0").AddComponent<SkinnedMeshRenderer>(),
                new GameObject("go1").AddComponent<SkinnedMeshRenderer>(),
            };
            view.originalVisibleRenderers.Returns(x => smrs);
            userProfile.model.name = "testing#name";
            view.ClearReceivedCalls();

            await backpackEditorHUDController.VrmExport(default);

            Received.InOrder(() =>
            {
                // Assert disabling buttons while exporting
                view?.SetVRMButtonEnabled(false);
                view?.SetVRMSuccessToastActive(false);

                // Assert analytics
                backpackAnalyticsService.SendVRMExportStarted();

                // Assert exportation
                vrmExporter.Export(Arg.Any<string>(), Arg.Any<string>(), smrs);
                fileBrowserService.SaveFileAsync(Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Is<string>(s => s.StartsWith("testing_name")),
                    Arg.Any<byte[]>(),
                    Arg.Any<ExtensionFilter[]>());

                // Assert enabling buttons after exporting
                view?.SetVRMSuccessToastActive(true);
                view?.SetVRMButtonEnabled(true);
                view?.SetVRMSuccessToastActive(false);

                // Assert analytics
                backpackAnalyticsService.SendVRMExportSucceeded();
            });
            for (int i = smrs.Count - 1; i >= 0; i--)
                Object.Destroy(smrs[i].gameObject);
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
