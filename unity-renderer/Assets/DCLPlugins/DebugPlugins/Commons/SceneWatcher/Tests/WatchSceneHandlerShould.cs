using System;
using System.Collections.Generic;
using DCL.Controllers;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class WatchSceneHandlerShould
    {
        private IParcelScene scene;
        private ISceneListener listener;

        [SetUp]
        public void SetUp()
        {
            scene = Substitute.For<IParcelScene>();
            listener = Substitute.For<ISceneListener>();
        }

        [Test]
        public void TriggerEntityAdded()
        {
            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);

            IDCLEntity entity = Substitute.For<IDCLEntity>();
            scene.OnEntityAdded += Raise.Event<Action<IDCLEntity>>(entity);

            listener.Received(1).OnEntityAdded(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerEntityAddedWhenEntityExistBeforeHandler()
        {
            IDCLEntity entity = Substitute.For<IDCLEntity>();
            var sceneEntities = new Dictionary<string, IDCLEntity>() { { "1", entity } };
            scene.entities.Returns(sceneEntities);

            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);

            listener.Received(1).OnEntityAdded(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerEntityRemoved()
        {
            WatchSceneHandler handler = new WatchSceneHandler(scene, listener);

            IDCLEntity entity = Substitute.For<IDCLEntity>();
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
    }
}