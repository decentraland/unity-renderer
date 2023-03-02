using System.Collections;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace SceneBoundariesCheckerTests
{
    public class SceneBoundariesCheckerTests_DebugMode : IntegrationTestSuite_Legacy
    {
        private ParcelScene scene;
        private CoreComponentsPlugin coreComponentsPlugin;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            scene = TestUtils.CreateTestScene() as ParcelScene;
            scene.isPersistent = false;
            coreComponentsPlugin = new CoreComponentsPlugin();
            
            DataStore.i.debugConfig.isDebugMode.Set(true);

            Environment.i.world.sceneBoundsChecker.SetFeedbackStyle(new SceneBoundsFeedbackStyle_RedBox());
            Environment.i.world.sceneBoundsChecker.timeBetweenChecks = 0f;

            UnityEngine.Assertions.Assert.IsTrue(Environment.i.world.sceneBoundsChecker.enabled);
            UnityEngine.Assertions.Assert.IsTrue(
                Environment.i.world.sceneBoundsChecker.GetFeedbackStyle() is SceneBoundsFeedbackStyle_RedBox);

            TestUtils_NFT.RegisterMockedNFTShape(Environment.i.world.componentFactory);
        }

        protected override IEnumerator TearDown()
        {
            coreComponentsPlugin.Dispose();
            yield return base.TearDown();
            DataStore.i.debugConfig.isDebugMode.Set(false);
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

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);

            yield return null;

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, true);
            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            yield return null;

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);

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

            LoadWrapper gltfShape2 = Environment.i.world.state.GetLoaderForEntity(entity2);

            yield return new UnityEngine.WaitUntil(() => gltfShape2.alreadyLoaded);
            yield return null;

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity2.meshesInfo, true);
        }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode() { yield return SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundariesDebugMode() { yield return SBC_Asserts.GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundaries(scene); }
        
        [UnityTest]
        public IEnumerator NFTShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode() { yield return SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene); }

        [UnityTest]
        public IEnumerator PShapeIsInvalidatedWhenLeavingBoundsDebugMode() { yield return SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
        public IEnumerator GLTFShapeIsInvalidatedWhenLeavingBoundsDebugMode() { yield return SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene); }

        [UnityTest]
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
        
        [UnityTest]
        public IEnumerator GLTFShapeRendererIsNotDisabledWhenInvalidatedInDebugMode()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(50, 1, 50) });

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded);
            yield return null;

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);
            
            Assert.IsTrue(entity.meshesInfo.renderers[0].enabled);
        }
        
        [UnityTest]
        public IEnumerator PShapeRendererIsNotDisabledWhenInvalidatedInDebugMode()
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(50, 1, 50));

            yield return null;
            yield return null;
            var entity = boxShape.attachedEntities.First();
            
            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);
            
            Assert.IsTrue(entity.meshesInfo.renderers[0].enabled);
        }
        
        [UnityTest]
        public IEnumerator NFTShapeRendererIsNotDisabledWhenInvalidatedInDebugMode()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            yield return component.routine;

            TestUtils.SharedComponentAttach(component, entity);

            LoadWrapper shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
            yield return new UnityEngine.WaitUntil(() => shapeLoader.alreadyLoaded);

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, true);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            yield return null;

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);
            
            Assert.IsTrue(entity.meshesInfo.renderers[0].enabled);
        }
    }
}