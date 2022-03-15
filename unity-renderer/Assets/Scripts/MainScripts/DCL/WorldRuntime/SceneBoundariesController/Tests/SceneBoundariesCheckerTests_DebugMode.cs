using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests_DebugMode : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene();

            Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedBox());
            Environment.i.world.sceneBoundsChecker.timeBetweenChecks = 0f;

            UnityEngine.Assertions.Assert.IsTrue(Environment.i.world.sceneBoundsChecker.enabled);
            UnityEngine.Assertions.Assert.IsTrue(
                Environment.i.world.sceneBoundsChecker.GetFeedbackStyle() is SceneBoundsFeedbackStyle_RedBox);
        }

        [UnityTest]
        public IEnumerator ResetMaterialCorrectlyWhenInvalidEntitiesAreRemoved()
        {
            var entity = TestUtils.CreateSceneEntity(scene);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });
            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));

            LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);

            yield return null;

            SBC_Asserts.AssertMeshIsValid(entity.meshesInfo);
            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            yield return null;

            SBC_Asserts.AssertMeshIsInvalid(entity.meshesInfo);

            TestUtils.RemoveSceneEntity(scene, entity.entityId);

            Environment.i.platform.parcelScenesCleaner.CleanMarkedEntities();

            yield return null;

            var entity2 = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity2, new DCLTransform.Model { position = new Vector3(8, 1, 8) });
            TestUtils.CreateAndSetShape(scene, entity2.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));

            LoadWrapper gltfShape2 = GLTFShape.GetLoaderForEntity(entity2);

            yield return new UnityEngine.WaitUntil(() => gltfShape2.alreadyLoaded);
            yield return null;

            SBC_Asserts.AssertMeshIsValid(entity2.meshesInfo);
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode() { yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode() { yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBoundsDebugMode() { yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBoundsDebugMode() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsInvalidatedWhenLeavingBoundsDebugMode() { yield return SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator PShapeIsResetWhenReenteringBoundsDebugMode() { yield return SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene); }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator NFTShapeIsResetWhenReenteringBoundsDebugMode() { yield return SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene); }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedDebugMode() { yield return SBC_Asserts.ChildShapeIsEvaluated(scene); }

        [UnityTest]
        public IEnumerator ChildShapeIsEvaluatedOnShapelessParentDebugMode() { yield return SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene); }

        [UnityTest]
        public IEnumerator HeightIsEvaluatedDebugMode() { yield return SBC_Asserts.HeightIsEvaluated(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsResetWhenReenteringBoundsDebugMode() { yield return SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene); }
    }
}