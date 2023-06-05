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

public class GLTFShape_Tests : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        coreComponentsPlugin = new CoreComponentsPlugin();
        scene = TestUtils.CreateTestScene();
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        yield return base.TearDown();
    }

    [UnityTest]
    public IEnumerator ShapeUpdate()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>() != null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
    }

    [Test]
    public void CustomContentProvider()
    {
        long entityId = 1;
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


        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);

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
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);

        var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        {
            var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();

            Assert.IsTrue(gltfObject != null, "MeshRenderer is null in first object!");
        }

        TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
            }));

        gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        {
            var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<MeshRenderer>();

            Assert.IsTrue(gltfObject != null, "MeshRenderer is null in second object!");
        }
    }

    [UnityTest]
    public IEnumerator PreExistentShapeImmediateUpdate()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);

        var componentId = TestUtils.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        TestUtils.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
            new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb"
            }));

        gltfShape = Environment.i.world.state.GetLoaderForEntity(scene.entities[entityId]);
        yield return new DCL.WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.AreEqual(1,
            scene.entities[entityId].gameObject.GetComponentsInChildren<MeshRenderer>().Length,
            "Only 1 'MeshRenderer' should remain once the GLTF shape has been updated");
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
        var gltf1 = scene.componentsManagerLegacy.GetSceneSharedComponent(gltfId1);

        LoadWrapper gltfLoader = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => gltfLoader.alreadyLoaded);

        Assert.AreEqual(gltf1, scene.componentsManagerLegacy.GetSharedComponent(entity, typeof(BaseShape)));

        // set second GLTF
        string gltfId2 = TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            }));

        gltfLoader = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => gltfLoader.alreadyLoaded);

        Assert.AreEqual(scene.componentsManagerLegacy.GetSceneSharedComponent(gltfId2), scene.componentsManagerLegacy.GetSharedComponent(entity, typeof(BaseShape)));
        Assert.IsFalse(gltf1.GetAttachedEntities().Contains(entity));
    }

    // NOTE: Since every deployed scene carries compiled core code of the sdk at the moment of deploying, most of them have GLTFs default 'withCollisions' value
    // different than the new default value, that's why we are forcing the colliders to be ON on EVERY GLTF until that gets fixed, and that's why this test fails.
    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator CollisionProperty()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape.Model();
        shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb";

        var shapeComponent = TestUtils.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestUtils.TestShapeCollision(shapeComponent, shapeModel, entity);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator VisibleProperty()
    {
        long entityId = 1;
        TestUtils.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape.Model();
        shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/PalmTree_01.glb";

        var shapeComponent = TestUtils.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestUtils.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new DCL.WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestUtils.TestOnPointerEventWithShapeVisibleProperty(shapeComponent, shapeModel, entity);
    }

    [UnityTest]
    public IEnumerator AnimationTogglingOnVisibilityChange()
    {
        // GLTFShape without DCLAnimator should toggle its animation on visibility changes
        var entity = TestUtils.CreateSceneEntity(scene);
        TestUtils.SetEntityTransform(scene, entity, new Vector3(8, 2, 8), Quaternion.identity, Vector3.one);

        Assert.IsTrue(entity.gameObject.GetComponentInChildren<MeshRenderer>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        var shapeModel = new LoadableShape.Model();
        shapeModel.src = TestAssetsUtils.GetPath() + "/GLB/CesiumMan/CesiumMan.glb";

        string shape1Id = TestUtils.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(shapeModel));
        var shape1Component = scene.componentsManagerLegacy.GetSceneSharedComponent(shape1Id);

        LoadWrapper gltfShape = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new UnityEngine.WaitUntil(() => gltfShape.alreadyLoaded == true);

        Animation animation = entity.meshRootGameObject.GetComponentInChildren<Animation>();

        Assert.IsTrue(animation != null);
        Assert.IsTrue(animation.isPlaying);

        shapeModel.visible = false;
        yield return TestUtils.SharedComponentUpdate(shape1Component, shapeModel);

        Assert.IsFalse(animation.enabled);

        shapeModel.visible = true;
        yield return TestUtils.SharedComponentUpdate(shape1Component, shapeModel);

        Assert.IsTrue(animation.enabled);

        // GLTFShape with DCLAnimator shouldn't toggle its animation on visibility changes
        var entity2 = TestUtils.CreateSceneEntity(scene);

        Assert.IsTrue(entity2.gameObject.GetComponentInChildren<MeshRenderer>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        string shape2Id = TestUtils.CreateAndSetShape(scene, entity2.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(shapeModel));
        var shape2Component = scene.componentsManagerLegacy.GetSceneSharedComponent(shape2Id);

        string clipName = "Clip_0";
        DCLAnimator.Model animatorModel = new DCLAnimator.Model
        {
            states = new[]
            {
                new DCLAnimator.Model.DCLAnimationState
                {
                    name = "clip01",
                    clip = clipName,
                    playing = true,
                    weight = 1,
                    speed = 1,
                    looping = true
                }
            }
        };

        DCLAnimator animator = TestUtils.EntityComponentCreate<DCLAnimator, DCLAnimator.Model>(scene, entity, animatorModel);

        LoadWrapper gltfShape2 = Environment.i.world.state.GetLoaderForEntity(entity);
        yield return new UnityEngine.WaitUntil(() => gltfShape2.alreadyLoaded == true);

        Assert.IsTrue(animator.animComponent != null);
        Assert.AreEqual(clipName, animator.animComponent.clip.name);
        Assert.IsTrue(animator.animComponent.isPlaying);

        shapeModel.visible = false;
        yield return TestUtils.SharedComponentUpdate(shape2Component, shapeModel);

        Assert.IsTrue(animation.enabled);
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
}
