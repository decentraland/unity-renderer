using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

public class IntegrationTestController : MonoBehaviour
{
    string entityId = "a5f571bd-bce1-4cf8-a158-b8f3e92e4fb0";

    void Awake()
    {
        var sceneController = Object.FindObjectOfType<SceneController>();

        var scenesToLoad = new
        {
            parcelsToLoad = new[] {
        new LoadParcelScenesMessage.UnityParcelScene() {
          id = "the-loaded-scene",
          basePosition = new Vector2Int(3, 3),
          parcels = new [] {
            new Vector2Int(3, 3),
            new Vector2Int(3, 4)
          },
          baseUrl = "http://localhost:9991/local-ipfs/contents/",
          owner = "0x0f5d2fb29fb7d3cfee444a200298f468908cc942"
        }
      }
        };

        Assert.IsTrue(sceneController != null, "Cannot find SceneController");

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(JsonConvert.SerializeObject(scenesToLoad));

        var scene = sceneController.loadedScenes["the-loaded-scene"];

        sceneController.SendSceneMessage(
          TestHelpers.CreateSceneMessage("the-loaded-scene", "CreateEntity", entityId)
        );

        sceneController.SendSceneMessage(
          TestHelpers.CreateSceneMessage(
            "the-loaded-scene",
            "SetEntityParent",
            JsonConvert.SerializeObject(new
            {
                entityId = entityId,
                parentId = "0"
            })
          )
        );

        // 1st message
        scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
        {
            entityId = entityId,
            name = "shape",
            classId = (int)CLASS_ID.BOX_SHAPE,
            json = "{}"
        }));

        scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
        {
            entityId = entityId,
            name = "transform",
            classId = (int)CLASS_ID.TRANSFORM,
            json = "{\"tag\":\"transform\",\"position\":{\"x\":0,\"y\":0,\"z\":0},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
        }));

        // 2nd message
        scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
        {
            entityId = entityId,
            name = "shape",
            classId = (int)CLASS_ID.BOX_SHAPE,
            json = "{}"
        }));

        scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
        {
            entityId = entityId,
            name = "transform",
            classId = (int)CLASS_ID.TRANSFORM,
            json = "{\"tag\":\"transform\",\"position\":{\"x\":6,\"y\":0,\"z\":5},\"rotation\":{\"x\":0,\"y\":0.39134957508996265,\"z\":0,\"w\":0.9202420931897769},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
        }));
    }

    public void Verify()
    {
        var sceneController = FindObjectOfType<SceneController>();
        var scene = sceneController.loadedScenes["the-loaded-scene"];
        var cube = scene.entities[entityId];

        Assert.IsTrue(cube != null);
        Assert.AreEqual(cube.gameObject.transform.localPosition, new Vector3(6, 0, 5));

        // because basePosition is at 3,3
        Assert.AreEqual(cube.gameObject.transform.position, new Vector3(36, 0, 35));

        var mesh = cube.gameObject.GetComponentInChildren<MeshFilter>().mesh;

        Assert.AreEqual(mesh.name, "DCL Box Instance");

        {
            // 3nd message, the box should remain the same, including references
            scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
            {
                entityId = entityId,
                name = "shape",
                classId = (int)CLASS_ID.BOX_SHAPE,
                json = "{}"
            }));

            var newMesh = cube.gameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Box Instance");
            Assert.IsTrue(mesh == newMesh, "A new instance of the box was created");
        }

        {
            // 3nd message, the box should remain the same, including references
            scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
            {
                entityId = entityId,
                name = "shape",
                classId = (int)CLASS_ID.BOX_SHAPE,
                json = "{}"
            }));

            var newMesh = cube.gameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Box Instance");
            Assert.IsTrue(mesh == newMesh, "A new instance of the box was created");
        }

        {
            // 4nd message, the box should be disposed and the new mesh should be a sphere
            scene.UpdateEntityComponent(JsonConvert.SerializeObject(new UpdateEntityComponentMessage
            {
                entityId = entityId,
                name = "shape",
                classId = (int)CLASS_ID.SPHERE_SHAPE,
                json = "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"sphere\"}"
            }));

            var newMesh = cube.gameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Sphere Instance");
            Assert.IsTrue(mesh != newMesh, "The mesh instance remains the same, a new instance should have been created.");
        }

        // TODO: test ComponentRemoved
    }
}
