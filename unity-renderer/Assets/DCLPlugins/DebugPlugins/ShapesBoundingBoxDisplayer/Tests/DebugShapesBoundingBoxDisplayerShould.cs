using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    public class DebugShapesBoundingBoxDisplayerShould
    {
        private IWorldState worldState;
        private ISceneController sceneController;

        private Dictionary<string, IParcelScene> loadedScenes = new Dictionary<string, IParcelScene>();
        private Dictionary<string, Dictionary<string, IDCLEntity>> entities = new Dictionary<string, Dictionary<string, IDCLEntity>>();

        private BaseDictionary<string, bool> isBoundingBoxEnabledVariable = new BaseDictionary<string, bool>();

        [SetUp]
        public void SetUp()
        {
            worldState = Substitute.For<IWorldState>();
            worldState.loadedScenes.Returns(loadedScenes);

            sceneController = Substitute.For<ISceneController>();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var sceneId in entities.Keys)
            {
                foreach (var entity in entities[sceneId].Values)
                {
                    Object.Destroy(entity.gameObject);
                }
            }

            loadedScenes.Clear();
            entities.Clear();
        }

        [Test]
        public void StartWatchingSceneOnInstantiation()
        {
            CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void WaitTargetWatchSceneToBeLoaded()
        {
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            Assert.AreEqual(1, controller.pendingScenesId.Count);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            CreateAndAddScene("temptation");
            Assert.AreEqual(0, controller.pendingScenesId.Count);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void StopWatchingSceneCorrectly()
        {
            CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            isBoundingBoxEnabledVariable.AddOrSet("temptation", false);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void AddWireframeCorrectly()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var entity = CreateEntityWithShape("temptationEntity");

            AddEntity(scene, entity);
            Assert.AreEqual(1, GetWireframesCount());

            controller.Dispose();
        }

        [Test]
        public void AddWireframeCorrectlyWhenShapeAdded()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var entity = CreateEntityWithoutShape("temptationEntity");

            AddEntity(scene, entity);
            Assert.AreEqual(0, GetWireframesCount());

            AddShapeToEntity(entity, GameObject.CreatePrimitive(PrimitiveType.Cube));
            Assert.AreEqual(1, GetWireframesCount());

            controller.Dispose();
        }

        [Test]
        public void RemoveWireframeCorrectlyWhenShapeRemoved()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var entity = CreateEntityWithShape("temptationEntity");

            AddEntity(scene, entity);
            entity.OnMeshesInfoCleaned.Invoke(entity);

            Assert.AreEqual(0, GetWireframesCount());

            controller.Dispose();
        }

        [Test]
        public void RemoveWireframeCorrectlyWhenEntityRemoved()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var entity = CreateEntityWithShape("temptationEntity");

            AddEntity(scene, entity);
            RemoveEntity(entity);

            Assert.AreEqual(0, GetWireframesCount());

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator RemoveAllWireframesCorrectlyWhenFeatureIsDisabled()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var sceneEntities = new[]
            {
                CreateEntityWithShape("temptationEntity1"),
                CreateEntityWithShape("temptationEntity2"),
                CreateEntityWithShape("temptationEntity3"),
                CreateEntityWithShape("temptationEntity4"),
            };

            foreach (var entity in sceneEntities)
            {
                AddEntity(scene, entity);
            }

            Assert.AreEqual(sceneEntities.Length, GetWireframesCount());

            int allWireframes = GetWireframesCount(true);

            // entities + 1 for original cached gameobject
            Assert.AreEqual(sceneEntities.Length + 1, allWireframes);

            // Disable feature for scene
            isBoundingBoxEnabledVariable.AddOrSet("temptation", false);

            // wait a frame for Object.Destroy
            yield return null;

            allWireframes = GetWireframesCount(true);
            Assert.AreEqual(0, allWireframes);

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator RemoveAllWireframesCorrectlyWhenSceneIsDestroyed()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var sceneEntities = new[]
            {
                CreateEntityWithShape("temptationEntity1"),
                CreateEntityWithShape("temptationEntity2"),
                CreateEntityWithShape("temptationEntity3"),
                CreateEntityWithShape("temptationEntity4"),
            };

            foreach (var entity in sceneEntities)
            {
                AddEntity(scene, entity);
            }

            Assert.AreEqual(sceneEntities.Length, GetWireframesCount());

            int allWireframes = GetWireframesCount(true);

            // entities + 1 for original cached gameobject
            Assert.AreEqual(sceneEntities.Length + 1, allWireframes);

            // Destroy scene
            RemoveScene(scene);

            // wait a frame for Object.Destroy
            yield return null;

            allWireframes = GetWireframesCount(true);
            Assert.AreEqual(0, allWireframes);

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator RemoveAllWireframesCorrectlyOnControllerDisposed()
        {
            var scene = CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController);
            var sceneEntities = new[]
            {
                CreateEntityWithShape("temptationEntity1"),
                CreateEntityWithShape("temptationEntity2"),
                CreateEntityWithShape("temptationEntity3"),
                CreateEntityWithShape("temptationEntity4"),
            };

            foreach (var entity in sceneEntities)
            {
                AddEntity(scene, entity);
            }

            Assert.AreEqual(sceneEntities.Length, GetWireframesCount());

            int allWireframes = GetWireframesCount(true);

            // entities + 1 for original cached gameobject
            Assert.AreEqual(sceneEntities.Length + 1, allWireframes);

            // Dispose controller
            controller.Dispose();

            // wait a frame for Object.Destroy
            yield return null;

            allWireframes = GetWireframesCount(true);
            Assert.AreEqual(0, allWireframes);
        }

        private IParcelScene CreateAndAddScene(string id)
        {
            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { id = id });

            loadedScenes.Add(id, scene);
            entities[id] = new Dictionary<string, IDCLEntity>();

            scene.entities.Returns(entities[id]);

            sceneController.OnNewSceneAdded += Raise.Event<Action<IParcelScene>>(scene);

            return scene;
        }

        private void RemoveScene(IParcelScene scene)
        {
            var entitiesToRemove = entities[scene.sceneData.id].Values.ToArray();
            for (int i = 0; i < entitiesToRemove.Length; i++)
            {
                RemoveEntity(entitiesToRemove[i]);
            }
        }

        private IDCLEntity CreateEntityWithoutShape(string id)
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(id);

            var gameObject = new GameObject(id);
            entity.gameObject.Returns(gameObject);

            CreateMeshesInfoForEntity(entity);

            return entity;
        }

        private IDCLEntity CreateEntityWithShape(string id)
        {
            IDCLEntity entity = CreateEntityWithoutShape(id);

            AddShapeToEntity(entity, GameObject.CreatePrimitive(PrimitiveType.Cube));

            return entity;
        }

        private void CreateMeshesInfoForEntity(IDCLEntity entity)
        {
            var meshesInfo = new MeshesInfo();
            meshesInfo.OnUpdated += () => entity.OnMeshesInfoUpdated.Invoke(entity);
            meshesInfo.OnCleanup += () => entity.OnMeshesInfoCleaned.Invoke(entity);
            entity.meshesInfo.Returns(meshesInfo);
        }

        private void AddShapeToEntity(IDCLEntity entity, GameObject shape)
        {
            entity.meshesInfo.currentShape = Substitute.For<IShape>();
            entity.meshesInfo.meshRootGameObject = shape;
        }

        private void AddEntity(IParcelScene scene, IDCLEntity entity)
        {
            entities[scene.sceneData.id].Add(entity.entityId, entity);
            entity.scene.Returns(scene);
            scene.OnEntityAdded += Raise.Event<Action<IDCLEntity>>(entity);
        }

        private void RemoveEntity(IDCLEntity entity)
        {
            if (entities[entity.scene.sceneData.id].Remove(entity.entityId))
            {
                Object.Destroy(entity.gameObject);
                entity.scene.OnEntityRemoved += Raise.Event<Action<IDCLEntity>>(entity);
            }
        }

        private int GetWireframesCount(bool includeInactive = false)
        {
            return Object
                .FindObjectsOfType<GameObject>(includeInactive)
                .Count(go => go.name.StartsWith(SceneEntitiesTracker.WIREFRAME_GAMEOBJECT_NAME));
        }
    }
}