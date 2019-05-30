using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace DCL.Helpers
{
    public class WaitForAllMessagesProcessed : CustomYieldInstruction
    {
        public override bool keepWaiting => SceneController.i.hasPendingMessages;
    }

    // NOTE(Brian): Attribute used to determine if tests are visual. Those tests will be run to generate the baseline images.
    [AttributeUsage(AttributeTargets.Method)]
    public class VisualTestAttribute : Attribute
    {
    }

    public class VisualTestsBase : TestsBase
    {
        protected override IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true)
        {
            yield return InitUnityScene("MainVisualTest");

            sceneController = TestHelpers.InitializeSceneController(usesWebServer);

            yield return new WaitForSeconds(0.01f);

            scene = sceneController.CreateTestScene();

            yield return new WaitForSeconds(0.01f);

            if (spawnCharController)
            {
                if (DCLCharacterController.i == null)
                {
                    GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
                }
            }
        }
    }

    public class TestsBase
    {
        protected SceneController sceneController;
        protected ParcelScene scene;

        protected IEnumerator InitUnityScene(string sceneName = null)
        {
            yield return TestHelpers.UnloadAllUnityScenes();

            Scene? newScene;

            if (string.IsNullOrEmpty(sceneName))
            {
                newScene = SceneManager.CreateScene(TestHelpers.testingSceneName + (TestHelpers.testSceneIteration++));
                if (newScene.HasValue)
                {
                    SceneManager.SetActiveScene(newScene.Value);
                }
            }
            else
            {
                yield return SceneManager.LoadSceneAsync(sceneName);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            }
        }

        protected virtual IEnumerator InitScene(bool usesWebServer = false, bool spawnCharController = true)
        {
            yield return InitUnityScene();

            sceneController = TestHelpers.InitializeSceneController(usesWebServer);

            yield return new WaitForSeconds(0.01f);

            scene = sceneController.CreateTestScene();

            yield return new WaitForSeconds(0.01f);

            if (spawnCharController)
            {
                if (DCLCharacterController.i == null)
                {
                    GameObject.Instantiate(Resources.Load("Prefabs/CharacterController"));
                }
            }
        }

        protected IEnumerator WaitForUICanvasUpdate()
        {
            yield break;
        }

        protected Vector2 CalculateAlignedPosition(Rect parentRect, Rect elementRect, string vAlign = "center",
            string hAlign = "center")
        {
            Vector2 result = Vector2.zero;

            switch (vAlign)
            {
                case "top":
                    result.y = -elementRect.height / 2;
                    break;
                case "bottom":
                    result.y = -(parentRect.height - elementRect.height / 2);
                    break;
                default: // center
                    result.y = -parentRect.height / 2;
                    break;
            }

            switch (hAlign)
            {
                case "left":
                    result.x = elementRect.width / 2;
                    break;
                case "right":
                    result.x = (parentRect.width - elementRect.width / 2);
                    break;
                default: // center
                    result.x = parentRect.width / 2;
                    break;
            }

            return result;
        }
    }

    public static class TestHelpers
    {
        public static int testSceneIteration;
        public const string testingSceneName = "DCL_Testing_";

        public static string GetTestsAssetsPath(bool useWebServerPath = false)
        {
            if (useWebServerPath)
            {
                return "http://127.0.0.1:9991";
            }
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

        public static T EntityComponentCreate<T, K>(ParcelScene scene, DecentralandEntity entity, K model,
            CLASS_ID_COMPONENT classId = CLASS_ID_COMPONENT.NONE)
            where T : BaseComponent
            where K : new()
        {
            int componentClassId = classId == CLASS_ID_COMPONENT.NONE
                ? (int)scene.ownerController.componentFactory.GetIdForType<T>()
                : (int)classId;
            string componentInstanceId = GetComponentUniqueId(scene, typeof(T).Name, componentClassId, entity.entityId);

            return scene.EntityComponentCreate(JsonUtility.ToJson(new EntityComponentCreateMessage
            {
                entityId = entity.entityId,
                name = componentInstanceId,
                classId = componentClassId,
                json = JsonUtility.ToJson(model)
            })) as T;
        }

        public static void SetEntityParent(ParcelScene scene, string childEntityId, string parentEntityId)
        {
            scene.SetEntityParent(JsonUtility.ToJson(new SetEntityParentMessage
            {
                entityId = childEntityId,
                parentId = parentEntityId
            }));
        }

        public static DCLTexture CreateDCLTexture(ParcelScene scene,
            string url,
            DCLTexture.BabylonWrapMode wrapMode = DCLTexture.BabylonWrapMode.CLAMP,
            FilterMode filterMode = FilterMode.Bilinear)
        {
            return SharedComponentCreate<DCLTexture, DCLTexture.Model>
            (
                scene,
                DCL.Models.CLASS_ID.TEXTURE,
                new DCLTexture.Model
                {
                    src = url,
                    wrap = wrapMode,
                    samplingMode = filterMode,
                }
            );
        }

        public static void SharedComponentUpdate<T, K>(ParcelScene scene, T component, K model = null)
            where T : BaseDisposable
            where K : class, new()
        {
            if (model == null)
            {
                model = new K();
            }

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = component.id,
                json = JsonUtility.ToJson(model)
            }));
        }

        public static T SharedComponentCreate<T, K>(ParcelScene scene, CLASS_ID id, K model = null)
            where T : BaseDisposable
            where K : class, new()
        {
            if (model == null)
            {
                model = new K();
            }

            disposableIdCounter++;

            string uniqueId = GetComponentUniqueId(scene, "material", (int)id, "-shared-" + disposableIdCounter);

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
                id = component.id
            }));
        }

        public static void SetEntityTransform(ParcelScene scene, DecentralandEntity entity, DCLTransform.Model model)
        {
            SetEntityTransform(scene, entity, model.position, model.rotation, model.scale);
        }

        public static void SetEntityTransform(ParcelScene scene, DecentralandEntity entity)
        {
            SetEntityTransform(scene, entity, Vector3.zero, Quaternion.identity, Vector3.one);
        }

        public static void SetEntityTransform(ParcelScene scene, DecentralandEntity entity, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            scene.EntityComponentCreate(JsonUtility.ToJson(new EntityComponentCreateMessage
            {
                entityId = entity.entityId,
                name = "",
                classId = (int)CLASS_ID_COMPONENT.TRANSFORM,
                json = JsonConvert.SerializeObject(new
                {
                    position = position,
                    rotation = rotation,
                    scale = scale
                })
            }));
        }

        public static TextShape InstantiateEntityWithTextShape(ParcelScene scene, Vector3 position, TextShape.Model model)
        {
            DecentralandEntity entity = CreateSceneEntity(scene);
            string componentId =
                GetComponentUniqueId(scene, "textShape", (int)CLASS_ID_COMPONENT.TEXT_SHAPE, entity.entityId);

            TextShape textShape = EntityComponentCreate<TextShape, TextShape.Model>(scene, entity, model);

            SetEntityTransform(scene, entity);

            return textShape;
        }

        public static GLTFShape AttachGLTFShape(DecentralandEntity entity, ParcelScene scene, Vector3 position, GLTFShape.Model model)
        {
            string componentId = GetComponentUniqueId(scene, "gltfShape", (int)CLASS_ID.GLTF_SHAPE, entity.entityId);
            GLTFShape gltfShape = SharedComponentCreate<GLTFShape, GLTFShape.Model>(scene, CLASS_ID.GLTF_SHAPE, model);

            SetEntityTransform(scene, entity, position, Quaternion.identity, Vector3.one);
            SharedComponentAttach(gltfShape, entity);
            return gltfShape;
        }

        public static GLTFShape CreateEntityWithGLTFShape(ParcelScene scene, Vector3 position, string url)
        {
            DecentralandEntity entity = null;
            return CreateEntityWithGLTFShape(scene, position, new GLTFShape.Model() { src = url }, out entity);
        }

        public static GLTFShape CreateEntityWithGLTFShape(ParcelScene scene, Vector3 position, string url,
            out DecentralandEntity entity)
        {
            return CreateEntityWithGLTFShape(scene, position, new GLTFShape.Model() { src = url }, out entity);
        }

        public static GLTFShape CreateEntityWithGLTFShape(ParcelScene scene, Vector3 position, GLTFShape.Model model)
        {
            DecentralandEntity entity = null;
            return CreateEntityWithGLTFShape(scene, position, model, out entity);
        }

        public static GLTFShape CreateEntityWithGLTFShape(ParcelScene scene, Vector3 position, GLTFShape.Model model,
            out DecentralandEntity entity)
        {
            entity = CreateSceneEntity(scene);
            GLTFShape gltfShape = AttachGLTFShape(entity, scene, position, model);
            return gltfShape;
        }

        public static BoxShape CreateEntityWithBoxShape(ParcelScene scene, Vector3 position,
            BoxShape.Model model = null)
        {
            return CreateEntityWithPrimitive<BoxShape, BoxShape.Model>(scene, position, CLASS_ID.BOX_SHAPE, model);
        }

        public static BoxShape CreateEntityWithBoxShape(ParcelScene scene, Vector3 position, bool visible)
        {
            return CreateEntityWithBoxShape(scene, position, new BoxShape.Model { visible = visible });
        }

        public static SphereShape CreateEntityWithSphereShape(ParcelScene scene, Vector3 position,
            SphereShape.Model model = null)
        {
            return CreateEntityWithPrimitive<SphereShape, SphereShape.Model>(scene, position, CLASS_ID.SPHERE_SHAPE,
                model);
        }

        public static SphereShape CreateEntityWithSphereShape(ParcelScene scene, Vector3 position, bool visible)
        {
            return CreateEntityWithSphereShape(scene, position, new SphereShape.Model { visible = visible });
        }

        public static PlaneShape CreateEntityWithPlaneShape(ParcelScene scene, Vector3 position,
            PlaneShape.Model model = null)
        {
            return CreateEntityWithPrimitive<PlaneShape, PlaneShape.Model>(scene, position, CLASS_ID.PLANE_SHAPE,
                model);
        }

        public static PlaneShape CreateEntityWithPlaneShape(ParcelScene scene, Vector3 position, bool visible)
        {
            return CreateEntityWithPlaneShape(scene, position, new PlaneShape.Model { visible = visible });
        }

        public static CylinderShape CreateEntityWithCylinderShape(ParcelScene scene, Vector3 position,
            CylinderShape.Model model = null)
        {
            return CreateEntityWithPrimitive<CylinderShape, CylinderShape.Model>(scene, position,
                CLASS_ID.CYLINDER_SHAPE, model);
        }

        public static CylinderShape CreateEntityWithCylinderShape(ParcelScene scene, Vector3 position, bool visible)
        {
            return CreateEntityWithCylinderShape(scene, position, new CylinderShape.Model { visible = visible });
        }

        public static ConeShape CreateEntityWithConeShape(ParcelScene scene, Vector3 position,
            ConeShape.Model model = null)
        {
            return CreateEntityWithPrimitive<ConeShape, ConeShape.Model>(scene, position, CLASS_ID.CONE_SHAPE, model);
        }

        public static ConeShape CreateEntityWithConeShape(ParcelScene scene, Vector3 position, bool visible)
        {
            return CreateEntityWithConeShape(scene, position, new ConeShape.Model { visible = visible });
        }

        private static T CreateEntityWithPrimitive<T, K>(ParcelScene scene, Vector3 position, CLASS_ID classId,
            K model = null)
            where T : BaseParametrizedShape<K>
            where K : BaseShape.Model, new()
        {
            if (model == null)
            {
                model = new K();
            }

            DecentralandEntity entity = CreateSceneEntity(scene);
            T shape = SharedComponentCreate<T, K>(scene, classId, model);
            SharedComponentAttach(shape, entity);
            SetEntityTransform(scene, entity, position, Quaternion.identity, Vector3.one);
            return shape;
        }

        public static BasicMaterial CreateEntityWithBasicMaterial(ParcelScene scene, BasicMaterial.Model model,
            out DecentralandEntity entity)
        {
            InstantiateEntityWithShape<BoxShape, BoxShape.Model>(scene, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero,
                out entity);
            BasicMaterial material =
                SharedComponentCreate<BasicMaterial, BasicMaterial.Model>(scene, CLASS_ID.BASIC_MATERIAL, model);
            SharedComponentAttach(material, entity);
            return material;
        }

        public static PBRMaterial CreateEntityWithPBRMaterial(ParcelScene scene, PBRMaterial.Model model,
            out DecentralandEntity entity)
        {
            InstantiateEntityWithShape<BoxShape, BoxShape.Model>(scene, DCL.Models.CLASS_ID.BOX_SHAPE, Vector3.zero,
                out entity);
            PBRMaterial material =
                SharedComponentCreate<PBRMaterial, PBRMaterial.Model>(scene, CLASS_ID.PBR_MATERIAL, model);
            SharedComponentAttach(material, entity);
            return material;
        }

        public static T InstantiateEntityWithShape<T, K>(ParcelScene scene, DCL.Models.CLASS_ID classId,
            Vector3 position, out DecentralandEntity entity, K model = null)
            where T : BaseShape
            where K : class, new()
        {
            if (model == null)
            {
                model = new K();
            }

            entity = CreateSceneEntity(scene);
            string shapeId = "";

            shapeId = CreateAndSetShape(scene, entity.entityId, classId, JsonConvert.SerializeObject(model));

            T shape = scene.disposableComponents[shapeId] as T;

            SetEntityTransform(scene, entity, position, Quaternion.identity, Vector3.one);

            return shape;
        }

        public static void InstantiateEntityWithShape(ParcelScene scene, string entityId, DCL.Models.CLASS_ID classId,
            Vector3 position, string remoteSrc = "")
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

            SetEntityTransform(scene, scene.entities[entityId], position, Quaternion.identity, Vector3.one);
        }

        public static void DetachSharedComponent(ParcelScene scene, string fromEntityId, string sharedComponentId)
        {
            DecentralandEntity entity = null;

            if (!scene.entities.TryGetValue(fromEntityId, out entity))
            {
                return;
            }

            scene.GetSharedComponent(sharedComponentId).DetachFrom(entity);
        }

        public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position,
            BasicMaterial.Model basicMaterial, string materialComponentID = "a-material")
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


        public static void InstantiateEntityWithMaterial(ParcelScene scene, string entityId, Vector3 position,
            PBRMaterial.Model pbrMaterial, string materialComponentID = "a-material")
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

        public static string GetComponentUniqueId(ParcelScene scene, string salt, int classId, string entityId)
        {
            string baseId = salt + "-" + (int)classId + "-" + entityId;
            string finalId = baseId;

            while (scene.GetSharedComponent(finalId) != null)
            {
                finalId = baseId + UnityEngine.Random.Range(1, 10000);
            }

            return finalId;
        }

        public static string CreateAndSetShape(ParcelScene scene, string entityId, CLASS_ID classId, string model)
        {
            string componentId = GetComponentUniqueId(scene, "shape", (int)classId, entityId);

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

        static object GetRandomValueForType(Type t)
        {
            if (t == typeof(float))
            {
                return 79014.5f;
            }
            else if (t == typeof(int))
            {
                return 79014;
            }
            else if (t == typeof(string))
            {
                return "test";
            }
            else if (t == typeof(Color))
            {
                return Color.magenta;
            }
            else if (t == typeof(object))
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }

        public static void CompareWithDefaultedInstance<TModel, TComponent>(TComponent component)
            where TModel : class, new()
            where TComponent : IComponent
        {
            MemberInfo modelMember = null;

            modelMember = typeof(TComponent).GetRuntimeProperties().FirstOrDefault((x) => x.Name == "model");

            if (modelMember == null)
            {
                modelMember = typeof(TComponent).GetRuntimeFields().FirstOrDefault((x) => x.Name == "model");
            }

            Assert.IsTrue(modelMember != null, "model is null!!");
            TModel defaultedModel = new TModel();

            foreach (FieldInfo f in typeof(TModel).GetFields())
            {
                System.Type t = f.GetType();

                object model = null;

                if (modelMember is FieldInfo)
                {
                    model = (modelMember as FieldInfo).GetValue(component);
                }
                else if (modelMember is PropertyInfo)
                {
                    model = (modelMember as PropertyInfo).GetValue(component);
                }

                object defaultValue = f.GetValue(defaultedModel);
                object modelValue = f.GetValue(model);
                string fieldName = f.Name;

                //NOTE(Brian): Corner case, strings are defaulted as null, but json deserialization inits them as ""
                if (modelValue is string && string.IsNullOrEmpty(modelValue as string))
                {
                    modelValue = null;
                }

                Assert.AreEqual(defaultValue, modelValue, $"Checking {fieldName} failed! Is not default value! error!");
            }
        }

        public static IEnumerator TestEntityComponentDefaultsOnUpdate<TModel, TComponent>(ParcelScene scene)
            where TComponent : BaseComponent
            where TModel : class, new()
        {
            TModel generatedModel = new TModel();

            foreach (FieldInfo f in typeof(TModel).GetFields())
            {
                System.Type t = f.FieldType;
                object valueToSet = GetRandomValueForType(t);
                f.SetValue(generatedModel, valueToSet);
            }

            DecentralandEntity e = CreateSceneEntity(scene);
            TComponent component = EntityComponentCreate<TComponent, TModel>(scene, e, generatedModel);

            if (component.routine != null)
            {
                yield return component.routine;
            }

            int id = (int)scene.ownerController.componentFactory.GetIdForType<TComponent>();

            scene.EntityComponentUpdate(e, (CLASS_ID_COMPONENT)id, "{}");

            if (component.routine != null)
            {
                yield return component.routine;
            }

            CompareWithDefaultedInstance<TModel, TComponent>(component);
            TestHelpers.RemoveSceneEntity(scene, e.entityId);
        }

        public static IEnumerator TestAttachedSharedComponentOfSameTypeIsReplaced<TModel, TComponent>(ParcelScene scene,
            CLASS_ID classId)
            where TComponent : BaseDisposable
            where TModel : class, new()
        {
            // Create scene entity and 1st component
            DecentralandEntity entity = CreateSceneEntity(scene);

            var component = SharedComponentCreate<TComponent, TModel>(scene, classId);

            if (component.routine != null)
            {
                yield return component.routine;
            }

            Type componentType = typeof(TComponent);

            if (component is BaseShape)
            {
                componentType = typeof(BaseShape);
            }

            // Attach 1st component to entity
            TestHelpers.SharedComponentAttach(component, entity);

            Assert.IsTrue(entity.GetSharedComponent(componentType) != null);
            Assert.AreEqual(component, entity.GetSharedComponent(componentType));

            // Assign 2nd component to same entity
            var component2 = SharedComponentCreate<TComponent, TModel>(scene, classId);

            if (component2.routine != null)
            {
                yield return component2.routine;
            }

            TestHelpers.SharedComponentAttach(component2, entity);

            Assert.IsTrue(entity.GetSharedComponent(componentType) != null);
            Assert.AreEqual(component2, entity.GetSharedComponent(componentType));
            Assert.IsFalse(component.attachedEntities.Contains(entity));
        }

        public static IEnumerator TestSharedComponentDefaultsOnUpdate<TModel, TComponent>(ParcelScene scene,
            CLASS_ID id)
            where TComponent : BaseDisposable
            where TModel : class, new()
        {
            TComponent component = TestHelpers.SharedComponentCreate<TComponent, TModel>(scene, id);

            if (component.routine != null)
            {
                yield return component.routine;
            }

            TModel generatedModel = new TModel();

            foreach (FieldInfo f in typeof(TModel).GetFields())
            {
                System.Type t = f.FieldType;
                object valueToSet = GetRandomValueForType(t);
                f.SetValue(generatedModel, valueToSet);
            }

            SharedComponentUpdate(scene, component, generatedModel);

            if (component.routine != null)
            {
                yield return component.routine;
            }

            scene.SharedComponentUpdate(JsonUtility.ToJson(new DCL.Models.SharedComponentUpdateMessage
            {
                id = component.id,
                json = "{}"
            }));

            yield return component.routine;

            CompareWithDefaultedInstance<TModel, TComponent>(component);

            component.Dispose();
        }

        public static IEnumerator UnloadAllUnityScenes()
        {
            for (int i = SceneManager.sceneCount - 1; i >= 0; i--)
            {
                var scene = SceneManager.GetSceneAt(i);

                if (scene.name.Contains(testingSceneName))
                {
                    yield return SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        public static SceneController InitializeSceneController(bool usesWebServer = false)
        {
            var sceneController = UnityEngine.Object.FindObjectOfType<SceneController>();

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

        static string lastMessageFromEngineType;
        static string lastMessageFromEnginePayload;

        public static IEnumerator WaitForMessageFromEngine(string targetMessageType, string targetMessageJSONPayload,
            System.Action OnIterationStart, System.Action OnSuccess)
        {
            // Hook up to web interface engine message reporting
            DCL.Interface.WebInterface.OnMessageFromEngine += OnMessageFromEngine;

            lastMessageFromEngineType = "";
            lastMessageFromEnginePayload = "";

            bool awaitedConditionMet = false;
            yield return new DCL.WaitUntil(() =>
            {
                OnIterationStart?.Invoke();

                if (lastMessageFromEngineType == targetMessageType &&
                    lastMessageFromEnginePayload == targetMessageJSONPayload)
                {
                    DCL.Interface.WebInterface.OnMessageFromEngine -= OnMessageFromEngine;

                    OnSuccess?.Invoke();
                    awaitedConditionMet = true;
                }

                return awaitedConditionMet;
            }, 2f);
        }

        // Used in WaitForMessageFromEngine() to capture the triggered event messages from engine
        static void OnMessageFromEngine(string eventType, string eventPayload)
        {
            lastMessageFromEngineType = eventType;
            lastMessageFromEnginePayload = eventPayload;
        }

        public static void TestRectTransformMaxStretched(RectTransform rt)
        {
            Assert.AreEqual(Vector2.zero, rt.anchorMin,
                $"Rect transform {rt.name} isn't stretched out!. unexpected anchorMin value.");
            Assert.AreEqual(Vector2.zero, rt.offsetMin,
                $"Rect transform {rt.name} isn't stretched out!. unexpected offsetMin value.");
            Assert.AreEqual(Vector2.one, rt.anchorMax,
                $"Rect transform {rt.name} isn't stretched out!. unexpected anchorMax value.");
            Assert.AreEqual(Vector2.one, rt.offsetMax,
                $"Rect transform {rt.name} isn't stretched out!. unexpected offsetMax value.");
            Assert.AreEqual(Vector2.zero, rt.sizeDelta,
                $"Rect transform {rt.name} isn't stretched out!. unexpected sizeDelta value.");
        }

        public static void ForceUnloadAllScenes(SceneController sceneController)
        {
            if (sceneController == null)
            {
                return;
            }

            foreach (var keyValuePair in sceneController.loadedScenes.Where(x => x.Value.isPersistent))
            {
                keyValuePair.Value.isPersistent = false;
            }

            sceneController.UnloadAllScenes();
        }
    }
}
