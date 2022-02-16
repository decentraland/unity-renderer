using DCL.Components;
using DCL.Models;
using DCLPlugins.DebugPlugins.Commons;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class WatchEntityShapeHandlerShould
    {
        private IDCLEntity entity;
        private IShapeListener listener;
        private MeshesInfo meshesInfo;
        private GameObject meshesInfoGameObject;

        [SetUp]
        public void SetUp()
        {
            entity = Substitute.For<IDCLEntity>();
            listener = Substitute.For<IShapeListener>();
            meshesInfo = new MeshesInfo();
            meshesInfoGameObject = new GameObject("MeshesInfoGO");

            entity.meshesInfo.Returns(meshesInfo);
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(meshesInfoGameObject);
        }

        [Test]
        public void TriggerShapeUpdated()
        {
            WatchEntityShapeHandler handler = new WatchEntityShapeHandler(entity, listener);

            meshesInfo.currentShape = Substitute.For<IShape>();
            meshesInfo.meshRootGameObject = meshesInfoGameObject;
            entity.OnMeshesInfoUpdated.Invoke(entity);

            listener.Received(1).OnShapeUpdated(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerShapeUpdatedWhenShapeExistBeforeHandler()
        {
            meshesInfo.currentShape = Substitute.For<IShape>();
            meshesInfo.meshRootGameObject = meshesInfoGameObject;
            entity.OnMeshesInfoUpdated.Invoke(entity);

            WatchEntityShapeHandler handler = new WatchEntityShapeHandler(entity, listener);

            listener.Received(1).OnShapeUpdated(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerShapeCleaned()
        {
            meshesInfo.currentShape = Substitute.For<IShape>();
            WatchEntityShapeHandler handler = new WatchEntityShapeHandler(entity, listener);

            entity.OnMeshesInfoCleaned.Invoke(entity);

            listener.Received(1).OnShapeCleaned(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void DisposeListenerOnDisposed()
        {
            WatchEntityShapeHandler handler = new WatchEntityShapeHandler(entity, listener);
            handler.Dispose();
            listener.Received(1).Dispose();
        }
    }
}