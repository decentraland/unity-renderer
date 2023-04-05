using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.ComponentsFactory;
using MainScripts.DCL.Controllers.AssetManager.Addressables.Editor;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.Factory
{
    [TestFixture]
    //[Category("EditModeCI")]
    // It is required to build bundles to load them, to prevent an additional overhead I excluded the test from CI
    public class MapRendererChunkComponentsFactoryShould
    {
        private MapRendererChunkComponentsFactory factory;

        [SetUp]
        public void Setup()
        {
            var serviceLocator = ServiceLocatorTestFactory.CreateMocked();
            serviceLocator.Register<IAddressableResourceProvider>(() => new EditorAddressableResourceProvider());
            Environment.Setup(serviceLocator);

            factory = new MapRendererChunkComponentsFactory(20, 1000, 5);
        }

        [Test]
        public async Task ProvideChunkAtlasPrefab()
        {
            var chunkAtlas = await factory.GetAtlasChunkPrefab(CancellationToken.None);
            Assert.IsTrue(chunkAtlas);
            AssertCanInstantiate(chunkAtlas);
        }

        [Test]
        public async Task ProvideHotUserMarkerPrefab()
        {
            var hotUser = await factory.hotUsersMarkersInstaller.GetPrefab(CancellationToken.None);
            Assert.IsTrue(hotUser);
            AssertCanInstantiate(hotUser);
        }

        [Test]
        public async Task ProvideColdUserMarkerPrefab()
        {
            var coldUser = await factory.coldUsersMarkersInstaller.GetPrefab(CancellationToken.None);
            Assert.IsTrue(coldUser);
            AssertCanInstantiate(coldUser);
        }

        [Test]
        public async Task ProvideSceneOfInterestPrefab()
        {
            var coldUser = await factory.sceneOfInterestsMarkersInstaller.GetPrefab(CancellationToken.None);
            Assert.IsTrue(coldUser);
            AssertCanInstantiate(coldUser);
        }

        [Test]
        public async Task ProvideHomePointPrefab()
        {
            var coldUser = await factory.homePointMarkerInstaller.GetPrefab(CancellationToken.None);
            Assert.IsTrue(coldUser);
            AssertCanInstantiate(coldUser);
        }

        [Test]
        public async Task ProvidePlayerMarkerPrefab()
        {
            var coldUser = await factory.playerMarkerInstaller.GetPrefab(CancellationToken.None);
            Assert.IsTrue(coldUser);
            AssertCanInstantiate(coldUser);
        }

        private void AssertCanInstantiate<T>(T prefab) where T : Component
        {
            var parent = new GameObject("Parent").transform;

            T instance = null;

            Assert.DoesNotThrow(() =>
            {
                instance = Object.Instantiate(prefab, parent);
            });

            Assert.IsTrue(instance);
        }

    }
}
