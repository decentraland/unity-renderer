using DCL.Interface;

internal class Reporter : IReporter
{
    void IReporter.ReportAvatarSceneChange(string entityId, string avatarId, string sceneId)
    {
        WebInterface.ReportAvatarSceneChanged(entityId, avatarId, sceneId);
    }
    void IReporter.ReportAvatarRemoved(string entityId, string avatarId)
    {
        WebInterface.ReportAvatarRemoved(entityId, avatarId);
    }
}