﻿using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea;
using MainScripts.DCL.Controllers.HotScenes;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct ColdUsersMarkersInstaller
    {
        private const int COLD_USER_MARKERS_LIMIT = 100;

        private const string COLD_USER_MARKER_ADDRESS = "ColdUserMarker";

        private Service<IHotScenesFetcher> hotScenesFetcher;
        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            Dictionary<MapLayer, IMapLayerController> writer,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            CancellationToken cancellationToken)
        {
            var prefab = await GetPrefab(cancellationToken);

            var controller = new UsersMarkersColdAreaController(configuration.ColdUserMarkersRoot, prefab, ColdUserMarker.Create,
                hotScenesFetcher.Ref, DataStore.i.realm.realmName, CommonScriptableObjects.playerCoords, KernelConfig.i, coordsUtils, cullingController, MapRendererDrawOrder.COLD_USER_MARKERS, COLD_USER_MARKERS_LIMIT);

            await controller.Initialize(cancellationToken).SuppressCancellationThrow();
            writer.Add(MapLayer.ColdUsersMarkers, controller);
        }

        internal async UniTask<ColdUserMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<ColdUserMarkerObject>(COLD_USER_MARKER_ADDRESS, cancellationToken);
    }
}
