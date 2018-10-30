using UnityEngine;
using NUnit.Framework;
using System.Collections;

/*
 * Play Mode Testing Highlits:
 * - All Monobehaviour methods are invoked
 * - Tests run in a standalone window
 * - Tests may run slower, depending on the build target
 */

public class PlayModeTests {
  [Test]
  public void PlayMode_EntityCreationTest() {
    DecentralandEntity entityObject;

    // We need to load the required objects as prefabs to test, as we can't skip a frame in a NUnit tests, so we can't get already-instantiated scene objects.
    SceneController sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

    Assert.IsTrue(sceneController != null);

    string entityJSON = "{\"id\": \"1\"}";

    sceneController.entities.Clear();
    sceneController.CreateEntity(entityJSON);

    sceneController.entities.TryGetValue("1", out entityObject);

    Assert.IsTrue(entityObject != null);

    Assert.AreEqual(entityObject.id, "1");
  }
}
