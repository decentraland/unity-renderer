using DCL.Interface;

internal class Reporter : IReporter
{
    void IReporter.ReportAvatarSceneChange(long entityId, string avatarId, string sceneId)
    {
        WebInterface.ReportAvatarSceneChanged(entityId, avatarId, sceneId);
    }
    void IReporter.ReportAvatarRemoved(long entityId, string avatarId)
    {
        WebInterface.ReportAvatarRemoved(entityId, avatarId);
    }
}