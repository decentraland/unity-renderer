using System;
using System.Collections;
using DCL;
using DCL.Interface;
using UnityEngine;

public class TeleportController : ITeleportController
{
    
    public void Teleport(int x, int y)
    {
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
        CoroutineStarter.Start(WaitForLoadingHUDVisible(() => WebInterface.GoTo(x, y)));
    }

    public void JumpIn(int coordsX, int coordsY, string serverName, string layerName)
    {
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
        CoroutineStarter.Start(WaitForLoadingHUDVisible(() => WebInterface.JumpIn(coordsX, coordsY, serverName, layerName)));
    }

    public void GoToCrowd()
    {
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
        CoroutineStarter.Start(WaitForLoadingHUDVisible(WebInterface.GoToCrowd));
    }

    public void GoToMagic()
    {
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
        CoroutineStarter.Start(WaitForLoadingHUDVisible(WebInterface.GoToMagic));
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
    }
    
    public void Initialize()
    {
        
    }
    
}

