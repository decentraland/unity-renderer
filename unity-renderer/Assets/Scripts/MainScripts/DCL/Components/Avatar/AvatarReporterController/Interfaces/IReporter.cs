using UnityEngine;

internal interface IReporter
{
    void ReportAvatarPosition(string entityId, string avatarId, Vector3 position);
    void ReportAvatarRemoved(string entityId, string avatarId);
}