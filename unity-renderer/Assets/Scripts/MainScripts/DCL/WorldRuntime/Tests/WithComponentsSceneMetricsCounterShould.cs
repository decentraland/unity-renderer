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

public class WithComponentsSceneMetricsCounterShould : IntegrationTestSuite
{
    private ParcelScene scene;

    private readonly string[] texturePaths =
    {
        "/Images/alphaTexture.png",
        "/Images/atlas.png",
        "/Images/avatar.png",
        "/Images/avatar2.png"
    };

    protected override void InitializeServices(ServiceLocator serviceLocator)
    {
        serviceLocator.Register<IWorldState>(() => new WorldState());
        serviceLocator.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory(Resources.Load ("RuntimeComponentFactory") as IPoolableComponentFactory));
        serviceLocator.Register<IWebRequestController>(WebRequestController.Create);
        serviceLocator.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
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

    [UnityTearDown]
    protected override IEnumerator TearDown()
    {
        scene.Cleanup(true);
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator CountParametrizedShapesWhenRemoved()
    {
        ConeShape coneShape = CreateCone();
        PlaneShape planeShape = CreatePlane();
        DCLTexture dclTexture = CreateTexture(texturePaths[0]);
        BasicMaterial basicMaterial = CreateBasicMaterial(dclTexture.id);

        IDCLEntity entity = CreateEntityWithTransform();
        IDCLEntity entity2 = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(dclTexture, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity2);
        TestUtils.SharedComponentAttach(coneShape, entity);
        TestUtils.SharedComponentAttach(planeShape, entity2);

        yield return basicMaterial.routine;

        TestUtils.RemoveSceneEntity(scene, entity);
        TestUtils.RemoveSceneEntity(scene, entity2);
        dclTexture.Dispose();

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
    public IEnumerator CountParametrizedShapesWhenAdded()
    {
        ConeShape coneShape = CreateCone();
        PlaneShape planeShape = CreatePlane();
        DCLTexture dclTexture = CreateTexture(texturePaths[0]);
        BasicMaterial basicMaterial = CreateBasicMaterial(dclTexture.id);

        IDCLEntity entity = CreateEntityWithTransform();
        IDCLEntity entity2 = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(basicMaterial, entity);
        TestUtils.SharedComponentAttach(basicMaterial, entity2);
        TestUtils.SharedComponentAttach(coneShape, entity);
        TestUtils.SharedComponentAttach(planeShape, entity2);

        yield return basicMaterial.routine;

        AssertMetricsModel(scene,
            triangles: 105,
            materials: 1,
            entities: 2,
            meshes: 2,
            bodies: 2,
            textures: 1);
    }

    [UnityTest]
    public IEnumerator CountSameTextureOnManyBasicMaterials()
    {
        var texture = CreateTexture(texturePaths[0]);

        List<BasicMaterial> materials = new List<BasicMaterial>();
        List<IDCLEntity> entities = new List<IDCLEntity>();

        PlaneShape planeShape = CreatePlane();

        yield return planeShape.routine;
        yield return texture.routine;

        for ( int i = 0; i < 10; i++ )
        {
            materials.Add( CreateBasicMaterial(texture.id) );
            entities.Add( CreateEntityWithTransform());
            TestUtils.SharedComponentAttach(planeShape, entities[i]);
            TestUtils.SharedComponentAttach(materials[i], entities[i]);
            TestUtils.SharedComponentAttach(texture, entities[i]);
            yield return materials[i].routine;
        }

        SceneMetricsModel inputModel = scene.metricsCounter.currentCount;

        Assert.That( inputModel.materials, Is.EqualTo(10) );
        Assert.That( inputModel.textures, Is.EqualTo(1) );

        for ( int i = 0; i < 10; i++ )
        {
            materials[i].Dispose();
        }

        texture.Dispose();
        planeShape.Dispose();

        inputModel = scene.metricsCounter.currentCount;

        Assert.That( inputModel.materials, Is.EqualTo(0) );
        Assert.That( inputModel.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator CountManyAttachedTexturesOnSingleBasicMaterial()
    {
        var material = CreateBasicMaterial("");

        List<DCLTexture> textures = new List<DCLTexture>();
        SceneMetricsModel sceneMetrics;

        for ( int i = 0; i < 10; i++ )
        {
            textures.Add( CreateTexture(texturePaths[0]));
            yield return textures[i].routine;
        }

        IDCLEntity entity = CreateEntityWithTransform();
        TestUtils.SharedComponentAttach(material, entity);

        PlaneShape planeShape = CreatePlane();
        TestUtils.SharedComponentAttach(planeShape, entity);

        for ( int i = 0; i < 10; i++ )
        {
            TestUtils.SharedComponentAttach(textures[i], entity);

            yield return TestUtils.SharedComponentUpdate(material, new BasicMaterial.Model()
            {
                texture = textures[i].id
            });

            yield return null;

            sceneMetrics = scene.metricsCounter.currentCount;

            Assert.That( sceneMetrics.materials, Is.EqualTo(1) );
            Assert.That( sceneMetrics.textures, Is.EqualTo(1) );
        }

        foreach ( var texture in textures )
        {
            texture.Dispose();
        }

        material.Dispose();

        yield return null;

        sceneMetrics = scene.metricsCounter.currentCount;

        Debug.Log("SceneMetrics = " + sceneMetrics);

        Assert.That( sceneMetrics.materials, Is.EqualTo(0) );
        Assert.That( sceneMetrics.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator NotCountMaterialsAndTexturesWhenNoShapeIsPresent()
    {
        var texture1 = CreateTexture(texturePaths[0]);
        var material1 = CreatePBRMaterial(texture1.id, texture1.id, texture1.id, texture1.id);

        yield return texture1.routine;
        yield return material1.routine;

        Assert.That( scene.metricsCounter.currentCount.materials, Is.EqualTo(0) );
        Assert.That( scene.metricsCounter.currentCount.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator NotCountIdenticalTexturesWhenManyTextureComponentsAreAdded()
    {
        var texture1 = CreateTexture(texturePaths[0]);
        var texture2 = CreateTexture(texturePaths[0]);
        var texture3 = CreateTexture(texturePaths[0]);
        var texture4 = CreateTexture(texturePaths[0]);
        var material1 = CreatePBRMaterial(texture1.id, texture2.id, texture3.id, texture4.id);
        var planeShape = CreatePlane();
        var entity = CreateEntityWithTransform();

        TestUtils.SharedComponentAttach(planeShape, entity);
        TestUtils.SharedComponentAttach(material1, entity);
        TestUtils.SharedComponentAttach(texture1, entity);
        TestUtils.SharedComponentAttach(texture2, entity);
        TestUtils.SharedComponentAttach(texture3, entity);
        TestUtils.SharedComponentAttach(texture4, entity);

        yield return material1.routine;

        Assert.That( scene.metricsCounter.currentCount.textures, Is.EqualTo(1) );

        texture1.Dispose();
        texture2.Dispose();
        texture3.Dispose();
        texture4.Dispose();
        material1.Dispose();
        planeShape.Dispose();

        yield return null;

        Assert.That( scene.metricsCounter.currentCount.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator CountManyAttachedTexturesOnSinglePBRMaterial()
    {
        SceneMetricsModel sceneMetrics = null;

        var texture1 = CreateTexture(texturePaths[0]);
        var texture2 = CreateTexture(texturePaths[1]);
        var texture3 = CreateTexture(texturePaths[2]);
        var texture4 = CreateTexture(texturePaths[3]);
        var planeShape = CreatePlane();
        var entity = CreateEntityWithTransform();
        var material1 = CreatePBRMaterial(texture1.id, texture2.id, texture3.id, texture4.id);

        TestUtils.SharedComponentAttach(planeShape, entity);
        TestUtils.SharedComponentAttach(material1, entity);
        TestUtils.SharedComponentAttach(texture1, entity);
        TestUtils.SharedComponentAttach(texture2, entity);
        TestUtils.SharedComponentAttach(texture3, entity);
        TestUtils.SharedComponentAttach(texture4, entity);

        yield return material1.routine;

        sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(1) );
        Assert.That( sceneMetrics.textures, Is.EqualTo(4) );

        texture1.Dispose();
        texture2.Dispose();
        texture3.Dispose();
        texture4.Dispose();
        material1.Dispose();
        planeShape.Dispose();

        yield return null;

        sceneMetrics = scene.metricsCounter.currentCount;

        Assert.That( sceneMetrics.materials, Is.EqualTo(0) );
        Assert.That( sceneMetrics.textures, Is.EqualTo(0) );
    }

    [UnityTest]
    public IEnumerator CountBasicMaterialWhenAdded() { yield break; }

    [UnityTest]
    public IEnumerator CountBasicMaterialWhenRemoved() { yield break; }

    [UnityTest]
    public IEnumerator CountPBRMaterialWhenAdded() { yield break; }

    [UnityTest]
    public IEnumerator CountPBRMaterialWhenRemoved() { yield break; }

    [UnityTest]
    public IEnumerator CountTextureWhenAdded() { yield break; }

    [UnityTest]
    public IEnumerator CountTextureWhenRemoved() { yield break; }

    [UnityTest]
    public IEnumerator CountGLTFShapesWhenAdded()
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

        yield return null;

        AssertMetricsModel(scene,
            triangles: 612,
            materials: 1,
            entities: 2,
            meshes: 1,
            bodies: 2,
            textures: 1);

        yield return null;
    }

    [UnityTest]
    public IEnumerator CountGLTFShapesWhenRemoved()
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

        entity1shape.Dispose();
        entity2shape.Dispose();

        yield return null;

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CountNFTShapes()
    {
        var entity = TestUtils.CreateSceneEntity(scene);

        Assert.IsTrue(entity.meshRootGameObject == null, "entity mesh object should be null as the NFTShape hasn't been initialized yet");

        var componentModel = new NFTShape.Model()
        {
            src = "ethereum://0x06012c8cf97BEaD5deAe237070F9587f8E7A266d/558536"
        };

        NFTShape component = TestUtils.SharedComponentCreate<NFTShape, NFTShape.Model>(scene, CLASS_ID.NFT_SHAPE, componentModel);
        Debug.Log(scene.metricsCounter.currentCount);
        TestUtils.SharedComponentAttach(component, entity);

        LoadWrapper_NFT wrapper = LoadableShape.GetLoaderForEntity(entity) as LoadWrapper_NFT;
        yield return new WaitUntil(() => wrapper.alreadyLoaded);

        Debug.Log(scene.metricsCounter.currentCount);
        AssertMetricsModel(scene,
            triangles: 190,
            materials: 6,
            entities: 1,
            meshes: 4,
            bodies: 4,
            textures: 0);

        component.Dispose();

        yield return null;

        AssertMetricsModel(scene,
            triangles: 0,
            materials: 0,
            entities: 0,
            meshes: 0,
            bodies: 0,
            textures: 0);
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

        TestUtils.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
        TestUtils.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(8, 1, 8));

        yield return null;

        AssertMetricsModel(scene,
            triangles: 1126,
            materials: 2,
            entities: 4,
            meshes: 4,
            bodies: 4,
            textures: 1);

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

        yield return null;
    }

    PlaneShape CreatePlane()
    {
        PlaneShape planeShape = TestUtils.SharedComponentCreate<PlaneShape, PlaneShape.Model>(
            scene,
            DCL.Models.CLASS_ID.PLANE_SHAPE,
            new PlaneShape.Model()
            {
                height = 1.5f,
                width = 1
            }
        );
        return planeShape;
    }

    ConeShape CreateCone()
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
        return coneShape;
    }

    PBRMaterial CreatePBRMaterial( string albedoTexId, string alphaTexId, string bumpTexId, string emissiveTexId )
    {
        PBRMaterial basicMaterial = TestUtils.SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(
            scene,
            DCL.Models.CLASS_ID.PBR_MATERIAL,
            new PBRMaterial.Model()
            {
                albedoTexture = albedoTexId,
                alphaTexture = alphaTexId,
                bumpTexture = bumpTexId,
                emissiveTexture = emissiveTexId
            });
        return basicMaterial;
    }

    BasicMaterial CreateBasicMaterial( string textureId )
    {
        BasicMaterial basicMaterial = TestUtils.SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(
            scene,
            DCL.Models.CLASS_ID.BASIC_MATERIAL,
            new BasicMaterial.Model()
            {
                texture = textureId
            });
        return basicMaterial;
    }

    DCLTexture CreateTexture( string path )
    {
        return TestUtils.CreateDCLTexture(scene, TestAssetsUtils.GetPath() + path);
    }

    IDCLEntity CreateEntityWithTransform()
    {
        IDCLEntity entity = TestUtils.CreateSceneEntity(scene);
        TestUtils.SetEntityTransform(scene, entity, Vector3.one, Quaternion.identity, Vector3.one);
        return entity;
    }

    void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies,
        int textures)
    {
        SceneMetricsModel inputModel = scene.metricsCounter.currentCount;

        Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: " + triangles);
        Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
        Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
        Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
        Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
        Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
    }
}