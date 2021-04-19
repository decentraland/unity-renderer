using DCL.Components;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

public class LoadableShapesMiscTests : IntegrationTestSuite_Legacy
{
    [UnityTest]
    public IEnumerator OBJShapeUpdate()
    {
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);

        Material placeholderLoadingMaterial = Resources.Load<Material>("Materials/AssetLoading");

        yield return null;

        Assert.IsTrue(scene.entities[entityId].meshRootGameObject == null,
            "Since the shape hasn't been updated yet, the child mesh shouldn't exist");

        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.OBJ_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = Utils.GetTestsAssetsPath() + "/OBJ/teapot.obj"
            }));

        LoadWrapper objShape = LoadableShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new WaitUntil(() => objShape.alreadyLoaded);

        Assert.IsTrue(scene.entities[entityId].meshRootGameObject != null,
            "Every entity with a shape should have the mandatory 'Mesh' object as a child");

        var childRenderer = scene.entities[entityId].meshRootGameObject.GetComponentInChildren<MeshRenderer>();
        Assert.IsTrue(childRenderer != null,
            "Since the shape has already been updated, the child renderer should exist");
        Assert.AreNotSame(placeholderLoadingMaterial, childRenderer.sharedMaterial,
            "Since the shape has already been updated, the child renderer found shouldn't have the 'AssetLoading' placeholder material");
    }

    [UnityTest]
    public IEnumerator PreExistentShapeUpdate()
    {
        string entityId = "1";
        TestHelpers.CreateSceneEntity(scene, entityId);
        var entity = scene.entities[entityId];

        Assert.IsTrue(entity.meshRootGameObject == null, "meshGameObject should be null");

        // Set its shape as a BOX
        var componentId = TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");
        yield return ((scene.GetSharedComponent(componentId)) as IDelayedComponent).routine;

        var meshName = entity.meshRootGameObject.GetComponent<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Box Instance", meshName);

        // Update its shape to a cylinder
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.CYLINDER_SHAPE, "{}");
        yield return (scene.GetSharedComponent(componentId) as IDelayedComponent).routine;

        Assert.IsTrue(entity.meshRootGameObject != null, "meshGameObject should not be null");

        meshName = entity.meshRootGameObject.GetComponent<MeshFilter>().mesh.name;
        Assert.AreEqual("DCL Cylinder Instance", meshName);
        Assert.IsTrue(entity.meshRootGameObject.GetComponent<MeshFilter>() != null,
            "After updating the entity shape to a basic shape, the mesh filter shouldn't be removed from the object");

        Assert.IsTrue(entity.meshesInfo.currentShape != null, "current shape must exist 1");
        Assert.IsTrue(entity.meshesInfo.currentShape is CylinderShape, "current shape is BoxShape");

        // Update its shape to a GLTF
        TestHelpers.CreateAndSetShape(scene, entityId, DCL.Models.CLASS_ID.GLTF_SHAPE, JsonConvert.SerializeObject(
            new
            {
                src = Utils.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb"
            }));

        LoadWrapper gltfShape = GLTFShape.GetLoaderForEntity(scene.entities[entityId]);
        yield return new WaitUntil(() => gltfShape.alreadyLoaded);

        Assert.IsTrue(entity.meshesInfo.currentShape != null, "current shape must exist 2");
        Assert.IsTrue(entity.meshesInfo.currentShape is GLTFShape, "current shape is GLTFShape");

        Assert.IsTrue(entity.meshRootGameObject != null);

        Assert.IsTrue(entity.meshRootGameObject.GetComponent<MeshFilter>() == null,
            "After updating the entity shape to a GLTF shape, the mesh filter should be removed from the object");
        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() != null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component should exist if the GLTF was loaded correctly");

        // Update its shape to a sphere
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.SPHERE_SHAPE, "{}");
        yield return (scene.GetSharedComponent(componentId) as IDelayedComponent).routine;

        yield return null;

        Assert.IsTrue(entity.meshRootGameObject != null);

        meshName = entity.meshRootGameObject.GetComponent<MeshFilter>().mesh.name;

        Assert.AreEqual("DCL Sphere Instance", meshName);

        Assert.IsTrue(
            scene.entities[entityId].gameObject.GetComponentInChildren<UnityGLTF.InstantiatedGLTFObject>() == null,
            "'GLTFScene' child object with 'InstantiatedGLTF' component shouldn't exist after the shape is updated to a non-GLTF shape");
    }
}