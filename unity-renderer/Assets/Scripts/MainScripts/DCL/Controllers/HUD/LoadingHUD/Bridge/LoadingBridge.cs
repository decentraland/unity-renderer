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

    private bool prewarmedShaderVariants = false;

    public void SetLoadingScreen(string jsonMessage)
    {
        Payload payload = JsonUtility.FromJson<Payload>(jsonMessage);
        DataStore.i.HUDs.loadingHUD.visible.Set(payload.isVisible);

        if (!string.IsNullOrEmpty(payload.message))
            DataStore.i.HUDs.loadingHUD.message.Set(payload.message);
        DataStore.i.HUDs.loadingHUD.showTips.Set(payload.showTips);

        if (!prewarmedShaderVariants)
        {
            Resources.Load<ShaderVariantCollection>("ShaderVariantCollections/shaderVariants-selected").WarmUp();
            prewarmedShaderVariants = true;
        }
    }
}