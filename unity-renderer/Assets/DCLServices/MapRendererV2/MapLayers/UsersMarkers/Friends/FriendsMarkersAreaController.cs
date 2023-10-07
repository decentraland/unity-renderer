using Cysharp.Threading.Tasks;
using DCL.Social.Friends;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SocialPlatforms.Impl;
using Environment = DCL.Environment;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.Friends
{
    /// <summary>
    /// Updates players' positions within the comms radius
    /// </summary>
    internal class FriendsMarkersAreaController : MapLayerControllerBase, IMapLayerController, IZoomScalingLayer
    {
        internal const int PREWARM_PER_FRAME = 20;

        private readonly IUnityObjectPool<FriendUserMarkerObject> objectsPool;
        private readonly IObjectPool<IFriendUserMarker> wrapsPool;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IFriendsController friendsController;

        private readonly BaseDictionary<string, Player> otherPlayers;

        private readonly int prewarmCount;

        private readonly Dictionary<string, IFriendUserMarker> markers = new ();

        internal IReadOnlyDictionary<string, IFriendUserMarker> Markers => markers;

        public FriendsMarkersAreaController(BaseDictionary<string, Player> otherPlayers,
            IUnityObjectPool<FriendUserMarkerObject> objectsPool, IObjectPool<IFriendUserMarker> wrapsPool,
            int prewarmCount, Transform parent, ICoordsUtils coordsUtils, IMapCullingController cullingController, IUserProfileBridge userProfileBridge,
            IFriendsController friendsController)
            : base(parent, coordsUtils, cullingController)
        {
            this.otherPlayers = otherPlayers;
            this.objectsPool = objectsPool;
            this.prewarmCount = prewarmCount;
            this.wrapsPool = wrapsPool;
            this.userProfileBridge = userProfileBridge;
            this.friendsController = friendsController;
        }

        public UniTask Initialize(CancellationToken cancellationToken)
        {
            wrapsPool.Prewarm(prewarmCount);
            return objectsPool.PrewarmAsync(prewarmCount, PREWARM_PER_FRAME, LinkWithDisposeToken(cancellationToken).Token);
        }

        protected override void DisposeImpl()
        {
            objectsPool.Clear();
            wrapsPool.Clear();

            otherPlayers.OnAdded -= OnOtherPlayerAdded;
            otherPlayers.OnRemoved -= OnOtherPlayerRemoved;
        }

        private void OnOtherPlayerAdded(string id, Player player)
        {
            CheckIfIsFriend(player).Forget();
        }

        private async UniTaskVoid CheckIfIsFriend(Player player)
        {
            if (await friendsController.GetFriendshipStatus(player.id) != FriendshipStatus.FRIEND)
                return;

            if (markers.ContainsKey(player.id))
                return;

            UserProfile recipientProfile = userProfileBridge.Get(player.id);

            try { recipientProfile ??= await userProfileBridge.RequestFullUserProfileAsync(player.id, CancellationToken.None); }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                throw;
            }

            var wrap = wrapsPool.Get();
            wrap.TrackPlayer(player);
            wrap.SetProfilePicture(recipientProfile.face256SnapshotURL);
            wrap.SetPlayerName(player.name);
            markers.Add(player.id, wrap);
        }

        private void OnOtherPlayerRemoved(string id, Player player)
        {
            if (markers.TryGetValue(id, out var marker))
            {
                wrapsPool.Release(marker);
                markers.Remove(id);
            }
        }

        public void ApplyCameraZoom(float baseZoom, float zoom)
        {
            foreach (var marker in markers.Values)
                marker.SetZoom(coordsUtils.ParcelSize, baseZoom, zoom);
        }

        public void ResetToBaseScale()
        {
            foreach (var marker in markers.Values)
                marker.ResetScale(coordsUtils.ParcelSize);
        }

        public UniTask Enable(CancellationToken cancellationToken)
        {
            foreach (var pair in otherPlayers.Get())
                OnOtherPlayerAdded(pair.Key, pair.Value);

            otherPlayers.OnAdded += OnOtherPlayerAdded;
            otherPlayers.OnRemoved += OnOtherPlayerRemoved;
            return UniTask.CompletedTask;
        }

        public UniTask Disable(CancellationToken cancellationToken)
        {
            otherPlayers.OnAdded -= OnOtherPlayerAdded;
            otherPlayers.OnRemoved -= OnOtherPlayerRemoved;

            foreach (IFriendUserMarker marker in markers.Values)
                wrapsPool.Release(marker);

            markers.Clear();
            return UniTask.CompletedTask;
        }
    }
}
