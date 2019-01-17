using UnityEngine;
using DCL.Configuration;
using DCL.Controllers;
using Newtonsoft.Json;
using DCL.Components;

namespace DCL.Helpers
{
    public static class TestHelpers
    {
        public static string CreateSceneMessage(string sceneId, string method, string payload)
        {
            return $"{sceneId}\t{method}\t{payload}\n";
        }

        public static void InstantiateEntityWithShape(ParcelScene scene, string entityId, DCL.Models.CLASS_ID classId, Vector3 position, string remoteSrc = "", bool hasCollision = false)
        {
            scene.CreateEntity(entityId);

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "shape",
                classId = (int)classId,
                json = JsonConvert.SerializeObject(new
                {
                    tag = "test tag",
                    src = remoteSrc,
                    withCollisions = hasCollision
                })
            }));

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "transform",
                classId = (int)DCL.Models.CLASS_ID.TRANSFORM,
                json = JsonConvert.SerializeObject(new
                {
                    position = position,
                    scale = new Vector3(1, 1, 1),
                    rotation = new
                    {
                        x = 0,
                        y = 0,
                        z = 0,
                        w = 1
                    }
                })
            }));
        }

        public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position, BasicMaterial.Model basicMaterial, string materialComponentID = "a-material")
        {
            InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, position);

            scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.BASIC_MATERIAL,
                id = materialComponentID,
                name = "material"
            }));

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = materialComponentID,
                json = JsonUtility.ToJson(basicMaterial)
            }));

            scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = entityId,
                id = materialComponentID,
                name = "material"
            }));
        }

        public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position, PBRMaterial.Model pbrMaterial, string materialComponentID = "a-material")
        {
            InstantiateEntityWithShape(scene, entityId, DCL.Models.CLASS_ID.BOX_SHAPE, position);

            scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)DCL.Models.CLASS_ID.PBR_MATERIAL,
                id = materialComponentID,
                name = "material"
            }));

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = materialComponentID,
                json = JsonUtility.ToJson(pbrMaterial)
            }));

            scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = entityId,
                id = materialComponentID,
                name = "material"
            }));
        }

        public static SceneController InitializeSceneController(bool usesWebServer = false)
        {
            var sceneController = Object.FindObjectOfType<SceneController>();

            if (sceneController == null)
            {
                var GO = new GameObject();
                sceneController = GO.AddComponent<SceneController>();
            }

            if (usesWebServer)
            {
                var webServer = sceneController.GetComponent<WebServerComponent>();
                if (webServer != null)
                {
                    webServer.Restart(); // We restart the server to avoid issues with consecutive tests using it
                }
                else
                {
                    sceneController.gameObject.AddComponent<WebServerComponent>();
                }
            }

            sceneController.UnloadAllScenes();

            return sceneController;
        }
    }
}
