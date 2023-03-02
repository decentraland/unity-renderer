using DCL.Interface;

internal class Reporter : IReporter
{
    void IReporter.ReportAvatarSceneChange(string avatarId, int sceneNumber)
    {
        WebInterface.ReportAvatarSceneChanged(avatarId, sceneNumber);
    }
    void IReporter.ReportAvatarRemoved(string avatarId)
    {
        WebInterface.ReportAvatarRemoved(avatarId);
    }
}