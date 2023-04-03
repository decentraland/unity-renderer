using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Components;
using DCL.Controllers;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Tests
{
    [Category("Legacy")]
    public class DebugShapesBoundingBoxDisplayerShould
    {
        private IWorldState worldState;
        private ISceneController sceneController;
        private IUpdateEventHandler updateEventHandler;

        private Dictionary<int, IParcelScene> loadedScenes = new Dictionary<int, IParcelScene>();
        private Dictionary<int, Dictionary<long, IDCLEntity>> entities = new Dictionary<int, Dictionary<long, IDCLEntity>>();

        private BaseDictionary<int, bool> isBoundingBoxEnabledVariable = new BaseDictionary<int, bool>();

        [SetUp]
        public void SetUp()
        {
            worldState = Substitute.For<IWorldState>();

            sceneController = Substitute.For<ISceneController>();
            updateEventHandler = Substitute.For<IUpdateEventHandler>();
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
            CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void WaitTargetWatchSceneToBeLoaded()
        {
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
            Assert.AreEqual(1, controller.pendingSceneNumbers.Count);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            CreateAndAddScene(666);
            Assert.AreEqual(0, controller.pendingSceneNumbers.Count);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void StopWatchingSceneCorrectly()
        {
            CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            isBoundingBoxEnabledVariable.AddOrSet(666, false);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            controller.Dispose();
        }

        [Test]
        public void AddWireframeCorrectly()
        {
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
            var entity = CreateEntityWithShape("temptationEntity");

            AddEntity(scene, entity);
            Assert.AreEqual(1, GetWireframesCount());

            controller.Dispose();
        }

        [Test]
        public void AddWireframeCorrectlyWhenShapeAdded()
        {
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
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
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
            var entity = CreateEntityWithShape("temptationEntity");

            AddEntity(scene, entity);
            entity.OnMeshesInfoCleaned.Invoke(entity);

            Assert.AreEqual(0, GetWireframesCount());

            controller.Dispose();
        }

        [Test]
        public void RemoveWireframeCorrectlyWhenEntityRemoved()
        {
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
            var entity = CreateEntityWithShape("temptationEntity");

            AddEntity(scene, entity);
            RemoveEntity(entity);

            Assert.AreEqual(0, GetWireframesCount());

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator RemoveAllWireframesCorrectlyWhenFeatureIsDisabled()
        {
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
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
            isBoundingBoxEnabledVariable.AddOrSet(666, false);

            // wait a frame for Object.Destroy
            yield return null;

            allWireframes = GetWireframesCount(true);
            Assert.AreEqual(0, allWireframes);

            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator RemoveAllWireframesCorrectlyWhenSceneIsDestroyed()
        {
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
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
            var scene = CreateAndAddScene(666);
            isBoundingBoxEnabledVariable.AddOrSet(666, true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldState, sceneController, updateEventHandler);
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

        [Test]
        public void UpdateWireframeCorrectly()
        {
            var entity = CreateEntityWithShape("temptationEntity");
            var shape = entity.meshesInfo.meshRootGameObject;
            var shapeRenderer = shape.GetComponent<Renderer>();

            var wireFrame = Object.Instantiate(Resources.Load<GameObject>(SceneEntitiesTracker.WIREFRAME_PREFAB_NAME));
            var controller = new EntityWireframe(wireFrame, updateEventHandler);

            Vector3 rendererPosition = shapeRenderer.bounds.center;
            Vector3 rendererSize = shapeRenderer.bounds.size;

            ((IShapeListener)controller).OnShapeUpdated(entity);
            updateEventHandler.Received().RemoveListener(IUpdateEventHandler.EventType.LateUpdate, Arg.Any<Action>());
            updateEventHandler.Received().AddListener(IUpdateEventHandler.EventType.LateUpdate, Arg.Any<Action>());
            updateEventHandler.ClearReceivedCalls();

            controller.LateUpdate();

            AreVectorEqual(rendererPosition, controller.entityWireframes[0].transform.position);
            AreVectorEqual(rendererSize * EntityWireframe.WIREFRAME_SIZE_MULTIPLIER, controller.entityWireframes[0].transform.localScale);

            shape.transform.position = new Vector3(10, 3, 7);
            shape.transform.localScale = new Vector3(0.5f, 2, 1);

            Vector3 newRendererPosition = shapeRenderer.bounds.center;
            Vector3 newRendererSize = shapeRenderer.bounds.size;

            controller.LateUpdate();

            AreVectorEqual(newRendererPosition, controller.entityWireframes[0].transform.position);
            AreVectorEqual(newRendererSize * EntityWireframe.WIREFRAME_SIZE_MULTIPLIER, controller.entityWireframes[0].transform.localScale);

            ((IDisposable)controller).Dispose();
            Object.DestroyImmediate(wireFrame);
            updateEventHandler.Received().RemoveListener(IUpdateEventHandler.EventType.LateUpdate, Arg.Any<Action>());
        }

        private void AreVectorEqual(Vector3 v1, Vector3 v2)
        {
            Assert.AreEqual(v1.ToString(), v2.ToString());
        }

        private IParcelScene CreateAndAddScene(int sceneNumber)
        {
            IParcelScene scene = Substitute.For<IParcelScene>();
            scene.sceneData.Returns(new LoadParcelScenesMessage.UnityParcelScene() { sceneNumber = sceneNumber });

            loadedScenes.Add(sceneNumber, scene);
            entities[sceneNumber] = new Dictionary<long, IDCLEntity>();

            scene.entities.Returns(entities[sceneNumber]);

            sceneController.OnNewSceneAdded += Raise.Event<Action<IParcelScene>>(scene);

            worldState.GetLoadedScenes().Returns(loadedScenes);
            worldState.TryGetScene(sceneNumber, out Arg.Any<IParcelScene>()).Returns(param => param[1] = scene);

            return scene;
        }

        private void RemoveScene(IParcelScene scene)
        {
            var entitiesToRemove = entities[scene.sceneData.sceneNumber].Values.ToArray();
            for (int i = 0; i < entitiesToRemove.Length; i++)
            {
                RemoveEntity(entitiesToRemove[i]);
            }
        }

        private IDCLEntity CreateEntityWithoutShape(string id)
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(id.GetHashCode());

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
            entities[scene.sceneData.sceneNumber].Add(entity.entityId, entity);
            entity.scene.Returns(scene);
            scene.OnEntityAdded += Raise.Event<Action<IDCLEntity>>(entity);
        }

        private void RemoveEntity(IDCLEntity entity)
        {
            if (entities[entity.scene.sceneData.sceneNumber].Remove(entity.entityId))
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
