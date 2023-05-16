using Cysharp.Threading.Tasks;
using System.Collections;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Threading.Tasks;
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

        [Test]
        public async Task ResetMaterialCorrectlyWhenInvalidEntitiesAreRemoved()
        {
            var entity = TestUtils.CreateSceneEntity(scene);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));

            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
            await UniTask.WaitUntil(() => gltfShape.alreadyLoaded);

            await UniTask.WaitForFixedUpdate();

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, true);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);

            TestUtils.RemoveSceneEntity(scene, entity.entityId);

            Environment.i.platform.parcelScenesCleaner.CleanMarkedEntities();

            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            var entity2 = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity2, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            TestUtils.CreateAndSetShape(scene, entity2.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));

            LoadWrapper gltfShape2 = Environment.i.world.state.GetLoaderForEntity(entity2);

            await UniTask.WaitUntil(() => gltfShape2.alreadyLoaded);
            await UniTask.WaitForFixedUpdate();

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity2.meshesInfo, true);
        }

        [Test]
        public async Task PShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            await SBC_Asserts.PShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [Test]
        public async Task GLTFShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            await SBC_Asserts.GLTFShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [Test]
        public async Task GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundariesDebugMode()
        {
            await SBC_Asserts.GLTFShapeCollidersCheckedWhenEvaluatingSceneInnerBoundaries(scene);
        }

        [Test]
        public async Task NFTShapeIsInvalidatedWhenStartingOutOfBoundsDebugMode()
        {
            await SBC_Asserts.NFTShapeIsInvalidatedWhenStartingOutOfBounds(scene);
        }

        [Test]
        public async Task PShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            await SBC_Asserts.PShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [Test]
        public async Task GLTFShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            await SBC_Asserts.GLTFShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [Test]
        public async Task NFTShapeIsInvalidatedWhenLeavingBoundsDebugMode()
        {
            await SBC_Asserts.NFTShapeIsInvalidatedWhenLeavingBounds(scene);
        }

        [Test]
        public async Task PShapeIsResetWhenReenteringBoundsDebugMode()
        {
            await SBC_Asserts.PShapeIsResetWhenReenteringBounds(scene);
        }

        [Test]
        [Explicit]
        [Category("Explicit")]
        public async Task NFTShapeIsResetWhenReenteringBoundsDebugMode()
        {
            await SBC_Asserts.NFTShapeIsResetWhenReenteringBounds(scene);
        }

        [Test]
        public async Task ChildShapeIsEvaluatedDebugMode()
        {
            await SBC_Asserts.ChildShapeIsEvaluated(scene);
        }

        [Test]
        public async Task ChildShapeIsEvaluatedOnShapelessParentDebugMode()
        {
            await SBC_Asserts.ChildShapeIsEvaluatedOnShapelessParent(scene);
        }

        [Test]
        public async Task HeightIsEvaluatedDebugMode()
        {
            await SBC_Asserts.HeightIsEvaluated(scene);
        }

        [Test]
        public async Task GLTFShapeIsResetWhenReenteringBoundsDebugMode()
        {
            await SBC_Asserts.GLTFShapeIsResetWhenReenteringBounds(scene);
        }

        [Test]
        public async Task GLTFShapeRendererIsNotDisabledWhenInvalidatedInDebugMode()
        {
            scene.isPersistent = true;
            var entity = TestUtils.CreateSceneEntity(scene);
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(50, 1, 50) });

            await UniTask.WaitForFixedUpdate(); // preliminary check
            await UniTask.WaitForFixedUpdate(); // immortal process catches up

            TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
                new
                {
                    src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
                }));
            LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);

            await UniTask.WaitUntil(() => gltfShape.alreadyLoaded);
            await UniTask.WaitForFixedUpdate(); // preliminary check
            await UniTask.WaitForFixedUpdate(); // immortal process catches up

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, true);

            Assert.IsTrue(entity.meshesInfo.renderers[0].enabled);
        }

        [Test]
        public async Task PShapeRendererIsNotDisabledWhenInvalidatedInDebugMode()
        {
            var boxShape = TestUtils.CreateEntityWithBoxShape(scene, new Vector3(50, 1, 50));

            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            var entity = boxShape.attachedEntities.First();

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);

            Assert.IsTrue(entity.meshesInfo.renderers[0].enabled);
        }

        [Test]
        public async Task NFTShapeRendererIsNotDisabledWhenInvalidatedInDebugMode()
        {
            var entity = TestUtils.CreateSceneEntity(scene);

            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(8, 1, 8) });

            var componentModel = new NFTShape.Model()
            {
                src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
            };

            NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            TestUtils.SharedComponentAttach(component, entity);

            LoadWrapper shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, true);

            // Move object to surpass the scene boundaries
            TestUtils.SetEntityTransform(scene, entity, new DCLTransform.Model { position = new Vector3(18, 1, 18) });

            await UniTask.WaitForFixedUpdate();
            await UniTask.WaitForFixedUpdate();

            SBC_Asserts.AssertMeshesAndCollidersValidState(entity.meshesInfo, false);

            Assert.IsTrue(entity.meshesInfo.renderers[0].enabled);
        }
    }
}
