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

namespace Tests
{
    public class SceneTests : TestsBase
    {
        [UnityTest]
        public IEnumerator PerformanceLimitControllerTests()
        {
            yield return TestHelpers.UnloadAllUnityScenes();
            var sceneController = TestHelpers.InitializeSceneController();
            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

            yield return new WaitForSeconds(0.1f);

            sceneController.UnloadAllScenes();

            yield return new WaitForSeconds(0.1f);

            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForAllMessagesProcessed();

            var scene = sceneController.loadedScenes["0,0"];

            TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(-3, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(0, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "3", DCL.Models.CLASS_ID.PLANE_SHAPE, new Vector3(2, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "4", DCL.Models.CLASS_ID.CONE_SHAPE, new Vector3(4, 1, 0));
            TestHelpers.InstantiateEntityWithShape(scene, "5", DCL.Models.CLASS_ID.CYLINDER_SHAPE,
                new Vector3(6, 1, 0));

            TestHelpers.InstantiateEntityWithShape(scene, "6", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 6),
                TestHelpers.GetTestsAssetsPath() + "/GLB/Lantern/Lantern.glb");
            yield return null;
            GLTFLoader gltfShape = scene.entities["6"].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            TestHelpers.InstantiateEntityWithShape(scene, "7", DCL.Models.CLASS_ID.OBJ_SHAPE, new Vector3(10, 1, 0),
                TestHelpers.GetTestsAssetsPath() + "/OBJ/teapot.obj");
            yield return null;
            OBJLoader objshape = scene.entities["7"].gameObject.GetComponentInChildren<OBJLoader>(true);
            yield return new WaitUntil(() => objshape.alreadyLoaded);

            TestHelpers.InstantiateEntityWithShape(scene, "8", DCL.Models.CLASS_ID.GLTF_SHAPE, new Vector3(0, 1, 12),
                TestHelpers.GetTestsAssetsPath() + "/GLB/CesiumMan/CesiumMan.glb");
            yield return null;
            gltfShape = scene.entities["8"].gameObject.GetComponentInChildren<GLTFLoader>(true);
            yield return new WaitUntil(() => gltfShape.alreadyLoaded);

            AssertMetricsModel(scene,
                triangles: 17521,
                materials: 5,
                entities: 8,
                meshes: 11,
                bodies: 13,
                textures: 0);

            TestHelpers.RemoveSceneEntity(scene, "8");

            yield return null;

            AssertMetricsModel(scene,
                triangles: 12849,
                materials: 4,
                entities: 7,
                meshes: 10,
                bodies: 12,
                textures: 0);

            sceneController.UnloadAllScenes();
            yield return null;
        }

        void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies,
            int textures)
        {
            SceneMetricsController.Model inputModel = scene.metricsController.GetModel();

            Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: " + triangles);
            Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
            Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
            Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
            Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
            Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
        }

        public IEnumerator InitializeSceneControllerAndRemoveCharacterController()
        {
            sceneController = TestHelpers.InitializeSceneController();

            if (DCLCharacterController.i != null)
            {
                Object.Destroy(DCLCharacterController.i.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneLoading()
        {
            yield return InitializeSceneControllerAndRemoveCharacterController();

            sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

            yield return new WaitForAllMessagesProcessed();

            string loadedSceneID = "0,0";

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

            Assert.IsTrue(sceneController.loadedScenes[loadedSceneID] != null);

            TestHelpers.ForceUnloadAllScenes(sceneController);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneUnloading()
        {
            yield return InitializeSceneControllerAndRemoveCharacterController();

            sceneController.LoadParcelScenes((Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text);

            yield return new WaitForAllMessagesProcessed();

            string loadedSceneID = "0,0";

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID));

            // Add 1 entity to the loaded scene
            TestHelpers.CreateSceneEntity(sceneController.loadedScenes[loadedSceneID], "6");

            var sceneRootGameObject = sceneController.loadedScenes[loadedSceneID];
            var sceneEntities = sceneController.loadedScenes[loadedSceneID].entities;

            sceneController.UnloadScene(loadedSceneID);

            yield return new WaitForSeconds(0.01f); // We wait to let unity destroy gameobjects.

            Assert.IsTrue(sceneController.loadedScenes.ContainsKey(loadedSceneID) == false);

            Assert.IsTrue(sceneRootGameObject == null, "Scene root gameobject reference is not getting destroyed.");

            Assert.AreEqual(sceneEntities.Count, 0, "Every entity should be removed");

            TestHelpers.ForceUnloadAllScenes(sceneController);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SeveralParcelsFromJSON()
        {
            yield return InitializeSceneControllerAndRemoveCharacterController();

            string severalparcelsJson = (Resources.Load("TestJSON/TestSceneSeveralParcels") as TextAsset).text;

            //Not really elegant, but does the trick
            var jsonScenes = JsonConvert
                .DeserializeObject<LoadParcelScenesMessage.UnityParcelScene[]>(severalparcelsJson)
                .Select(x => JsonUtility.ToJson(x));

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            foreach (string jsonScene in jsonScenes)
            {
                sceneController.LoadParcelScenes(jsonScene);
            }

            yield return new WaitForAllMessagesProcessed();

            var referenceCheck = new List<DCL.Controllers.ParcelScene>();

            foreach (var kvp in sceneController.loadedScenes)
            {
                referenceCheck.Add(kvp.Value);
            }

            ;

            Assert.AreEqual(11, sceneController.loadedScenes.Count);

            foreach (var jsonScene in jsonScenes)
            {
                sceneController.LoadParcelScenes(jsonScene);
            }

            Assert.AreEqual(11, sceneController.loadedScenes.Count);

            foreach (var reference in referenceCheck)
            {
                Assert.IsTrue(sceneController.loadedScenes.ContainsValue(reference), "References must be the same");
            }

            ;

            TestHelpers.ForceUnloadAllScenes(sceneController);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PositionParcels()
        {
            yield return InitializeSceneControllerAndRemoveCharacterController();

            var jsonMessageToLoad =
                "{\"id\":\"xxx\",\"basePosition\":{\"x\":0,\"y\":0},\"parcels\":[{\"x\":-1,\"y\":0}, {\"x\":0,\"y\":0}, {\"x\":-1,\"y\":1}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";

            DCL.Configuration.Environment.DEBUG = true;

            Assert.AreEqual(sceneController.loadedScenes.Count, 0);
            sceneController.SetDebug();
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return new WaitForAllMessagesProcessed();

            Assert.AreEqual(sceneController.loadedScenes.Count, 1);

            var theScene = sceneController.loadedScenes["xxx"];

            Assert.AreEqual(theScene.sceneData.parcels.Length, 3);
            Assert.AreEqual(theScene.transform.childCount, 3);

            Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
                              DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(ParcelSettings.PARCEL_SIZE / 2,
                              DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
                              DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT,
                              ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE / 2));

            TestHelpers.ForceUnloadAllScenes(sceneController);
            yield return null;
        }

        [UnityTest]
        public IEnumerator PositionParcels2()
        {
            yield return InitializeSceneControllerAndRemoveCharacterController();

            var jsonMessageToLoad =
                "{\"id\":\"xxx\",\"basePosition\":{\"x\":90,\"y\":90},\"parcels\":[{\"x\":89,\"y\":90}, {\"x\":90,\"y\":90}, {\"x\":89,\"y\":91}],\"baseUrl\":\"http://localhost:9991/local-ipfs/contents/\",\"contents\":[],\"owner\":\"0x0f5d2fb29fb7d3cfee444a200298f468908cc942\"}";

            DCL.Configuration.Environment.DEBUG = true;

            Assert.AreEqual(0, sceneController.loadedScenes.Count);
            sceneController.SetDebug();
            sceneController.LoadParcelScenes(jsonMessageToLoad);

            yield return new WaitForAllMessagesProcessed();

            Assert.AreEqual(1, sceneController.loadedScenes.Count);

            var theScene = sceneController.loadedScenes["xxx"];

            Assert.AreEqual(3, theScene.sceneData.parcels.Length);
            Assert.AreEqual(3, theScene.transform.childCount);

            Assert.IsTrue(theScene.transform.GetChild(0).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
                              DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(1).localPosition == new Vector3(ParcelSettings.PARCEL_SIZE / 2,
                              DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT, ParcelSettings.PARCEL_SIZE / 2));
            Assert.IsTrue(theScene.transform.GetChild(2).localPosition == new Vector3(-ParcelSettings.PARCEL_SIZE / 2,
                              DCL.Configuration.ParcelSettings.DEBUG_FLOOR_HEIGHT,
                              ParcelSettings.PARCEL_SIZE + ParcelSettings.PARCEL_SIZE / 2));

            TestHelpers.ForceUnloadAllScenes(sceneController);
            yield return null;
        }


        [UnityTest]
        public IEnumerator CreateUIScene()
        {
            yield return base.InitScene();

            // Position character inside parcel (0,0)
            TestHelpers.SetCharacterPosition(Vector3.zero);

            string sceneId = "Test UI Scene";
            sceneController.CreateUIScene(JsonUtility.ToJson(new CreateUISceneMessage() { id = sceneId }));

            GameObject sceneGo = GameObject.Find("ui scene:" + sceneId);

            GlobalScene scene = sceneController.loadedScenes[sceneId] as GlobalScene;

            Assert.IsTrue(scene != null, "Scene isn't a GlobalScene?");
            Assert.IsTrue(sceneGo != null, "scene game object not found!");
            Assert.IsTrue(sceneController.loadedScenes[sceneId] != null, "Scene not in loaded dictionary!");
            Assert.IsTrue(sceneController.loadedScenes[sceneId].unloadWithDistance == false,
                "Scene will unload when far!");

            Assert.IsTrue(scene.IsInsideSceneBoundaries(new Vector2(1000, 1000)),
                "IsInsideSceneBoundaries() should always return true.");
            Assert.IsTrue(scene.IsInsideSceneBoundaries(new Vector2(-1000, -1000)),
                "IsInsideSceneBoundaries() should always return true.");

            yield return null;


            // Position character inside parcel (0,0)
            TestHelpers.SetCharacterPosition(new Vector3(100f, 0f, 100f));

            yield return null;

            sceneGo = GameObject.Find("ui scene:" + sceneId);

            Assert.IsTrue(sceneGo != null, "scene game object not found! UIScenes must not be unloaded by distance!");
            Assert.IsTrue(sceneController.loadedScenes[sceneId] != null,
                "Scene not in loaded dictionary when far! UIScenes must not be unloaded by distance!");

            TestHelpers.ForceUnloadAllScenes(sceneController);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SceneUnloadByDistance()
        {
            yield return base.InitScene();

            DCLCharacterController.i.gravity = 0;

            // Position character inside parcel (0,0)
            TestHelpers.SetCharacterPosition(Vector3.zero);

            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;

            yield return new WaitForSeconds(0.1f);

            sceneController.UnloadAllScenes();

            yield return new WaitForSeconds(0.1f);

            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForAllMessagesProcessed();

            var sceneId = "0,0";
            var scene = sceneController.loadedScenes[sceneId];

            GameObject sceneGo = GameObject.Find("scene:" + sceneId);

            Assert.IsTrue(sceneGo != null, "scene game object not found!");
            Assert.IsTrue(sceneController.loadedScenes[sceneId] != null, "Scene not in loaded dictionary!");
            Assert.IsTrue(sceneController.loadedScenes[sceneId].unloadWithDistance == true,
                "Scene should unload when far!");

            yield return null;

            // Position character outside parcel (0,0)
            TestHelpers.SetCharacterPosition(new Vector3(ParcelSettings.UNLOAD_DISTANCE + ParcelSettings.PARCEL_SIZE / 2.0f + 1f, 0f, ParcelSettings.UNLOAD_DISTANCE + ParcelSettings.PARCEL_SIZE / 2.0f + 1f));
            yield return new WaitForSeconds(0.1f);

            sceneGo = GameObject.Find("scene:" + sceneId);

            Assert.IsTrue(sceneGo == null, "scene game object found! Scene must be unloaded by distance!");
            Assert.IsFalse(sceneController.loadedScenes.ContainsKey(sceneId),
                "Scene is still loaded in dictionary when far! Scenes must be unloaded by distance!");

            TestHelpers.ForceUnloadAllScenes(sceneController);

            yield return null;
        }
    }
}
