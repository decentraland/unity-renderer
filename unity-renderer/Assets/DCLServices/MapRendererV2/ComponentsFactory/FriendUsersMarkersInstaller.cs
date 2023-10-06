using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCL.Social.Friends;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.Friends;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct FriendUsersMarkersInstaller
    {
        private const string FRIEND_USER_MARKER_ADDRESS = "FriendUserMarker";
        private const int FRIEND_USER_MARKERS_PREWARM_COUNT = 30;

        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            Dictionary<MapLayer, IMapLayerController> writer,
            List<IZoomScalingLayer> zoomScalingWriter,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            IUserProfileBridge userProfileBridge,
            IFriendsController friendsController,
            CancellationToken cancellationToken)
        {
            var prefab = await GetPrefab(cancellationToken);

            void OnCreate(FriendUserMarkerObject obj)
            {
                for (var i = 0; i < obj.spriteRenderers.Length; i++)
                    obj.spriteRenderers[i].sortingOrder = MapRendererDrawOrder.FRIEND_USER_MARKERS;

                obj.profileName.sortingOrder = MapRendererDrawOrder.FRIEND_USER_MARKERS;
                coordsUtils.SetObjectScale(obj);
            }

            var objectsPool = new UnityObjectPool<FriendUserMarkerObject>(prefab, configuration.FriendUserMarkersRoot, actionOnCreate: OnCreate, defaultCapacity: FRIEND_USER_MARKERS_PREWARM_COUNT);

            IFriendUserMarker CreateWrap() =>
                new FriendUserMarker(objectsPool, cullingController, coordsUtils, CommonScriptableObjects.worldOffset);

            var wrapsPool = new ObjectPool<IFriendUserMarker>(CreateWrap, actionOnRelease: m => m.Dispose(), defaultCapacity: FRIEND_USER_MARKERS_PREWARM_COUNT);

            var controller = new FriendsMarkersAreaController(
                DataStore.i.player.otherPlayers,
                objectsPool,
                wrapsPool,
                FRIEND_USER_MARKERS_PREWARM_COUNT,
                configuration.HotUserMarkersRoot,
                coordsUtils, cullingController,
                userProfileBridge,
                friendsController);

            await controller.Initialize(cancellationToken);
            writer.Add(MapLayer.Friends, controller);
            zoomScalingWriter.Add(controller);
        }

        internal async Task<FriendUserMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<FriendUserMarkerObject>(FRIEND_USER_MARKER_ADDRESS, cancellationToken);
    }
}
