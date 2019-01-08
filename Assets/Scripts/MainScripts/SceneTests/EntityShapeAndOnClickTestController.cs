using DCL.Controllers;
using DCL.Helpers;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class EntityShapeAndOnClickTestController : MonoBehaviour {
  IEnumerator Start() {
    var sceneController = FindObjectOfType<SceneController>();
    var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

    sceneController.UnloadAllScenes();
    sceneController.LoadParcelScenes(scenesToLoad);

    var scene = sceneController.loadedScenes["0,0"];

    TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-3, 1, 0));
    TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(0, 1, 0));
    TestHelpers.InstantiateEntityWithShape(scene, "3", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(2, 1, 0));
    TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(4, 1, 0));
    TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.CYLINDER_SHAPE, new Vector3(6, 1, 0));
    TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 6), "http://127.0.0.1:9991/GLB/Lantern/Lantern.glb");
    TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.OBJ_SHAPE, new Vector3(10, 1, 0), "http://127.0.0.1:9991/OBJ/teapot.obj");

    Assert.IsNotNull(scene.entities["1"]);
    Assert.IsNotNull(scene.entities["2"]);
    Assert.IsNotNull(scene.entities["3"]);
    Assert.IsNotNull(scene.entities["4"]);
    Assert.IsNotNull(scene.entities["5"]);
    Assert.IsNotNull(scene.entities["6"]);
    Assert.IsNotNull(scene.entities["7"]);

    AddOnClickComponent(scene, "1");
    AddOnClickComponent(scene, "2");
    AddOnClickComponent(scene, "3");
    AddOnClickComponent(scene, "4");
    AddOnClickComponent(scene, "5");
    AddOnClickComponent(scene, "6");
    AddOnClickComponent(scene, "7");

    yield return new WaitForSeconds(5f);

    scene.UpdateEntityComponent(JsonUtility.ToJson(new DCL.Models.UpdateEntityComponentMessage {
      entityId = "6",
      name = "shape",
      classId = (int)DCL.Models.CLASS_ID.GLTF_SHAPE,
      json = JsonConvert.SerializeObject(new {
        src = "http://127.0.0.1:9991/GLB/DamagedHelmet/DamagedHelmet.glb"
      })
    }));

    yield return null;
  }

  void AddOnClickComponent(ParcelScene scene, string entityID) {
    scene.UpdateEntityComponent(JsonUtility.ToJson(new DCL.Models.UpdateEntityComponentMessage {
      entityId = entityID,
      name = "onclick",
      classId = (int)DCL.Models.CLASS_ID.ONCLICK,
      json = JsonConvert.SerializeObject(new { })
    }));
  }
}
