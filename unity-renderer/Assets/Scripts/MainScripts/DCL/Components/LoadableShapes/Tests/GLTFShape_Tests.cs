using DCL;
using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class GLTFShape_Tests : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
    }

    [UnityTest]
    public IEnumerator ShapeUpdate()
    {
        string entityId = "1";
        TestUtils.CreateSceneEntity(scene, entityId);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
    }

    [Test]
    public void CustomContentProvider()
    {
        string entityId = "1";
        TestUtils.CreateSceneEntity(scene, entityId);

        string mockupAssetId = "cdd5a4ea94388dd21babdecd26dd560f739dce2fbb8c99cc10a45bb8306b6076";
        string mockupKey = "key";
        string mockupValue = "Value";

        SceneAssetPack sceneAssetPack = new SceneAssetPack();
        sceneAssetPack.assets = new System.Collections.Generic.List<SceneObject>();
        sceneAssetPack.id = "mockupId";

        SceneObject sceneObject = new SceneObject();
        sceneObject.id = mockupAssetId;
        sceneObject.contents = new System.Collections.Generic.Dictionary<string, string>();
        sceneObject.contents.Add(mockupKey, mockupValue);

        sceneAssetPack.assets.Add(sceneObject);

        var catalog = TestUtils.CreateComponentWithGameObject<AssetCatalogBridge>("AssetCatalogBridge");
        catalog.AddSceneAssetPackToCatalog(sceneAssetPack);

        TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                assetId = mockupAssetId,
                src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
            }));


        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);

        Assert.IsTrue(gltfShape is LoadWrapper_GLTF);

        LoadWrapper_GLTF gltfWrapper = (LoadWrapper_GLTF) gltfShape;
        ContentProvider customContentProvider = AssetCatalogBridge.i.GetContentProviderForAssetIdInSceneObjectCatalog(mockupAssetId);
        Assert.AreEqual(customContentProvider.baseUrl, gltfWrapper.customContentProvider.baseUrl);
        Assert.AreEqual(mockupKey, gltfWrapper.customContentProvider.contents[0].file);
        Assert.AreEqual(mockupValue, gltfWrapper.customContentProvider.contents[0].hash);

        Object.Destroy( catalog.gameObject );
    }

    [UnityTest]
    public IEnumerator PreExistentShapeUpdate()
    {
        string entityId = "1";
        TestUtils.CreateSceneEntity(scene, entityId);

        var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        {
            var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

            Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in first object!");
            Assert.IsTrue(gltfObject.transform.Find("TreeStump_01") != null, "Can't find \"TreeStump_01!\"");
        }

        TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
            }));

        gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        {
            var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

            Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in second object!");
            Assert.IsTrue(gltfObject.transform.Find("PalmTree_01") != null,
                "Can't find \"PalmTree_01\"!");
        }
    }

    [UnityTest]
    public IEnumerator PreExistentShapeImmediateUpdate()
    {
        string entityId = "1";
        TestUtils.CreateSceneEntity(scene, entityId);

        var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = LoadableShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
            }));

        gltfShape = LoadableShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.AreEqual(1,
            scene.entities[entityId].gameObject.GetComponentsInChildren<InstantiatedGLTFObject>().Length,
            "Only 1 'InstantiatedGLTFObject' should remain once the GLTF shape has been updated");
    }

    [UnityTest]
    public IEnumerator AttachedGetsReplacedOnNewAttachment()
    {
        var entity = TestUtils.CreateSceneEntity(scene);

        // set first GLTF
        string gltfId1 = TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
            }));
        var gltf1 = scene.GetSharedComponent(gltfId1);

        LoadWrapper gltfLoader = LoadableShape.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => gltfLoader.alreadyLoaded);

        Assert.AreEqual(gltf1, entity.GetSharedComponent(typeof(BaseShape)));

        // set second GLTF
        string gltfId2 = TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        gltfLoader = LoadableShape.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => gltfLoader.alreadyLoaded);

        Assert.AreEqual(scene.GetSharedComponent(gltfId2), entity.GetSharedComponent(typeof(BaseShape)));
        Assert.IsFalse(gltf1.GetAttachedEntities().Contains(entity));
    }

    // NOTE: Since every deployed scene carries compiled core code of the sdk at the moment of deploying, most of them have GLTFs default 'withCollisions' value
    // different than the new default value, that's why we are forcing the colliders to be ON on EVERY GLTF until that gets fixed, and that's why this test fails.
    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CollisionProperty()
    {
        string entityId = "entityId";
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape.Model();
        shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb";

        var shapeComponent = TestUtils.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = LoadableShape.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator VisibleProperty()
    {
        string entityId = "entityId";
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape.Model();
        shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb";

        var shapeComponent = TestUtils.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = LoadableShape.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestUtils.TestShapeVisibility(shapeComponent, shapeModel, entity);
    }

    [Test]
    public void OnReadyBeforeLoading()
    {
        GLTFShape gltfShape = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/Trevor/Trevor.glb", out IDCLEntity entity);

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });

        Assert.IsFalse(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnReadyWaitLoading()
    {
        GLTFShape gltfShape = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/Trevor/Trevor.glb", out IDCLEntity entity);

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });
        yield return TestUtils.WaitForGLTFLoad(entity);

        Assert.IsTrue(isOnReady);
    }

    [Test]
    [Explicit]
    [Category("Explicit")]
    public void OnReadyWithoutAttachInstantlyCalled()
    {
        GLTFShape gltfShape = TestUtils.SharedComponentCreate<GLTFShape, GLTFShape.Model>(scene, CLASS_ID.GLTF_SHAPE, new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Trevor/Trevor.glb"
        });

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });

        Assert.IsTrue(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnReadyAfterLoadingInstantlyCalled()
    {
        GLTFShape gltfShape = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/Trevor/Trevor.glb", out IDCLEntity entity);
        yield return TestUtils.WaitForGLTFLoad(entity);

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });
        Assert.IsTrue(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnDestroyWhileLoading()
    {
        GLTFShape gltfShape = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/Trevor/Trevor.glb", out IDCLEntity entity);
        GLTFShape gltfShape2 = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb", out IDCLEntity entity2);
        GLTFShape gltfShape3 = TestUtils.CreateEntityWithGLTFShape(scene, Vector3.zero, TestAssetsUtils.GetPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb", out IDCLEntity entity3);

        TestUtils.SetEntityParent(scene, entity2, entity);
        TestUtils.SetEntityParent(scene, entity3, entity);

        yield return null;
        yield return null;
        yield return null;

        Object.Destroy(entity.gameObject);

        yield return null;
    }
}