using Cysharp.Threading.Tasks;
using System;
using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECSRuntime;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using RPC.Context;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Environment = DCL.Environment;
using Object = UnityEngine.Object;

public class SceneTests : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;
    private CoreComponentsPlugin coreComponentsPlugin;
    private UUIDEventsPlugin uuidComponentsPlugin;
    private ISceneController sceneController => DCL.Environment.i.world.sceneController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
        scene.isPersistent = false;
        coreComponentsPlugin = new CoreComponentsPlugin();
        uuidComponentsPlugin = new UUIDEventsPlugin();

        DataStore.i.debugConfig.isDebugMode.Set(true);
    }

    protected override IEnumerator TearDown()
    {
        coreComponentsPlugin.Dispose();
        uuidComponentsPlugin.Dispose();
        yield return base.TearDown();
    }

    protected override ServiceLocator InitializeServiceLocator()
    {
        var result = base.InitializeServiceLocator();
        result.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
        return result;
    }

    [UnityTest]
    public IEnumerator CreateGlobalScene()
    {
        // Position character inside parcel (0,0)
        TestUtils.SetCharacterPosition(Vector3.zero);

        string sceneGameObjectNamePrefix = "Global Scene - ";
        int sceneNumber = 56;
        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage() { sceneNumber = sceneNumber });

        GameObject sceneGo = GameObject.Find(sceneGameObjectNamePrefix + sceneNumber);

        GlobalScene globalScene = Environment.i.world.state.GetScene(sceneNumber) as GlobalScene;

        Assert.IsTrue(globalScene != null, "Scene isn't a GlobalScene?");
        Assert.IsTrue(sceneGo != null, "scene game object not found!");
        Assert.IsTrue(Environment.i.world.state.GetScene(sceneNumber) != null, "Scene not in loaded dictionary!");
        Assert.IsTrue(globalScene.unloadWithDistance == false,
            "Scene will unload when far!");

        Assert.IsTrue(globalScene.IsInsideSceneBoundaries(new Vector2Int(1000, 1000)),
            "IsInsideSceneBoundaries() should always return true.");
        Assert.IsTrue(globalScene.IsInsideSceneBoundaries(new Vector2Int(-1000, -1000)),
            "IsInsideSceneBoundaries() should always return true.");

        yield return null;

        // Position character inside parcel (0,0)
        TestUtils.SetCharacterPosition(new Vector3(100f, 0f, 100f));

        yield return null;

        sceneGo = GameObject.Find(sceneGameObjectNamePrefix + sceneNumber);

        Assert.IsTrue(sceneGo != null, "scene game object not found! GlobalScenes must not be unloaded by distance!");
        Assert.IsTrue(Environment.i.world.state.GetScene(sceneNumber) != null,
            "Scene not in loaded dictionary when far! GlobalScenes must not be unloaded by distance!");
    }

    [UnityTest]
    public IEnumerator CreateSdk7GlobalScene()
    {

        Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>();
        CRDTServiceContext rpcCrdtServiceContext = Substitute.For<CRDTServiceContext>();
        ECSComponentsManager componentsManager = new ECSComponentsManager(new Dictionary<int, ECSComponentsFactory.ECSComponentBuilder>());
        CrdtExecutorsManager executorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController, rpcCrdtServiceContext);

        int sceneNumberWithoutSdk7 = 83;
        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage() { sceneNumber = sceneNumberWithoutSdk7 });
        Assert.AreEqual(0, crdtExecutors.Count);

        int sceneNumberWithSdk7 = 84;
        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage() { sceneNumber = sceneNumberWithSdk7, sdk7 = true});
        Assert.AreEqual(1, crdtExecutors.Count);

        sceneController.UnloadParcelSceneExecute(sceneNumberWithoutSdk7);
        sceneController.UnloadParcelSceneExecute(sceneNumberWithSdk7);
        executorsManager.Dispose();

        yield break;
    }

    [UnityTest]
    public IEnumerator UnloadGlobalScene()
    {
        int sceneNumber = 56;

        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage() { sceneNumber = sceneNumber });

        Assert.IsTrue(Environment.i.world.state.ContainsScene(sceneNumber), "Scene not in loaded dictionary!");

        sceneController.UnloadParcelSceneExecute(sceneNumber);

        Assert.IsFalse(Environment.i.world.state.ContainsScene(sceneNumber), "Scene not unloaded correctly!");

        yield break;
    }


    [UnityTest]
    public IEnumerator TrackPortableExperiencesInDataStore()
    {
        DataStore_World worldData = DataStore.i.Get<DataStore_World>();
        string sceneId = "Test Global Scene";
        int sceneNumber = 56;

        GameObject experiencesViewerMockedGo = new GameObject();
        DataStore.i.experiencesViewer.isInitialized.Set(experiencesViewerMockedGo.transform);

        // Ensure its added to DataStore when created
        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage()
            {id = sceneId, sceneNumber = sceneNumber, isPortableExperience = true});
        Assert.IsTrue(worldData.portableExperienceIds.Contains(sceneId));

        // Ensure its removed from DataStore when unloaded
        sceneController.UnloadParcelSceneExecute(sceneNumber);
        Assert.IsFalse(worldData.portableExperienceIds.Contains(sceneId));

        // If re-added when isPortableExperience is false, then it shouldn't be in the data store
        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage()
            {id = sceneId, sceneNumber = sceneNumber, isPortableExperience = false});
        Assert.IsFalse(worldData.portableExperienceIds.Contains(sceneId));

        // Whe re-added with isPortableExperience as true, it should work again
        sceneController.UnloadParcelSceneExecute(sceneNumber);
        sceneController.CreateGlobalScene(new CreateGlobalSceneMessage()
            {id = sceneId, sceneNumber = sceneNumber, isPortableExperience = true});
        Assert.IsTrue(worldData.portableExperienceIds.Contains(sceneId));

        Object.Destroy(experiencesViewerMockedGo);
        DataStore.i.experiencesViewer.isInitialized.Set(null);
        yield break;
    }


    [UnityTest]
    public IEnumerator LoadScene()
    {
        sceneController.LoadParcelScenes(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/MainScripts/DCL/WorldRuntime/Tests/TestJSON/SceneLoadingTest.json").text);
        yield return new WaitForAllMessagesProcessed();

        string loadedSceneID = "0,0";
        int loadedSceneNumber = 666;

        Assert.IsTrue(Environment.i.world.state.ContainsScene(loadedSceneNumber));
        Assert.IsTrue(Environment.i.world.state.GetScene(loadedSceneNumber) != null);
    }

    [UnityTest]
    public IEnumerator UnloadScene()
    {
        sceneController.LoadParcelScenes(AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/MainScripts/DCL/WorldRuntime/Tests/TestJSON/SceneLoadingTest.json").text);

        yield return new WaitForAllMessagesProcessed();

        string loadedSceneID = "0,0";
        int loadedSceneNumber = 666;

        Assert.IsTrue(Environment.i.world.state.ContainsScene(loadedSceneNumber));

        var loadedScene = Environment.i.world.state.GetScene(loadedSceneNumber) as ParcelScene;
        // Add 1 entity to the loaded scene
        TestUtils.CreateSceneEntity(loadedScene, 6);

        var sceneEntities = loadedScene.entities;

        sceneController.UnloadScene(loadedSceneNumber);

        yield return new WaitForAllMessagesProcessed();
        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(Environment.i.world.state.ContainsScene(loadedSceneNumber) == false);

        Assert.IsTrue(loadedScene == null, "Scene root gameobject reference is not getting destroyed.");

        foreach (var entity in sceneEntities)
        {
            Assert.IsFalse(entity.Value.gameObject.activeInHierarchy, "Every entity should be disabled after returning to the pool");
        }

        sceneController.UnloadAllScenes(includePersistent: true);

        yield return null;
    }

    [UnityTest]
    public IEnumerator LoadManyParcelsFromJSON()
    {
        string severalParcelsJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/MainScripts/DCL/WorldRuntime/Tests/TestJSON/TestSceneSeveralParcels.json").text;

        //Not really elegant, but does the trick
        var jsonScenes = JsonConvert
            .DeserializeObject<LoadParcelScenesMessage.UnityParcelScene[]>(severalParcelsJson)
            .Select(x => JsonUtility.ToJson(x));

        Assert.AreEqual(Environment.i.world.state.GetLoadedScenes().Count(), 1);

        foreach (string jsonScene in jsonScenes)
        {
            sceneController.LoadParcelScenes(jsonScene);
        }

        yield return new WaitForAllMessagesProcessed();

        var referenceCheck = new List<DCL.Controllers.ParcelScene>();

        foreach (var kvp in Environment.i.world.state.GetLoadedScenes())
        {
            referenceCheck.Add(kvp.Value as ParcelScene);
        }

        Assert.AreEqual(12, Environment.i.world.state.GetLoadedScenes().Count());

        foreach (var jsonScene in jsonScenes)
        {
            sceneController.LoadParcelScenes(jsonScene);
        }

        Assert.AreEqual(12, Environment.i.world.state.GetLoadedScenes().Count());

        var loadedScenes = Environment.i.world.state.GetLoadedScenes().Select(kvp => kvp.Value);
        foreach (var reference in referenceCheck)
        {
            CollectionAssert.Contains(loadedScenes, reference, "References must be the same");
        }

        sceneController.UnloadAllScenes(includePersistent: true);
        yield return null;
    }

    [Test]
    public void ParcelScene_TrackDisposables_AfterInitDone()
    {
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);

        scene.sceneLifecycleHandler.SetInitMessagesDone();

        Assert.AreEqual(0, scene.sceneLifecycleHandler.pendingResourcesCount);
    }

    [Test]
    public void ParcelScene_TrackDisposables_Empty()
    {
        Assert.AreEqual(0, scene.sceneLifecycleHandler.pendingResourcesCount);
    }

    [UnityTest]
    public IEnumerator PositionParcels()
    {
        Assert.AreEqual(1, Environment.i.world.state.GetLoadedScenes().Count());

        var jsonMessageToLoad = "{\"id\":\"xxx\",\"sceneNumber\":777,\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        sceneController.LoadParcelScenes(jsonMessageToLoad);

        yield return new WaitForAllMessagesProcessed();

        Assert.AreEqual(2, Environment.i.world.state.GetLoadedScenes().Count());

        var theScene = Environment.i.world.state.GetScene(777);
        yield return null;

        Assert.AreEqual(3, theScene.sceneData.parcels.Length);
        Assert.AreEqual(3, theScene.GetSceneTransform().childCount);

        Assert.IsTrue(theScene.GetSceneTransform().GetChild(0).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
            DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
        Assert.IsTrue(theScene.GetSceneTransform().GetChild(1).localPosition == new Vector3(ParcelSettings.PARCEL_SIZE / 2,
            DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
        Assert.IsTrue(theScene.GetSceneTransform().GetChild(2).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
            DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT,
            ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE / 2));
    }

    [UnityTest]
    public IEnumerator PositionParcels2()
    {
        Assert.AreEqual(1, Environment.i.world.state.GetLoadedScenes().Count());

        var jsonMessageToLoad = "{\"id\":\"xxx\",\"sceneNumber\":777,\"basePosition\":{\"x\":90,\"y\":90},\"parcels\":[{\"x\":89,\"y\":90}, {\"x\":90,\"y\":90}, {\"x\":89,\"y\":91}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        sceneController.LoadParcelScenes(jsonMessageToLoad);

        yield return new WaitForAllMessagesProcessed();

        Assert.AreEqual(2, Environment.i.world.state.GetLoadedScenes().Count());

        var theScene = Environment.i.world.state.GetScene(777) as ParcelScene;
        yield return null;

        Assert.AreEqual(3, theScene.sceneData.parcels.Length);
        Assert.AreEqual(3, theScene.transform.childCount);

        Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
            DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
        Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(ParcelSettings.PARCEL_SIZE / 2,
            DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
        Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
            DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT,
            ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE / 2));
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ParcelScene_TrackDisposables_OneGLTF()
    {
        var entity = TestUtils.CreateSceneEntity(scene);

        TestUtils.AttachGLTFShape(entity, scene, Vector3.zero, new LoadableShape.Model()
        {
            src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
        });

        Assert.AreEqual(1, scene.sceneLifecycleHandler.pendingResourcesCount);
        scene.sceneLifecycleHandler.SetInitMessagesDone();
        Assert.AreEqual(1, scene.sceneLifecycleHandler.pendingResourcesCount);
        yield return TestUtils.WaitForGLTFLoad(entity);
        Assert.AreEqual(0, scene.sceneLifecycleHandler.pendingResourcesCount);
    }

    [Test]
    [Explicit]
    [Category("Explicit")]
    public void ParcelScene_TrackDisposables_BeforeInitDone()
    {
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);

        Assert.AreEqual(3, scene.sceneLifecycleHandler.pendingResourcesCount);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ParcelScene_TrackDisposables_InstantReadyDisposable()
    {
        var boxShape = TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        Assert.AreEqual(1, scene.sceneLifecycleHandler.pendingResourcesCount);
        scene.sceneLifecycleHandler.SetInitMessagesDone();
        Assert.AreEqual(0, scene.sceneLifecycleHandler.pendingResourcesCount);
        yield return boxShape.routine;
        Assert.AreEqual(0, scene.sceneLifecycleHandler.pendingResourcesCount);
    }

    [Test]
    public async Task ParcelScene_SetEntityParent()
    {
        var entityId = 1134;
        var entityId2 = 3124;
        var entity = TestUtils.CreateSceneEntity(scene, entityId);
        var entity2 = TestUtils.CreateSceneEntity(scene, entityId2);

        // Make sure that it doesn't have a parent
        Assert.IsNull(entity.parent);
        await UniTask.WaitForFixedUpdate();
        Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Set player reference as parent
        TestUtils.SetEntityParent(scene, entityId, (long) SpecialEntityId.FIRST_PERSON_CAMERA_ENTITY_REFERENCE);
        Assert.AreEqual(entity.gameObject.transform.parent,
            DCLCharacterController.i.firstPersonCameraGameObject.transform);
        await UniTask.WaitForFixedUpdate();
        Assert.IsTrue(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Set another entity as parent and ensure is not added as persistent
        TestUtils.SetEntityParent(scene, entityId, entityId2);

        await UniTask.WaitForFixedUpdate();
        Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Set avatar position reference as parent
        TestUtils.SetEntityParent(scene, entityId, (long) SpecialEntityId.AVATAR_ENTITY_REFERENCE);
        Assert.AreEqual(entity.gameObject.transform.parent, DCLCharacterController.i.avatarGameObject.transform);

        await UniTask.WaitForFixedUpdate();
        Assert.IsTrue(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Remove all parents
        TestUtils.SetEntityParent(scene, entityId, (long) SpecialEntityId.SCENE_ROOT_ENTITY);
        Assert.IsNull(entity.parent);

        await UniTask.WaitForFixedUpdate();
        Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));
    }

    [UnityTest]
    public IEnumerator EntityComponentShouldBeRemovedCorreclty()
    {
        var classIds = Enum.GetValues(typeof(CLASS_ID_COMPONENT));
        //var classIds = new[] { CLASS_ID_COMPONENT.UUID_ON_CLICK };
        var ignoreIds = new List<CLASS_ID_COMPONENT>() { CLASS_ID_COMPONENT.NONE, CLASS_ID_COMPONENT.TRANSFORM, CLASS_ID_COMPONENT.UUID_CALLBACK };

        IDCLEntity entity = scene.CreateEntity(1);

        foreach (CLASS_ID_COMPONENT classId in classIds)
        {
            if (ignoreIds.Contains(classId))
                continue;

            IEntityComponent component = scene.componentsManagerLegacy.EntityComponentCreateOrUpdate(entity.entityId, classId, "{}");
            yield return null;

            GameObject componentGO = component.GetTransform()?.gameObject;

            bool hasGameObject = componentGO != null;
            bool released = true;
            bool isPooleable = false;
            bool isGameObjectDestroyed = false;

            if (hasGameObject)
            {
                DestroyGameObjectCallback destroy = componentGO.AddComponent<DestroyGameObjectCallback>();
                destroy.OnDestroyed += () => isGameObjectDestroyed = true;
            }

            if (component is IPoolableObjectContainer pooleable && pooleable.poolableObject != null)
            {
                released = false;
                isPooleable = true;
                pooleable.poolableObject.OnRelease += () => released = true;
            }

            Assert.AreNotEqual(entity.gameObject, componentGO, $"component {classId} has same GameObject as entity");

            scene.componentsManagerLegacy.EntityComponentRemove(entity.entityId, component.componentName);

            yield return null;

            if (!isPooleable && hasGameObject)
            {
                Assert.IsTrue(isGameObjectDestroyed, $"GameObject not destroyed for component {component.componentName} id {classId}");
            }
            Assert.IsTrue(released, $"component {component.componentName} id {classId} is IPoolableObjectContainer but was not released");
            Assert.IsFalse(scene.componentsManagerLegacy.HasComponent(entity, classId), $"component {component.componentName} id {classId} was not removed from entity components dictionary");
        }
    }

    // Test scenario when a scene is unloaded
    // and loaded again before `ParcelScenesCleaner` finishes unloading of the scene
    // leaving `ParcelScene` `GameObject` instantiated forever
    [UnityTest]
    public IEnumerator ReloadedSceneShouldBeCleanedProperly()
    {
        const int loadedSceneNumber = 666;
        string sceneJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Scripts/MainScripts/DCL/WorldRuntime/Tests/TestJSON/SceneLoadingTest.json").text;
        sceneController.LoadParcelScenes(sceneJson);
        yield return new WaitForAllMessagesProcessed();

        var loadedScene = Environment.i.world.state.GetScene(loadedSceneNumber) as ParcelScene;
        TestUtils.CreateSceneEntity(loadedScene, 6);

        sceneController.UnloadScene(loadedSceneNumber);
        yield return new WaitForAllMessagesProcessed();

        sceneController.LoadParcelScenes(sceneJson);
        yield return new WaitForAllMessagesProcessed();

        // Force ParcelScenesCleaner clean
        Environment.i.platform.parcelScenesCleaner.CleanMarkedEntities();

        // Wait a frame for Object.Destroy scene
        yield return null;

        var loadedScenes = Object.FindObjectsOfType<ParcelScene>(true);

        // Disregard global scene created on SetUp
        var loadedScenesCount = loadedScenes.Count(s => s != scene);

        Assert.AreEqual(1, loadedScenesCount);
    }

    class DestroyGameObjectCallback : MonoBehaviour
    {
        public event Action OnDestroyed;
        private void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }
}
