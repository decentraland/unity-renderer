internal interface IReporter
{
    void ReportAvatarSceneChange(string avatarId, int sceneNumber);
    void ReportAvatarRemoved(string avatarId);
}