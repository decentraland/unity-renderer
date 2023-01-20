using DCL;
using Sentry;
using UnityEngine;
public class SentryPlugin : IPlugin
{
    public SentryPlugin()
    {
        DataStore.i.player.playerGridPosition.OnChange += PlayerGridPositionOnOnChange;
        DataStore.i.player.otherPlayers.OnAdded += OtherPlayersOnChanged;
        DataStore.i.player.otherPlayers.OnRemoved += OtherPlayersOnChanged;
        DataStore.i.player.lastTeleportPosition.OnChange += LastTeleportPositionOnOnChange;
        DataStore.i.realm.realmName.OnChange += RealmNameOnOnChange;
    }

    private void RealmNameOnOnChange(string current, string previous)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("Current Realm", current);
            scope.SetTag("Previous realm", previous);
        });
    }

    private void LastTeleportPositionOnOnChange(Vector3 current, Vector3 previous)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetExtra("Current Teleport Position", $"{current.x},{current.y}");
            scope.SetExtra("Last Teleport Position", $"{previous.x},{previous.y}");
        });
    }

    private void OtherPlayersOnChanged(string _, Player __)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetExtra("Total Other Players", $"{DataStore.i.player.otherPlayers.Count()}");
        });
    }

    private void PlayerGridPositionOnOnChange(Vector2Int current, Vector2Int previous)
    {
        SentrySdk.ConfigureScope(scope =>
        {
            scope.SetTag("Current Position", $"{current.x},{current.y}");
            scope.SetTag("Previous Position", $"{previous.x},{previous.y}");
        });
    }

    public void Dispose()
    {
    }
}
