using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.ParcelHighlight;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.Tests.MapCameraController
{
    [Category("EditModeCI")]
    [TestFixture]
    public class MapCameraInteractivityControllerShould
    {
        private MapCameraInteractivityController controller;
        private Camera camera;
        private IObjectPool<IParcelHighlightMarker> pool;
        private IParcelHighlightMarker marker;
        private ICoordsUtils coordUtils;

        [SetUp]
        public void Setup()
        {
            camera = new GameObject("Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.aspect = 1f;
            camera.transform.position = Vector3.zero;

            pool = Substitute.For<IObjectPool<IParcelHighlightMarker>>();
            pool.Get().Returns(marker = Substitute.For<IParcelHighlightMarker>());

            coordUtils = Substitute.For<ICoordsUtils>();
            coordUtils.PositionToCoords(Arg.Any<Vector3>()).Returns(call => new Vector2Int((int)call.Arg<Vector3>().x, (int)call.Arg<Vector3>().y));
            coordUtils.CoordsToPosition(Arg.Any<Vector2Int>()).Returns(call => (Vector3) (Vector2) call.Arg<Vector2Int>());

            coordUtils.TryGetCoordsWithinInteractableBounds(Arg.Any<Vector3>(), out Arg.Any<Vector2Int>())
                      .Returns(call =>
                       {
                           var coords = call.ArgAt<Vector3>(0);
                           call[1] = new Vector2Int((int)coords.x, (int)coords.y);
                           return true;
                       });

            controller = new MapCameraInteractivityController(null, camera, pool, coordUtils);
        }

        [Test]
        [TestCase(MapLayer.HomePoint | MapLayer.ColdUsersMarkers | MapLayer.PlayerMarker | MapLayer.ParcelHoverHighlight)]
        [TestCase(MapLayer.ParcelHoverHighlight)]
        [TestCase(MapLayer.ParcelHoverHighlight | MapLayer.Atlas)]
        public void InitializedInActiveState(MapLayer layer)
        {
            controller.Initialize(layer);
            Assert.IsTrue(controller.HighlightEnabled);
            pool.Received(1).Get();
        }

        [Test]
        [TestCase(MapLayer.HomePoint | MapLayer.ColdUsersMarkers | MapLayer.PlayerMarker)]
        [TestCase(MapLayer.Atlas)]
        [TestCase(MapLayer.HotUsersMarkers | MapLayer.Atlas)]
        public void InitializeInInActiveState(MapLayer layer)
        {
            controller.Initialize(layer);
            Assert.IsFalse(controller.HighlightEnabled);
            pool.DidNotReceive().Get();
        }

        [Test]
        public void ActivateOnHighlight()
        {
            controller.Initialize(MapLayer.ParcelHoverHighlight);
            controller.HighlightParcel(new Vector2Int(0, 0));

            marker.Received(1).Activate();
        }

        [Test]
        [TestCaseSource(nameof(SetPositionOnHighlightTestCases))]
        public void SetPositionOnHighlight(Vector3 cameraPos, Vector2 norm, Vector3 expected)
        {
            camera.transform.localPosition = cameraPos;

            controller.Initialize(MapLayer.ParcelHoverHighlight);

            Assert.IsTrue(controller.TryGetParcel(norm, out var parcel));
            Assert.AreEqual(expected, new Vector3(parcel.x, parcel.y, 0));

            controller.HighlightParcel(parcel);

            marker.Received(1).SetCoordinates(Arg.Any<Vector2Int>(), expected);
        }

        public static object[] SetPositionOnHighlightTestCases =
        {
            new object[] { Vector3.zero, new Vector2(0.5f, 0.5f), Vector3.zero},
            new object[] { new Vector3(1f, 1f, -2f), new Vector2(0.5f, 0.5f), new Vector3(1f, 1f, 0)},
            new object[] { new Vector3(4f, 0f, 30f), new Vector2(0.2f, 0.1f), new Vector3(1f, -4f, 0f)},
            new object[] { new Vector3(-2f, 2f, 10f), new Vector2(1f, 1f), new Vector3(3f, 7f, 0f)}
        };

        [Test]
        public void RemoveHighlight()
        {
            controller.Initialize(MapLayer.ParcelHoverHighlight);
            controller.Release();

            pool.Received(1).Release(marker);
        }
    }
}
