using System.Collections;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
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
            new RuntimeComponentFactory(Resources.Load ("RuntimeComponentFactory") as IPoolableComponentFactory),
            new SceneBoundsChecker() // Only used for GetOriginalMaterials(). We should remove this dependency on the future.
        );
    }

    protected override PlatformContext CreatePlatformContext()
    {
        return DCL.Tests.PlatformContextFactory.CreateWithGenericMocks(
            WebRequestController.Create(),
            new ServiceProviders());
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        scene = (ParcelScene)Environment.i.world.sceneController.CreateTestScene();
        scene.contentProvider = new ContentProvider_Dummy();
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
    }

    [UnityTest]
    public IEnumerator CountParametrizedShapes()
    {
        ConeShape coneShape = TestHelpers.SharedComponentCreate<ConeShape, ConeShape.Model>(
            scene,
            DCL.Models.CLASS_ID.CONE_SHAPE,
            new ConeShape.Model()
            {
                radiusTop = 1,
                radiusBottom = 0
            }
        );

        PlaneShape planeShape = TestHelpers.SharedComponentCreate<PlaneShape, PlaneShape.Model>(
            scene,
            DCL.Models.CLASS_ID.PLANE_SHAPE,
            new PlaneShape.Model()
            {
                height = 1.5f,
                width = 1
            }
        );

        IDCLEntity entity = TestHelpers.CreateSceneEntity(scene);
        TestHelpers.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.identity, Vector3.one);

        IDCLEntity entity2 = TestHelpers.CreateSceneEntity(scene);
        TestHelpers.SetEntityTransform(scene, entity2, Vector3.zero, Quaternion.identity, Vector3.one);

        TestHelpers.SharedComponentAttach(coneShape, entity);
        TestHelpers.SharedComponentAttach(planeShape, entity2);

        yield return new WaitForAllMessagesProcessed();

        AssertMetricsModel(scene,
            triangles: 105,
            materials: 1,
            entities: 2,
            meshes: 2,
            bodies: 2,
            textures: 0);

        TestHelpers.RemoveSceneEntity(scene, entity);
        TestHelpers.RemoveSceneEntity(scene, entity2);

        yield return new WaitForAllMessagesProcessed();

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
    }

    [UnityTest]
    public IEnumerator CountGLTFShapes()
    {
        IDCLEntity entity1 = TestHelpers.CreateSceneEntity(scene);
        GLTFShape entity1shape = TestHelpers.AttachGLTFShape(entity1,
            scene,
            new Vector3(8, 1, 8),
            new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            });

        yield return TestHelpers.WaitForGLTFLoad(entity1);

        IDCLEntity entity2 = TestHelpers.CreateSceneEntity(scene);
        GLTFShape entity2shape = TestHelpers.AttachGLTFShape(entity2,
            scene,
            new Vector3(8, 1, 8),
            new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            });

        yield return TestHelpers.WaitForGLTFLoad(entity2);

        yield return new WaitForAllMessagesProcessed();

        Debug.Log(scene.metricsController.GetModel());
        AssertMetricsModel(scene,
            triangles: 612,
            materials: 1,
            entities: 2,
            meshes: 1,
            bodies: 2,
            textures: 0);

        TestHelpers.RemoveSceneEntity(scene, entity1);
        TestHelpers.RemoveSceneEntity(scene, entity2);
        yield return new WaitForAllMessagesProcessed();

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
    }

    [UnityTest]
    public IEnumerator CountNFTShapes()
    {
        var entity = TestHelpers.CreateSceneEntity(scene);

        Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        CommonScriptableObjects.rendererState.Set(true);

        NFTShape component = TestHelpers.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        Debug.Log(scene.metricsController.GetModel());
        TestHelpers.SharedComponentAttach(component, entity);

        LoadWrapper_NFT wrapper = LoadableShape.GetLoaderForEntity(entity) as LoadWrapper_NFT;
        yield return new WaitUntil(() => wrapper.alreadyLoaded);

        Debug.Log(scene.metricsController.GetModel());
        AssertMetricsModel(scene,
            triangles: 190,
            materials: 6,
            entities: 1,
            meshes: 4,
            bodies: 4,
            textures: 0);

        TestHelpers.RemoveSceneEntity(scene, entity);
        yield return new WaitForAllMessagesProcessed();

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
    }

    [UnityTest]
    public IEnumerator NotCountWhenEntityIsExcluded()
    {
        yield break;
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

        TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
        TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(8, 1, 8));

        yield return new WaitForAllMessagesProcessed();

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