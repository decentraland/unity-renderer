using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Interface;
using UnityEngine;
using Environment = DCL.Environment;

public class TeleportController : ITeleportController
{

    private bool teleportInvoked;
    private Action unloadSceneMessages;

    public void Teleport(int x, int y)
    {
        teleportInvoked = true;
        WebInterface.GoTo(x,y);
        CoroutineStarter.Start(WaitForLoadingHUDVisible());
    }
    public void JumpIn(int coordsX, int coordsY, string serverName, string layerName)
    {
        teleportInvoked = true;
        WebInterface.JumpIn(coordsX,coordsY,serverName,layerName);
        CoroutineStarter.Start(WaitForLoadingHUDVisible());
    }
    public void GoToCrowd()
    {
        teleportInvoked = true;
        WebInterface.GoToCrowd();
        CoroutineStarter.Start(WaitForLoadingHUDVisible());
    }
    public void GoToMagic()
    {
        teleportInvoked = true;
        WebInterface.GoToMagic();
        CoroutineStarter.Start(WaitForLoadingHUDVisible());
    }
    public void QueueSceneToUnload(string sceneId)
    {
        if (teleportInvoked)
        {
            unloadSceneMessages += () => Environment.i.world.sceneController.UnloadScene(sceneId);
        }
        else
        {
            Environment.i.world.sceneController.UnloadScene(sceneId);
        }
    }

    private void TeleportHUDVisible()
    {
        unloadSceneMessages?.Invoke();
        unloadSceneMessages = null;
        if(DataStore.i.HUDs.navmapVisible.Get())
            DataStore.i.HUDs.navmapVisible.Set(false);
    }

    IEnumerator WaitForLoadingHUDVisible()
    {
        while (!DataStore.i.HUDs.loadingHUD.visible.Get())
        {
            yield return null;
        }
        TeleportHUDVisible();
    }

    public void Dispose()
    {
        
    }
    public void Initialize()
    {
    }
}
