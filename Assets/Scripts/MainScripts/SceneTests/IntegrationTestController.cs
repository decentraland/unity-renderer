using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

public class IntegrationTestController : MonoBehaviour {
  string entityId = "a5f571bd-bce1-4cf8-a158-b8f3e92e4fb0";

  void Awake() {
    var sceneController = Object.FindObjectOfType<SceneController>();

    var scenesToLoad = new {
      parcelsToLoad = new[] {
        new {
          id = "the-loaded-scene",
          basePosition = new Vector2Int(3, 3),
          parcels = new [] {
            new Vector2Int(3, 3),
            new Vector2Int(3, 4)
          },
          baseUrl = "http://localhost:8080/local-ipfs/contents/",
          owner = "0x0f5d2fb29fb7d3cfee444a200298f468908cc942",
          contents= new[] {
            new {
              file = "game.js",
              hash = "Qm39j2mCramRURmogUMqtrXNHtZ3z8taShBRLinPZGWmjFzKnL2A1JTortYo77aUYETPmRoHJdn2qhYqWk3acKoqnW"
            }
          }
        }
      }
    };

    Assert.IsTrue(sceneController != null, "Cannot find SceneController");

    sceneController.UnloadAllScenes();
    sceneController.LoadParcelScenes(JsonConvert.SerializeObject(scenesToLoad));

    var scene = sceneController.loadedScenes["the-loaded-scene"];

    scene.CreateEntity(entityId);

    scene.SetEntityParent(JsonConvert.SerializeObject(new {
      entityId = entityId,
      parentId = "0"
    }));

    // 1st message
    scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
      entityId = entityId,
      name = "shape",
      json = "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"box\"}"
    }));

    scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
      entityId = entityId,
      name = "transform",
      json = "{\"tag\":\"transform\",\"position\":{\"x\":0,\"y\":0,\"z\":0},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
    }));

    // 2nd message
    scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
      entityId = entityId,
      name = "shape",
      json = "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"box\"}"
    }));

    scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
      entityId = entityId,
      name = "transform",
      json = "{\"tag\":\"transform\",\"position\":{\"x\":6,\"y\":0,\"z\":5},\"rotation\":{\"x\":0,\"y\":0.39134957508996265,\"z\":0,\"w\":0.9202420931897769},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
    }));
  }

  public void Verify() {
    var sceneController = FindObjectOfType<SceneController>();
    var scene = sceneController.loadedScenes["the-loaded-scene"];
    var cube = scene.entities[entityId];

    Assert.IsTrue(cube != null);
    Assert.AreEqual(cube.gameObject.transform.localPosition, new Vector3(6, 0, 5));

    // because basePosition is at 3,3
    Assert.AreEqual(cube.gameObject.transform.position, new Vector3(36, 0, 35));

    var mesh = cube.gameObject.GetComponent<MeshFilter>().mesh;

    Assert.AreEqual(mesh.name, "DCL Box Instance");

    {
      // 3nd message, the box should remain the same, including references
      scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
        entityId = entityId,
        name = "shape",
        json = "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"box\"}"
      }));

      var newMesh = cube.gameObject.GetComponent<MeshFilter>().mesh;

      Assert.AreEqual(newMesh.name, "DCL Box Instance");
      Assert.IsTrue(mesh == newMesh);
    }

    {
      // 4nd message, the box should be disposed and the new mesh should be a sphere
      scene.UpdateEntityComponent(JsonConvert.SerializeObject(new {
        entityId = entityId,
        name = "shape",
        json = "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"sphere\"}"
      }));

      var newMesh = cube.gameObject.GetComponent<MeshFilter>().mesh;

      Assert.AreEqual(newMesh.name, "DCL Sphere Instance");
      Assert.IsTrue(mesh != newMesh, "meshes should change");
    }

    // TODO: test ComponentRemoved
  }
}
