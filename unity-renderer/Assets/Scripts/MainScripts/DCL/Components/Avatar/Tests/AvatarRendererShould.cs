using System;
using DCL;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

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

            var avatarShape = AvatarShapeTestHelpers.CreateAvatarShape(scene, avatarModel);
            yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);

            avatarRenderer = avatarShape.avatarRenderer;
        }

        [UnityTest]
        public IEnumerator FailGracefullyWhenIdsCannotBeResolved()
        {
            avatarModel.wearables = new List<string>() {"Scioli_right_arm", "Peron_hands"};
            avatarModel.bodyShape = "Invalid_id";

            avatarRenderer.SetVisibility(true);

            bool success = false;
            avatarRenderer.ApplyModel(avatarModel, () => success = true, null);
            yield return new DCL.WaitUntil(() => success, 4);

            LogAssert.Expect(LogType.Error, "Bodyshape Invalid_id not found in catalog");
            LogAssert.Expect(LogType.Error, "Wearable Scioli_right_arm not found in catalog");
            LogAssert.Expect(LogType.Error, "Wearable Peron_hands not found in catalog");

            UnityEngine.Assertions.Assert.IsTrue(success);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessVisibilityTrueWhenSetBeforeLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            avatarRenderer.SetVisibility(true);

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessVisibilityFalseWhenSetBeforeLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};
            avatarRenderer.SetVisibility(false);

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => !x.enabled));
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessVisibilityTrueWhenSetWhileLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            avatarRenderer.SetVisibility(true);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessVisibilityFalseWhenSetWhileLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            avatarRenderer.SetVisibility(false);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => !x.enabled));
        }


        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessVisibilityTrueWhenSetAfterLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);
            avatarRenderer.SetVisibility(true);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => x.enabled));
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessVisibilityFalseWhenSetAfterLoading()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID, BLUE_BANDANA_ID};

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);
            avatarRenderer.SetVisibility(false);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).myAssetRenderers.All(x => !x.enabled));
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessHideListProperly_HeadHidden()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);
            catalog.Get(SUNGLASSES_ID).hides = new [] { WearableLiterals.Misc.HEAD};

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID};

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsFalse(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).headRenderer.enabled);
            Assert.IsFalse(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).eyebrowsRenderer.enabled);
            Assert.IsFalse(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).eyesRenderer.enabled);
            Assert.IsFalse(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).mouthRenderer.enabled);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ProcessHideListProperly_HeadShowing()
        {
            //Clean hides/replaces to avoid interferences
            CleanWearableHidesAndReplaces(SUNGLASSES_ID);
            CleanWearableHidesAndReplaces(BLUE_BANDANA_ID);

            avatarModel.wearables = new List<string>() {SUNGLASSES_ID};

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).headRenderer.enabled);
            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).eyebrowsRenderer.enabled);
            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).eyesRenderer.enabled);
            Assert.IsTrue(AvatarRenderer_Mock.GetBodyShapeController(avatarRenderer).mouthRenderer.enabled);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        [TestCase(null, ExpectedResult = null)]
        [TestCase("wave", ExpectedResult = null)]
        public IEnumerator ProcessExpression(string expressionId)
        {
            var animator = avatarRenderer.animator;
            var timestamp = DateTime.UtcNow.Ticks;

            avatarModel.expressionTriggerId = expressionId;
            avatarModel.expressionTriggerTimestamp = timestamp;

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.AreEqual(animator.blackboard.expressionTriggerId, expressionId);
            Assert.AreEqual(animator.blackboard.expressionTriggerTimestamp, timestamp);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator ChangeWearablesRepresentationWhenBodyShapeChanges()
        {
            string wearableId = "dcl://base-avatars/MultipleRepresentations";
            avatarModel.wearables = new List<string> { wearableId };
            avatarModel.bodyShape = WearableLiterals.BodyShapes.MALE;

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            var wearableController = avatarRenderer.wearableControllers.Values.FirstOrDefault(x => x.id == wearableId);
            Assert.NotNull(wearableController);
            Assert.AreEqual("M_Feet_Espadrilles.glb" ,wearableController.lastMainFileLoaded);

            avatarModel.bodyShape = WearableLiterals.BodyShapes.FEMALE;
            ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            wearableController = avatarRenderer.wearableControllers.Values.FirstOrDefault(x => x.id == wearableId);
            Assert.NotNull(wearableController);
            Assert.AreEqual("Feet_XmasSockets.glb" ,wearableController.lastMainFileLoaded);
        }

        private void CleanWearableHidesAndReplaces(string id)
        {
            catalog.Get(id).hides = new string[] { };
            catalog.Get(id).replaces = new string[] { };
            foreach (WearableItem.Representation representation in catalog.Get(id).representations)
            {
                representation.overrideHides = new string[] { };
                representation.overrideReplaces = new string[] { };
            }
        }
    }

    public class AnimatorLegacyShould : TestsBase
    {
        private AvatarModel avatarModel;
        private AvatarRenderer avatarRenderer;
        private AvatarAnimatorLegacy animator;

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();

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

            AvatarAssetsTestHelpers.CreateTestCatalogLocal();

            var avatarShape = AvatarShapeTestHelpers.CreateAvatarShape(scene, avatarModel);
            yield return new DCL.WaitUntil(() => avatarShape.everythingIsLoaded, 20);

            avatarRenderer = avatarShape.avatarRenderer;
            animator = avatarRenderer.animator;
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator NotTriggerExpressionWithSameTimestamp()
        {
            avatarModel.expressionTriggerId = "wave";
            avatarModel.expressionTriggerTimestamp = 1;
            animator.blackboard.expressionTriggerTimestamp = 1;

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.True(animator.currentState != animator.State_Expression);
        }

        [UnityTest]
        [Category("Explicit")]
        [Explicit("Test too slow")]
        public IEnumerator TriggerExpressionWithSameTimestamp()
        {
            avatarModel.expressionTriggerId = "wave";
            avatarModel.expressionTriggerTimestamp = DateTime.UtcNow.Ticks;
            animator.blackboard.expressionTriggerTimestamp = -1;

            bool ready = false;
            avatarRenderer.ApplyModel(avatarModel, () => ready = true, null);
            yield return new DCL.WaitUntil(() => ready);

            Assert.True(animator.currentState == animator.State_Expression);
        }
    }
}