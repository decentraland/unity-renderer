using System;
using System.Collections;
using DCL.Interface;
using UnityEngine;
using DataStore = DCL.DataStore;

public static class BuilderInWorldTeleportAndEdit
{
    public static event Action<Vector2Int> OnTeleportStart;
    public static event Action<Vector2Int> OnTeleportEnd;

    private static bool inProgress = false;

    public static bool TeleportAndEdit(Vector2Int targetCoords)
    {
        if (inProgress)
            return false;
        
        CoroutineStarter.Start(TeleportAndEditRoutine(targetCoords));
        return true;
    }

    private static IEnumerator TeleportAndEditRoutine(Vector2Int targetCoords)
    {
        inProgress = true;
        OnTeleportStart?.Invoke(targetCoords);
        
        bool isPlayerTeleported = false;

        void OnPlayerTeleportToNewPosition(Vector3 current, Vector3 prev)
        {
            isPlayerTeleported = true;
        }

        DataStore.i.player.lastTeleportPosition.OnChange += OnPlayerTeleportToNewPosition;

        WebInterface.GoTo(targetCoords.x, targetCoords.y);
        
        yield return new WaitUntil(() => isPlayerTeleported);
        DataStore.i.player.lastTeleportPosition.OnChange -= OnPlayerTeleportToNewPosition;
        
        yield return new WaitUntil(() => CommonScriptableObjects.rendererState.Get());
        yield return null;
        
        inProgress = false;
        OnTeleportEnd?.Invoke(targetCoords);
    }
}
