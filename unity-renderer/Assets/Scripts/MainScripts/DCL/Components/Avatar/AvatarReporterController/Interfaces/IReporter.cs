internal interface IReporter
{
    void ReportAvatarSceneChange(string avatarId, string sceneId);
    void ReportAvatarRemoved(string avatarId);
}