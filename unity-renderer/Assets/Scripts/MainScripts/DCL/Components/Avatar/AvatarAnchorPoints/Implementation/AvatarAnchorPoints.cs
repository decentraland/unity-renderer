using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AvatarAnchorPoints : IAvatarAnchorPoints
{
    private static readonly Dictionary<AvatarAnchorPointIds, string> boneMapping = new Dictionary<AvatarAnchorPointIds, string>()
    {
        { AvatarAnchorPointIds.LeftHand, "Avatar_LeftHand" },
        { AvatarAnchorPointIds.RightHand, "Avatar_RightHand" },
    };

    private readonly Dictionary<AvatarAnchorPointIds, Transform> boneTransformMapping = new Dictionary<AvatarAnchorPointIds, Transform>();

    private Transform avatarTransform;
    private float nameTagY;

    void IAvatarAnchorPoints.Prepare(Transform avatarTransform, Transform[] bones, float nameTagY)
    {
        this.avatarTransform = avatarTransform;
        this.nameTagY = nameTagY;

        boneTransformMapping.Clear();
        
        if(bones == null)
            return;

        foreach (var bone in bones)
        {
            if (TryGetIdFromBoneName(bone.name, out AvatarAnchorPointIds anchorPointId))
            {
                boneTransformMapping.Add(anchorPointId, bone);
            }
        }
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

    private static bool TryGetIdFromBoneName(string boneName, out AvatarAnchorPointIds id)
    {
        var result = boneMapping.FirstOrDefault(pair => pair.Value == boneName);
        id = result.Key;
        return !string.IsNullOrEmpty(result.Value);
    }
}