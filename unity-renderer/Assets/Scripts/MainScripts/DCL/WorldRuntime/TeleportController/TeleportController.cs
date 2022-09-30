using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.Interface;
using UnityEngine;
using Environment = DCL.Environment;

public class TeleportController : ITeleportController
{


    public void Teleport(int x, int y)
    {
        CoroutineStarter.Start(WaitForLoadingHUDVisible(() => WebInterface.GoTo(x, y)));
    }

    public void JumpIn(int coordsX, int coordsY, string serverName, string layerName)
    {
        CoroutineStarter.Start(WaitForLoadingHUDVisible(() => WebInterface.JumpIn(coordsX, coordsY, serverName, layerName)));
    }

    public void GoToCrowd()
    {
        CoroutineStarter.Start(WaitForLoadingHUDVisible(WebInterface.GoToCrowd));
    }

    public void GoToMagic()
    {
        CoroutineStarter.Start(WaitForLoadingHUDVisible(WebInterface.GoToMagic));
    }

    public void SetLoadingPayload(string jsonMessage)
    {
        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);

        if (payload.isVisible && !DataStore.i.HUDs.loadingHUD.visible.Get())
            DataStore.i.HUDs.loadingHUD.fadeIn.Set(true);

        if (!payload.isVisible && DataStore.i.HUDs.loadingHUD.visible.Get() && payload.message.Contains("Loading"))
            DataStore.i.HUDs.loadingHUD.fadeOut.Set(true);

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

    public void Dispose() { }
    public void Initialize() { }
    
}

[Serializable]
public class Payload
{
    public bool isVisible = false;
    public string message = "";
    public bool showTips = false;
}