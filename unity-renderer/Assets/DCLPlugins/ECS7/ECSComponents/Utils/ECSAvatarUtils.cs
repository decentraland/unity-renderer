using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents.Utils
{
    public static class ECSAvatarUtils
    {
        internal const string AVATAR_TRIGGER_LAYER = "AvatarTriggerDetection";
    
        public static HashSet<GameObject> DetectAvatars(in UnityEngine.Vector3 box, in UnityEngine.Vector3 center, in Quaternion rotation, in HashSet<Collider> excludeColliders = null)
        {
            Collider[] colliders = Physics.OverlapBox(center, box * 0.5f, rotation,
                LayerMask.GetMask(AVATAR_TRIGGER_LAYER), QueryTriggerInteraction.Collide);

            if (colliders.Length == 0)
                return null;

            bool hasExcludeList = excludeColliders != null;
            HashSet<GameObject> result = new HashSet<GameObject>();
            foreach (Collider collider in colliders)
            {
                if (hasExcludeList && excludeColliders.Contains(collider))
                    continue;
            
                result.Add(collider.transform.parent.gameObject);
            }
            return result;
        }
    }
}