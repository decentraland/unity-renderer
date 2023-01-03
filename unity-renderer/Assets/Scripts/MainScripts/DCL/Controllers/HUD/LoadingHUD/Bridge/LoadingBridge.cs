using DCL;
using DCL.Interface;
using System;
using System.Collections;
using UnityEngine;

public class LoadingBridge : MonoBehaviour
{
    private readonly DataStoreRef<DataStore_LoadingScreen> dataStoreLoadingScreen;
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

    [Obsolete]
    public void SetLoadingScreen(string jsonMessage)
    {
        if (isDecoupledLoadingScreenEnabled) return;

        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);

        if (payload.isVisible && !dataStoreLoadingScreen.Ref.loadingHUD.fadeIn.Get() && !dataStoreLoadingScreen.Ref.loadingHUD.visible.Get())
            dataStoreLoadingScreen.Ref.loadingHUD.fadeIn.Set(true);

        if (!payload.isVisible && !dataStoreLoadingScreen.Ref.loadingHUD.fadeOut.Get())
            dataStoreLoadingScreen.Ref.loadingHUD.fadeOut.Set(true);

        if (!string.IsNullOrEmpty(payload.message))
            dataStoreLoadingScreen.Ref.loadingHUD.message.Set(payload.message);

        dataStoreLoadingScreen.Ref.loadingHUD.showTips.Set(payload.showTips);
    }

    [Obsolete]
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
            dataStoreLoadingScreen.Ref.loadingHUD.message.Set(payload.message);
        else
            dataStoreLoadingScreen.Ref.loadingHUD.message.Set("Teleporting to " + payload.xCoord + ", " + payload.yCoord + "...");

        dataStoreLoadingScreen.Ref.loadingHUD.percentage.Set(0);
        dataStoreLoadingScreen.Ref.loadingHUD.fadeIn.Set(true);

        while (!dataStoreLoadingScreen.Ref.loadingHUD.visible.Get())
            yield return null;

        WebInterface.LoadingHUDReadyForTeleport(payload.xCoord, payload.yCoord);
    }
}
