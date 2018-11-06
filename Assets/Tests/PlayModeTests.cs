using UnityEngine;
using NUnit.Framework;

/*
 * Play Mode Testing Highlits:
 * - All Monobehaviour methods are invoked
 * - Tests run in a standalone window
 * - Tests may run slower, depending on the build target
 */

public class PlayModeTests {
  DecentralandEntity entityObject;
  SceneController sceneController;

  [Test]
  public void PlayMode_EntityCreationTest() {
    // We need to load the required objects as prefabs to test, as we can't skip a frame in a NUnit tests, so we can't get already-instantiated scene objects.
    sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();
    
    Assert.IsTrue(sceneController != null);

    sceneController.entities.Clear();

    // Create first entity
    entityObject = null;
    string entityId = "1";
    
    sceneController.CreateEntity(entityId);
    sceneController.entities.TryGetValue(entityId, out entityObject);

    Assert.IsTrue(entityObject != null);

    Assert.AreEqual(entityObject.id, entityId);

    // Create second entity
    entityObject = null;
    entityId = "2";

    sceneController.CreateEntity(entityId);
    sceneController.entities.TryGetValue(entityId, out entityObject);

    Assert.IsTrue(entityObject != null);

    Assert.AreEqual(entityObject.id, entityId);
  }

  [Test]
  public void PlayMode_EntityRemovalTest() {
    entityObject = null;
    string entityId = "2";

    if (sceneController == null)
      sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

    sceneController.entities.Clear();
    sceneController.CreateEntity(entityId);

    sceneController.entities.TryGetValue(entityId, out entityObject);

    // We commented out checking the gameobject is being destroyed as it would force us to use DestroyImmediate() instead of Destroy() (which takes some time before destroying the object) to have a valid test.
    //entityGameObject = entityObject.gameObjectReference;

    sceneController.RemoveEntity(entityId);

    Assert.IsTrue(!sceneController.entities.ContainsKey(entityId));

    //Assert.IsTrue(entityGameObject == null);
  }

  [Test]
  public void PlayMode_EntityParentingTest() {
    DecentralandEntity parentEntityObject = null;
    entityObject = null;

    string entityId = "2";
    string parentEntityId = "3";

    sceneController.entities.Clear();

    // Create entity
    sceneController.CreateEntity(entityId);
    sceneController.entities.TryGetValue(entityId, out entityObject);

    // Create parent entity
    sceneController.CreateEntity(parentEntityId);
    sceneController.entities.TryGetValue(parentEntityId, out parentEntityObject);

    string rawJSON = "{\"id\": \"" + entityId + "\"," +
                      "\"parentId\": \"" + parentEntityId + "\"}";

    sceneController.SetEntityParent(rawJSON);

    Assert.IsNotNull(parentEntityObject.gameObjectReference.transform.Find(entityId));
  }

  // TODO: Tests to be implemented
  // * SceneController.UpdateEntity() updates the entity correctly. Could be several tests for different kinds of update (components update, etc.)

}
