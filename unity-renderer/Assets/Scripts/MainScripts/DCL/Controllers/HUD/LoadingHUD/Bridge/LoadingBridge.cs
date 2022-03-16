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
        public bool showTips = false;
    }

    public void SetLoadingScreen(string jsonMessage)
    {
        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);
        //if(payload.isVisible && )
        if (payload.isVisible && !DataStore.i.HUDs.loadingHUD.fadeIn.Get())
            DataStore.i.HUDs.loadingHUD.fadeIn.Set(true);

        if (!payload.isVisible && !DataStore.i.HUDs.loadingHUD.fadeOut.Get())
            DataStore.i.HUDs.loadingHUD.fadeOut.Set(true);

        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.HUDs.loadingHUD.message.Set(payload.message);
        DataStore.i.HUDs.loadingHUD.showTips.Set(payload.showTips);
    }
}