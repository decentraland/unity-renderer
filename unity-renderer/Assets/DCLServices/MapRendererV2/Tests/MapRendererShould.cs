using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCLServices.MapRendererV2.ComponentsFactory;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using MainScripts.DCL.Helpers.Utils;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.Tests
{
    [TestFixture]
    public class MapRendererShould
    {
        private MapRenderer mapRenderer;

        private Dictionary<MapLayer, IMapLayerController> layers;

        private static readonly MapLayer[] TEST_MAP_LAYERS =
        {
            MapLayer.Atlas,
            MapLayer.PlayerMarker | MapLayer.ColdUsersMarkers,
            MapLayer.HomePoint | MapLayer.ScenesOfInterest | MapLayer.ColdUsersMarkers,
            MapLayer.Atlas | MapLayer.HotUsersMarkers | MapLayer.HomePoint | MapLayer.ScenesOfInterest
        };

        [SetUp]
        public async Task Setup()
        {
            var componentsFactory = Substitute.For<IMapRendererComponentsFactory>();

            componentsFactory.Create(Arg.Any<CancellationToken>())
                             .Returns(
                                  new UniTask<MapRendererComponents>(
                                      new MapRendererComponents(
                                          new GameObject("map_configuration_test").AddComponent<MapRendererConfiguration>(),
                                          EnumUtils.Values<MapLayer>().Where(l => l != MapLayer.None).ToDictionary(x => x, x => Substitute.For<IMapLayerController>()),
                                          Substitute.For<IMapCullingController>(),
                                          Substitute.For<IObjectPool<IMapCameraControllerInternal>>())));

            mapRenderer = new MapRenderer(componentsFactory);
            await mapRenderer.InitializeAsync(CancellationToken.None);

            layers = mapRenderer.layersDictionary_Test;
        }

        [Test]
        public void InitializeLayers()
        {
            CollectionAssert.AreEquivalent(EnumUtils.Values<MapLayer>().Where(l => l != MapLayer.None), mapRenderer.initializedLayers_Test);
        }

        [Test]
        public void EnableLayerByMask([ValueSource(nameof(TEST_MAP_LAYERS))] MapLayer mask)
        {
            mapRenderer.EnableLayers_Test(mask);

            foreach (MapLayer mapLayer in EnumUtils.Values<MapLayer>())
            {
                if (EnumUtils.HasFlag(mask, mapLayer))
                    layers[mapLayer].Received(1).Enable(Arg.Any<CancellationToken>());
            }
        }

        [Test]
        public void DisableLayerByMask([ValueSource(nameof(TEST_MAP_LAYERS))] MapLayer mask)
        {
            mapRenderer.EnableLayers_Test(mask);
            mapRenderer.DisableLayers_Test(mask);

            foreach (MapLayer mapLayer in EnumUtils.Values<MapLayer>())
            {
                if (EnumUtils.HasFlag(mask, mapLayer))
                    layers[mapLayer].Received(1).Disable(Arg.Any<CancellationToken>());
            }
        }

        [Test]
        public void NotDisableLayerIfStillUsed([ValueSource(nameof(TEST_MAP_LAYERS))] MapLayer mask)
        {
            mapRenderer.EnableLayers_Test(mask);
            mapRenderer.EnableLayers_Test(mask);

            mapRenderer.DisableLayers_Test(mask);

            foreach (MapLayer mapLayer in EnumUtils.Values<MapLayer>())
            {
                if (EnumUtils.HasFlag(mask, mapLayer))
                {
                    if (EnumUtils.HasFlag(mask, mapLayer))
                        layers[mapLayer].DidNotReceive().Disable(default);
                }
            }
        }
    }
}
