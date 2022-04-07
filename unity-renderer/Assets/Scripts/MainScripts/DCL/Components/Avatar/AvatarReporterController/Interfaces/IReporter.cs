internal interface IReporter
{
    void ReportAvatarSceneChange(long entityId, string avatarId, string sceneId);
    void ReportAvatarRemoved(long entityId, string avatarId);
}