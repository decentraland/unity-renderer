using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.HomePoint;
using NSubstitute;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.HomePoint
{
    [TestFixture]
    [Category("EditModeCI")]
    public class HomePointMarkerControllerShould
    {
        private HomePointMarkerController controller;
        private BaseVariable<Vector2Int> homeCoordinates;
        private HomePointMarkerController.HomePointMarkerBuilder builder;
        private IHomePointMarker marker;

        [SetUp]
        public void Setup()
        {
            builder = Substitute.For<HomePointMarkerController.HomePointMarkerBuilder>();
            builder.Invoke(Arg.Any<Transform>()).Returns(marker = Substitute.For<IHomePointMarker>());

            var coordUtils = Substitute.For<ICoordsUtils>();
            coordUtils.CoordsToPosition(Arg.Any<Vector2Int>()).Returns(info => (Vector3)(Vector2)info.Arg<Vector2Int>());

            controller = new HomePointMarkerController(
                homeCoordinates = new BaseVariable<Vector2Int>(Vector2Int.left),
                builder,
                null,
                coordUtils,
                Substitute.For<IMapCullingController>());

            controller.Initialize();
        }

        [Test]
        public void DeactivateOnInitialization()
        {
            marker.Received(1).SetActive(false);
        }

        [Test]
        public void InstantiatePrefab()
        {
            builder.Received(1).Invoke(Arg.Any<Transform>());
            Assert.IsNotNull(marker);
        }

        [Test]
        public async Task ReactOnPositionChangeIfEnabled()
        {
            await controller.Enable(CancellationToken.None);

            marker.ClearReceivedCalls();

            homeCoordinates.Set(new Vector2Int(10, 10));
            marker.Received(1).SetPosition(new Vector3(10, 10, 0));
        }

        [Test]
        public async Task IgnorePositionChangeIfDisabled()
        {
            await controller.Enable(CancellationToken.None);
            await controller.Disable(CancellationToken.None);

            marker.ClearReceivedCalls();

            homeCoordinates.Set(new Vector2Int(10, 10));
            homeCoordinates.Set(new Vector2Int(20, 20));
            marker.DidNotReceive().SetPosition(Arg.Any<Vector3>());
        }

        [Test]
        public async Task Enable()
        {
            marker.ClearReceivedCalls();
            await controller.Enable(CancellationToken.None);

            marker.Received(1).SetActive(true);
            marker.Received(1).SetPosition((Vector2)homeCoordinates.Get());
        }

        [Test]
        public async Task Disable()
        {
            marker.ClearReceivedCalls();
            await controller.Disable(CancellationToken.None);

            marker.Received(1).SetActive(false);
        }
    }
}
