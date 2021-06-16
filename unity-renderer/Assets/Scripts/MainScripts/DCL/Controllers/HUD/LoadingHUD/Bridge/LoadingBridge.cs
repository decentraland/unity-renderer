using System;
using DCL;
using UnityEngine;

public class LoadingBridge : MonoBehaviour
{
    [Serializable]
    public class Payload
    {
        public bool isVisible = false;
        public string message = "";
        public bool showWalletPrompt = false;
        public bool showTips = false;
    }

    public void SetLoadingScreen(string jsonMessage)
    {
        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);
        DataStore.i.HUDs.loadingHUD.visible.Set(payload.isVisible);
        if (!payload.isVisible)
            DataStore.i.HUDs.loadingHUD.percentage.Set(0);

        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.HUDs.loadingHUD.message.Set(payload.message);
        DataStore.i.HUDs.loadingHUD.showWalletPrompt.Set(payload.showWalletPrompt);
        DataStore.i.HUDs.loadingHUD.showTips.Set(payload.showTips);
    }
}