using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public class IntegrationTestController : MonoBehaviour
{
    string entityId = "a5f571bd-bce1-4cf8-a158-b8f3e92e4fb0";
    string sceneName = "the-loaded-scene";

    public IEnumerator Initialize()
    {
        var sceneController = TestHelpers.InitializeSceneController();
        DCLCharacterController.i.gravity = 0;
        DCLCharacterController.i.Teleport(JsonConvert.SerializeObject(new
        {
            x = 0f,
            y = 0f,
            z = 0f
        }));

        var scenesToLoad = new LoadParcelScenesMessage.UnityParcelScene()
        {
            id = sceneName,
            basePosition = new Vector2Int(3, 3),
            parcels = new[]
            {
                new Vector2Int(3, 3),
                new Vector2Int(3, 4)
            },
            baseUrl = "http://localhost:9991/local-ipfs/contents/"
        };

        Assert.IsTrue(sceneController != null, "Cannot find SceneController");

        sceneController.UnloadAllScenes();
        sceneController.LoadParcelScenes(JsonConvert.SerializeObject(scenesToLoad));

        yield return new WaitForAllMessagesProcessed();

        var scene = Environment.i.worldState.loadedScenes[sceneName];

        //NOTE(Brian): This is making my eyes bleed.
        sceneController.SendSceneMessage(
            TestHelpers.CreateSceneMessage(
                sceneName,
                entityId,
                "CreateEntity",
                JsonConvert.SerializeObject(
                    new Protocol.CreateEntity()
                    {
                        entityId = entityId
                    }))
        );

        //NOTE(Brian): This is making my eyes bleed. (Zak): Twice
        sceneController.SendSceneMessage(
            TestHelpers.CreateSceneMessage(
                sceneName,
                entityId,
                "SetEntityParent",
                JsonConvert.SerializeObject(
                    new
                    {
                        entityId = entityId,
                        parentId = "0"
                    })
            )
        );

        yield return new WaitForAllMessagesProcessed();

        Assert.IsTrue(scene.entities[entityId].meshRootGameObject == null, "meshGameObject must be null");

        // 1st message
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

        {
            scene.EntityComponentCreateOrUpdate(
                entityId,
                CLASS_ID_COMPONENT.TRANSFORM,
                "{\"tag\":\"transform\",\"position\":{\"x\":0,\"y\":0,\"z\":0},\"rotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
                , out CleanableYieldInstruction routine);
        }


        // 2nd message
        TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");
        {
            scene.EntityComponentCreateOrUpdate(
                entityId,
                CLASS_ID_COMPONENT.TRANSFORM,
                "{\"tag\":\"transform\",\"position\":{\"x\":6,\"y\":0,\"z\":5},\"rotation\":{\"x\":0,\"y\":0.39134957508996265,\"z\":0,\"w\":0.9202420931897769},\"scale\":{\"x\":1,\"y\":1,\"z\":1}}"
                , out CleanableYieldInstruction routine);
        }

        TestHelpers.InstantiateEntityWithTextShape(scene, new Vector3(10, 10, 10),
            new TextShape.Model() {value = "Hello World!!!"});
    }

    public IEnumerator Verify()
    {
        var scene = Environment.i.worldState.loadedScenes[sceneName];
        var cube = scene.entities[entityId];

        Assert.IsTrue(cube != null);
        Vector3 cubePosition = new Vector3(6, 0, 5);
        Assert.AreEqual(cube.gameObject.transform.localPosition, cubePosition);

        // because basePosition is at 3,3
        Assert.AreEqual(cube.gameObject.transform.position,
            new Vector3(3 * ParcelSettings.PARCEL_SIZE + cubePosition.x, cubePosition.y,
                3 * ParcelSettings.PARCEL_SIZE + cubePosition.z));
        Assert.IsTrue(cube.meshRootGameObject != null);
        Assert.IsTrue(cube.meshRootGameObject.GetComponentInChildren<MeshFilter>() != null);

        var mesh = cube.meshRootGameObject.GetComponentInChildren<MeshFilter>().mesh;

        Assert.AreEqual(mesh.name, "DCL Box Instance");

        {
            // 3nd message, the box should remain the same, including references
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

            var newMesh = cube.meshRootGameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Box Instance");
            Assert.AreEqual(mesh.name, newMesh.name, "A new instance of the box was created");
        }

        {
            // 3nd message, the box should remain the same, including references
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.BOX_SHAPE, "{}");

            var newMesh = cube.meshRootGameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Box Instance");
            Assert.AreEqual(mesh.name, newMesh.name, "A new instance of the box was created");
        }

        {
            // 4nd message, the box should be disposed and the new mesh should be a sphere
            TestHelpers.CreateAndSetShape(scene, entityId, CLASS_ID.SPHERE_SHAPE,
                "{\"withCollisions\":false,\"billboard\":0,\"visible\":true,\"tag\":\"sphere\"}");

            var newMesh = cube.meshRootGameObject.GetComponentInChildren<MeshFilter>().mesh;

            Assert.AreEqual(newMesh.name, "DCL Sphere Instance");
            Assert.AreNotEqual(mesh.name, newMesh.name,
                "The mesh instance remains the same, a new instance should have been created.");
        }

        // TODO: test ComponentRemoved

        yield return null;
    }
}