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

        private MeshRendererHandler CreateMeshRenderer(ECS7TestEntity entity, PBMeshRenderer model)
        {
            var meshRendererHandler = new MeshRendererHandler(new DataStore_ECS7(),
                internalEcsComponents.texturizableComponent,
                internalEcsComponents.renderersComponent);
            meshRendererHandler.OnComponentCreated(scene, entity);
            meshRendererHandler.OnComponentModelUpdated(scene, entity, model);
            return meshRendererHandler;
        }

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
            var sphereRendererHandler = CreateMeshRenderer(sphereEntity, new PBMeshRenderer() { Sphere = new PBMeshRenderer.Types.SphereMesh() });
            yield return null;

            // Cube
            var cubeEntity = scene.CreateEntity(667);
            cubeEntity.gameObject.transform.position = new Vector3(-1, 10, 0);
            var cubeRendererHandler = CreateMeshRenderer(cubeEntity, new PBMeshRenderer() { Box = new PBMeshRenderer.Types.BoxMesh() });
            yield return null;

            // Plane
            var planeEntity = scene.CreateEntity(668);
            planeEntity.gameObject.transform.position = new Vector3(1, 10, 0);
            planeEntity.gameObject.transform.Rotate(Vector3.up, 75);
            var planeRendererHandler = CreateMeshRenderer(planeEntity, new PBMeshRenderer() { Plane = new PBMeshRenderer.Types.PlaneMesh() });
            yield return null;

            // Cylinder
            var cylinderEntity = scene.CreateEntity(669);
            cylinderEntity.gameObject.transform.position = new Vector3(3, 10, 0);
            var cylinderRendererHandler = CreateMeshRenderer(cylinderEntity, new PBMeshRenderer() { Cylinder = new PBMeshRenderer.Types.CylinderMesh() });
            yield return null;

            yield return VisualTestUtils.TakeSnapshot(SNAPSHOT_BASE_FILENAME + "VisualTest1", camera);

            sphereRendererHandler.OnComponentRemoved(scene, sphereEntity);
            cubeRendererHandler.OnComponentRemoved(scene, cubeEntity);
            planeRendererHandler.OnComponentRemoved(scene, planeEntity);
            cylinderRendererHandler.OnComponentRemoved(scene, cylinderEntity);
        }
    }
}
