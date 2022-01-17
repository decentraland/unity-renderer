using DCL.Components;
using DCL.Models;
using NSubstitute;
using NUnit.Framework;

namespace DCLPlugins.DebugPlugins.Commons.Tests
{
    public class WatchEntityShapeHandlerShould
    {
        private IDCLEntity entity;
        private IShapeListener listener;
        private MeshesInfo meshesInfo;

        [SetUp]
        public void SetUp()
        {
            entity = Substitute.For<IDCLEntity>();
            listener = Substitute.For<IShapeListener>();
            meshesInfo = Substitute.ForPartsOf<MeshesInfo>();

            entity.meshesInfo.Returns(meshesInfo);
        }

        [Test]
        public void TriggerShapeUpdated()
        {
            WatchEntityShapeHandler handler = new WatchEntityShapeHandler(entity, listener);

            meshesInfo.currentShape = Substitute.For<IShape>();
            entity.OnMeshesInfoUpdated.Invoke(entity);

            listener.Received(1).OnShapeUpdated(Arg.Is(entity));

            handler.Dispose();
        }

        [Test]
        public void TriggerShapeUpdatedWhenShapeExistBeforeHandler()
        {
            meshesInfo.currentShape = Substitute.For<IShape>();
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