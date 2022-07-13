using System;
using DCL;

internal class OtherPlayerGetAnchorPointsHandler : IAnchorPointsGetterHandler
{
    public event Action<string> OnAvatarRemoved;
    public event Action<string, IAvatarAnchorPoints> OnAvatarFound;

    private IBaseDictionary<string, Player> otherPlayers => DataStore.i.player.otherPlayers;
    private string avatarId;

    public OtherPlayerGetAnchorPointsHandler()
    {
        otherPlayers.OnRemoved += OnOtherPlayersRemoved;
    }

    public void GetAnchorPoints(string id)
    {
        avatarId = id;

        if (otherPlayers.TryGetValue(avatarId, out Player player))
        {
            OnAvatarFound?.Invoke(avatarId, player.anchorPoints);
            return;
        }

        otherPlayers.OnAdded -= OnOtherPlayersAdded;
        otherPlayers.OnAdded += OnOtherPlayersAdded;
    }

    public void CleanUp()
    {
        avatarId = null;
        otherPlayers.OnAdded -= OnOtherPlayersAdded;
    }

    public void Dispose()
    {
        CleanUp();
        otherPlayers.OnRemoved -= OnOtherPlayersRemoved;
    }

    private void OnOtherPlayersAdded(string id, Player player)
    {
        if (id != avatarId)
        {
            return;
        }
        OnAvatarFound?.Invoke(avatarId, player.anchorPoints);
        otherPlayers.OnAdded -= OnOtherPlayersAdded;
    }

    private void OnOtherPlayersRemoved(string id, Player player)
    {
        if (id != avatarId)
        {
            return;
        }
        OnAvatarRemoved?.Invoke(avatarId);
    }
}