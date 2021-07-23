using System.Collections.Generic;
using DCL;

namespace AvatarNamesHUD
{

    public class AvatarNamesHUDController : IHUD
    {
        public const int MAX_AVATAR_NAMES = 200;

        private BaseDictionary<string, PlayerStatus> otherPlayersStatus => DataStore.i.player.otherPlayersStatus;

        internal IAvatarNamesHUDView view;
        internal readonly HashSet<string> trackingPlayers = new HashSet<string>();
        internal readonly LinkedList<string> reservePlayers = new LinkedList<string>();

        internal virtual IAvatarNamesHUDView CreateView() { return AvatarNamesHUDView.CreateView(); }

        public void Initialize()
        {
            view = CreateView();

            otherPlayersStatus.OnAdded += OnOtherPlayersStatusAdded;
            otherPlayersStatus.OnRemoved += OnOtherPlayersStatusRemoved;

            using var enumerator = otherPlayersStatus.Get().GetEnumerator();
            while (enumerator.MoveNext())
            {
                OnOtherPlayersStatusAdded(enumerator.Current.Key, enumerator.Current.Value);
            }
        }

        private void OnOtherPlayersStatusAdded(string userId, PlayerStatus playerStatus)
        {
            if (trackingPlayers.Contains(userId))
                return;

            if (trackingPlayers.Count < MAX_AVATAR_NAMES)
            {
                trackingPlayers.Add(userId);
                view?.TrackPlayer(playerStatus);
            }
            else
                reservePlayers.AddLast(userId);
        }

        private void OnOtherPlayersStatusRemoved(string userId, PlayerStatus playerStatus)
        {
            if (reservePlayers.Remove(userId))
                view?.UntrackPlayer(playerStatus);

            if (!trackingPlayers.Remove(userId))
                return;

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
        }
    }
}