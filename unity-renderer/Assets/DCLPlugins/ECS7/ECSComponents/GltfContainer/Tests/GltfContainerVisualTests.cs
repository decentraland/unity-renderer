using DCL;
using DCL.ECSComponents;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class GltfContainerVisualTests : ECSVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_GltfContainerVisualTests_";
        private ECS7TestEntity entity;
        private GltfContainerHandler handler;

        [SetUp]
        public override void SetUp()
        {
            base.SetUp();

            entity = scene.CreateEntity(23423);

            // Dcl shaders won't render anything below Y = 0 so the entity has to be repositioned
            entity.gameObject.transform.position += Vector3.up * 10;

            scene.contentProvider.baseUrl = $"{TestAssetsUtils.GetPath()}/GLB/";
            scene.contentProvider.fileToHash.Add("gltf-materials", "MaterialsScene.glb");
            scene.contentProvider.fileToHash.Add("palmtree", "PalmTree_01.glb");
            scene.contentProvider.fileToHash.Add("sharknado", "Shark/shark_anim.gltf");
            scene.contentProvider.fileToHash.Add("sci-fi-helmet", "DamagedHelmet/DamagedHelmet.glb");

            var renderersComponent = internalEcsComponents.renderersComponent;
            var pointerColliderComponent = internalEcsComponents.onPointerColliderComponent;
            var physicColliderComponent = internalEcsComponents.physicColliderComponent;
            var customLayerColliderComponent = internalEcsComponents.customLayerColliderComponent;
            var gltfContainerLoadingStateComponent = internalEcsComponents.GltfContainerLoadingStateComponent;
            var dataStoreEcs7 = new DataStore_ECS7();

            handler = new GltfContainerHandler(pointerColliderComponent,
                physicColliderComponent,
                customLayerColliderComponent,
                renderersComponent,
                gltfContainerLoadingStateComponent,
                dataStoreEcs7,
                new DataStore_FeatureFlag());

            handler.OnComponentCreated(scene, entity);
        }

        [TearDown]
        public override void TearDown()
        {
            AssetPromiseKeeper_GLTFast_Instance.i.Cleanup();
            handler.OnComponentRemoved(scene, entity);

            base.TearDown();
        }

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest1()
        {
            Vector3 cameraPos = entity.gameObject.transform.position + new Vector3(-8, 4, -8);
            VisualTestUtils.RepositionVisualTestsCamera(
                camera,
                cameraPos,
                cameraPos + Vector3.forward + (Vector3.down * 0.25f));

            entity.gameObject.transform.Rotate(-Vector3.left, 90);

            PBGltfContainer model = new PBGltfContainer() { Src = "gltf-materials" };
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.gltfLoader.Promise;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);
        }

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest2_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest2()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest2()
        {
            VisualTestUtils.RepositionVisualTestsCamera(
                camera,
                entity.gameObject.transform.position + new Vector3(-1.5f, 0, -5),
                entity.gameObject.transform.position);

            entity.gameObject.transform.Rotate(Vector3.up, 90);

            PBGltfContainer model = new PBGltfContainer() { Src = "sharknado" };
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.gltfLoader.Promise;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest2", camera);
        }

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest3_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest3()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest3()
        {
            VisualTestUtils.RepositionVisualTestsCamera(
                camera,
                entity.gameObject.transform.position + new Vector3(0, 2, -2),
                entity.gameObject.transform.position);

            entity.gameObject.transform.Rotate(Vector3.up, 124);

            PBGltfContainer model = new PBGltfContainer() { Src = "sci-fi-helmet" };
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.gltfLoader.Promise;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest3", camera);
        }

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest4_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest4()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest4()
        {
            VisualTestUtils.RepositionVisualTestsCamera(
                camera,
                entity.gameObject.transform.position + new Vector3(0, 10, -10),
                entity.gameObject.transform.position);

            PBGltfContainer model = new PBGltfContainer() { Src = "palmtree" };
            handler.OnComponentModelUpdated(scene, entity, model);
            yield return handler.gltfLoader.Promise;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest4", camera);
        }
    }
}
