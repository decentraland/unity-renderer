using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents.Utils
{
    public static class ECSAvatarUtils
    {
        private const string AVATAR_TRIGGER_LAYER = "AvatarTriggerDetection";
        private const int MAX_AVATARS = 100;

        private static int mask = LayerMask.GetMask(AVATAR_TRIGGER_LAYER);
        private static Collider[] resultColliders = new Collider[MAX_AVATARS]; // max 100 avatars

        public static HashSet<GameObject> DetectAvatars(in Vector3 box, in Vector3 center, in Quaternion rotation, in HashSet<Collider> excludeColliders = null)
        {
            HashSet<GameObject> result = new HashSet<GameObject>();

            int collidersFoundAmount = Physics.OverlapBoxNonAlloc(center, box * 0.5f, resultColliders, rotation, mask, QueryTriggerInteraction.Collide);
            if (collidersFoundAmount == 0)
                return result;

            bool hasExcludeList = excludeColliders != null;

            for (var i = 0; i < collidersFoundAmount; i++)
            {
                var collider = resultColliders[i];
                if (hasExcludeList && excludeColliders.Contains(collider))
                    continue;

                result.Add(collider.transform.parent.gameObject);
            }
            return result;
        }
    }
}
