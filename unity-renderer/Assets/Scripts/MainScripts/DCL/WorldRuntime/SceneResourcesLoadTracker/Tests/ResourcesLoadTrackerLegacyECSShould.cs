using DCL;
using DCL.Controllers;
using DCL.WorldRuntime;
using NSubstitute;
using NUnit.Framework;

namespace Tests
{
    public class ResourcesLoadTrackerLegacyECSShould
    {
        private IECSComponentsManagerLegacy componentsManager;
        private ResourcesLoadTrackerLegacyECS trackerLegacyEcs;

        [SetUp]
        public void SetUp()
        {
            componentsManager = new ECSComponentsManagerLegacy(Substitute.For<IParcelScene>());
            trackerLegacyEcs = new ResourcesLoadTrackerLegacyECS(componentsManager, Substitute.For<IWorldState>());
        }

        [Test]
        public void WaitPendingResources()
        {
            var loadedSubscriber = Substitute.For<IDummyEventSubscriber>();
            var updateSubscriber = Substitute.For<IDummyEventSubscriber>();

            trackerLegacyEcs.OnResourcesLoaded += loadedSubscriber.React;
            trackerLegacyEcs.OnStatusUpdate += updateSubscriber.React;

            var component0 = new MockedSharedComponent("temptation0");
            var component1 = new MockedSharedComponent("temptation1");
            var component2 = new MockedSharedComponent("temptation2");

            componentsManager.AddSceneSharedComponent(component0.id, component0);

            Assert.AreEqual(1, trackerLegacyEcs.pendingResourcesCount);

            componentsManager.AddSceneSharedComponent(component1.id, component1);
            componentsManager.AddSceneSharedComponent(component2.id, component2);

            Assert.IsTrue(trackerLegacyEcs.CheckPendingResources());
            Assert.AreEqual(3, trackerLegacyEcs.pendingResourcesCount);
            Assert.AreEqual(0, trackerLegacyEcs.loadingProgress);
            Assert.IsTrue(trackerLegacyEcs.CheckPendingResources());

            loadedSubscriber.DidNotReceive().React();

            component0.SetAsReady();

            updateSubscriber.Received(1).React();
            loadedSubscriber.DidNotReceive().React();

            component1.SetAsReady();
            component2.SetAsReady();

            updateSubscriber.Received(2).React();
            loadedSubscriber.Received(1).React();

            Assert.AreEqual(0, trackerLegacyEcs.pendingResourcesCount);
            Assert.AreEqual(100, trackerLegacyEcs.loadingProgress);
        }
    }
}