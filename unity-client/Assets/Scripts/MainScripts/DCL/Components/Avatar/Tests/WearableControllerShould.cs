using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using WaitUntil = DCL.WaitUntil;

namespace AvatarShape_Tests
{
    public class WearableControllerShould : IntegrationTestSuite_Legacy
    {
        private const string SUNGLASSES_ID = "dcl://base-avatars/black_sun_glasses";

        private BaseDictionary<string, WearableItem> catalog;
        private Transform wearableHolder;
        private List<WearableController> toCleanUp = new List<WearableController>();

        [UnitySetUp]
        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            catalog = AvatarAssetsTestHelpers.CreateTestCatalogLocal();

            toCleanUp.Clear();
            wearableHolder = CreateTestGameObject("_Holder").transform;
        }

        [UnityTest]
        public IEnumerator LoadSuccessfully()
        {
            //Arrange
            catalog.TryGetValue(SUNGLASSES_ID, out WearableItem wearableItem);
            WearableController wearable = new WearableController(wearableItem);
            toCleanUp.Add(wearable);

            //Act
            bool succeeded = false;
            bool failed = false;
            wearable.Load(WearableLiterals.BodyShapes.FEMALE, wearableHolder, x => succeeded = true, x => failed = true);
            yield return new WaitUntil(() => succeeded || failed);

            //Assert
            Assert.IsTrue(succeeded);
            Assert.IsFalse(failed);
        }

        [UnityTest]
        public IEnumerator FailsGracefully()
        {
            //Arrange
            WearableItem unexistentWearableItem = new WearableItem
            {
                representations = new []{ new WearableItem.Representation
                {
                    mainFile = "NothingHere",
                    contents = new [] { new ContentServerUtils.MappingPair{file = "NothingHere", hash = "NothingHere"} },
                    bodyShapes = new [] {WearableLiterals.BodyShapes.FEMALE, WearableLiterals.BodyShapes.MALE}
                }}
            };
            WearableController wearable = new WearableController(unexistentWearableItem);
            toCleanUp.Add(wearable);

            //Act
            bool succeeded = false;
            bool failed = false;
            RendereableAssetLoadHelper.LoadingType cacheLoadingType = RendereableAssetLoadHelper.loadingType;
            RendereableAssetLoadHelper.loadingType = RendereableAssetLoadHelper.LoadingType.ASSET_BUNDLE_ONLY;
            wearable.Load(WearableLiterals.BodyShapes.FEMALE, wearableHolder, x => succeeded = true, x => failed = true);
            yield return new WaitUntil(() => succeeded || failed);
            RendereableAssetLoadHelper.loadingType = cacheLoadingType;

            //Assert
            Assert.IsFalse(succeeded);
            Assert.IsTrue(failed);
        }

        [UnityTest]
        public IEnumerator SetAnimatorBonesProperly()
        {
            //Arrange
            SkinnedMeshRenderer skinnedMeshRenderer = CreateTestGameObject("_SMR_Holder").AddComponent<SkinnedMeshRenderer>();
            skinnedMeshRenderer.rootBone = CreateTestGameObject("_rootBone").transform;
            skinnedMeshRenderer.bones = new Transform[5];
            for (var i = 0; i < skinnedMeshRenderer.bones.Length; i++)
            {
                skinnedMeshRenderer.bones[i] = CreateTestGameObject($"_rootBone_{i}").transform;
            }
            catalog.TryGetValue(SUNGLASSES_ID, out WearableItem wereableItem);
            WearableController wearable = new WearableController(wereableItem);
            toCleanUp.Add(wearable);
            wearable.Load(WearableLiterals.BodyShapes.FEMALE, wearableHolder, null, null);
            yield return new WaitUntil(() => wearable.isReady);

            //Act
            wearable.SetAnimatorBones(skinnedMeshRenderer);

            //Assert
            SkinnedMeshRenderer wearableSMR = wearable.assetContainer.GetComponentInChildren<SkinnedMeshRenderer>();
            Assert.AreEqual(skinnedMeshRenderer.rootBone, wearableSMR.rootBone);
            for (int index = 0; index < wearableSMR.bones.Length; index++)
            {
                Assert.AreEqual(skinnedMeshRenderer.bones[index], wearableSMR.bones[index]);
            }
        }

        [UnityTest]
        public IEnumerator UpdateVisibilityProperly_True()
        {
            //Arrange
            catalog.TryGetValue(SUNGLASSES_ID, out WearableItem wereableItem);
            WearableController wearable = new WearableController(wereableItem);
            toCleanUp.Add(wearable);
            wearable.Load(WearableLiterals.BodyShapes.FEMALE, wearableHolder, null, null);
            yield return new WaitUntil(() => wearable.isReady);
            SkinnedMeshRenderer skinnedMeshRenderer = wearable.assetContainer.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.enabled = false;

            //Act
            wearable.UpdateVisibility(new HashSet<string>());

            //Assert
            Assert.IsTrue(skinnedMeshRenderer.enabled);
        }

        [UnityTest]
        public IEnumerator UpdateVisibilityProperly_False()
        {
            //Arrange
            catalog.TryGetValue(SUNGLASSES_ID, out WearableItem wereableItem);
            WearableController wearable = new WearableController(wereableItem);
            toCleanUp.Add(wearable);
            wearable.Load(WearableLiterals.BodyShapes.FEMALE, wearableHolder, null, null);
            yield return new WaitUntil(() => wearable.isReady);
            SkinnedMeshRenderer skinnedMeshRenderer = wearable.assetContainer.GetComponentInChildren<SkinnedMeshRenderer>();
            skinnedMeshRenderer.enabled = true;

            //Act
            wearable.UpdateVisibility(new HashSet<string> {wearable.wearable.category});

            //Assert
            Assert.IsFalse(skinnedMeshRenderer.enabled);
        }

        protected override IEnumerator TearDown()
        {
            for (int index = toCleanUp.Count - 1; index >= 0; index--)
            {
                toCleanUp[index].CleanUp();
            }
            toCleanUp.Clear();
            return base.TearDown();
        }
    }
}
