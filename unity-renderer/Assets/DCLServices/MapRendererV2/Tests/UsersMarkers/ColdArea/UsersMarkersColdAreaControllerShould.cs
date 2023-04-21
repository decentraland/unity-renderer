using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea;
using KernelConfigurationTypes;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Environment = DCL.Environment;

namespace DCLServices.MapRendererV2.Tests.UsersMarkers.ColdArea
{
    [Category("EditModeCI")]
    [TestFixture]
    public class UsersMarkersColdAreaControllerShould
    {
        private const int MAX_MARKERS = 100;
        private UsersMarkersColdAreaController controller;
        private UsersMarkersColdAreaController.ColdUserMarkerBuilder builder;
        private BaseVariable<string> realmName;
        private Vector2IntVariable userPosition;
        private ICoordsUtils coordsUtils;
        private IMapCullingController cullingController;
        private IHotScenesFetcher hotScenesFetcher;

        [SetUp]
        public void Setup()
        {
            hotScenesFetcher = Substitute.For<IHotScenesFetcher>();

            KernelConfig.i.Set(new KernelConfigModel {comms = new Comms {commRadius = 2}});

            controller = new UsersMarkersColdAreaController(
                null,
                null,
                builder = Substitute.For<UsersMarkersColdAreaController.ColdUserMarkerBuilder>(),
                hotScenesFetcher,
                realmName = new BaseVariable<string>("TEST_REALM"),
                userPosition = ScriptableObject.CreateInstance<Vector2IntVariable>(),
                KernelConfig.i,
                coordsUtils = Substitute.For<ICoordsUtils>(),
                cullingController = Substitute.For<IMapCullingController>(),
                MAX_MARKERS);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public async Task CreateMarkersInBatches()
        {
            var invocationFrames = new List<int>();

            int frameNumber = 0;

            void CountEditorFrames()
            {
                frameNumber++;
            }

            try
            {
                EditorApplication.update += CountEditorFrames;

                builder.Invoke(Arg.Any<ColdUserMarkerObject>(), Arg.Any<Transform>())
                       .Returns(_ =>
                        {
                            invocationFrames.Add(frameNumber);
                            return Substitute.For<IColdUserMarker>();
                        });

                var expectedFrames = new List<int>();

                var count = MAX_MARKERS;
                var i = 0;
                var skipBatch = true;

                // first two batches happen the same frame because of the special logic in UniTask

                while (count > 0)
                {
                    for (var j = 0; j < UsersMarkersColdAreaController.CREATE_PER_BATCH && count > 0; j++)
                    {
                        expectedFrames.Add(i);
                        count--;
                    }

                    if (skipBatch)
                        skipBatch = false;
                    else i++;
                }

                await controller.Initialize(CancellationToken.None);
                CollectionAssert.AreEqual(expectedFrames, invocationFrames);
            }
            finally { EditorApplication.update -= CountEditorFrames; }
        }

        [Test]
        public async Task CreateCorrectNumberOfMarkers()
        {
            builder.Invoke(Arg.Any<ColdUserMarkerObject>(), Arg.Any<Transform>())
                   .Returns(_ => Substitute.For<IColdUserMarker>());

            await controller.Initialize(CancellationToken.None);
            Assert.AreEqual(MAX_MARKERS, controller.Storage_Test.Capacity);
        }

        [Test]
        public async Task SetNewRentsDirty()
        {
            await controller.Initialize(CancellationToken.None);

            var s1 = CreateSceneInfos(1, 10, 2);
            controller.RenewSceneInfos_Test(s1);

            var s2 = CreateSceneInfos(1, 10, 4);
            controller.RenewSceneInfos_Test(s2);

            var newRents = controller.Storage_Test.Markers.Slice(20, 20).ToArray();

            foreach (IColdUserMarker marker in newRents)
            {
                cullingController.Received().StartTracking(marker, controller);
                cullingController.Received().SetTrackedObjectPositionDirty(marker);
            }
        }

        [Test]
        public async Task StopTrackingRecycledMarkers()
        {
            await controller.Initialize(CancellationToken.None);

            var s1 = CreateSceneInfos(1, 10, 4);
            controller.RenewSceneInfos_Test(s1);

            var recycled = controller.Storage_Test.Markers.Slice(20, 20).ToArray();

            var s2 = CreateSceneInfos(1, 10, 2);
            controller.RenewSceneInfos_Test(s2);

            foreach (IColdUserMarker marker in recycled)
                cullingController.Received().StopTracking(marker);
        }

        [Test]
        public void ChangeRealmForActiveMarkers()
        {
            realmName.Set("VALUE_2");

            foreach (IColdUserMarker marker in controller.Storage_Test.Markers)
                marker.Received().OnRealmChanged("VALUE_2");
        }

        [Test]
        public void SetCulled()
        {
            var marker = Substitute.For<IColdUserMarker>();
            controller.OnMapObjectCulled(marker);

            marker.Received(1).SetCulled(true);
        }

        [Test]
        public void SetVisible()
        {
            var marker = Substitute.For<IColdUserMarker>();
            controller.OnMapObjectBecameVisible(marker);

            marker.Received(1).SetCulled(false);
        }

        [Test]
        public async Task SetUpdateModeOnEnable()
        {
            await controller.Initialize(CancellationToken.None);

            var cts = new CancellationTokenSource();

            hotScenesFetcher.ScenesInfo.Returns(new AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>>(Array.Empty<IHotScenesController.HotSceneInfo>()));

            try
            {
                controller.Enable(cts.Token);
                hotScenesFetcher.Received(1).SetUpdateMode(IHotScenesFetcher.UpdateMode.FOREGROUND);
            }
            finally
            {
                cts.Cancel();
            }
        }

        [Test]
        public void SetUpdateModeOnDisable()
        {
            var cts = new CancellationTokenSource();

            try
            {
                controller.Disable(cts.Token);
                hotScenesFetcher.Received(1).SetUpdateMode(IHotScenesFetcher.UpdateMode.BACKGROUND);
            }
            finally
            {
                cts.Cancel();
            }
        }

        [Test]
        public async Task ReactOnNewHotScenes()
        {
            await controller.Initialize(CancellationToken.None);
            var cts = new CancellationTokenSource();

            try
            {
                var s1 = CreateSceneInfos(2, 2, 3);

                var reactiveProp = new AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>>(s1);
                hotScenesFetcher.ScenesInfo.Returns(reactiveProp);

                controller.Enable(cts.Token);

                await UniTask.DelayFrame(1, cancellationToken: cts.Token);

                // Should not skip the first one
                Assert.AreEqual(12, controller.Storage_Test.Markers.Length);

                await UniTask.DelayFrame(1, cancellationToken: cts.Token);
                var s2 = CreateSceneInfos(3, 5, 3);
                reactiveProp.Value = s2;

                await UniTask.DelayFrame(1, cancellationToken: cts.Token);
                Assert.AreEqual(45, controller.Storage_Test.Markers.Length);
            }
            finally
            {
                cts.Cancel();
            }
        }

        [Test]
        public async Task SuspendUpdateLoopGracefully()
        {
            await controller.Initialize(CancellationToken.None);

            var cts = new CancellationTokenSource();

            var s1 = CreateSceneInfos(2, 2, 3);

            var reactiveProp = new AsyncReactiveProperty<IReadOnlyList<IHotScenesController.HotSceneInfo>>(s1);
            hotScenesFetcher.ScenesInfo.Returns(reactiveProp);

            controller.Enable(cts.Token);

            await UniTask.DelayFrame(1);

            cts.Cancel();

            await UniTask.DelayFrame(1);
            var s2 = CreateSceneInfos(3, 5, 3);
            reactiveProp.Value = s2;

            Assert.AreEqual(12, controller.Storage_Test.Markers.Length);
        }

        private static IReadOnlyList<IHotScenesController.HotSceneInfo> CreateSceneInfos(int scenesCount, int realmsCount, int parcelsCount)
        {
            var result = new IHotScenesController.HotSceneInfo[scenesCount];
            var realmId = 0;
            var parcelId = 0;

            for (var i = 0; i < scenesCount; i++)
            {
                var scene = new IHotScenesController.HotSceneInfo
                {
                    realms = Enumerable.Repeat(0, realmsCount)
                                       .Select(_ =>
                                        {
                                            var realm = new IHotScenesController.HotSceneInfo.Realm
                                            {
                                                serverName = realmId.ToString(),
                                                userParcels = Enumerable.Repeat(0, parcelsCount)
                                                                        .Select(_ =>
                                                                         {
                                                                             var d = new Vector2Int(parcelId, parcelId);
                                                                             parcelId++;
                                                                             return d;
                                                                         })
                                                                        .ToArray()
                                            };

                                            realmId++;
                                            return realm;
                                        })
                                       .ToArray(),
                    usersTotalCount = realmsCount * parcelsCount
                };

                result[i] = scene;
            }

            return result;
        }
    }
}
