using Cysharp.Threading.Tasks.Linq;
using DCLServices.MapRendererV2.ComponentsFactory;
using DCLServices.MapRendererV2.MapLayers;
using MainScripts.DCL.Helpers.Utils;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            MapLayer.HomePoint | MapLayer.PointsOfInterest | MapLayer.ColdUsersMarkers,
            MapLayer.Atlas | MapLayer.HotUsersMarkers | MapLayer.HomePoint | MapLayer.PointsOfInterest
        };

        [SetUp]
        public async Task Setup()
        {
            var componentsFactory = Substitute.For<IMapRendererComponentsFactory>();

            componentsFactory.Create(Arg.Any<CancellationToken>()).Returns(
                UniTaskAsyncEnumerable.Create<(MapLayer, IMapLayerController)>(async (writer, token) =>
                {
                    layers = new Dictionary<MapLayer, IMapLayerController>();

                    foreach (var layer in EnumUtils.Values<MapLayer>().Where(l => l != MapLayer.None))
                    {
                        var controller = Substitute.For<IMapLayerController>();
                        layers[layer] = controller;
                        await writer.YieldAsync((layer, controller));
                    }
                }));

            mapRenderer = new MapRenderer(componentsFactory);
            await mapRenderer.InitializeAsync(CancellationToken.None);
        }

        [Test]
        public void InitializeLayers()
        {
            CollectionAssert.AreEquivalent(EnumUtils.Values<MapLayer>().Where(l => l != MapLayer.None), mapRenderer.initializedLayersInternal);
        }

        [Test]
        public void EnableLayerByMask([ValueSource(nameof(TEST_MAP_LAYERS))] MapLayer mask)
        {
            mapRenderer.EnableLayersInternal(mask);

            foreach (MapLayer mapLayer in EnumUtils.Values<MapLayer>())
            {
                if (EnumUtils.HasFlag(mask, mapLayer))
                    layers[mapLayer].Received(1).Enable(Arg.Any<CancellationToken>());
            }
        }

        [Test]
        public void DisableLayerByMask([ValueSource(nameof(TEST_MAP_LAYERS))] MapLayer mask)
        {
            mapRenderer.EnableLayersInternal(mask);
            mapRenderer.DisableLayersInternal(mask);

            foreach (MapLayer mapLayer in EnumUtils.Values<MapLayer>())
            {
                if (EnumUtils.HasFlag(mask, mapLayer))
                    layers[mapLayer].Received(1).Disable(Arg.Any<CancellationToken>());
            }
        }

        [Test]
        public void NotDisableLayerIfStillUsed([ValueSource(nameof(TEST_MAP_LAYERS))] MapLayer mask)
        {
            mapRenderer.EnableLayersInternal(mask);
            mapRenderer.EnableLayersInternal(mask);

            mapRenderer.DisableLayersInternal(mask);

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
