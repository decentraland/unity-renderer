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
        private SceneResourcesLoadTracker resourcesLoadTracker;

        [SetUp]
        public void SetUp()
        {
            componentsManager = new ECSComponentsManagerLegacy(Substitute.For<IParcelScene>());
            resourcesLoadTracker = new SceneResourcesLoadTracker();
        }

        [Test]
        public void WaitSharedComponents()
        {
            resourcesLoadTracker.Track(componentsManager, Substitute.For<IWorldState>());

            var loadedSubscriber = Substitute.For<IDummyEventSubscriber>();
            var updateSubscriber = Substitute.For<IDummyEventSubscriber>();

            resourcesLoadTracker.OnResourcesLoaded += loadedSubscriber.React;
            resourcesLoadTracker.OnResourcesStatusUpdate += updateSubscriber.React;

            var component0 = MockedSharedComponentHelper.Create("temptation0");
            var component1 = MockedSharedComponentHelper.Create("temptation1");

            componentsManager.AddSceneSharedComponent(component0.id, component0.component);

            Assert.AreEqual(1, resourcesLoadTracker.pendingResourcesCount);

            componentsManager.AddSceneSharedComponent(component1.id, component1.component);

            Assert.IsTrue(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(2, resourcesLoadTracker.pendingResourcesCount);
            Assert.AreEqual(0, resourcesLoadTracker.loadingProgress);
            Assert.IsTrue(resourcesLoadTracker.ShouldWaitForPendingResources());

            loadedSubscriber.DidNotReceive().React();

            component0.SetAsReady();

            updateSubscriber.Received(1).React();
            loadedSubscriber.DidNotReceive().React();

            component1.SetAsReady();

            updateSubscriber.Received(1).React();
            loadedSubscriber.Received(1).React();

            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
        }

        [Test]
        public void NotWaitIfNoSharedComponents()
        {
            resourcesLoadTracker.Track(componentsManager, Substitute.For<IWorldState>());
            Assert.IsFalse(resourcesLoadTracker.ShouldWaitForPendingResources());
            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);
        }

        [Test]
        public void IgnoreSharedComponentsAfterDisposed()
        {
            resourcesLoadTracker.Track(componentsManager, Substitute.For<IWorldState>());

            var loadedSubscriber = Substitute.For<IDummyEventSubscriber>();

            resourcesLoadTracker.OnResourcesLoaded += loadedSubscriber.React;

            var component0 = MockedSharedComponentHelper.Create("temptation0");
            var component1 = MockedSharedComponentHelper.Create("temptation1");

            componentsManager.AddSceneSharedComponent(component0.id, component0.component);

            Assert.IsTrue(resourcesLoadTracker.ShouldWaitForPendingResources());
            component0.SetAsReady();

            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);

            resourcesLoadTracker.Dispose();
            componentsManager.AddSceneSharedComponent(component1.id, component1.component);

            Assert.AreEqual(100, resourcesLoadTracker.loadingProgress);
            Assert.AreEqual(0, resourcesLoadTracker.pendingResourcesCount);

            loadedSubscriber.Received(1).React();
        }
    }
}