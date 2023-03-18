using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea
{
    /// <summary>
    /// Updates players' positions within the comms radius
    /// </summary>
    internal class UsersMarkersHotAreaController : MapLayerControllerBase, IMapLayerController
    {
        internal const int PREWARM_PER_FRAME = 20;

        private readonly IUnityObjectPool<HotUserMarkerObject> objectsPool;
        private readonly IObjectPool<IHotUserMarker> wrapsPool;

        private readonly BaseDictionary<string, Player> otherPlayers;

        private readonly int prewarmCount;

        private readonly Dictionary<string, IHotUserMarker> markers = new ();

        internal IReadOnlyDictionary<string, IHotUserMarker> Markers => markers;

        public UsersMarkersHotAreaController(BaseDictionary<string, Player> otherPlayers,
            IUnityObjectPool<HotUserMarkerObject> objectsPool, IObjectPool<IHotUserMarker> wrapsPool,
            int prewarmCount, Transform parent, ICoordsUtils coordsUtils, IMapCullingController cullingController)
            : base(parent, coordsUtils, cullingController)
        {
            this.otherPlayers = otherPlayers;
            this.objectsPool = objectsPool;
            this.prewarmCount = prewarmCount;

            this.wrapsPool = wrapsPool;
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
            var wrap = wrapsPool.Get();
            wrap.TrackPlayer(player);
            markers.Add(id, wrap);
        }

        private void OnOtherPlayerRemoved(string id, Player player)
        {
            if (markers.TryGetValue(id, out var marker))
            {
                wrapsPool.Release(marker);
                markers.Remove(id);
            }
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

            foreach (IHotUserMarker marker in markers.Values)
                wrapsPool.Release(marker);

            markers.Clear();
            return UniTask.CompletedTask;
        }
    }
}
