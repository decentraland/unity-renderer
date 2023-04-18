using DCL.Configuration;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.PlayerMarker;
using NSubstitute;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.PlayerMarker
{
    [TestFixture]
    [Category("EditModeCI")]
    public class PlayerMarkerControllerShould
    {
        private PlayerMarkerController controller;
        private IPlayerMarker marker;
        private BaseVariable<Vector3> position;
        private Vector3Variable rotation;
        private PlayerMarkerController.PlayerMarkerBuilder builder;

        [SetUp]
        public void Setup()
        {
            var coordUtils = Substitute.For<ICoordsUtils>();

            coordUtils.CoordsToPositionWithOffset(Arg.Any<Vector2>())
                      .Returns(info => (Vector3)info.Arg<Vector2>());

            builder = Substitute.For<PlayerMarkerController.PlayerMarkerBuilder>();
            builder.Invoke(Arg.Any<Transform>()).Returns(_ => marker = Substitute.For<IPlayerMarker>());

            controller = new PlayerMarkerController(
                builder,
                position = new BaseVariable<Vector3>(Vector3.zero),
                rotation = ScriptableObject.CreateInstance<Vector3Variable>(),
                null,
                coordUtils,
                Substitute.For<IMapCullingController>()
            );

            controller.Initialize();
        }

        [Test]
        public void Initialize()
        {
            builder.Received(1).Invoke(Arg.Any<Transform>());
        }

        [Test]
        public async Task SetActiveOnEnable()
        {
            await controller.Enable(CancellationToken.None);
            marker.Received(1).SetActive(true);
        }

        [Test]
        public async Task SetPositionOnEnable()
        {
            position.Set(new Vector3(ParcelSettings.PARCEL_SIZE * 2f, 5, ParcelSettings.PARCEL_SIZE * 3f));

            await controller.Enable(CancellationToken.None);
            marker.Received(1).SetPosition(new Vector3(2f, 3f));
        }

        [Test]
        public async Task SetRotationOnEnable()
        {
            rotation.Set(new Vector3(-1, 0, 0));

            await controller.Enable(CancellationToken.None);
            marker.Received(1).SetRotation(Quaternion.Euler(0, 0, 90f));
        }

        [Test]
        public async Task ListenToPositionChangeIfEnabled()
        {
            await controller.Enable(CancellationToken.None);
            marker.ClearReceivedCalls();

            position.Set(new Vector3(ParcelSettings.PARCEL_SIZE * 2f, 5, ParcelSettings.PARCEL_SIZE * 3f));
            marker.Received(1).SetPosition(new Vector3(2f, 3f));
        }

        [Test]
        public async Task ListenToRotationChangeIfEnabled()
        {
            await controller.Enable(CancellationToken.None);
            marker.ClearReceivedCalls();

            rotation.Set(new Vector3(-3, 0, 3));
            marker.Received(1).SetRotation(Quaternion.Euler(0, 0, 45f));
        }

        [Test]
        public void Dispose()
        {
            controller.Dispose();
            marker.Received(1).Dispose();
        }

        [Test]
        public async Task DeactivateOnDisable()
        {
            await controller.Enable(CancellationToken.None);
            await controller.Disable(CancellationToken.None);

            marker.Received().SetActive(false);
        }

        [Test]
        public async Task NotListenToPositionChangeIfDisabled()
        {
            await controller.Enable(CancellationToken.None);
            await controller.Disable(CancellationToken.None);
            marker.ClearReceivedCalls();

            position.Set(new Vector3(ParcelSettings.PARCEL_SIZE * 2f, 5, ParcelSettings.PARCEL_SIZE * 3f));
            marker.DidNotReceive().SetPosition(Arg.Any<Vector3>());
        }

        [Test]
        public async Task NotListenToRotationChangeIfDisabled()
        {
            await controller.Enable(CancellationToken.None);
            await controller.Disable(CancellationToken.None);
            marker.ClearReceivedCalls();

            rotation.Set(new Vector3(1, 1, 1));
            marker.DidNotReceive().SetRotation(Arg.Any<Quaternion>());
        }
    }
}
