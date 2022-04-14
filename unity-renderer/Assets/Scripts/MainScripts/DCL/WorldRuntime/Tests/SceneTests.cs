using DCL;
using DCL.Components;
using DCL.Configuration;
using DCL.Controllers;
using DCL.Helpers;
using DCL.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

public class SceneTests : IntegrationTestSuite_Legacy
{
    private ParcelScene scene;
    private ISceneController sceneController => DCL.Environment.i.world.sceneController;

    protected override IEnumerator SetUp()
    {
        yield return base.SetUp();
        scene = TestUtils.CreateTestScene();
        DataStore.i.debugConfig.isDebugMode.Set(true);
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
        string sceneId = "Test Global Scene";
        sceneController.CreateGlobalScene(JsonUtility.ToJson(new CreateGlobalSceneMessage() { id = sceneId }));

        GameObject sceneGo = GameObject.Find(sceneGameObjectNamePrefix + sceneId);

        GlobalScene globalScene = Environment.i.world.state.loadedScenes[sceneId] as GlobalScene;

        Assert.IsTrue(globalScene != null, "Scene isn't a GlobalScene?");
        Assert.IsTrue(sceneGo != null, "scene game object not found!");
        Assert.IsTrue(Environment.i.world.state.loadedScenes[sceneId] != null, "Scene not in loaded dictionary!");
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

        sceneGo = GameObject.Find(sceneGameObjectNamePrefix + sceneId);

        Assert.IsTrue(sceneGo != null, "scene game object not found! GlobalScenes must not be unloaded by distance!");
        Assert.IsTrue(Environment.i.world.state.loadedScenes[sceneId] != null,
            "Scene not in loaded dictionary when far! GlobalScenes must not be unloaded by distance!");
    }

    [Test]
    public void ParcelScene_TrackDisposables_AfterInitDone()
    {
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);

        scene.sceneLifecycleHandler.SetInitMessagesDone();

        Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
    }

    [Test]
    public void ParcelScene_TrackDisposables_Empty()
    {
        Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
    }

    [UnityTest]
    public IEnumerator SceneLoading()
    {
        sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);
        yield return new WaitForAllMessagesProcessed();

        string loadedSceneID = "0,0";

        Assert.IsTrue(Environment.i.world.state.loadedScenes.ContainsKey(loadedSceneID));
        Assert.IsTrue(Environment.i.world.state.loadedScenes[loadedSceneID] != null);
    }

    [UnityTest]
    public IEnumerator SceneUnloading()
    {
        sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

        yield return new WaitForAllMessagesProcessed();

        string loadedSceneID = "0,0";

        Assert.IsTrue(Environment.i.world.state.loadedScenes.ContainsKey(loadedSceneID));

        var loadedScene = Environment.i.world.state.loadedScenes[loadedSceneID] as ParcelScene;
        // Add 1 entity to the loaded scene
        TestUtils.CreateSceneEntity(loadedScene, "6");

        var sceneEntities = loadedScene.entities;

        sceneController.UnloadScene(loadedSceneID);

        yield return new WaitForAllMessagesProcessed();
        yield return new WaitForSeconds(0.5f);

        Assert.IsTrue(Environment.i.world.state.loadedScenes.ContainsKey(loadedSceneID) == false);

        Assert.IsTrue(loadedScene == null, "Scene root gameobject reference is not getting destroyed.");

        foreach (var entity in sceneEntities)
        {
            Assert.IsFalse(entity.Value.gameObject.activeInHierarchy, "Every entity should be disabled after returning to the pool");
        }

        sceneController.UnloadAllScenes(includePersistent: true);

        yield return null;
    }

    [UnityTest]
    public IEnumerator SeveralParcelsFromJSON()
    {
        string severalParcelsJson = (Resources.Load("TestJSON/TestSceneSeveralParcels") as TextAsset).text;

        //Not really elegant, but does the trick
        var jsonScenes = JsonConvert
            .DeserializeObject<LoadParcelScenesMessage.UnityParcelScene[]>(severalParcelsJson)
            .Select(x => JsonUtility.ToJson(x));

        Assert.AreEqual(Environment.i.world.state.loadedScenes.Count, 1);

        foreach (string jsonScene in jsonScenes)
        {
            sceneController.LoadParcelScenes(jsonScene);
        }

        yield return new WaitForAllMessagesProcessed();

        var referenceCheck = new List<DCL.Controllers.ParcelScene>();

        foreach (var kvp in Environment.i.world.state.loadedScenes)
        {
            referenceCheck.Add(kvp.Value as ParcelScene);
        }

        Assert.AreEqual(12, Environment.i.world.state.loadedScenes.Count);

        foreach (var jsonScene in jsonScenes)
        {
            sceneController.LoadParcelScenes(jsonScene);
        }

        Assert.AreEqual(12, Environment.i.world.state.loadedScenes.Count);

        foreach (var reference in referenceCheck)
        {
            Assert.IsTrue(Environment.i.world.state.loadedScenes.ContainsValue(reference), "References must be the same");
        }

        sceneController.UnloadAllScenes(includePersistent: true);
        yield return null;
    }

    [UnityTest]
    public IEnumerator PositionParcels()
    {
        Assert.AreEqual(1, Environment.i.world.state.loadedScenes.Count);

        var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        sceneController.LoadParcelScenes(jsonMessageToLoad);

        yield return new WaitForAllMessagesProcessed();

        Assert.AreEqual(2, Environment.i.world.state.loadedScenes.Count);

        var theScene = Environment.i.world.state.loadedScenes["xxx"];
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
        Assert.AreEqual(1, Environment.i.world.state.loadedScenes.Count);

        var jsonMessageToLoad = "{\"id\":\"xxx\",\"basePosition\":{\"x\":90,\"y\":90},\"parcels\":[{\"x\":89,\"y\":90}, {\"x\":90,\"y\":90}, {\"x\":89,\"y\":91}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";
        sceneController.LoadParcelScenes(jsonMessageToLoad);

        yield return new WaitForAllMessagesProcessed();

        Assert.AreEqual(2, Environment.i.world.state.loadedScenes.Count);

        var theScene = Environment.i.world.state.loadedScenes["xxx"] as ParcelScene;
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

        Assert.AreEqual(1, scene.sceneLifecycleHandler.disposableNotReadyCount);
        scene.sceneLifecycleHandler.SetInitMessagesDone();
        Assert.AreEqual(1, scene.sceneLifecycleHandler.disposableNotReadyCount);
        yield return TestUtils.WaitForGLTFLoad(entity);
        Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
    }

    [Test]
    [Explicit]
    [Category("Explicit")]
    public void ParcelScene_TrackDisposables_BeforeInitDone()
    {
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);

        Assert.AreEqual(3, scene.sceneLifecycleHandler.disposableNotReadyCount);
    }

    [UnityTest]
    [Explicit]
    [Category("Explicit")]
    public IEnumerator ParcelScene_TrackDisposables_InstantReadyDisposable()
    {
        var boxShape = TestUtils.CreateEntityWithBoxShape(scene, Vector3.zero, true);
        Assert.AreEqual(1, scene.sceneLifecycleHandler.disposableNotReadyCount);
        scene.sceneLifecycleHandler.SetInitMessagesDone();
        Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
        yield return boxShape.routine;
        Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
    }

    [Test]
    public void ParcelScene_SetEntityParent()
    {
        var entityId = "entityId";
        var entity = TestUtils.CreateSceneEntity(scene, entityId);

        // Make sure that it doesn't have a parent
        Assert.IsNull(entity.parent);
        Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Set player reference as parent
        TestUtils.SetEntityParent(scene, entityId, "FirstPersonCameraEntityReference");
        Assert.AreEqual(entity.parent, DCLCharacterController.i.firstPersonCameraReference);
        Assert.IsTrue(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Set avatar position reference as parent
        TestUtils.SetEntityParent(scene, entityId, "AvatarEntityReference");
        Assert.AreEqual(entity.parent, DCLCharacterController.i.avatarReference);
        Assert.IsTrue(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

        // Remove all parents
        TestUtils.SetEntityParent(scene, entityId, "0");
        Assert.IsNull(entity.parent);
        Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));
    }
}