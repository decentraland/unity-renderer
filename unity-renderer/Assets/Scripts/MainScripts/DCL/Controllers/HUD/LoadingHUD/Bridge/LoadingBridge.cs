using System;
using System.Collections;
using DCL;
using DCL.Interface;
using UnityEngine;

public class LoadingBridge : MonoBehaviour
{
    [Serializable]
    public class Payload
    {
        public bool isVisible = false;
        public string message = "";
        public bool showTips = false;
    }

    [Serializable]
    public class PayloadCoords
    {
        public int xCoord;
        public int yCoord;
        public string message;
    }

    public void SetLoadingScreen(string jsonMessage)
    {
        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);

        if (payload.isVisible && !DataStore.i.HUDs.loadingHUD.fadeIn.Get() && !DataStore.i.HUDs.loadingHUD.visible.Get())
            DataStore.i.HUDs.loadingHUD.fadeIn.Set(true);

        if (!payload.isVisible && !DataStore.i.HUDs.loadingHUD.fadeOut.Get())
            DataStore.i.HUDs.loadingHUD.fadeOut.Set(true);

        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.HUDs.loadingHUD.message.Set(payload.message);
        DataStore.i.HUDs.loadingHUD.showTips.Set(payload.showTips);
    }
    
    public void FadeInLoadingHUD(string jsonMessage)
    {
        //TODO: Logic to be cleaned by the RFC-1
        StartCoroutine(WaitForLoadingHUDVisible(jsonMessage));
    }
    
    IEnumerator WaitForLoadingHUDVisible(string jsonMessage)
    {
        //TODO: Logic to be cleaned by the RFC-1
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());
        
        PayloadCoords payload = JsonUtility.FromJson<PayloadCoords>(jsonMessage);
        if (!string.IsNullOrEmpty(payload.message))
        {
            DataStore.i.HUDs.loadingHUD.message.Set(payload.message);
        }
        else
        {
            DataStore.i.HUDs.loadingHUD.message.Set("Teleporting to " + payload.xCoord + ", " + payload.yCoord + "...");
        }
        DataStore.i.HUDs.loadingHUD.percentage.Set(0);
        DataStore.i.HUDs.loadingHUD.fadeIn.Set(true);

        while (!DataStore.i.HUDs.loadingHUD.visible.Get())
        {
            yield return null;
        }
        
        WebInterface.LoadingHUDReadyForTeleport(payload.xCoord, payload.yCoord);
    }
}