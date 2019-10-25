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
    }

    class WearableController_Mock : WearableController
    {
        public WearableController_Mock(WearableItem wearableItem, string bodyShapeType) : base(wearableItem, bodyShapeType) { }
        public WearableController_Mock(WearableController original) : base(original) { }

        public Renderer[] myAssetRenderers => assetRenderers;
    }

    public class WearableItemsShould : TestsBase
    {
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
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            avatarModel.wearables = new List<string>() { sunglassesId };

            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[sunglassesId]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByGeneralHides()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.hides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { sunglassesId, bandanaId };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[sunglassesId]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[bandanaId]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByWrongGeneralHides()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.hides = new [] { "NonExistentCategory" };
            avatarModel.wearables = new List<string>() { sunglassesId, bandanaId };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[sunglassesId]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[bandanaId]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator BeHiddenByOverrideHides()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.GetRepresentation(avatarModel.bodyShape).overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { sunglassesId, bandanaId };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[sunglassesId]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[bandanaId]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => !x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator NotBeHiddenByOverrideHides()
        {
            var sunglassesId = "dcl://base-avatars/black_sun_glasses";
            var sunglasses = catalog.Get(sunglassesId);
            var bandanaId = "dcl://base-avatars/blue_bandana";
            var bandana = catalog.Get(bandanaId);

            bandana.GetRepresentation("dcl://base-avatars/BaseMale").overrideHides = new [] { sunglasses.category };
            avatarModel.wearables = new List<string>() { sunglassesId, bandanaId };
            yield return avatarShape.ApplyChanges(JsonUtility.ToJson(avatarModel));

            var sunglassesController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[sunglassesId]);
            var bandanaController = new WearableController_Mock(AvatarRenderer_Mock.GetWearableControllers(avatarShape.avatarRenderer)[bandanaId]);
            Assert.IsTrue(sunglassesController.myAssetRenderers.All(x => x.enabled));
            Assert.IsTrue(bandanaController.myAssetRenderers.All(x => x.enabled));
        }
    }
}