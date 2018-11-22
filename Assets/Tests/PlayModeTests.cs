using System.Collections;
using UnityEngine;
using NUnit.Framework;
using DCL.Models;
/*
 * Play Mode Testing Highlights:
 * - All Monobehaviour methods are invoked
 * - Tests run in a standalone window
 * - Tests may run slower, depending on the build target
 */
using UnityEngine.TestTools;

namespace Tests {
  public class PlayModeTests {
    [Test(Description = "Instantiate 2 entities")]
    public void PlayMode_EntityCreationTest() {
      var sceneController = GetOrInitializeSceneController();

      Assert.AreNotEqual(sceneController, null);

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);

      Assert.AreNotEqual(scene, null);

      // Create first entity
      string entityId = "1";

      scene.CreateEntity(entityId);
      var entityObject = scene.entities[entityId];

      Assert.IsTrue(entityObject != null);

      Assert.AreEqual(entityObject.id, entityId);

      // Create second entity
      entityObject = null;
      entityId = "2";

      scene.CreateEntity(entityId);
      scene.entities.TryGetValue(entityId, out entityObject);

      Assert.IsTrue(entityObject != null);

      Assert.AreEqual(entityObject.id, entityId);
    }

    [Test(Description = "Set entity parent one time")]
    public void PlayMode_EntityParentingTest() {
      var sceneController = GetOrInitializeSceneController();

      Assert.AreNotEqual(sceneController, null);

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);

      Assert.AreNotEqual(scene, null);

      string entityId = "2";
      string parentEntityId = "3";

      scene.CreateEntity(entityId);
      scene.CreateEntity(parentEntityId);

      var parentEntityObject = scene.entities[parentEntityId];

      string rawJSON = "{\"id\": \"" + entityId + "\"," +
                       "\"parentId\": \"" + parentEntityId + "\"}";

      scene.SetEntityParent(rawJSON);

      Assert.IsNotNull(parentEntityObject.gameObjectReference.transform.Find(entityId));
    }

    [Test(Description = "Update entity transform")]
    public void PlayMode_EntityTransformUpdate() {
      var sceneController = GetOrInitializeSceneController();

      Assert.AreNotEqual(sceneController, null);

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);

      Assert.AreNotEqual(scene, null);

      string entityId = "1";
      scene.CreateEntity(entityId);

      var entityObject = scene.entities[entityId];

      Assert.IsTrue(entityObject != null);

      Vector3 originalTransformPosition = entityObject.gameObjectReference.transform.position;
      Quaternion originalTransformRotation = entityObject.gameObjectReference.transform.rotation;
      Vector3 originalTransformScale = entityObject.gameObjectReference.transform.localScale;

      string rawJSON = (Resources.Load("TestJSON/EntityUpdate/EntityTransformUpdateTest") as TextAsset).text;

      Assert.IsTrue(!string.IsNullOrEmpty(rawJSON));

      scene.UpdateEntity(rawJSON);

      Assert.AreNotEqual(entityObject.gameObjectReference.transform.position, originalTransformPosition);
      Assert.AreEqual(entityObject.gameObjectReference.transform.position, new Vector3(5f, 1f, 5f));

      Assert.AreNotEqual(entityObject.gameObjectReference.transform.rotation, originalTransformRotation);
      Assert.AreEqual(entityObject.gameObjectReference.transform.rotation.ToString(),
        Quaternion.Euler(10f, 50f, -90f).ToString());

      Assert.AreNotEqual(entityObject.gameObjectReference.transform.localScale, originalTransformScale);
      Assert.AreEqual(entityObject.gameObjectReference.transform.localScale, new Vector3(0.7f, 0.7f, 0.7f));
    }

    [Test(Description = "Update entity adding a box shape component")]
    public void PlayMode_EntityBoxShapeUpdate() {
      var sceneController = GetOrInitializeSceneController();

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);
      string entityId = "1";
      scene.CreateEntity(entityId);
      scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityBoxShapeUpdateTest") as TextAsset).text);

      var meshName = scene.entities[entityId].gameObjectReference.GetComponentInChildren<MeshFilter>().mesh.name;
      Assert.AreEqual(meshName, "DCL Box Instance");
    }

    [Test(Description = "Update entity adding a sphere shape component")]
    public void PlayMode_EntitySphereShapeUpdate() {
      var sceneController = GetOrInitializeSceneController();

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);
      string entityId = "2";
      scene.CreateEntity(entityId);
      scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntitySphereShapeUpdateTest") as TextAsset).text);

      var meshName = scene.entities[entityId].gameObjectReference.GetComponentInChildren<MeshFilter>().mesh.name;
      Assert.AreEqual(meshName, "DCL Sphere Instance");
    }

    [Test(Description = "Update entity adding a plane shape component")]
    public void PlayMode_EntityPlaneShapeUpdate() {
      var sceneController = GetOrInitializeSceneController();

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);
      string entityId = "3";
      scene.CreateEntity(entityId);
      scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityPlaneShapeUpdateTest") as TextAsset).text);

      var meshName = scene.entities[entityId].gameObjectReference.GetComponentInChildren<MeshFilter>().mesh.name;
      Assert.AreEqual(meshName, "DCL Plane Instance");
    }

    [Test(Description = "Update entity adding a cylinder shape component")]
    public void PlayMode_EntityCylinderShapeUpdate() {
      var sceneController = GetOrInitializeSceneController();

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);
      string entityId = "5";
      scene.CreateEntity(entityId);
      scene.UpdateEntity((Resources.Load("TestJSON/EntityUpdate/EntityCylinderShapeUpdateTest") as TextAsset).text);

      var meshName = scene.entities[entityId].gameObjectReference.GetComponentInChildren<MeshFilter>().mesh.name;
      Assert.AreEqual(meshName, "DCL Cylinder Instance");
    }

    [Test(Description = "Load a decentraland scene")]
    public void PlayMode_SceneLoading() {
      var sceneController = GetOrInitializeSceneController();

      sceneController.LoadDecentralandScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

      string loadedSceneID = "-4,0";

      Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

      Assert.IsTrue(sceneController.loadedScenes[loadedSceneID].rootGameObject != null);
    }

    [UnityTest]
    public IEnumerator PlayMode_EntityRemovalTest() {
      var sceneController = GetOrInitializeSceneController();

      string entityId = "2";

      Assert.AreNotEqual(sceneController, null);

      var sceneData = new LoaderScene();
      var scene = sceneController.CreateTestScene(sceneData);

      Assert.AreNotEqual(scene, null);

      scene.CreateEntity(entityId);

      var gameObjectReference = scene.entities[entityId].gameObjectReference;

      scene.RemoveEntity(entityId);

      yield return new WaitForSeconds(0.01f);
      
      Assert.IsTrue(!scene.entities.ContainsKey(entityId));

      Assert.IsTrue(gameObjectReference == null, "Entity gameobject reference is not getting destroyed.");
    }

    [UnityTest]
    public IEnumerator PlayMode_SceneUnloading() {
      var sceneController = GetOrInitializeSceneController();

      sceneController.LoadDecentralandScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

      string loadedSceneID = "-4,0";

      Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

      // Add 1 entity to the loaded scene
      sceneController.loadedScenes[loadedSceneID].CreateEntity("6");

      var sceneRootGameObject = sceneController.loadedScenes[loadedSceneID].rootGameObject;
      var sceneEntities = sceneController.loadedScenes[loadedSceneID].entities;

      sceneController.UnloadScene(loadedSceneID);

      yield return new WaitForSeconds(0.01f); // We wait to let unity destroy gameobjects.

      Assert.IsTrue(!sceneController.loadedScenes.ContainsKey(loadedSceneID));

      Assert.IsTrue(sceneRootGameObject == null, "Scene root gameobject reference is not getting destroyed.");

      Assert.IsTrue(sceneEntities.Count == 0);
    }

    SceneController GetOrInitializeSceneController() {
      var sceneController = Object.FindObjectOfType<SceneController>();

      if (sceneController == null) {
        sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
      }

      sceneController.UnloadAllScenes();

      return sceneController;
    }

    // TODO: Tests to be implemented
    /* 
   * PlayMode_EntityConeShapeUpdate
   * PlayMode_EntityGLTFShapeUpdate
   * PlayMode_EntityOBJShapeUpdate
   */
  }
}