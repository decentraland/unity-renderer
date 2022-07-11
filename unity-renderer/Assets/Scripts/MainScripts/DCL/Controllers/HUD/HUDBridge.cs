using System.Collections;
using System.Collections.Generic;
using DCL;
using UnityEngine;

public class HUDBridge : MonoBehaviour
{
    #region MessagesFromKernel

    [System.Serializable]
    class ConfigureHUDElementMessage
    {
        public HUDElementID hudElementId;
        public HUDConfiguration configuration;
        public string extraPayload;
    }

    public void ConfigureHUDElement(string payload)
    {
        ConfigureHUDElementMessage message = JsonUtility.FromJson<ConfigureHUDElementMessage>(payload);

        HUDElementID id = message.hudElementId;
        HUDConfiguration configuration = message.configuration;
        string extraPayload = message.extraPayload;

        HUDController.i.ConfigureHUDElement(id, configuration, extraPayload);
    }

    public void TriggerSelfUserExpression(string id) { UserProfile.GetOwnUserProfile().SetAvatarExpression(id, UserProfile.EmoteSource.Command); }

    public void AirdroppingRequest(string payload)
    {
        var model = JsonUtility.FromJson<AirdroppingHUDController.Model>(payload);
        HUDController.i.airdroppingHud.AirdroppingRequested(model);
    }

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

    public void RequestTeleport(string teleportDataJson) { HUDController.i.teleportHud?.RequestTeleport(teleportDataJson); }

    public void UpdateBalanceOfMANA(string balance) { HUDController.i.profileHud?.SetManaBalance(balance); }

    public void ShowAvatarEditorInSignUp()
    {
        if (HUDController.i.avatarEditorHud != null)
        {
            DataStore.i.common.isSignUpFlow.Set(true);
            HUDController.i.avatarEditorHud?.SetVisibility(true);
        }
    }

    #endregion
}