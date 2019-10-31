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
    class AvatarRenderer_Mock : AvatarRenderer
    {
        public static Dictionary<string, WearableController> GetWearableControllers(AvatarRenderer renderer)
        {
            var avatarRendererMock = new GameObject("Temp").AddComponent<AvatarRenderer_Mock>();
            avatarRendererMock.CopyFrom(renderer);

            var toReturn = avatarRendererMock.wearablesController;
            Destroy(avatarRendererMock.gameObject);

            return toReturn;
        }

        protected override void OnDestroy() { }
    }

    class WearableController_Mock : WearableController
    {
        public WearableController_Mock(WearableItem wearableItem, string bodyShapeType) : base(wearableItem, bodyShapeType) { }
        public WearableController_Mock(WearableController original) : base(original) { }

        public Renderer[] myAssetRenderers => assetRenderers;
        public GameObject myAssetContainer => this.assetContainer;
    }

    public class WearableItemsShould : TestsBase
    {
        private const string SUNGLASSES_WEARABLE_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BANDANA_WEARABLE_ID = "dcl://base-avatars/blue_bandana";
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
                bodyShape = "dcl://base-avatars/BaseFemale",
                wearables = new List<string>()
                {
                }
            };
            catalog = AvatarTestHelpers.CreateTestCatalog();
            avatarShape = AvatarTestHelpers.CreateAvatar(scene, avatarModel);

            yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);
        }

        [UnityTest]
        public IEnumerator BeVisibleByDefault()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID };

            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_WEARABLE_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByGeneralHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_WEARABLE_ID);
            var bandana = catalog.Get(BANDANA_WEARABLE_ID);

            bandana.hides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID, BANDANA_WEARABLE_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_WEARABLE_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BANDANA_WEARABLE_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByWrongGeneralHides()
        {
            var bandana = catalog.Get(BANDANA_WEARABLE_ID);

            bandana.hides = new [] { "NonExistentCategory" };
            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID, BANDANA_WEARABLE_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_WEARABLE_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BANDANA_WEARABLE_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_WEARABLE_ID);
            var bandana = catalog.Get(BANDANA_WEARABLE_ID);

            bandana.GetRepresentation(avatarModel.bodyShape).overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID, BANDANA_WEARABLE_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[SUNGLASSES_WEARABLE_ID]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[BANDANA_WEARABLE_ID]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByOverrideHides()
        {
            var sunglasses = catalog.Get(SUNGLASSES_WEARABLE_ID);
            var bandana = catalog.Get(BANDANA_WEARABLE_ID);

            bandana.GetRepresentation("dcl://base-avatars/BaseMale").overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID, BANDANA_WEARABLE_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = GetWearableControlled(SUNGLASSES_WEARABLE_ID);
            var bandanaController = GetWearableControlled(BANDANA_WEARABLE_ID);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeUnequipedProperly()
        {
            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            avatarModel.wearables = new List<string>() { };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            var wearableControllers = AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer);

            Assert.IsFalse(wearableControllers.ContainsKey(SUNGLASSES_WEARABLE_ID));
        }

        [UnityTest]
        public IEnumerator BeUnequipedProperlyMultipleTimes()
        {
            List<GameObject> containers = new List<GameObject>();

            for (int i = 0; i < 6; i++)
            {
                avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
                containers.Add(GetWearableControlled(SUNGLASSES_WEARABLE_ID)?.myAssetContainer);

                avatarModel.wearables = new List<string>() { };
                yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));
            }

            Assert.IsTrue(containers.All(x => x == null));
        }

        [UnityTest]
        public IEnumerator BeRetrievedWithoutPoolableObject()
        {
            List<GameObject> containers = new List<GameObject>();

            avatarModel.wearables = new List<string>() { SUNGLASSES_WEARABLE_ID, BANDANA_WEARABLE_ID };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesAssetContainer = GetWearableControlled(SUNGLASSES_WEARABLE_ID)?.myAssetContainer;
            var bandanaAssetContainer = GetWearableControlled(BANDANA_WEARABLE_ID)?.myAssetContainer;
            var sunglassesPoolableObject = sunglassesAssetContainer.GetComponentInChildren<PoolableObject>();
            var bandanaPoolableObject = bandanaAssetContainer.GetComponentInChildren<PoolableObject>();
            Assert.IsNull(sunglassesPoolableObject);
            Assert.IsNull(bandanaPoolableObject);
        }

        private WearableController_Mock GetWearableControlled(string id)
        {
            var wearableControllers = AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer);
            if (!wearableControllers.ContainsKey(id))
                return null;

            return new WearableController_Mock(wearableControllers[id]);
        }
    }
}