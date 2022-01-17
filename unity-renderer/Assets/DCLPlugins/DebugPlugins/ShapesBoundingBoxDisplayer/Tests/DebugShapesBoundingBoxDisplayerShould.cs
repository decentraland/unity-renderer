using System;
using System.Collections.Generic;
using System.Linq;
using DCL;
using DCL.Controllers;
using DCL.Models;
using DCL.Tests;
using NSubstitute;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace Tests
{
    public class DebugShapesBoundingBoxDisplayerShould
    {
        private WorldRuntimeContext worldRuntime;
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

            worldRuntime = WorldRuntimeContextFactory.CreateWithGenericMocks(worldState, sceneController);
        }

        [Test]
        public void StartWatchingSceneOnInstantiation()
        {
            CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldRuntime);
            Assert.AreEqual(1, controller.scenesWatcher.Count);
        }

        [Test]
        public void WaitTargetWatchSceneToBeLoaded()
        {
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldRuntime);
            Assert.AreEqual(1, controller.pendingScenesId.Count);
            Assert.AreEqual(0, controller.scenesWatcher.Count);

            CreateAndAddScene("temptation");
            Assert.AreEqual(0, controller.pendingScenesId.Count);
            Assert.AreEqual(1, controller.scenesWatcher.Count);
        }

        [Test]
        public void StopWatchingSceneCorrectly()
        {
            CreateAndAddScene("temptation");
            isBoundingBoxEnabledVariable.AddOrSet("temptation", true);

            var controller = new DebugShapesBoundingBoxDisplayer(isBoundingBoxEnabledVariable, worldRuntime);
            Assert.AreEqual(1, controller.scenesWatcher.Count);

            isBoundingBoxEnabledVariable.AddOrSet("temptation", false);
            Assert.AreEqual(0, controller.scenesWatcher.Count);
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

        private IDCLEntity CreateEntity(string id)
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.entityId.Returns(id);

            return entity;
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
    }
}