using System;
public interface IHUDController : IDisposable
{
    void Initialize(IHUDFactory factory);
    void ConfigureHUDElement(string payload);
    void ConfigureHUDElement(HUDElementID hudElementId, HUDConfiguration configuration, string extraPayload = null);
    IHUD GetHUDElement(HUDElementID id);
    void TriggerSelfUserExpression(string id);
    void AirdroppingRequest(string payload);
    void ShowTermsOfServices(string payload);
    void SetPlayerTalking(string talking);
    void SetVoiceChatEnabledByScene(int enabledPayload);
    void SetUserTalking(string payload);
    void SetUsersMuted(string payload);
    void RequestTeleport(string teleportDataJson);
    void UpdateBalanceOfMANA(string balance);
    void ShowAvatarEditorInSignUp();
    void Cleanup();
}