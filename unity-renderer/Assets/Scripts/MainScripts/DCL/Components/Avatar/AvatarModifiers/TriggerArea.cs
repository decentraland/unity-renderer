using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class TriggerArea
{
    public abstract HashSet<GameObject> DetectAvatars(in Vector3 center, in Quaternion rotation, in HashSet<Collider> excludeColliders = null);

}

[Serializable]
public class BoxTriggerArea : TriggerArea
{
    internal const string AVATAR_TRIGGER_LAYER = "AvatarTriggerDetection";
    public Vector3 box;

    public override HashSet<GameObject> DetectAvatars(in Vector3 center, in Quaternion rotation, in HashSet<Collider> excludeColliders = null)
    {
        Collider[] colliders = Physics.OverlapBox(center, box * 0.5f, rotation,
            LayerMask.GetMask(AVATAR_TRIGGER_LAYER), QueryTriggerInteraction.Collide);

        if (colliders.Length == 0)
        {
            return null;
        }

        bool hasExcludeList = excludeColliders != null;
        HashSet<GameObject> result = new HashSet<GameObject>();
        foreach (Collider collider in colliders)
        {
            if (hasExcludeList && excludeColliders.Contains(collider))
            {
                continue;
            }
            result.Add(collider.transform.parent.gameObject);
        }
        return result;
    }
}