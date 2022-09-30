using System;
using System.Collections;
using DCL;
using DCL.Interface;
using UnityEngine;

public class TeleportController : ITeleportController
{

    private bool teleportDone;
    
    public void Teleport(int x, int y)
    {
        teleportDone = true;
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CoroutineStarter.Start(WaitForLoadingHUDVisible(() => WebInterface.GoTo(x, y)));
    }

    public void JumpIn(int coordsX, int coordsY, string serverName, string layerName)
    {
        teleportDone = true;
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CoroutineStarter.Start(WaitForLoadingHUDVisible(() => WebInterface.JumpIn(coordsX, coordsY, serverName, layerName)));
    }

    public void GoToCrowd()
    {
        teleportDone = true;
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CoroutineStarter.Start(WaitForLoadingHUDVisible(WebInterface.GoToCrowd));
    }

    public void GoToMagic()
    {
        teleportDone = true;
        CommonScriptableObjects.playerCoords.OnChange += OnPlayerCoordsChange;
        CoroutineStarter.Start(WaitForLoadingHUDVisible(WebInterface.GoToMagic));
    }
    private void OnPlayerCoordsChange(Vector2Int current, Vector2Int previous)
    {
        teleportDone = false;
        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChange;
    }

    public void SetLoadingPayload(string jsonMessage)
    {
        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);

        if (payload.isVisible && !DataStore.i.HUDs.loadingHUD.visible.Get())
        {
            DataStore.i.HUDs.loadingHUD.fadeIn.Set(true);
        }
        if (!payload.isVisible && DataStore.i.HUDs.loadingHUD.visible.Get() && !teleportDone)
        {
            DataStore.i.HUDs.loadingHUD.fadeOut.Set(true);
        }
        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.HUDs.loadingHUD.message.Set(payload.message);

        DataStore.i.HUDs.loadingHUD.showTips.Set(payload.showTips);
    }

    IEnumerator WaitForLoadingHUDVisible(Action teleportToRun)
    {
        DataStore.i.HUDs.loadingHUD.fadeIn.Set(true);
        while (!DataStore.i.HUDs.loadingHUD.visible.Get())
        {
            yield return null;
        }
        teleportToRun?.Invoke();
    }

    public void Dispose()
    {
        CommonScriptableObjects.playerCoords.OnChange -= OnPlayerCoordsChange;
    }
    
    public void Initialize()
    {
        
    }
    
}

[Serializable]
public class Payload
{
    public bool isVisible = false;
    public string message = "";
    public bool showTips = false;
}