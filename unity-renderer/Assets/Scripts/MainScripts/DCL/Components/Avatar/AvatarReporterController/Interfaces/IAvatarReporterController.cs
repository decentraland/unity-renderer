using UnityEngine;

public interface IAvatarReporterController
{
    internal IReporter reporter { get; set; }

    void SetUp(string sceneId, long entityId, string avatarId);
    void ReportAvatarPosition(Vector3 position);
    void ReportAvatarRemoved();
}