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
    public class SceneTests : IntegrationTestSuite_Legacy
    {
        protected override bool enableSceneIntegrityChecker => false;

        protected override IEnumerator SetUp()
        {
            yield return base.SetUp();
            Environment.i.platform.debugController.SetDebug();
            DCL.Configuration.EnvironmentSettings.DEBUG = true;
        }

        [UnityTest]
        public IEnumerator CreateGlobalScene()
        {
            // Position character inside parcel (0,0)
            TestHelpers.SetCharacterPosition(Vector3.zero);

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
            TestHelpers.SetCharacterPosition(new Vector3(100f, 0f, 100f));

            yield return null;

            sceneGo = GameObject.Find(sceneGameObjectNamePrefix + sceneId);

            Assert.IsTrue(sceneGo != null, "scene game object not found! GlobalScenes must not be unloaded by distance!");
            Assert.IsTrue(Environment.i.world.state.loadedScenes[sceneId] != null,
                "Scene not in loaded dictionary when far! GlobalScenes must not be unloaded by distance!");
        }

        [Test]
        public void ParcelScene_TrackDisposables_AfterInitDone()
        {
            SetUp_TestScene();
            TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);

            scene.sceneLifecycleHandler.SetInitMessagesDone();

            Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
        }

        [Test]
        public void ParcelScene_TrackDisposables_Empty()
        {
            SetUp_TestScene();
            Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
        }

        [UnityTest]
        public IEnumerator PerformanceLimitControllerTests()
        {
            yield return new WaitForAllMessagesProcessed();

            var scenesToLoad = (Resources.Load("TestJSON/SceneLoadingTest") as TextAsset).text;
            sceneController.LoadParcelScenes(scenesToLoad);

            yield return new WaitForAllMessagesProcessed();

            string loadedSceneID = "0,0";
            Assert.IsTrue(Environment.i.world.state.loadedScenes.ContainsKey(loadedSceneID));

            ParcelScene scene = Environment.i.world.state.loadedScenes[loadedSceneID] as ParcelScene;

            var coneShape = TestHelpers.SharedComponentCreate<ConeShape, ConeShape.Model>(scene, DCL.Models.CLASS_ID.CONE_SHAPE, new ConeShape.Model()
            {
                radiusTop = 1,
                radiusBottom = 0
            });

            var planeShape = TestHelpers.SharedComponentCreate<PlaneShape, PlaneShape.Model>(scene, DCL.Models.CLASS_ID.PLANE_SHAPE, new PlaneShape.Model()
            {
                height = 1.5f,
                width = 1
            });


            var shapeEntity = TestHelpers.CreateSceneEntity(scene);
            TestHelpers.SetEntityTransform(scene, shapeEntity, Vector3.one, Quaternion.identity, Vector3.one);
            TestHelpers.SharedComponentAttach(coneShape, shapeEntity);

            TestHelpers.UpdateShape(scene, coneShape.id, JsonUtility.ToJson(new ConeShape.Model()
            {
                segmentsRadial = 180,
                segmentsHeight = 1.5f
            }));

            TestHelpers.DetachSharedComponent(scene, shapeEntity.entityId, coneShape.id);
            TestHelpers.SharedComponentAttach(planeShape, shapeEntity);

            var lanternEntity = TestHelpers.CreateSceneEntity(scene);
            var lanternShape = TestHelpers.AttachGLTFShape(lanternEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            });
            yield return TestHelpers.WaitForGLTFLoad(lanternEntity);

            var cesiumManEntity = TestHelpers.CreateSceneEntity(scene);
            var cesiumManShape = TestHelpers.AttachGLTFShape(cesiumManEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Shark/shark_anim.gltf"
            });
            yield return TestHelpers.WaitForGLTFLoad(cesiumManEntity);

            TestHelpers.RemoveSceneEntity(scene, lanternEntity);
            yield return null;

            TestHelpers.DetachSharedComponent(scene, cesiumManEntity.entityId, cesiumManShape.id);
            cesiumManShape = TestHelpers.AttachGLTFShape(cesiumManEntity, scene, new Vector3(8, 1, 8), new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Trunk/Trunk.glb"
            });
            yield return TestHelpers.WaitForGLTFLoad(cesiumManEntity);

            TestHelpers.InstantiateEntityWithShape(scene, "1", DCL.Models.CLASS_ID.BOX_SHAPE, new Vector3(8, 1, 8));
            TestHelpers.InstantiateEntityWithShape(scene, "2", DCL.Models.CLASS_ID.SPHERE_SHAPE, new Vector3(8, 1, 8));

            AssertMetricsModel(scene,
                triangles: 1126,
                materials: 2,
                entities: 4,
                meshes: 4,
                bodies: 4,
                textures: 0);

            TestHelpers.RemoveSceneEntity(scene, "1");
            TestHelpers.RemoveSceneEntity(scene, "2");
            TestHelpers.RemoveSceneEntity(scene, cesiumManEntity);

            yield return null;

            AssertMetricsModel(scene,
                triangles: 4,
                materials: 1,
                entities: 1,
                meshes: 1,
                bodies: 1,
                textures: 0);

            sceneController.UnloadAllScenes();
            yield return null;
        }

        void AssertMetricsModel(ParcelScene scene, int triangles, int materials, int entities, int meshes, int bodies,
            int textures)
        {
            SceneMetricsModel inputModel = scene.metricsController.GetModel();

            Assert.AreEqual(triangles, inputModel.triangles, "Incorrect triangle count, was: " + triangles);
            Assert.AreEqual(materials, inputModel.materials, "Incorrect materials count");
            Assert.AreEqual(entities, inputModel.entities, "Incorrect entities count");
            Assert.AreEqual(meshes, inputModel.meshes, "Incorrect geometries/meshes count");
            Assert.AreEqual(bodies, inputModel.bodies, "Incorrect bodies count");
            Assert.AreEqual(textures, inputModel.textures, "Incorrect textures count");
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
            TestHelpers.CreateSceneEntity(loadedScene, "6");

            var sceneEntities = loadedScene.entities;

            sceneController.UnloadScene(loadedSceneID);

            yield return new WaitForAllMessagesProcessed();
            yield return new WaitForSeconds(0.1f);

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
            SetUp_TestScene();
            var entity = TestHelpers.CreateSceneEntity(scene);

            TestHelpers.AttachGLTFShape(entity, scene, Vector3.zero, new LoadableShape.Model()
            {
                src = TestAssetsUtils.GetPath() + "/GLB/Lantern/Lantern.glb"
            });

            Assert.AreEqual(1, scene.sceneLifecycleHandler.disposableNotReadyCount);
            scene.sceneLifecycleHandler.SetInitMessagesDone();
            Assert.AreEqual(1, scene.sceneLifecycleHandler.disposableNotReadyCount);
            yield return TestHelpers.WaitForGLTFLoad(entity);
            Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
        }

        [Test]
        [Explicit]
        [Category("Explicit")]
        public void ParcelScene_TrackDisposables_BeforeInitDone()
        {
            SetUp_TestScene();
            TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);

            Assert.AreEqual(3, scene.sceneLifecycleHandler.disposableNotReadyCount);
        }

        [UnityTest]
        [Explicit]
        [Category("Explicit")]
        public IEnumerator ParcelScene_TrackDisposables_InstantReadyDisposable()
        {
            SetUp_TestScene();
            var boxShape = TestHelpers.CreateEntityWithBoxShape(scene, Vector3.zero, true);
            Assert.AreEqual(1, scene.sceneLifecycleHandler.disposableNotReadyCount);
            scene.sceneLifecycleHandler.SetInitMessagesDone();
            Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
            yield return boxShape.routine;
            Assert.AreEqual(0, scene.sceneLifecycleHandler.disposableNotReadyCount);
        }

        [Test]
        public void ParcelScene_SetEntityParent()
        {
            SetUp_TestScene();
            var entityId = "entityId";
            var entity = TestHelpers.CreateSceneEntity(scene, entityId);

            // Make sure that it doesn't have a parent
            Assert.IsNull(entity.parent);
            Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

            // Set player reference as parent
            TestHelpers.SetEntityParent(scene, entityId, "FirstPersonCameraEntityReference");
            Assert.AreEqual(entity.parent, DCLCharacterController.i.firstPersonCameraReference);
            Assert.IsTrue(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

            // Set avatar position reference as parent
            TestHelpers.SetEntityParent(scene, entityId, "AvatarEntityReference");
            Assert.AreEqual(entity.parent, DCLCharacterController.i.avatarReference);
            Assert.IsTrue(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));

            // Remove all parents
            TestHelpers.SetEntityParent(scene, entityId, "0");
            Assert.IsNull(entity.parent);
            Assert.IsFalse(Environment.i.world.sceneBoundsChecker.WasAddedAsPersistent(entity));
        }
    }
}