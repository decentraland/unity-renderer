using UnityEngine;
using NUnit.Framework;
using System.Collections;

/*
 * Edit Mode Testing Highlits:
 * - Monobehaviour methods Awake and Start are not invoked.
 * - Tests run directly in the editor
 * - Tests run faster
 */

// TODO: Fix 'editMode' command-line-triggered tests to load this tests correctly (they are not being recognized). Falling back to using 'playMode' tests exclusively for now.

/*public class EditModeTests {
    [Test]
    public void EditMode_JSONParsingTest() {
        DecentralandEntity entityObject;

        // We need to load the required objects as prefabs to test, as we can't skip a frame in a NUnit PlayMode test, so we can't get initial scene objects.
        SceneController sceneController = Resources.Load<GameObject>("Prefabs/SceneController").GetComponent<SceneController>();

        Assert.IsTrue(sceneController != null);

        string entityJSON = "{\"entityIdParam\": \"1\"}";
        sceneController.entities.Clear();
        sceneController.CreateEntity(entityJSON);

        sceneController.entities.TryGetValue("1", out entityObject);

        Assert.IsTrue(entityObject != null);

        Assert.AreEqual(entityObject.entityIdParam, "1");
    }
}*/
