using System;
using DCL;

internal class OwnPlayerGetAnchorPointsHandler : IAnchorPointsGetterHandler
{
    public event Action<string> OnAvatarRemoved;
    public event Action<string, IAvatarAnchorPoints> OnAvatarFound;

    private IBaseVariable<Player> ownPlayer => DataStore.i.player.ownPlayer;
    private string avatarId;

    public void GetAnchorPoints(string id)
    {
        avatarId = id;
        string ownPlayerId = ownPlayer.Get()?.id;

        // NOTE: ownPlayerId null means player avatar is not loaded yet
        // so we subscribe to the event for when player avatar is set.
        if (string.IsNullOrEmpty(ownPlayerId))
        {
            ownPlayer.OnChange -= OnOwnPlayerChanged;
            ownPlayer.OnChange += OnOwnPlayerChanged;
            return;
        }

        if (id == ownPlayerId)
        {
            OnAvatarFound?.Invoke(id, ownPlayer.Get().anchorPoints);
        }
    }

    public void CleanUp()
    {
        ownPlayer.OnChange -= OnOwnPlayerChanged;
    }

    public void Dispose()
    {
        CleanUp();
    }

    private void OnOwnPlayerChanged(Player player, Player prev)
    {
        if (player.id != avatarId)
        {
            return;
        }

        ownPlayer.OnChange -= OnOwnPlayerChanged;
        OnAvatarFound?.Invoke(player.id, player.anchorPoints);
    }
}