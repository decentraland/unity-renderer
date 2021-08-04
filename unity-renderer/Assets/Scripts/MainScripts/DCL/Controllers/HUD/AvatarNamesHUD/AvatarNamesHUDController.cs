using System.Collections.Generic;
using DCL;

namespace AvatarNamesHUD
{
    public class AvatarNamesHUDController : IHUD
    {
        private const int DEFAULT_MAX_AVATARS = 200;

        private BaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;

        internal IAvatarNamesHUDView view;
        internal readonly HashSet<string> trackingPlayers = new HashSet<string>();
        internal readonly LinkedList<string> reservePlayers = new LinkedList<string>();

        internal virtual IAvatarNamesHUDView CreateView() { return AvatarNamesHUDView.CreateView(); }

        private int maxAvatarNames;

        public AvatarNamesHUDController() : this(DEFAULT_MAX_AVATARS) { }
        public AvatarNamesHUDController(int maxAvatarNames) { this.maxAvatarNames = maxAvatarNames; }

        public void Initialize()
        {
            view = CreateView();
            view?.Initialize(maxAvatarNames);

            otherPlayers.OnAdded += OnOtherPlayersStatusAdded;
            otherPlayers.OnRemoved += OnOtherPlayersStatusRemoved;
            using var enumerator = otherPlayers.Get().GetEnumerator();
            while (enumerator.MoveNext())
            {
                OnOtherPlayersStatusAdded(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        internal void OnOtherPlayersStatusAdded(string userId, Player player)
        {
            if (trackingPlayers.Contains(userId))
                return;

            if (trackingPlayers.Count < maxAvatarNames)
            {
                trackingPlayers.Add(userId);
                view?.TrackPlayer(player);
            }
            else
                reservePlayers.AddLast(userId);
        }

        internal void OnOtherPlayersStatusRemoved(string userId, Player player)
        {
            reservePlayers.Remove(userId);
            if (!trackingPlayers.Remove(userId))
                return;

            view?.UntrackPlayer(userId);
            while (reservePlayers.Count > 0)
            {
                LinkedListNode<string> reserveNode = reservePlayers.First;
                reservePlayers.RemoveFirst();
                if (trackingPlayers.Add(reserveNode.Value))
                {
                    view?.TrackPlayer(player);
                    return;
                }
            }
        }

        public void SetVisibility(bool visible) { view?.SetVisibility(visible); }

        public void Dispose()
        {
            otherPlayers.OnAdded -= OnOtherPlayersStatusAdded;
            otherPlayers.OnRemoved -= OnOtherPlayersStatusRemoved;
            view?.Dispose();
        }
    }
}