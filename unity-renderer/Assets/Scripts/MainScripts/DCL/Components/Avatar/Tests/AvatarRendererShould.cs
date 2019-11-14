using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace AvatarShape_Tests
{
    public class AvatarRendererShould : TestsBase
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";
        private const string BLUE_BANDANA_ID = "dcl://base-avatars/blue_bandana";

        private AvatarModel avatarModel;
        private WearableDictionary catalog;
        private AvatarRenderer avatarRenderer;

        [UnitySetUp]
        public IEnumerator SetUp()
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

            var avatarShape = AvatarTestHelpers.CreateAvatarShape(scene, avatarModel);
            yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);

            avatarRenderer = avatarShape.avatarRenderer;
        }

        [UnityTest]
        public IEnumerator ProcessVisibilityTrueWhenSetBeforeLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            avatarRenderer.SetVisibility(true);

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator ProcessVisibilityFalseWhenSetBeforeLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };
            avatarRenderer.SetVisibility(false);

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => !x.enabled));
        }

        [UnityTest]
        public IEnumerator ProcessVisibilityTrueWhenSetWhileLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            avatarRenderer.SetVisibility(true);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator ProcessVisibilityFalseWhenSetWhileLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            avatarRenderer.SetVisibility(false);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => !x.enabled));
        }


        [UnityTest]
        public IEnumerator ProcessVisibilityTrueWhenSetAfterLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);
            avatarRenderer.SetVisibility(true);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        public IEnumerator ProcessVisibilityFalseWhenSetAfterLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() { SUNGLASSES_ID, BLUE_BANDANA_ID };

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);
            avatarRenderer.SetVisibility(false);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => !x.enabled));
        }

        private void CleanWearableHidesAndReplaces(string id)
        {
            catalog.Get(id).hides = new string[] { };
            catalog.Get(id).replaces = new string[] { };
            foreach ( WearableItem.Representation representation in catalog.Get(id).representations)
            {
                representation.overrideHides = new string[] { };
                representation.overrideReplaces = new string[] { };
            }
        }
    }
}