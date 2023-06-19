using DCL;
using DCL.ECSComponents;
using DCL.Helpers;
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;
using Vector3 = UnityEngine.Vector3;

namespace Tests
{
    public class MeshRendererVisualTests: ECSVisualTestsBase
    {
        private const string SNAPSHOT_BASE_FILENAME = "SDK7_MeshRendererVisualTests_";

        // Manually run to generate baseline image for later comparisons
        [UnityTest, VisualTest, Explicit]
        public IEnumerator VisualTest1_Generate() { yield return VisualTestUtils.GenerateBaselineForTest(VisualTest1()); }

        [UnityTest, VisualTest]
        public IEnumerator VisualTest1()
        {
            Vector3 cameraPos = (Vector3.up * 10) + (Vector3.back * 4);
            VisualTestUtils.RepositionVisualTestsCamera(camera, cameraPos, cameraPos + Vector3.forward);

            // Sphere
            var sphereEntity = scene.CreateEntity(666);
            sphereEntity.gameObject.transform.position = new Vector3(-3, 10, 0);
            var meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);
            PBMeshRenderer meshModel = new PBMeshRenderer() { Sphere = new PBMeshRenderer.Types.SphereMesh() };
            meshRendererHandler.OnComponentCreated(scene, sphereEntity);
            meshRendererHandler.OnComponentModelUpdated(scene, sphereEntity, meshModel);
            yield return null;

            // Cube
            var cubeEntity = scene.CreateEntity(667);
            cubeEntity.gameObject.transform.position = new Vector3(-1, 10, 0);
            meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);
            meshModel = new PBMeshRenderer() { Box = new PBMeshRenderer.Types.BoxMesh()};
            meshRendererHandler.OnComponentCreated(scene, cubeEntity);
            meshRendererHandler.OnComponentModelUpdated(scene, cubeEntity, meshModel);
            yield return null;

            // Plane
            var planeEntity = scene.CreateEntity(668);
            planeEntity.gameObject.transform.position = new Vector3(1, 10, 0);
            planeEntity.gameObject.transform.Rotate(Vector3.up, 75);
            meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);
            meshModel = new PBMeshRenderer() { Plane = new PBMeshRenderer.Types.PlaneMesh()};
            meshRendererHandler.OnComponentCreated(scene, planeEntity);
            meshRendererHandler.OnComponentModelUpdated(scene, planeEntity, meshModel);
            yield return null;

            // Cylinder
            var cylinderEntity = scene.CreateEntity(669);
            cylinderEntity.gameObject.transform.position = new Vector3(3, 10, 0);
            meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);
            meshModel = new PBMeshRenderer() { Cylinder = new PBMeshRenderer.Types.CylinderMesh()};
            meshRendererHandler.OnComponentCreated(scene, cylinderEntity);
            meshRendererHandler.OnComponentModelUpdated(scene, cylinderEntity, meshModel);
            yield return null;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);
        }
    }
}
