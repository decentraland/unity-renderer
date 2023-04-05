using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.PointsOfInterest;
using MainScripts.DCL.Helpers.Utils;
using NSubstitute;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.PointsOfInterest
{
    [TestFixture]
    [Category("EditModeCI")]
    public class ScenesOfInterestMarkersControllerShould
    {
        private const int PREWARM_COUNT = 100;

        private ScenesOfInterestMarkersController controller;
        private IUnityObjectPool<SceneOfInterestMarkerObject> objectPool;
        private IMapCullingController mapCullingController;
        private ICoordsUtils coordsUtils;
        private MinimapMetadata minimapMetadata;

        [SetUp]
        public async Task Setup()
        {
            controller = new ScenesOfInterestMarkersController(
                minimapMetadata = ScriptableObject.CreateInstance<MinimapMetadata>(),
                objectPool = Substitute.For<IUnityObjectPool<SceneOfInterestMarkerObject>>(),
                (pool, cullingController) => Substitute.For<ISceneOfInterestMarker>(),
                PREWARM_COUNT,
                null,
                coordsUtils = Substitute.For<ICoordsUtils>(),
                mapCullingController = Substitute.For<IMapCullingController>()
            );

            await controller.Initialize(CancellationToken.None);
        }

        [Test]
        public void Prewarm()
        {
            objectPool.Received(1).PrewarmAsync(PREWARM_COUNT, Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        [Test]
        public void AddSceneInfo()
        {
            var si = GetSceneInfo();
            minimapMetadata.AddSceneInfo(si);
            Assert.IsTrue(controller.Markers.ContainsKey(si));
        }

        [Test]
        public void CalculateCenterParcel()
        {
            coordsUtils.CoordsToPosition(Arg.Any<Vector2Int>()).Returns(info => (Vector3)(Vector2)info.Arg<Vector2Int>());

            var si = GetSceneInfo();
            minimapMetadata.AddSceneInfo(si);

            var marker = controller.Markers[si];
            // (20, 20) is closest to the center
            marker.Received(1).SetData(Arg.Any<string>(), new Vector3(20, 20, 0));
        }

        [Test]
        public void SetTitle()
        {
            var si = GetSceneInfo();
            minimapMetadata.AddSceneInfo(si);

            var marker = controller.Markers[si];
            marker.Received(1).SetData("TEST_SCENE_NAME", Arg.Any<Vector3>());
        }

        [Test]
        public async Task StartTrackingIfEnabled()
        {
            await controller.Enable(CancellationToken.None);

            var si = GetSceneInfo();
            minimapMetadata.AddSceneInfo(si);

            var marker = controller.Markers[si];
            mapCullingController.Received(1).StartTracking(marker, controller);
        }

        [Test]
        public async Task StartTrackingOnEnable()
        {
            var si = GetSceneInfo();
            minimapMetadata.AddSceneInfo(si);

            var marker = controller.Markers[si];

            mapCullingController.DidNotReceive().StartTracking(Arg.Any<ISceneOfInterestMarker>(), controller);

            await controller.Enable(CancellationToken.None);

            mapCullingController.Received(1).StartTracking(marker, controller);
        }

        [Test]
        public async Task StopTrackingOnDisable()
        {
            await controller.Enable(CancellationToken.None);

            var si = GetSceneInfo("1");
            minimapMetadata.AddSceneInfo(si);

            await controller.Disable(CancellationToken.None);

            mapCullingController.ClearReceivedCalls();

            si = GetSceneInfo("2");
            minimapMetadata.AddSceneInfo(si);

            mapCullingController.DidNotReceive().StartTracking(Arg.Any<ISceneOfInterestMarker>(), controller);
        }

        [Test]
        public void ClearPoolOnDispose()
        {
            controller.Dispose();
            objectPool.Received(1).Clear();
        }

        [Test]
        public void DisposeMarkersOnDispose()
        {
            var markers = Enumerable.Range(0, 10)
                                    .Select(i =>
                                     {
                                         var si = GetSceneInfo(i.ToString());
                                         minimapMetadata.AddSceneInfo(si);
                                         return controller.Markers[si];
                                     }).ToList();

            controller.Dispose();

            foreach (ISceneOfInterestMarker marker in markers)
                marker.Received(1).Dispose();

            CollectionAssert.IsEmpty(controller.Markers);
        }

        [Test]
        public void UnsubscribeFromMinimapMetadataOnDispose()
        {
            controller.Dispose();

            var si = GetSceneInfo();
            minimapMetadata.AddSceneInfo(si);

            CollectionAssert.IsEmpty(controller.Markers);
        }

        private static MinimapMetadata.MinimapSceneInfo GetSceneInfo(string name = "TEST_SCENE_NAME")
        {
            return new MinimapMetadata.MinimapSceneInfo
            {
                isPOI = true,
                parcels = new List<Vector2Int>
                {
                    new (10, 10),
                    new (20, 20),
                    new (50, 50)
                },
                name = name
            };
        }
    }
}
