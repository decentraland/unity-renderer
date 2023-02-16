using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea
{
    internal partial struct ColdUserMarkersStorage
    {
        private readonly IColdUserMarker[] markers;
        private readonly Action<IColdUserMarker, (string realmServer, Vector2Int coords)> setData;
        private int usedMarkers;

        internal ColdUserMarkersStorage(
            IColdUserMarker[] markers,
            Action<IColdUserMarker, (string realmServer, Vector2Int coords)> setData)
        {
            this.markers = markers;
            this.setData = setData;
            usedMarkers = 0;
        }

        internal ReadOnlySpan<IColdUserMarker> Markers => markers.AsSpan()[..usedMarkers];

        internal int Capacity => markers.Length;

        internal void AdjustPoolSize(
            int newCount,
            out ReadOnlySpan<IColdUserMarker> newRents,
            out ReadOnlySpan<IColdUserMarker> recycledRents)
        {
            var recycledCount = usedMarkers - newCount;
            if (recycledCount > 0)
            {
                var indexToRecycleFrom = usedMarkers - recycledCount;
                recycledRents = markers.AsSpan().Slice(indexToRecycleFrom, recycledCount);
                newRents = ReadOnlySpan<IColdUserMarker>.Empty;
            }
            else
            {
                newRents = markers.AsSpan().Slice(usedMarkers, newCount - usedMarkers);
                recycledRents = ReadOnlySpan<IColdUserMarker>.Empty;
            }

            usedMarkers = newCount;
        }

        internal void Dispose()
        {
            if (markers == null)
                return;

            foreach (IColdUserMarker marker in markers)
                marker.Dispose();
        }

        internal void Update(IReadOnlyList<IHotScenesController.HotSceneInfo> scenesList,
            out ReadOnlySpan<IColdUserMarker> newRents,
            out ReadOnlySpan<IColdUserMarker> recycledRents)
        {
            var scenesCount = scenesList.Count;

            // count users parcels first so we don't add superfluous elements
            var usersParcelsCount = 0;
            var maxMarkers = markers.Length;

            for (var i = 0; i < scenesList.Count; i++)
                usersParcelsCount += scenesList[i].usersTotalCount;

            // Partition parcels to fit `maxMarkers`
            var markersCount = Mathf.Min(usersParcelsCount, maxMarkers);

            AdjustPoolSize(markersCount, out newRents, out recycledRents);

            var step = Mathf.Max(1, usersParcelsCount / maxMarkers);

            var partitionPointer = 0;
            var dataIndex = 0;

            for (var i = 0; i < scenesCount; i++)
            {
                var sceneInfo = scenesList[i];

                if (sceneInfo.usersTotalCount <= 0)
                    continue;

                for (int realmIdx = 0; realmIdx < sceneInfo.realms.Length; realmIdx++)
                {
                    var realm = sceneInfo.realms[realmIdx];

                    for (int parcelIdx = 0; parcelIdx < realm.userParcels.Length && dataIndex < markersCount; parcelIdx++)
                    {
                        if (partitionPointer == 0)
                        {
                            var obj = markers[dataIndex];
                            setData(obj, (realm.serverName, realm.userParcels[parcelIdx]));

                            dataIndex++;
                            partitionPointer += step - 1;
                        }
                        else
                        {
                            partitionPointer--;
                        }
                    }
                }
            }
        }
    }
}
