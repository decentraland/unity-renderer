using System;
using System.Collections;
using System.Collections.Generic;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using Tests;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;

public class SceneMetricsCounterShould : IntegrationTestSuite
{
    private ParcelScene scene;

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        //serviceLocator.Register<ISceneController>(() => new SceneController());
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory(Resources.Load ("RuntimeComponentFactory") as IPoolableComponentFactory));
        //serviceLocator.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
        //serviceLocator.Register<IServiceProviders>(() => new ServiceProviders());
    }

    [UnitySetUp]
    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();

        scene = TestUtils.CreateTestScene();
        scene.contentProvider = new ContentProvider_Dummy();
        DCL.Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;

        // TODO(Brian): Move these variants to a DataStore object to avoid having to reset them
        //              like this.
        CommonScriptableObjects.isFullscreenHUDOpen.Set(false);
        CommonScriptableObjects.rendererState.Set(true);
    }

    [Test]
    public void CountEntitiesWhenAddedAndRemoved()
    {
        HashSet<string> entities = new HashSet<string>();
        entities.Add("1");
        var sceneMetricsCounter = new SceneMetricsCounter(new DataStore_WorldObjects(), "1", Vector2Int.zero, 10, () => entities);
        sceneMetricsCounter.Enable();

        Assert.That( sceneMetricsCounter.model.entities, Is.EqualTo(1));

        entities.Add("2");
        entities.Add("3");
        entities.Add("4");
        entities.Add("5");
        entities.Add("6");
        entities.Add("7");
        entities.Add("8");
        entities.Add("9");
        entities.Add("10");

        Assert.That( sceneMetricsCounter.model.entities, Is.EqualTo(10));

        entities.Remove("5");

        Assert.That( sceneMetricsCounter.model.entities, Is.EqualTo(9));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountUniqueMeshesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10, () => new HashSet<string>());
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Mesh mesh1 = new Mesh();
        Mesh mesh2 = new Mesh();
        Mesh mesh3 = new Mesh();
        Mesh mesh4 = new Mesh();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.meshes.Add(mesh1);
        rendereable1.meshes.Add(mesh2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.meshes.Add(mesh3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.meshes.Add(mesh4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.meshes, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.meshes, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.meshes, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(mesh1);
        UnityEngine.Object.Destroy(mesh2);
        UnityEngine.Object.Destroy(mesh3);
        UnityEngine.Object.Destroy(mesh4);
    }

    [Test]
    public void CountUniqueMaterialsWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10, () => new HashSet<string>());
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Material mat1 = new Material(Shader.Find("Standard"));
        Material mat2 = new Material(Shader.Find("Standard"));
        Material mat3 = new Material(Shader.Find("Standard"));
        Material mat4 = new Material(Shader.Find("Standard"));

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.materials.Add(mat1);
        rendereable1.materials.Add(mat2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.materials.Add(mat3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.materials.Add(mat4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.materials, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.materials, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.materials, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(mat1);
        UnityEngine.Object.Destroy(mat2);
        UnityEngine.Object.Destroy(mat3);
        UnityEngine.Object.Destroy(mat4);
    }

    [Test]
    public void CountUniqueTexturesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10, () => new HashSet<string>());
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Texture2D tex1 = new Texture2D(1, 1);
        Texture2D tex2 = new Texture2D(1, 1);
        Texture2D tex3 = new Texture2D(1, 1);
        Texture2D tex4 = new Texture2D(1, 1);

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.textures.Add(tex1);
        rendereable1.textures.Add(tex2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.textures.Add(tex3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.textures.Add(tex4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.textures, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.textures, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.textures, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(tex1);
        UnityEngine.Object.Destroy(tex2);
        UnityEngine.Object.Destroy(tex3);
        UnityEngine.Object.Destroy(tex4);
    }

    [Test]
    public void CountTotalTrianglesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10, () => new HashSet<string>());
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.totalTriangleCount = 30;

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.totalTriangleCount = 60;

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.triangles, Is.EqualTo(30));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.triangles, Is.EqualTo(20));

        sceneMetricsCounter.Dispose();
    }

    [Test]
    public void CountBodiesWhenAddedAndRemoved()
    {
        DataStore_WorldObjects dataStore = new DataStore_WorldObjects();
        var sceneMetricsCounter = new SceneMetricsCounter(dataStore, "1", Vector2Int.zero, 10, () => new HashSet<string>());
        sceneMetricsCounter.Enable();

        dataStore.sceneData.Add("1", new DataStore_WorldObjects.SceneData());
        dataStore.sceneData.Add("2", new DataStore_WorldObjects.SceneData());

        Renderer rend1 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend2 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend3 = new GameObject("Test").AddComponent<MeshRenderer>();
        Renderer rend4 = new GameObject("Test").AddComponent<MeshRenderer>();

        Rendereable rendereable1 = new Rendereable();
        rendereable1.ownerId = "A";
        rendereable1.renderers.Add(rend1);
        rendereable1.renderers.Add(rend2);

        Rendereable rendereable2 = new Rendereable();
        rendereable2.ownerId = "B";
        rendereable2.renderers.Add(rend3);

        Rendereable rendereable3 = new Rendereable();
        rendereable3.ownerId = "C";
        rendereable3.renderers.Add(rend4);

        dataStore.AddRendereable("1", rendereable1);
        dataStore.AddRendereable("1", rendereable2);
        dataStore.AddRendereable("2", rendereable3);

        Assert.That( sceneMetricsCounter.model.bodies, Is.EqualTo(3));

        dataStore.RemoveRendereable("1", rendereable2);

        Assert.That( sceneMetricsCounter.model.bodies, Is.EqualTo(2));

        dataStore.RemoveRendereable("1", rendereable1);

        Assert.That( sceneMetricsCounter.model.bodies, Is.EqualTo(0));

        sceneMetricsCounter.Dispose();

        UnityEngine.Object.Destroy(rend1.gameObject);
        UnityEngine.Object.Destroy(rend2.gameObject);
        UnityEngine.Object.Destroy(rend3.gameObject);
        UnityEngine.Object.Destroy(rend4.gameObject);
    }

    [UnityTest]
    public IEnumerator CountParametrizedShapes()
    {
        ConeShape coneShape = TestUtils.SharedComponentCreate<ConeShape, ConeShape.Model>(
            scene,
            DCL.Models.CLASS_ID.CONE_SHAPE,
            new ConeShape.Model()
            {
                radiusTop = 1,
                radiusBottom = 0
            }
        );

        PlaneShape planeShape = TestUtils.SharedComponentCreate<PlaneShape, PlaneShape.Model>(
            scene,
            DCL.Models.CLASS_ID.PLANE_SHAPE,
            new PlaneShape.Model()
            {
                height = 1.5f,
                width = 1
            }
        );


        DCLTexture dclTexture = TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + "/Images/avatar.png");

        BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
            scene,
            DCL.Models.CLASS_ID.BASIC_MATERIAL,
            new BasicMaterial.Model()
            {
                texture = dclTexture.id
            });

        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        TestUtils.SetEntityTransform(scene, entity, Vector3.zero, Quaternion.identity, Vector3.one);

        IDCLEntity entity2 = TestUtils.CreateSceneEntity(scene);
        TestUtils.SetEntityTransform(scene, entity2, Vector3.zero, Quaternion.identity, Vector3.one);

        TestUtils.SharedComponentAttach(basicMaterial, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity2);
        TestUtils.SharedComponentAttach(coneShape, entity);
        TestUtils.SharedComponentAttach(planeShape, entity2);

        yield return new WaitForAllMessagesProcessed();

        AssertMetricsModel(scene,
            triangles: 105,
            materials: 1,
            entities: 2,
            meshes: 2,
            bodies: 2,
            textures: 1);

        TestUtils.RemoveSceneEntity(scene, entity);
        TestUtils.RemoveSceneEntity(scene, entity2);

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
        IDCLEntity entity1 = TestUtils.CreateSceneEntity(scene);
        GLTFShape entity1shape = TestUtils.AttachGLTFShape(entity1,
            scene,
            new Vector3(8, 1, 8),
            new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            });

        yield return TestUtils.WaitForGLTFLoad(entity1);

        IDCLEntity entity2 = TestUtils.CreateSceneEntity(scene);
        GLTFShape entity2shape = TestUtils.AttachGLTFShape(entity2,
            scene,
            new Vector3(8, 1, 8),
            new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            });

        yield return TestUtils.WaitForGLTFLoad(entity2);

        yield return new WaitForAllMessagesProcessed();

        Debug.Log(scene.metricsCounter.GetModel());
        AssertMetricsModel(scene,
            triangles: 612,
            materials: 1,
            entities: 2,
            meshes: 1,
            bodies: 2,
            textures: 1);

        TestUtils.RemoveSceneEntity(scene, entity1);
        TestUtils.RemoveSceneEntity(scene, entity2);
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
        var entity = TestUtils.CreateSceneEntity(scene);

        Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        Debug.Log(scene.metricsCounter.GetModel());
        TestUtils.SharedComponentAttach(component, entity);

        LoadWrapper_NFT wrapper = LoadableShape.GetLoaderForEntity(entity) as LoadWrapper_NFT;
        yield return new WaitUntil(() => wrapper.alreadyLoaded);

        Debug.Log(scene.metricsCounter.GetModel());
        AssertMetricsModel(scene,
            triangles: 190,
            materials: 6,
            entities: 1,
            meshes: 4,
            bodies: 4,
            textures: 0);

        TestUtils.RemoveSceneEntity(scene, entity);
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

        var shapeEntity = TestUtils.CreateSceneEntity(scene);
        TestUtils.SetEntityTransform(scene, shapeEntity, Vector3.one, Quaternion.identity, Vector3.one);
        TestUtils.SharedComponentAttach(coneShape, shapeEntity);

        TestUtils.UpdateShape(scene, coneShape.id, JsonUtility.ToJson(new ConeShape.Model()
        {
            segmentsRadial = 180,
            segmentsHeight = 1.5f
        }));

        TestUtils.DetachSharedComponent(scene, shapeEntity.entityId, coneShape.id);
        TestUtils.SharedComponentAttach(planeShape, shapeEntity);

        yield return new WaitForAllMessagesProcessed();
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

        TestUtils.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
        TestUtils.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(8, 1, 8));

        yield return new WaitForAllMessagesProcessed();

        AssertMetricsModel(scene,
            triangles: 1126,
            materials: 2,
            entities: 4,
            meshes: 4,
            bodies: 4,
            textures: 0);

        TestUtils.RemoveSceneEntity(scene, "1");
        TestUtils.RemoveSceneEntity(scene, "2");
        TestUtils.RemoveSceneEntity(scene, shapeEntity2);

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
        SceneMetricsModel inputModel = scene.metricsCounter.GetModel();

        Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: " + triangles);
        Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
        Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
        Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
        Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
        Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
    }
}