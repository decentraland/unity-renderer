using UnityEngine;
using NUnit.Framework;
using DCL.Models;

/*
 * Play Mode Testing Highlights:
 * - All Monobehaviour methods are invoked
 * - Tests run in a standalone window
 * - Tests may run slower, depending on the build target
 */

// TODO: Find a way instantiate and destroy the SceneController in every test.

public class PlayModeTests {
  [Test]
  public void PlayMode_EntityCreationTest() {
    // We need to load the required objects as prefabs to test, as we can't skip a frame in a NUnit tests, so we can't get already-instantiated scene objects.
    var sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

    Assert.AreNotEqual(sceneController, null);

    sceneController.UnloadAllScenes();

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

    scene.entities.Clear();
  }

  [Test]
  public void PlayMode_EntityRemovalTest() {
    string entityId = "2";
    // We need to load the required objects as prefabs to test, as we can't skip a frame in a NUnit tests, so we can't get already-instantiated scene objects.
    var sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

    Assert.AreNotEqual(sceneController, null);

    sceneController.UnloadAllScenes();

    var sceneData = new LoaderScene();
    var scene = sceneController.CreateTestScene(sceneData);

    Assert.AreNotEqual(scene, null);


    scene.CreateEntity(entityId);

    var entityObject = scene.entities[entityId];

    // We commented out checking the gameobject is being destroyed as it would force us to use DestroyImmediate() instead of Destroy() (which takes some time before destroying the object) to have a valid test.
    //entityGameObject = entityObject.gameObjectReference;

    scene.RemoveEntity(entityId);

    Assert.IsTrue(!scene.entities.ContainsKey(entityId));
  }

  [Test(Description = "Set entity parent one time")]
  public void PlayMode_EntityParentingTest() {
    var sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

    Assert.AreNotEqual(sceneController, null);

    sceneController.UnloadAllScenes();

    var sceneData = new LoaderScene();
    var scene = sceneController.CreateTestScene(sceneData);

    Assert.AreNotEqual(scene, null);

    ///

    string entityId = "2";
    string parentEntityId = "3";


    // Create entity
    scene.CreateEntity(entityId);
    var entityObject = scene.entities[entityId];

    // Create parent entity
    scene.CreateEntity(parentEntityId);

    var parentEntityObject = scene.entities[parentEntityId];

    string rawJSON = "{\"id\": \"" + entityId + "\"," +
                      "\"parentId\": \"" + parentEntityId + "\"}";

    scene.SetEntityParent(rawJSON);

    Assert.IsNotNull(parentEntityObject.gameObjectReference.transform.Find(entityId));

    scene.entities.Clear();
  }

  [Test(Description = "Update transformation test")]
  public void PlayMode_EntityTransformUpdate() {
    var sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

    Assert.AreNotEqual(sceneController, null);

    sceneController.UnloadAllScenes();

    var sceneData = new LoaderScene();
    var scene = sceneController.CreateTestScene(sceneData);

    Assert.AreNotEqual(scene, null);

    ///

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
    Assert.AreEqual(entityObject.gameObjectReference.transform.rotation.ToString(), Quaternion.Euler(10f, 50f, -90f).ToString());

    Assert.AreNotEqual(entityObject.gameObjectReference.transform.localScale, originalTransformScale);
    Assert.AreEqual(entityObject.gameObjectReference.transform.localScale, new Vector3(0.7f, 0.7f, 0.7f));

    scene.entities.Clear();
  }

  // TODO: Tests to be implemented
  // * scene.UpdateEntity() updates the entity correctly. Could be several tests for different kinds of update (components update, etc.)

}
