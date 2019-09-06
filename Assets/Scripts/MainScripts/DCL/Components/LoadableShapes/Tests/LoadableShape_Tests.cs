using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
public class LoadableShape_Tests : TestsBase
{
    [UnityTest]
    public IEnumerator OBJShapeUpdate()
    {
        yield return InitScene();

        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        Material placeholderLoadingMaterial = Resources.Load<Material>("Materials/AssetLoading");

        yield return null;

        Assert.IsTrue(scene.entities[entityId].meshGameObject == null,
            "Since the shape hasn't been updated yet, the child mesh shouldn't exist");

        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/OBJ/teapot.obj"
            }));

        LoadWrapper_OBJ objShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_OBJ>(true);
        yield return new WaitUntil(() => objShape.alreadyLoaded);

        Assert.IsTrue(scene.entities[entityId].meshGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        var childRenderer = scene.entities[entityId].meshGameObject.GetComponentInChildren<MeshRenderer>();
        Assert.IsTrue(childRenderer != null,
            "Since the shape has already been updated, the child renderer should exist");
        Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial,
            "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
    }



    [UnityTest]
    public IEnumerator PreExistentShapeUpdate()
    {
        yield return InitScene();

        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];

        Assert.IsTrue(entity.meshGameObject == null, "meshGameObject should be null");

        // Set its shape as a BOX
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

        var meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Box Instance", meshName);

        // Update its shape to a cylinder
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");

        Assert.IsTrue(entity.meshGameObject != null, "meshGameObject should not be null");

        meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Cylinder Instance", meshName);
        Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() != null,
            "After updating the entity shape to a basic shape, the mesh filter shouldn't be removed from the object");

        Assert.IsTrue(entity.currentShape != null, "current shape must exist 1");
        Assert.IsTrue(entity.currentShape is CylinderShape, "current shape is BoxShape");

        // Update its shape to a GLTF
        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper_GLTF gltfShape = scene.entities[entityId].gameObject.GetComponentInChildren<LoadWrapper_GLTF>(true);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.IsTrue(entity.currentShape != null, "current shape must exist 2");
        Assert.IsTrue(entity.currentShape is GLTFShape, "current shape is GLTFShape");

        Assert.IsTrue(entity.meshGameObject != null);

        Assert.IsTrue(entity.meshGameObject.GetComponent<MeshFilter>() == null,
            "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

        // Update its shape to a sphere
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");

        yield return null;

        Assert.IsTrue(entity.meshGameObject != null);

        meshName = entity.meshGameObject.GetComponent<MeshFilter>().mesh.name;

        Assert.AreEqual("DCL Cylinder Instance", meshName);
        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
    }




}
