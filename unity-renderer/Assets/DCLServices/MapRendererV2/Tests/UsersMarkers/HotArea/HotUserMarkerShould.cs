using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea;
using MainScripts.DCL.Helpers.Utils;
using NSubstitute;
using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.UsersMarkers.HotArea
{
    [Category("EditModeCI")]
    public class HotUserMarkerShould
    {
        private HotUserMarker hotUserMarker;

        private IUnityObjectPool<HotUserMarkerObject> objectsPool;
        private IMapCullingController cullingController;
        private ICoordsUtils coordsUtils;
        private Vector3Variable worldOffset;

        [SetUp]
        public void Setup()
        {
            objectsPool = Substitute.For<IUnityObjectPool<HotUserMarkerObject>>();
            objectsPool.Prefab.Returns(new GameObject("hot_user_marker_test").AddComponent<HotUserMarkerObject>());

            hotUserMarker = new HotUserMarker(
                objectsPool,
                cullingController = Substitute.For<IMapCullingController>(),
                coordsUtils = Substitute.For<ICoordsUtils>(),
                worldOffset = ScriptableObject.CreateInstance<Vector3Variable>());
        }

        [Test]
        public void StartTrackingPosition()
        {
            var player = new Player { worldPosition = new Vector3(10, 10, 10) };

            hotUserMarker.TrackPlayer(player);

            cullingController.DidNotReceive().SetTrackedObjectPositionDirty(hotUserMarker);
            cullingController.Received().StartTracking(hotUserMarker, hotUserMarker);
        }

        [Test]
        public async Task ReactOnPositionChange()
        {
            var player = new Player { worldPosition = new Vector3(10, 10, 10) };

            hotUserMarker.TrackPlayer(player);

            cullingController.ClearReceivedCalls();
            player.worldPosition = new Vector3(15, 10, 10);

            await Task.Delay(100);

            cullingController.Received().SetTrackedObjectPositionDirty(hotUserMarker);
            cullingController.DidNotReceive().StartTracking(hotUserMarker, hotUserMarker);
        }
    }
}
