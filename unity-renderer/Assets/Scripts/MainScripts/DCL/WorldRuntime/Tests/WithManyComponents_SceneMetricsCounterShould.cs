using System.Collections;
using DCL.Components;
using DCL.Helpers;
using UnityEngine;
using UnityEngine.TestTools;

public class WithManyComponents_SceneMetricsCounterShould : IntegrationTestSuite_SceneMetricsCounter
{
    [UnityTest]
    public IEnumerator CountWhenStressed()
    {
        var coneShape = TestUtils.SharedComponentCreate<ConeShape, ConeShape.Model>(scene, DCL.Models.CLASS_ID.CONE_SHAPE, new ConeShape.Model()
        {
            radiusTop = 1,
            radiusBottom = 0
        });

        var planeShape = TestUtils.SharedComponentCreate<PlaneShape, PlaneShape.Model>(scene, DCL.Models.CLASS_ID.PLANE_SHAPE, new PlaneShape.Model()
        {
            height = 1.5f,
            width = 1
        });

        var shapeEntity = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(coneShape, shapeEntity);

        TestUtils.UpdateShape(scene, coneShape.id, JsonUtility.ToJson(new ConeShape.Model()
        {
            segmentsRadial = 180,
            segmentsHeight = 1.5f
        }));

        TestUtils.DetachSharedComponent(scene, shapeEntity.entityId, coneShape.id);
        TestUtils.SharedComponentAttach(planeShape, shapeEntity);

        yield return coneShape.routine;
        yield return planeShape.routine;
        yield return null;

        var lanternEntity = TestUtils.CreateSceneEntity(scene);
        var lanternShape = TestUtils.AttachGLTFShape(lanternEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
        });

        yield return TestUtils.WaitForGLTFLoad(lanternEntity);

        var shapeEntity2 = TestUtils.CreateSceneEntity(scene);
        var shape = TestUtils.AttachGLTFShape(shapeEntity2, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Shark/shark_anim.gltf"
        });
        yield return TestUtils.WaitForGLTFLoad(shapeEntity2);

        TestUtils.RemoveSceneEntity(scene, lanternEntity);
        yield return null;

        TestUtils.DetachSharedComponent(scene, shapeEntity2.entityId, shape.id);
        shape = TestUtils.AttachGLTFShape(shapeEntity2, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
        });

        yield return TestUtils.WaitForGLTFLoad(shapeEntity2);

        TestUtils.InstantiateEntityWithShape(scene, 1, DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
        TestUtils.InstantiateEntityWithShape(scene, 2, DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(8, 1, 8));

        yield return null;

        AssertMetricsModel(scene,
            triangles: 1126,
            materials: 1,
            entities: 4,
            meshes: 4,
            bodies: 4,
            textures: 1);

        TestUtils.RemoveSceneEntity(scene, 1);
        TestUtils.RemoveSceneEntity(scene, 2);
        TestUtils.RemoveSceneEntity(scene, shapeEntity2);

        AssertMetricsModel(scene,
            triangles: 4,
            materials: 0,
            entities: 1,
            meshes: 1,
            bodies: 1,
            textures: 0);

        yield return null;
    }
}