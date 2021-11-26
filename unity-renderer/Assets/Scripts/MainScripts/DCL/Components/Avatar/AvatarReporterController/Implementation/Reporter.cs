using DCL.Interface;
using UnityEngine;

internal class Reporter : IReporter
{
    void IReporter.ReportAvatarPosition(string entityId, string avatarId, Vector3 position)
    {
        WebInterface.ReportAvatarPosition(entityId, avatarId, position);
    }
    void IReporter.ReportAvatarRemoved(string entityId, string avatarId)
    {
        WebInterface.ReportAvatarRemoved(entityId, avatarId);
    }
}