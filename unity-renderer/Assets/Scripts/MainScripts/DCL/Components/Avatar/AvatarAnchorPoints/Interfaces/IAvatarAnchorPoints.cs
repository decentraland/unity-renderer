using UnityEngine;

public interface IAvatarAnchorPoints
{
    void Prepare(Transform avatarTransform, Transform[] bones, float nameTagY);
    (Vector3 position, Quaternion rotation, Vector3 scale) GetTransform(AvatarAnchorPointIds anchorPointId);
}