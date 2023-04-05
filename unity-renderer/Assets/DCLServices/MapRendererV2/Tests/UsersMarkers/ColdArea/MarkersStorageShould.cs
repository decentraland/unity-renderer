using DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea;
using MainScripts.DCL.Controllers.HotScenes;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCLServices.MapRendererV2.Tests.UsersMarkers.ColdArea
{
    [Category("EditModeCI")]
    [TestFixture]
    public class MarkersStorageShould
    {
        private ColdUserMarkersStorage storage;
        private IColdUserMarker[] markers;

        [SetUp]
        public void Setup()
        {
            storage = new ColdUserMarkersStorage(
                markers = Enumerable.Range(0, 10).Select(_ => Substitute.For<IColdUserMarker>()).ToArray(),
                (marker, tuple) => marker.SetData(tuple.realmServer, null, tuple.coords, default));
        }

        [Test]
        public void ReturnActiveMarkersOnly([Values(1, 3, 5, 8)] int newCount)
        {
            storage.AdjustPoolSize(10, out _, out _);
            storage.AdjustPoolSize(newCount, out _, out _);

            CollectionAssert.AreEqual(markers.AsSpan().Slice(0, newCount).ToArray(), storage.Markers.ToArray());
        }

        [Test]
        public void AssignNewRents()
        {
            storage.AdjustPoolSize(3, out _, out _);
            storage.AdjustPoolSize(7, out var newRents, out var recycledRents);

            CollectionAssert.AreEqual(markers.AsSpan().Slice(3, 4).ToArray(), newRents.ToArray());
            CollectionAssert.IsEmpty(recycledRents.ToArray());
        }

        [Test]
        public void AssignRecycledRents()
        {
            storage.AdjustPoolSize(8, out _, out _);
            storage.AdjustPoolSize(2, out var newRents, out var recycledRents);

            CollectionAssert.IsEmpty(newRents.ToArray());
            CollectionAssert.AreEqual(markers.AsSpan().Slice(2, 6).ToArray(), recycledRents.ToArray());
        }

        [Test]
        public void CleanUp()
        {
            storage.Dispose();

            foreach (IColdUserMarker marker in markers)
                marker.Received(1).Dispose();
        }

        [Test]
        [Sequential]
        public void PartitionParcelsProperly(
            [Values(1, 1, 4, 2)] int scenesCount,
            [Values(2, 5, 4, 5)] int realmsCount,
            [Values(2, 2, 1, 4)] int parcelsCount,
            [Values(new[] { 0, 1, 2, 3 }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new[] { 0, 4, 8, 12, 16, 20, 24, 28, 32, 36 })]
            int[] expectedParcelsIndices,
            [Values(new[] { 0, 0, 1, 1 }, new[] { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4 }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
            int[] expectedRealmIds,
            [Values(new[] { 4, 5, 6, 7, 8, 9 }, new int[0], new int[0], new int[0])] int[] unexpectedParcelsIndices)
        {
            storage.Update(CreateSceneInfos(scenesCount, realmsCount, parcelsCount), out _, out _);

            Assert.AreEqual(Mathf.Min(expectedParcelsIndices.Length, markers.Length), storage.Markers.Length);

            for (var markerIndex = 0; markerIndex < expectedParcelsIndices.Length; markerIndex++)
            {
                var parcelIndex = expectedParcelsIndices[markerIndex];

                markers[markerIndex]
                   .Received(1)
                   .SetData(expectedRealmIds[markerIndex].ToString(), Arg.Any<string>(), new Vector2Int(parcelIndex, parcelIndex), Arg.Any<Vector3>());
            }

            foreach (int parcelsIndex in unexpectedParcelsIndices)
                markers[parcelsIndex].DidNotReceive().SetData(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Vector2Int>(), Arg.Any<Vector3>());
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
