using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace AvatarShape_Tests
{
    public class WearableItemsShould : IntegrationTestSuite_Legacy
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BLUE_BANDANA_ID = "dcl://base-avatars/blue_bandana";

        private AvatarModel avatarModel;
        private BaseDictionary<string, WearableItem> catalog;
        private AvatarShape avatarShape;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            SetUp_SceneController();
            yield return SetUp_CharacterController();

            if (avatarShape == null)
            {
                avatarModel = new AvatarModel()
                {
                    name = " test",
                    hairColor = Color.white,
                    eyeColor = Color.white,
                    skinColor = Color.white,
                    bodyShape = WearableLiterals.BodyShapes.FEMALE,
                    wearables = new List<string>()
                    {
                    }
                };
                catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();
                avatarShape = AvatarShapeTestHelpers.CreateAvatarShape(scene, avatarModel);

                yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);
            }
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeVisibleByDefault()
        {
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID};

            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeHiddenByGeneralHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.hides = new[] {sunglasses.category};
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NotBeHiddenByWrongGeneralHides()
        {
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.hides = new[] {"NonExistentCategory"};
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.GetRepresentation(avatarModel.bodyShape).overrideHides = new[] {sunglasses.category};
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator NotBeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.GetRepresentation(WearableLiterals.BodyShapes.MALE).overrideHides = new[] {sunglasses.category};
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeUnequipedProperly()
        {
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            avatarModel.wearables = new List<string>() { };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            var wearableControllers = AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer);

            Assert.IsFalse(wearableControllers.ContainsKey(SUNGLASSES_ID));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeUnequipedProperlyMultipleTimes()
        {
            List<GameObject> containers = new List<GameObject>();

            for (int i = 0; i < 6; i++)
            {
                avatarModel.wearables = new List<string>() {SUNGLASSES_ID};
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
                containers.Add(AvatarRenderer_Mock.GetWearableController(avatarShape.avatarRenderer, SUNGLASSES_ID)?.myAssetContainer);

                avatarModel.wearables = new List<string>() { };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            }

            Assert.IsTrue(containers.All(x => x == null));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator SetTheCorrectMaterial()
        {
            avatarModel = AvatarShapeTestHelpers.GetTestAvatarModel("test", "TestAvatar.json");
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var wearableControllers = AvatarRenderer_Mock.GetWearableMockControllers(avatarShape.avatarRenderer);
            List<Material> materials = new List<Material>();
            foreach (var wearableControllerMock in wearableControllers.Values)
            {
                if (wearableControllerMock.category == WearableLiterals.Categories.EYES || wearableControllerMock.category == WearableLiterals.Categories.EYEBROWS || wearableControllerMock.category == WearableLiterals.Categories.MOUTH)
                    continue;

                materials.AddRange(wearableControllerMock.myAssetContainer.GetComponentsInChildren<Renderer>().SelectMany(x => x.materials).ToList());
            }

            Assert.IsTrue(materials.All(x => x.shader.name == "DCL/Toon Shader"));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator SetTheCorrectMaterialWhenLoadingMultipleTimes()
        {
            avatarModel = AvatarShapeTestHelpers.GetTestAvatarModel("test", "TestAvatar.json");
            avatarShape.avatarRenderer.ApplyModel(avatarModel, null, null);
            avatarShape.avatarRenderer.ApplyModel(avatarModel, null, null);
            avatarShape.avatarRenderer.ApplyModel(avatarModel, null, null);

            bool lastUpdateIsDone = false;
            avatarShape.avatarRenderer.ApplyModel(avatarModel, () => lastUpdateIsDone = true, null);

            yield return new DCL.WaitUntil(() => lastUpdateIsDone);

            var wearableControllers = AvatarRenderer_Mock.GetWearableMockControllers(avatarShape.avatarRenderer);
            List<Material> materials = new List<Material>();
            foreach (var wearableControllerMock in wearableControllers.Values)
            {
                if (wearableControllerMock.category == WearableLiterals.Categories.EYES || wearableControllerMock.category == WearableLiterals.Categories.EYEBROWS || wearableControllerMock.category == WearableLiterals.Categories.MOUTH)
                    continue;

                materials.AddRange(wearableControllerMock.myAssetContainer.GetComponentsInChildren<Renderer>().SelectMany(x => x.materials).ToList());
            }

            Assert.IsTrue(materials.All(x => x.shader.name == "DCL/Toon Shader"));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeRetrievedWithoutPoolableObject()
        {
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesAssetContainer = AvatarRenderer_Mock.GetWearableController(avatarShape.avatarRenderer, SUNGLASSES_ID)?.myAssetContainer;
            var bandanaAssetContainer = AvatarRenderer_Mock.GetWearableController(avatarShape.avatarRenderer, BLUE_BANDANA_ID)?.myAssetContainer;

            var sunglassesPoolableObject = PoolManager.i.GetPoolable(sunglassesAssetContainer);
            var bandanaPoolableObject = PoolManager.i.GetPoolable(bandanaAssetContainer);
            Assert.IsNull(sunglassesPoolableObject);
            Assert.IsNull(bandanaPoolableObject);
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator HideBodyShapeProperly()
        {
            catalog.Get(SUNGLASSES_ID).hides = new[] {WearableLiterals.Misc.HEAD};
            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var bodyShapeAssetContainer = AvatarRenderer_Mock.GetBodyShapeController(avatarShape.avatarRenderer)?.myAssetContainer;
            Assert.IsNotNull(bodyShapeAssetContainer);

            var renderers = bodyShapeAssetContainer.GetComponentsInChildren<Renderer>();
            Assert.IsTrue(renderers.All(x => !x.enabled));
        }

        [UnityTest]
        [Explicit("Test taking too long")]
        [Category("Explicit")]
        public IEnumerator BeHiddenUntilWholeAvatarIsReady()
        {
            avatarShape.avatarRenderer.CleanupAvatar();
            yield return null; //NOTE(Brian): Must wait a frame in order to all gameObjects finishes destroying.

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            avatarShape.avatarRenderer.ApplyModel(avatarModel, null, null);

            while (avatarShape.avatarRenderer.isLoading)
            {
                AssertAllAvatarRenderers(false);
                yield return null;
            }

            AssertAllAvatarRenderers(true);
        }

        private void AssertAllAvatarRenderers(bool shouldBeEnabled)
        {
            Renderer[] renderers = avatarShape.avatarRenderer.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r.enabled)
                {
                    if (shouldBeEnabled)
                        Assert.IsTrue(r.enabled, "All renderers should be enabled on cleanup!");
                    else
                        Assert.IsTrue(!r.enabled, "All renderers should be disabled on cleanup!");
                }
            }
        }
    }
}