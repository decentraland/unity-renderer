using System.Collections.Generic;
using DCL;

namespace AvatarNamesHUD
{
    public class AvatarNamesHUDController : IHUD
    {
        private int maxAvatarNames;

        private BaseDictionary<string, PlayerStatus> otherPlayersStatus => DataStore.i.player.otherPlayersStatus;

        internal IAvatarNamesHUDView view;
        internal readonly HashSet<string> trackingPlayers = new HashSet<string>();
        internal readonly LinkedList<string> reservePlayers = new LinkedList<string>();

        internal virtual IAvatarNamesHUDView CreateView() { return AvatarNamesHUDView.CreateView(); }

        public AvatarNamesHUDController() : this(200) { }
        public AvatarNamesHUDController(int maxAvatarNames) { this.maxAvatarNames = maxAvatarNames; }

        public void Initialize()
        {
            view = CreateView();
            view?.Initialize(maxAvatarNames);

            otherPlayersStatus.OnAdded += OnOtherPlayersStatusAdded;
            otherPlayersStatus.OnRemoved += OnOtherPlayersStatusRemoved;
            using var enumerator = otherPlayersStatus.Get().GetEnumerator();
            while (enumerator.MoveNext())
            {
                OnOtherPlayersStatusAdded(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        internal void OnOtherPlayersStatusAdded(string userId, PlayerStatus playerStatus)
        {
            if (trackingPlayers.Contains(userId))
                return;

            if (trackingPlayers.Count < maxAvatarNames)
            {
                trackingPlayers.Add(userId);
                view?.TrackPlayer(playerStatus);
            }
            else
                reservePlayers.AddLast(userId);
        }

        internal void OnOtherPlayersStatusRemoved(string userId, PlayerStatus playerStatus)
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
                    view?.TrackPlayer(playerStatus);
                    return;
                }
            }
        }

        public void SetVisibility(bool visible) { view?.SetVisibility(visible); }

        public void Dispose()
        {
            otherPlayersStatus.OnAdded -= OnOtherPlayersStatusAdded;
            otherPlayersStatus.OnRemoved -= OnOtherPlayersStatusRemoved;
            view?.Dispose();
        }
    }
}