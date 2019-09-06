using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using UnityGLTF;

public class GLTFShape_Tests : TestsBase
{

    [UnityTest]
    public IEnumerator ShapeUpdate()
    {
        yield return InitScene();

        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
            "Since the shape hasn't been updated yet, the 'GLTFScene' child object shouldn't exist");

        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");
    }


    [UnityTest]
    public IEnumerator PreExistentShapeUpdate()
    {
        yield return InitScene();

        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        {
            var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

            Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in first object!");
            Assert.IsTrue(gltfObject.transform.Find("Lantern") != null, "Can't find \"Lantern!\"");
        }

        TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

        gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        {
            var gltfObject = scene.entities[entityId].gameObject.GetComponentInChildren<InstantiatedGLTFObject>();

            Assert.IsTrue(gltfObject != null, "InstantiatedGLTFObject is null in second object!");
            Assert.IsTrue(gltfObject.transform.Find("node_damagedHelmet_-6514") != null,
                "Can't find \"node_damagedHelmet_-6514\"!");
        }
    }

    [UnityTest]
    public IEnumerator PreExistentShapeImmediateUpdate()
    {
        yield return InitScene();

        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        var componentId = TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        TestHelpers.UpdateShape(scene, componentId, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb"
            }));

        gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.AreEqual(1,
            scene.entities[entityId].gameObject.GetComponentsInChildren<InstantiatedGLTFObject>().Length,
            "Only 1 'InstantiatedGLTFObject' should remain once the GLTF shape has been updated");
    }

    [UnityTest]
    public IEnumerator AttachedGetsReplacedOnNewAttachment()
    {
        yield return InitScene();

        DecentralandEntity entity = TestHelpers.CreateSceneEntity(scene);

        // set first GLTF
        string gltfId1 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb"
            }));
        var gltf1 = scene.GetSharedComponent(gltfId1);

        LoadWrapper_GLTF gltfLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

        Assert.AreEqual(gltf1, entity.GetSharedComponent(typeof(BaseShape)));

        // set second GLTF
        string gltfId2 = TestHelpers.CreateAndSetShape(scene, entity.entityId, DCL.Models.CLASS_ID.GLTF_SHAPE,
            JsonConvert.SerializeObject(new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        gltfLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfLoader.alreadyLoaded);

        Assert.AreEqual(scene.GetSharedComponent(gltfId2), entity.GetSharedComponent(typeof(BaseShape)));
        Assert.IsFalse(gltf1.attachedEntities.Contains(entity));
    }



    [UnityTest]
    public IEnumerator CollisionProperty()
    {
        yield return InitScene();

        string entityId = "entityId";
        TestHelpers.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
        shapeModel.src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb";

        var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestHelpers.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestHelpers.TestShapeCollision(shapeComponent, shapeModel, entity);
    }

    [UnityTest]
    public IEnumerator VisibleProperty()
    {
        yield return InitScene();

        string entityId = "entityId";
        TestHelpers.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];
        yield return null;

        // Create shape component
        var shapeModel = new LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model();
        shapeModel.src = TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb";

        var shapeComponent = TestHelpers.SharedComponentCreate<LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>, LoadableShape<LoadWrapper_GLTF, LoadableShape.Model>.Model>(scene, CLASS_ID.GLTF_SHAPE, shapeModel);
        yield return shapeComponent.routine;

        TestHelpers.SharedComponentAttach(shapeComponent, entity);

        var shapeLoader = entity.gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => shapeLoader.alreadyLoaded);

        yield return TestHelpers.TestShapeVisibility(shapeComponent, shapeModel, entity);
    }


    [UnityTest]
    public IEnumerator OnReadyBeforeLoading()
    {
        yield return InitScene();
        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });

        Assert.IsFalse(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnReadyWaitLoading()
    {
        yield return InitScene();
        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });
        yield return TestHelpers.WaitForGLTFLoad(entity);

        Assert.IsTrue(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnReadyWithoutAttachInstantlyCalled()
    {
        yield return InitScene();

        GLTFShape gltfShape = TestHelpers.SharedComponentCreate<GLTFShape, GLTFShape.Model>(scene, CLASS_ID.GLTF_SHAPE, new LoadableShape.Model()
        {
            src = TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb"
        });

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });

        Assert.IsTrue(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnReadyAfterLoadingInstantlyCalled()
    {
        yield return InitScene();
        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);
        yield return TestHelpers.WaitForGLTFLoad(entity);

        bool isOnReady = false;
        gltfShape.CallWhenReady((x) => { isOnReady = true; });
        Assert.IsTrue(isOnReady);
    }

    [UnityTest]
    public IEnumerator OnDestroyWhileLoading()
    {
        yield return InitScene();

        GLTFShape gltfShape = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, TestHelpers.GetTestsAssetsPath() + "/GLB/Trevor/Trevor.glb", out DecentralandEntity entity);
        GLTFShape gltfShape2 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, TestHelpers.GetTestsAssetsPath() + "/GLB/PalmTree_01.glb", out DecentralandEntity entity2);
        GLTFShape gltfShape3 = TestHelpers.CreateEntityWithGLTFShape(scene, Vector3.zero, TestHelpers.GetTestsAssetsPath() + "/GLB/DamagedHelmet/DamagedHelmet.glb", out DecentralandEntity entity3);

        TestHelpers.SetEntityParent(scene, entity2, entity);
        TestHelpers.SetEntityParent(scene, entity3, entity);

        yield return null;
        yield return null;
        yield return null;

        Object.Destroy(entity.gameObject);

        yield return null;

        Debug.Break();
        yield return null;
    }
}
