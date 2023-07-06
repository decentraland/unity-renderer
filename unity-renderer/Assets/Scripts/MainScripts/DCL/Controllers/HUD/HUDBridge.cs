using Cysharp.Threading.Tasks;
using DCL;
using JetBrains.Annotations;
using System;
using System.Threading;
using UnityEngine;

public class HUDBridge : MonoBehaviour
{
    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        cancellationTokenSource = new CancellationTokenSource();
    }

    private void OnDestroy()
    {
        cancellationTokenSource.Cancel();
        cancellationTokenSource.Dispose();
    }

#region MessagesFromKernel

    [System.Serializable]
    class ConfigureHUDElementMessage
    {
        public HUDElementID hudElementId;
        public HUDConfiguration configuration;
        public string extraPayload;
    }

    [UsedImplicitly]
    public void ConfigureHUDElement(string payload)
    {
        ConfigureHUDElementMessage message = JsonUtility.FromJson<ConfigureHUDElementMessage>(payload);

        HUDElementID id = message.hudElementId;
        HUDConfiguration configuration = message.configuration;
        string extraPayload = message.extraPayload;

        HUDController.i.ConfigureHUDElement(id, configuration, cancellationTokenSource.Token, extraPayload).Forget();
    }

    public void TriggerSelfUserExpression(string id) { UserProfile.GetOwnUserProfile().SetAvatarExpression(id, UserProfile.EmoteSource.Command); }

    public void ShowTermsOfServices(string payload)
    {
        var model = JsonUtility.FromJson<TermsOfServiceHUDController.Model>(payload);
        HUDController.i.termsOfServiceHud?.ShowTermsOfService(model);
    }

    public void SetPlayerTalking(string talking) { HUDController.i.voiceChatHud?.SetVoiceChatRecording("true".Equals(talking)); }

    public void SetVoiceChatEnabledByScene(int enabledPayload)
    {
        bool isEnabled = enabledPayload != 0;
        HUDController.i.voiceChatHud?.SetVoiceChatEnabledByScene(isEnabled);
    }

    public void SetUserTalking(string payload)
    {
        var model = JsonUtility.FromJson<UserTalkingModel>(payload);
        HUDController.i.voiceChatHud?.SetUserRecording(model.userId, model.talking);
    }

    public void SetUsersMuted(string payload)
    {
        var model = JsonUtility.FromJson<UserMutedModel>(payload);
        HUDController.i.voiceChatHud?.SetUsersMuted(model.usersId, model.muted);
    }

    public void RequestTeleport(string teleportDataJson) { DataStore.i.world.requestTeleportData.Set(teleportDataJson, true); }

    public void UpdateBalanceOfMANA(string balance)
    {
        HUDController.i.profileHud?.SetManaBalance(balance);
        DataStore.i.wallet.currentEthereumManaBalance.Set(Convert.ToDouble(balance));
    }

    [Obsolete("Old kernel might be using this call")]
    public void ShowAvatarEditorInSignUp()
    {
        SetSignupFlow();
    }

    public void SetSignupFlow()
    {
        if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("seamless_login_variant:enabled"))
            DataStore.i.HUDs.tosPopupVisible.Set(true, true);
        else
        {
            DataStore.i.common.isSignUpFlow.Set(true);
            DataStore.i.HUDs.avatarEditorVisible.Set(true, true);
        }
    }

    #endregion
}
