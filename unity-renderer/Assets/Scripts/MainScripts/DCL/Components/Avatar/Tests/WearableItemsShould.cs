using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using Tests;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AvatarShape_Tests
{
    public class WearableItemsShould : TestsBase
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BLUE_BANDANA_ID = "dcl://base-avatars/blue_bandana";

        private AvatarModel avatarModel;
        private WearableDictionary catalog;
        private AvatarShape avatarShape;

        [UnitySetUp]
        private IEnumerator SetUp()
        {
            yield return InitScene();

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
            catalog = AvatarTestHelpers.CreateTestCatalog();
            avatarShape = AvatarTestHelpers.CreateAvatarShape(scene, avatarModel);

            yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);
        }

        [UnityTest]
        public IEnumerator BeVisibleByDefault()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID };

            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByGeneralHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.hides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByWrongGeneralHides()
        {
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.hides = new [] { "NonExistentCategory" };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.GetRepresentation(avatarModel.bodyShape).overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_ID);
            var bandana = catalog.Get(BLUE_BANDANA_ID);

            bandana.GetRepresentation(WearableLiterals.BodyShapes.MALE).overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BLUE_BANDANA_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeUnequipedProperly()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            avatarModel.wearables = new List<string>() { };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            var wearableControllers = AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer);

            Assert.IsFalse(wearableControllers.ContainsKey(SUNGLASSES_ID));
        }

        [UnityTest]
        public IEnumerator BeUnequipedProperlyMultipleTimes()
        {
            List<GameObject> containers = new List<GameObject>();

            for (int i = 0; i < 6; i++)
            {
                avatarModel.wearables = new List<string>() { SUNGLASSES_ID };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
                containers.Add(AvatarRenderer_Mock.GetWearableController(avatarShape.avatarRenderer, SUNGLASSES_ID)?.myAssetContainer);

                avatarModel.wearables = new List<string>() { };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            }

            Assert.IsTrue(containers.All(x => x == null));
        }

        [UnityTest]
        public IEnumerator SetTheCorrectMaterial()
        {
            avatarModel = AvatarTestHelpers.GetTestAvatarModel("test", "TestAvatar.json");
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
        public IEnumerator SetTheCorrectMaterialWhenLoadingMultipleTimes()
        {
            avatarModel = AvatarTestHelpers.GetTestAvatarModel("test", "TestAvatar.json");
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
        public IEnumerator BeRetrievedWithoutPoolableObject()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesAssetContainer = AvatarRenderer_Mock.GetWearableController(avatarShape.avatarRenderer, SUNGLASSES_ID)?.myAssetContainer;
            var bandanaAssetContainer = AvatarRenderer_Mock.GetWearableController(avatarShape.avatarRenderer, BLUE_BANDANA_ID)?.myAssetContainer;
            var sunglassesPoolableObject = sunglassesAssetContainer.GetComponentInChildren<PoolableObject>();
            var bandanaPoolableObject = bandanaAssetContainer.GetComponentInChildren<PoolableObject>();
            Assert.IsNull(sunglassesPoolableObject);
            Assert.IsNull(bandanaPoolableObject);
        }

        [UnityTest]
        public IEnumerator HideBodyShapeProperly()
        {
            catalog.Get(SUNGLASSES_ID).hides = new [] { WearableLiterals.Misc.HEAD };
            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var bodyShapeAssetContainer = AvatarRenderer_Mock.GetBodyShapeController(avatarShape.avatarRenderer)?.myAssetContainer;
            Assert.IsNotNull(bodyShapeAssetContainer);

            var renderers = bodyShapeAssetContainer.GetComponentsInChildren<Renderer>();
            Assert.IsTrue(renderers.All(x => !x.enabled));
        }
    }
}