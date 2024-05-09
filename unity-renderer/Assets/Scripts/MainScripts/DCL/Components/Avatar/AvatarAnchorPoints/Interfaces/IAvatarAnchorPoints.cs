using UnityEngine;

public interface IAvatarAnchorPoints
{
    void Prepare(Transform avatarTransform, (string AnchorName, Transform Bone)[] anchors, float nameTagY);
    (Vector3 position, Quaternion rotation, Vector3 scale) GetTransform(AvatarAnchorPointIds anchorPointId);
}
