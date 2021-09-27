using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;

public class SceneMetricsControllerShould : IntegrationTestSuite
{
    private ParcelScene scene;

    protected override WorldRuntimeContext CreateRuntimeContext()
    {
        return DCL.Tests.WorldRuntimeContextFactory.CreateWithGenericMocks
        (
            new SceneController(),
            new WorldState(),
            new RuntimeComponentFactory()
        );
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = Environment.i.world.sceneController.CreateTestScene() as ParcelScene;
    }

    [UnityTest]
    public IEnumerator CountMetricsCorrectlyWhenStressed()
    {
        var coneShape = TestHelpers.SharedComponentCreate<ConeShape, ConeShape.Model>(scene, DCL.Models.CLASS_ID.CONE_SHAPE, new ConeShape.Model()
        {
            radiusTop = 1,
            radiusBottom = 0
        });

        var planeShape = TestHelpers.SharedComponentCreate<PlaneShape, PlaneShape.Model>(scene, DCL.Models.CLASS_ID.PLANE_SHAPE, new PlaneShape.Model()
        {
            height = 1.5f,
            width = 1
        });

        var shapeEntity = TestHelpers.CreateSceneEntity(scene);
        TestHelpers.SetEntityTransform(scene, shapeEntity, Vector3.one, Quaternion.identity, Vector3.one);
        TestHelpers.SharedComponentAttach(coneShape, shapeEntity);

        TestHelpers.UpdateShape(scene, coneShape.id, JsonUtility.ToJson(new ConeShape.Model()
        {
            segmentsRadial = 180,
            segmentsHeight = 1.5f
        }));

        TestHelpers.DetachSharedComponent(scene, shapeEntity.entityId, coneShape.id);
        TestHelpers.SharedComponentAttach(planeShape, shapeEntity);

        yield return new WaitForAllMessagesProcessed();
        yield return null;

        var lanternEntity = TestHelpers.CreateSceneEntity(scene);
        var lanternShape = TestHelpers.AttachGLTFShape(lanternEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
        });

        yield return TestHelpers.WaitForGLTFLoad(lanternEntity);

        var shapeEntity2 = TestHelpers.CreateSceneEntity(scene);
        var shape = TestHelpers.AttachGLTFShape(shapeEntity2, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Shark/shark_anim.gltf"
        });
        yield return TestHelpers.WaitForGLTFLoad(shapeEntity2);

        TestHelpers.RemoveSceneEntity(scene, lanternEntity);
        yield return null;

        TestHelpers.DetachSharedComponent(scene, shapeEntity2.entityId, shape.id);
        shape = TestHelpers.AttachGLTFShape(shapeEntity2, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
        });
        yield return TestHelpers.WaitForGLTFLoad(shapeEntity2);
        Debug.Log(scene.metricsController.GetModel());

        TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
        TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(8, 1, 8));

        yield return new WaitForAllMessagesProcessed();
        Debug.Log(scene.metricsController.GetModel());

        AssertMetricsModel(scene,
            triangles: 1126,
            materials: 2,
            entities: 4,
            meshes: 4,
            bodies: 4,
            textures: 0);

        TestHelpers.RemoveSceneEntity(scene, "1");
        TestHelpers.RemoveSceneEntity(scene, "2");
        TestHelpers.RemoveSceneEntity(scene, shapeEntity2);

        yield return null;

        AssertMetricsModel(scene,
            triangles: 4,
            materials: 1,
            entities: 1,
            meshes: 1,
            bodies: 1,
            textures: 0);

        Environment.i.world.sceneController.UnloadAllScenes();
        yield return null;
    }

    void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies,
        int textures)
    {
        SceneMetricsModel inputModel = scene.metricsController.GetModel();

        Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: " + triangles);
        Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
        Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
        Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
        Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
        Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
    }
}