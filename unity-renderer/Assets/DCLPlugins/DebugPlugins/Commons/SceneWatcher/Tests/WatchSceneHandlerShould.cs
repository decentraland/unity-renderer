using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Tests
{
    public class WatchSceneHandlerShould
    {
        private IParcelScene scene;
        private ISceneListener listener;
        private GameObject meshesInfoGameObject;

        [SetUp]
        public void SetUp()
        {
            scene = Substitute.For<IParcelScene>();
            listener = Substitute.For<ISceneListener>();
            meshesInfoGameObject = new GameObject("MeshesInfoGO");
        }
        
        [TearDown]
        public void TearDown()
        {
            Object.Destroy(meshesInfoGameObject);
        }        

        [Test]
        public void TriggerEntityAdded()
        {
            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);

            IDCLEntity entity = CreateEntity();
            scene.OnEntityAdded += Raise.Event<Action<IDCLEntity>>(entity);

            listener.Received(1).OnEntityAdded(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerEntityAddedWhenEntityExistBeforeHandler()
        {
            IDCLEntity entity = CreateEntity();
            var sceneEntities = new Dictionary<long, IDCLEntity>() { { 1, entity } };
            scene.entities.Returns(sceneEntities);

            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);

            listener.Received(1).OnEntityAdded(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerEntityRemoved()
        {
            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);

            IDCLEntity entity = CreateEntity();
            scene.OnEntityRemoved += Raise.Event<Action<IDCLEntity>>(entity);

            listener.Received(1).OnEntityRemoved(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void DisposeListenerOnDisposed()
        {
            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);
            handler.Dispose();
            listener.Received(1).Dispose();
        }

        private IDCLEntity CreateEntity()
        {
            MeshesInfo meshesInfo = new MeshesInfo();
            meshesInfo.meshRootGameObject = meshesInfoGameObject;
            
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            entity.meshesInfo.Returns(meshesInfo);
            return entity;
        }
    }
}