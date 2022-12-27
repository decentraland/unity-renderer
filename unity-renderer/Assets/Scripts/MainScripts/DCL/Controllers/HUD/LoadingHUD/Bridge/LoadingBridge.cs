using DCL;
using DCL.Interface;
using System;
using System.Collections;
using UnityEngine;

public class LoadingBridge : MonoBehaviour
{
    private bool isDecoupledLoadingScreenEnabled => DataStore.i.featureFlags.flags.Get().IsFeatureEnabled(DataStore.i.featureFlags.DECOUPLED_LOADING_SCREEN_FF);

    [Serializable]
    public class Payload
    {
        public bool isVisible;
        public string message = "";
        public bool showTips;
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
        if (isDecoupledLoadingScreenEnabled) return;

        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);

        if (payload.isVisible && !DataStore.i.loadingScreen.loadingHUD.fadeIn.Get() && !DataStore.i.loadingScreen.loadingHUD.visible.Get())
            DataStore.i.loadingScreen.loadingHUD.fadeIn.Set(true);

        if (!payload.isVisible && !DataStore.i.loadingScreen.loadingHUD.fadeOut.Get())
            DataStore.i.loadingScreen.loadingHUD.fadeOut.Set(true);

        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.loadingScreen.loadingHUD.message.Set(payload.message);

        DataStore.i.loadingScreen.loadingHUD.showTips.Set(payload.showTips);
    }

    public void FadeInLoadingHUD(string jsonMessage)
    {
        if (isDecoupledLoadingScreenEnabled) return;

        //TODO: Logic to be cleaned by the RFC-1
        StartCoroutine(WaitForLoadingHUDVisible(jsonMessage));
    }

    private IEnumerator WaitForLoadingHUDVisible(string jsonMessage)
    {
        //TODO: Logic to be cleaned by the RFC-1
        WebInterface.ReportControlEvent(new WebInterface.DeactivateRenderingACK());

        PayloadCoords payload = JsonUtility.FromJson<PayloadCoords>(jsonMessage);

        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.loadingScreen.loadingHUD.message.Set(payload.message);
        else
            DataStore.i.loadingScreen.loadingHUD.message.Set("Teleporting to " + payload.xCoord + ", " + payload.yCoord + "...");

        DataStore.i.loadingScreen.loadingHUD.percentage.Set(0);
        DataStore.i.loadingScreen.loadingHUD.fadeIn.Set(true);

        while (!DataStore.i.loadingScreen.loadingHUD.visible.Get())
            yield return null;

        WebInterface.LoadingHUDReadyForTeleport(payload.xCoord, payload.yCoord);
    }
}
