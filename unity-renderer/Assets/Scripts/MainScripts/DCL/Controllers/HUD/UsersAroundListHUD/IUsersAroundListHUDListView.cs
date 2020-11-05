using System;

public interface IUsersAroundListHUDListView
{
    event Action<string, bool> OnRequestMuteUser;
    event Action<bool> OnRequestMuteGlobal;
    event Action OnGoToCrowdPressed;
    void AddOrUpdateUser(MinimapMetadata.MinimapUserInfo userInfo);
    void RemoveUser(string userId);
    void SetUserRecording(string userId, bool isRecording);
    void SetUserMuted(string userId, bool isMuted);
    void SetVisibility(bool visible);
    void Dispose();
}
