internal interface IReporter
{
    void ReportAvatarSceneChange(string entityId, string avatarId, string sceneId);
    void ReportAvatarRemoved(string entityId, string avatarId);
}