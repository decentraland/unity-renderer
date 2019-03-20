using UnityEngine;
using DCL.Configuration;
using DCL.Controllers;
using Newtonsoft.Json;
using DCL.Components;
using DCL.Models;
using UnityEngine.Assertions;

namespace DCL.Helpers
{
    public class WaitForAllMessagesProcessed : CustomYieldInstruction
    {
        public override bool keepWaiting => SceneController.i.hasPendingMessages;
    }

    public static class TestHelpers
    {
        public static string GetTestsAssetsPath(bool useWebServerPath = false)
        {
            if (useWebServerPath)
                return "http://127.0.0.1:9991";
            else
            {
                var uri = new System.Uri(Application.dataPath + "/../TestResources");
                var converted = uri.AbsoluteUri;
                return converted;
            }
        }

        public static string CreateSceneMessage(string sceneId, string method, string payload)
        {
            return $"{sceneId}\t{method}\t{payload}\n";
        }

        static int entityCounter = 123;
        static int disposableIdCounter = 123;

        public static DecentralandEntity CreateSceneEntity(ParcelScene scene)
        {
            entityCounter++;
            string id = $"{entityCounter}";
            return scene.CreateEntity(JsonUtility.ToJson(new DCL.Models.CreateEntityMessage
            {
                id = id
            }));
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

        public static T EntityComponentCreate<T, K>(ParcelScene scene, DecentralandEntity entity, K model)
            where T : BaseComponent
            where K : new()
        {
            int componentClassId = (int)scene.ownerController.componentFactory.GetIdForType<T>();
            string componentInstanceId = GetUniqueId(typeof(T).Name, componentClassId, entity.entityId);

            T result = scene.EntityComponentCreate(JsonUtility.ToJson(new EntityComponentCreateMessage
            {
                entityId = entity.entityId,
                name = componentInstanceId,
                classId = componentClassId,
                json = JsonUtility.ToJson(model)
            })) as T;

            return result;
        }



        public static T SharedComponentCreate<T, K>(ParcelScene scene, CLASS_ID id, K model) where T : BaseDisposable
        {
            disposableIdCounter++;

            string uniqueId = GetUniqueId("material", (int)id, "-shared-" + disposableIdCounter);

            T result = scene.SharedComponentCreate(JsonUtility.ToJson(new DCL.Models.SharedComponentCreateMessage
            {
                classId = (int)id,
                id = uniqueId,
                name = "material"
            })) as T;

            Assert.IsNotNull(result, "class-id mismatch!");

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = uniqueId,
                json = JsonUtility.ToJson(model)
            }));

            return result;
        }

        public static void SharedComponentAttach(BaseDisposable component, DecentralandEntity entity)
        {
            entity.scene.SharedComponentAttach(JsonUtility.ToJson(new DCL.Models.SharedComponentAttachMessage
            {
                entityId = entity.entityId,
                id = component.id,
                name = component.componentName //NOTE(Brian): useless?
            }));
        }

        public static TextShape InstantiateEntityWithTextShape(ParcelScene scene, Vector3 position, TextShape.Model model)
        {
            DecentralandEntity entity = CreateSceneEntity(scene);
            string componentId = GetUniqueId("textShape", (int)CLASS_ID_COMPONENT.TEXT_SHAPE, entity.entityId);

            TextShape textShape = EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, model);

            EntityComponentCreate<DCLTransform, DCLTransform.Model>(scene, entity,
                new DCLTransform.Model
                {
                    position = position,
                    rotation = Quaternion.identity,
                    scale = Vector3.one
                });

            return textShape;
        }

        public static GLTFShape AttachGLTFShape(DecentralandEntity entity, ParcelScene scene, Vector3 position, GLTFShape.Model model)
        {
            string componentId = GetUniqueId("gltfShape", (int)CLASS_ID.GLTF_SHAPE, entity.entityId);
            GLTFShape gltfShape = SharedComponentCreate<GLTFShape, GLTFShape.Model>(scene, CLASS_ID.GLTF_SHAPE, model);

            EntityComponentCreate<DCLTransform, DCLTransform.Model>(scene, entity,
                new DCLTransform.Model
                {
                    position = position,
                    rotation = Quaternion.identity,
                    scale = Vector3.one
                });

            SharedComponentAttach(gltfShape, entity);
            return gltfShape;
        }

        public static GLTFShape CreateEntityWithGLTFShape(ParcelScene scene, Vector3 position, GLTFShape.Model model)
        {
            DecentralandEntity entity = CreateSceneEntity(scene);
            GLTFShape gltfShape = AttachGLTFShape(entity, scene, position, model);
            return gltfShape;
        }

        public static T InstantiateEntityWithShape<T, K>(ParcelScene scene, DCL.Models.CLASS_ID classId, Vector3 position, out DecentralandEntity entity, K model)
            where T : BaseShape
            where K : class
        {
            entity = CreateSceneEntity(scene);
            string shapeId = "";

            shapeId = CreateAndSetShape(scene, entity.entityId, classId, JsonConvert.SerializeObject(model));

            T shape = scene.disposableComponents[shapeId] as T;

            EntityComponentCreate<DCLTransform, DCLTransform.Model>(scene, entity,
            new DCLTransform.Model
            {
                position = position,
                rotation = Quaternion.identity,
                scale = Vector3.one
            });

            return shape;
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

            EntityComponentCreate<DCLTransform, DCLTransform.Model>(scene, scene.entities[entityId],
            new DCLTransform.Model
            {
                position = position,
                rotation = Quaternion.identity,
                scale = Vector3.one
            });
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

        public static string GetUniqueId(string salt, int classId, string entityId)
        {
            return salt + "-" + (int)classId + "-" + entityId;
        }

        public static string CreateAndSetShape(ParcelScene scene, string entityId, CLASS_ID classId, string model)
        {
            string componentId = GetUniqueId("shape", (int)classId, entityId);

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

            if (sceneController != null && sceneController.componentFactory == null)
            {
                Utils.SafeDestroy(sceneController);
                sceneController = null;
            }

            if (sceneController == null)
            {
                GameObject GO = GameObject.Instantiate(Resources.Load("Prefabs/SceneController") as GameObject);
                sceneController = GO.GetComponent<SceneController>();
            }

            AssetManager_GLTF assetMgr = sceneController.GetComponentInChildren<AssetManager_GLTF>();
            Assert.IsTrue(assetMgr != null, "AssetManager is null???");
            assetMgr.ClearLibrary();

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

            Configuration.ParcelSettings.VISUAL_LOADING_ENABLED = false;
            sceneController.UnloadAllScenes();

            return sceneController;
        }
    }
}
