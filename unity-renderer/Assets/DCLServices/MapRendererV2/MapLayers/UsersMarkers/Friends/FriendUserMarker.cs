using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.MapRendererV2.CommonBehavior;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.Friends
{
    internal class FriendUserMarker : IFriendUserMarker
    {
        private const int MAX_NAME_LENGTH = 15;

        private readonly ICoordsUtils coordsUtils;
        private readonly Vector3Variable worldOffset;

        private readonly IMapCullingController cullingController;

        private CancellationTokenSource cts;

        public string CurrentPlayerId { get; private set; }
        public Vector3 CurrentPosition => poolableBehavior.currentPosition;

        public Vector2 Pivot { get; }

        private MapMarkerPoolableBehavior<FriendUserMarkerObject> poolableBehavior;
        internal string profilePicUrl { get; private set; }
        internal string profileName { get; private set; }
        private AssetPromise_Texture profilePicturePromise;

        internal FriendUserMarker(IUnityObjectPool<FriendUserMarkerObject> pool, IMapCullingController mapCullingController, ICoordsUtils coordsUtils, Vector3Variable worldOffset)
        {
            this.coordsUtils = coordsUtils;
            this.worldOffset = worldOffset;
            this.cullingController = mapCullingController;
            this.Pivot = pool.Prefab.pivot;

            poolableBehavior = new MapMarkerPoolableBehavior<FriendUserMarkerObject>(pool);
        }

        public void TrackPlayer(Player player)
        {
            cts = new CancellationTokenSource();

            CurrentPlayerId = player.id;
            TrackPosition(player, cts.Token).Forget();
        }

        public void SetProfilePicture(string url)
        {
            profilePicUrl = url;
        }

        public void SetPlayerName(string name)
        {
            this.profileName = name.Length > MAX_NAME_LENGTH ? name.Substring(0, MAX_NAME_LENGTH) : name;
        }

        public void SetZoom(float baseScale, float baseZoom, float zoom)
        {
            float newScale = Math.Max(zoom / baseZoom * baseScale, baseScale);

            if (poolableBehavior.instance != null)
                poolableBehavior.instance.transform.localScale = new Vector3(newScale, newScale, 1f);
        }

        public void ResetScale(float scale)
        {
            if (poolableBehavior.instance != null)
                poolableBehavior.instance.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private async UniTaskVoid TrackPosition(Player player, CancellationToken ct)
        {
            var startedTracking = false;

            // it takes the first value
            await foreach (var position in player.WorldPositionProp)
            {
                if (ct.IsCancellationRequested)
                    return;

                var gridPosition = Utils.WorldToGridPositionUnclamped(position + worldOffset.Get());
                poolableBehavior.SetCurrentPosition(coordsUtils.PivotPosition(this, coordsUtils.CoordsToPositionUnclamped(gridPosition)));

                if (startedTracking)
                    cullingController.SetTrackedObjectPositionDirty(this);
                else
                {
                    cullingController.StartTracking(this, this);
                    startedTracking = true;
                }
            }
        }

        private void ResetPlayer()
        {
            CurrentPlayerId = null;
            cts?.Cancel();
            cts?.Dispose();
        }

        public void Dispose()
        {
            OnMapObjectCulled(this);
            cullingController.StopTracking(this);
            ResetPlayer();
        }

        public void OnMapObjectBecameVisible(IFriendUserMarker obj)
        {
            FriendUserMarkerObject friendUserMarkerObject = poolableBehavior.OnBecameVisible();
            friendUserMarkerObject.profileName.text = profileName;
            profilePicturePromise = new AssetPromise_Texture(profilePicUrl);
            profilePicturePromise.OnSuccessEvent += (assetTexture) => OnTextureDownloaded(assetTexture, friendUserMarkerObject.profilePicture);
            AssetPromiseKeeper_Texture.i.Keep(profilePicturePromise);
        }

        private void OnTextureDownloaded(Asset_Texture obj, SpriteRenderer spriteRenderer) =>
            spriteRenderer.sprite = Sprite.Create(obj.texture, new Rect(0.0f, 0.0f, obj.texture.width, obj.texture.height), new Vector2(0.5f,0.5f));

        public void OnMapObjectCulled(IFriendUserMarker obj)
        {
            poolableBehavior.OnBecameInvisible();
            AssetPromiseKeeper_Texture.i.Forget(profilePicturePromise);
        }
    }
}
