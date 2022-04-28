using DCL.Interface;

internal class Reporter : IReporter
{
    void IReporter.ReportAvatarSceneChange(string avatarId, string sceneId)
    {
        WebInterface.ReportAvatarSceneChanged(avatarId, sceneId);
    }
    void IReporter.ReportAvatarRemoved(string avatarId)
    {
        WebInterface.ReportAvatarRemoved(avatarId);
    }
}