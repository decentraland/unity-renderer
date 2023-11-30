using System.Collections.Generic;
using UnityEngine;

public class AvatarAnchorPoints : IAvatarAnchorPoints
{
    private static readonly Dictionary<AvatarAnchorPointIds, string> boneMapping = new ()
    {
        { AvatarAnchorPointIds.LeftHand, "Avatar_LeftHand" },
        { AvatarAnchorPointIds.RightHand, "Avatar_RightHand" },
    };

    private readonly Dictionary<AvatarAnchorPointIds, Transform> boneTransformMapping = new ();

    private Transform avatarTransform;
    private float nameTagY;

    void IAvatarAnchorPoints.Prepare(Transform avatarTransform, (string AnchorName, Transform Bone)[] anchors, float nameTagY)
    {
        this.avatarTransform = avatarTransform;
        this.nameTagY = nameTagY;

        if (anchors == null)
            return;

        foreach ((string AnchorName, Transform Bone) anchor in anchors)
        foreach (KeyValuePair<AvatarAnchorPointIds, string> boneMap in boneMapping)
            if (anchor.AnchorName == boneMap.Value)
                boneTransformMapping[boneMap.Key] = anchor.Bone;
    }

    (Vector3 position, Quaternion rotation, Vector3 scale) IAvatarAnchorPoints.GetTransform(AvatarAnchorPointIds anchorPointId)
    {
        if (anchorPointId == AvatarAnchorPointIds.Position && avatarTransform != null)
        {
            return (avatarTransform.position, avatarTransform.rotation, Vector3.one);
        }

        if (anchorPointId == AvatarAnchorPointIds.NameTag && avatarTransform != null)
        {
            return (avatarTransform.position + Vector3.up * nameTagY, avatarTransform.rotation, Vector3.one);
        }

        if (boneTransformMapping.TryGetValue(anchorPointId, out Transform bone))
        {
            if (bone != null)
            {
                return (bone.position, bone.rotation, bone.lossyScale);
            }
        }
        return (Vector3.zero, Quaternion.identity, Vector3.one);
    }
}
