using UnityEngine;
using DCL.Configuration;
using DCL.Controllers;
using Newtonsoft.Json;
using DCL.Components;
using DCL.Models;

namespace DCL.Helpers
{
    public static class TestHelpers
    {
        public static string CreateSceneMessage(string sceneId, string method, string payload)
        {
            return $"{sceneId}\t{method}\t{payload}\n";
        }

        public static void CreateSceneEntity(ParcelScene scene, string id)
        {
            scene.CreateEntity(JsonUtility.ToJson(new DCL.Models.CreateEntityMessage
            {
                id = id
            }));
        }

        public static void RemoveSceneEntity(ParcelScene scene, string id)
        {
            scene.RemoveEntity(JsonUtility.ToJson(new DCL.Models.RemoveEntityMessage
            {
                id = id
            }));
        }

        public static void InstantiateEntityWithShape(ParcelScene scene, string entityId, DCL.Models.CLASS_ID classId, Vector3 position, string remoteSrc = "")
        {
            CreateSceneEntity(scene, entityId);

            if (string.IsNullOrEmpty(remoteSrc))
            {
                CreateAndSetShape(scene, entityId, classId, "{}");
            }
            else
            {
                CreateAndSetShape(scene, entityId, classId, JsonConvert.SerializeObject(new
                {
                    src = remoteSrc
                }));
            }

            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityId,
                name = "transform",
                classId = (int)DCL.Models.CLASS_ID_COMPONENT.TRANSFORM,
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

        public static void DetachSharedComponent(ParcelScene scene, string fromEntityId, string sharedComponentId)
        {
            DecentralandEntity entity = null;

            if (!scene.entities.TryGetValue(fromEntityId, out entity))
                return;

            scene.GetSharedComponent(sharedComponentId).DetachFrom(entity);

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


        public static string CreateAndSetShape(ParcelScene scene, string entityId, CLASS_ID classId, string model)
        {
            string componentId = "shape-" + (int)classId + "-" + entityId;

            scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)classId,
                id = componentId,
                name = "shape"
            }));

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = componentId,
                json = model
            }));

            scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = entityId,
                id = componentId,
                name = "shape"
            }));

            return componentId;
        }

        public static void AddOnClickComponent(ParcelScene scene, string entityID, string uuid)
        {
            scene.EntityComponentCreate(JsonUtility.ToJson(new DCL.Models.EntityComponentCreateMessage
            {
                entityId = entityID,
                name = "onClick",
                classId = (int)CLASS_ID_COMPONENT.UUID_CALLBACK,
                json = JsonUtility.ToJson(new DCL.Models.UUIDCallbackMessage
                {
                    type = "onClick",
                    uuid = uuid
                })
            }));
        }

        public static SceneController InitializeSceneController(bool usesWebServer = false)
        {
            var sceneController = Object.FindObjectOfType<SceneController>();

            if (sceneController == null)
            {
                GameObject GO = GameObject.Instantiate(Resources.Load("Prefabs/SceneController") as GameObject);
                
                sceneController = GO.GetComponent<SceneController>();
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
