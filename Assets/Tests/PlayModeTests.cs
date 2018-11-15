using UnityEngine;
using NUnit.Framework;

/*
 * Play Mode Testing Highlights:
 * - All Monobehaviour methods are invoked
 * - Tests run in a standalone window
 * - Tests may run slower, depending on the build target
 */

// TODO: Find a way instantiate and destroy the SceneController in every test.

public class PlayModeTests {
  SceneController sceneController;

  [Test]
  public void PlayMode_EntityCreationTest() {
    // We need to load the required objects as prefabs to test, as we can't skip a frame in a NUnit tests, so we can't get already-instantiated scene objects.
    if (sceneController == null) {
      sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
    }

    Assert.IsTrue(sceneController != null);

    // Create first entity
    DecentralandEntity entityObject = null;
    string entityId = "1";

    sceneController.CreateEntity(entityId);
    sceneController.decentralandEntities.TryGetValue(entityId, out entityObject);

    Assert.IsTrue(entityObject != null);

    Assert.AreEqual(entityObject.id, entityId);

    // Create second entity
    entityObject = null;
    entityId = "2";

    sceneController.CreateEntity(entityId);
    sceneController.decentralandEntities.TryGetValue(entityId, out entityObject);

    Assert.IsTrue(entityObject != null);

    Assert.AreEqual(entityObject.id, entityId);

    sceneController.decentralandEntities.Clear();
  }

  [Test]
  public void PlayMode_EntityRemovalTest() {
    DecentralandEntity entityObject = null;
    string entityId = "2";

    if (sceneController == null) {
      sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
    }

    sceneController.CreateEntity(entityId);

    sceneController.decentralandEntities.TryGetValue(entityId, out entityObject);

    // We commented out checking the gameobject is being destroyed as it would force us to use DestroyImmediate() instead of Destroy() (which takes some time before destroying the object) to have a valid test.
    //entityGameObject = entityObject.gameObjectReference;

    sceneController.RemoveEntity(entityId);

    Assert.IsTrue(!sceneController.decentralandEntities.ContainsKey(entityId));
  }

  [Test]
  public void PlayMode_EntityParentingTest() {
    DecentralandEntity parentEntityObject = null;
    DecentralandEntity entityObject = null;

    string entityId = "2";
    string parentEntityId = "3";

    if (sceneController == null) {
      sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
    }

    // Create entity
    sceneController.CreateEntity(entityId);
    sceneController.decentralandEntities.TryGetValue(entityId, out entityObject);

    // Create parent entity
    sceneController.CreateEntity(parentEntityId);
    sceneController.decentralandEntities.TryGetValue(parentEntityId, out parentEntityObject);

    string rawJSON = "{\"id\": \"" + entityId + "\"," +
                      "\"parentId\": \"" + parentEntityId + "\"}";

    sceneController.SetEntityParent(rawJSON);

    Assert.IsNotNull(parentEntityObject.gameObjectReference.transform.Find(entityId));

    sceneController.decentralandEntities.Clear();
  }

  [Test]
  public void PlayMode_EntityTransformUpdate() {
    DecentralandEntity entityObject = null;
    string entityId = "1";

    if (sceneController == null) {
      sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
    }

    sceneController.CreateEntity(entityId);

    sceneController.decentralandEntities.TryGetValue(entityId, out entityObject);
    Assert.IsTrue(entityObject != null);

    Vector3 originalTransformPosition = entityObject.gameObjectReference.transform.position;
    Quaternion originalTransformRotation = entityObject.gameObjectReference.transform.rotation;
    Vector3 originalTransformScale = entityObject.gameObjectReference.transform.localScale;

    string rawJSON = (Resources.Load("TestJSON/EntityUpdate/EntityTransformUpdateTest") as TextAsset).text;

    Assert.IsTrue(!string.IsNullOrEmpty(rawJSON));

    sceneController.UpdateEntity(rawJSON);

    Assert.IsTrue(entityObject.gameObjectReference.transform.position != originalTransformPosition &&
        entityObject.gameObjectReference.transform.position == new Vector3(5f, 1f, 5f));

    Assert.IsTrue(entityObject.gameObjectReference.transform.rotation != originalTransformRotation &&
        entityObject.gameObjectReference.transform.rotation == Quaternion.Euler(10f, 50f, -90f));

    Assert.IsTrue(entityObject.gameObjectReference.transform.localScale != originalTransformScale &&
        entityObject.gameObjectReference.transform.localScale == new Vector3(0.7f, 0.7f, 0.7f));

    sceneController.decentralandEntities.Clear();
  }

  // TODO: Tests to be implemented
  // * SceneController.UpdateEntity() updates the entity correctly. Could be several tests for different kinds of update (components update, etc.)

}
