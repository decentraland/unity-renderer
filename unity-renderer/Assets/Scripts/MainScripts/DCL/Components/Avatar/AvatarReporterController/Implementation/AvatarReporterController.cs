using UnityEngine;

public class AvatarReporterController : IAvatarReporterController
{
    public const string AVATAR_GLOBAL_SCENE = "dcl-gs-avatars";

    private string entityId;
    private string avatarId;
    private Vector3 reportedPosition;
    private bool isPositionReported = false;

    IReporter IAvatarReporterController.reporter { get; set; } = new Reporter();

    void IAvatarReporterController.SetUp(string sceneId, string entityId, string avatarId)
    {
        // NOTE: do not report avatars that doesn't belong to the global scene
        if (sceneId != AVATAR_GLOBAL_SCENE)
            return;

        this.entityId = entityId;
        this.avatarId = avatarId;
        isPositionReported = false;
    }

    void IAvatarReporterController.ReportAvatarPosition(Vector3 position)
    {
        if (!CanReport())
            return;

        if (!HasMoved(position))
            return;

        ((IAvatarReporterController)this).reporter.ReportAvatarPosition(entityId, avatarId, position);
        reportedPosition = position;
        isPositionReported = true;
    }

    void IAvatarReporterController.ReportAvatarRemoved()
    {
        if (!CanReport())
            return;

        ((IAvatarReporterController)this).reporter.ReportAvatarRemoved(entityId, avatarId);

        entityId = null;
        avatarId = null;
        isPositionReported = false;
    }

    private bool CanReport()
    {
        return !string.IsNullOrEmpty(entityId) && !string.IsNullOrEmpty(avatarId);
    }

    private bool HasMoved(Vector3 currentPosition)
    {
        if (!isPositionReported)
            return true;

        return Vector3.SqrMagnitude(currentPosition - reportedPosition) > 0.0001f;
    }
}