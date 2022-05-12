using DCL;
using DCL.Controllers;
using DCL.WorldRuntime;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class SceneResourcesLoadTrackerShould
    {
        private IECSComponentsManagerLegacy componentsManager;
        private SceneLoadTracker loadTracker;

        [SetUp]
        public void SetUp()
        {
            componentsManager = new ECSComponentsManagerLegacy(Substitute.For<IParcelScene>());
            loadTracker = new SceneLoadTracker();
        }

        [Test]
        public void WaitSharedComponents()
        {
            loadTracker.Track(componentsManager, Substitute.For<IWorldState>());

            var loadedSubscriber = Substitute.For<IDummyEventSubscriber>();
            var updateSubscriber = Substitute.For<IDummyEventSubscriber>();

            loadTracker.OnResourcesLoaded += loadedSubscriber.React;
            loadTracker.OnResourcesStatusUpdate += updateSubscriber.React;

            var component0 = MockedSharedComponentHelper.Create("temptation0");
            var component1 = MockedSharedComponentHelper.Create("temptation1");

            componentsManager.AddSceneSharedComponent(component0.id, component0.component);

            Assert.AreEqual(1, loadTracker.pendingResourcesCount);

            componentsManager.AddSceneSharedComponent(component1.id, component1.component);

            Assert.IsTrue(loadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(2, loadTracker.pendingResourcesCount);
            Assert.AreEqual(0, loadTracker.loadingProgress);
            Assert.IsTrue(loadTracker.ShouldWaitForPendingResources());

            loadedSubscriber.DidNotReceive().React();

            component0.SetAsReady();

            updateSubscriber.Received(1).React();
            loadedSubscriber.DidNotReceive().React();

            component1.SetAsReady();

            updateSubscriber.Received(1).React();
            loadedSubscriber.Received(1).React();

            Assert.AreEqual(0, loadTracker.pendingResourcesCount);
            Assert.AreEqual(100, loadTracker.loadingProgress);
        }

        [Test]
        public void NotWaitIfNoSharedComponents()
        {
            loadTracker.Track(componentsManager, Substitute.For<IWorldState>());
            Assert.IsFalse(loadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);
        }

        [Test]
        public void IgnoreSharedComponentsAfterDisposed()
        {
            loadTracker.Track(componentsManager, Substitute.For<IWorldState>());

            var loadedSubscriber = Substitute.For<IDummyEventSubscriber>();

            loadTracker.OnResourcesLoaded += loadedSubscriber.React;

            var component0 = MockedSharedComponentHelper.Create("temptation0");
            var component1 = MockedSharedComponentHelper.Create("temptation1");

            componentsManager.AddSceneSharedComponent(component0.id, component0.component);

            Assert.IsTrue(loadTracker.ShouldWaitForPendingResources());
            component0.SetAsReady();

            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);

            loadTracker.Dispose();
            componentsManager.AddSceneSharedComponent(component1.id, component1.component);

            Assert.AreEqual(100, loadTracker.loadingProgress);
            Assert.AreEqual(0, loadTracker.pendingResourcesCount);

            loadedSubscriber.Received(1).React();
        }
    }
}