using System.Collections.Generic;
using UnityEngine;

public class AvatarAnchorPoints : IAvatarAnchorPoints
{
    private AvatarAnchorPointsNameMapping boneNamesMapping = new AvatarAnchorPointsNameMapping();

    private Dictionary<AvatarAnchorPointIds, Transform> boneTransformMapping = new Dictionary<AvatarAnchorPointIds, Transform>();

    private Transform avatarTransform;
    private float nameTagY;

    void IAvatarAnchorPoints.Prepare(Transform avatarTransform, Transform[] bones, float nameTagY)
    {
        this.avatarTransform = avatarTransform;
        this.nameTagY = nameTagY;

        boneTransformMapping.Clear();

        foreach (var bone in bones)
        {
            if (boneNamesMapping.TryGet(bone.name, out AvatarAnchorPointIds anchorPointId))
            {
                boneTransformMapping.Add(anchorPointId, bone);
            }
        }
    }
    (Vector3 position, Quaternion rotation, Vector3 scale) IAvatarAnchorPoints.GetTransform(AvatarAnchorPointIds anchorPointId)
    {
        if (anchorPointId == AvatarAnchorPointIds.NameTag)
        {
            return (avatarTransform.position + Vector3.up * nameTagY, avatarTransform.rotation, Vector3.one);
        }

        if (!boneTransformMapping.TryGetValue(anchorPointId, out Transform bone))
        {
            return (Vector3.zero, Quaternion.identity, Vector3.one);
        }
        return (bone.position, bone.rotation, bone.lossyScale);
    }
}