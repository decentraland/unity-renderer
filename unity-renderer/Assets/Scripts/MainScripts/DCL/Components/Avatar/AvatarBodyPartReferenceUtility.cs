using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL
{
    public static class AvatarBodyPartReferenceUtility
    {
        public static Transform GetLeftHand(Transform[] bodyParts)
        {
            return GetBodyPart(bodyParts, "Avatar_LeftHand");
        }

        public static Transform GetRightHand(Transform[] bodyParts)
        {
            return GetBodyPart(bodyParts, "Avatar_RightHand");
        }

        public static Transform GetLeftToe(Transform[] bodyParts)
        {
            return GetBodyPart(bodyParts, "Avatar_LeftToeBase");
        }

        public static Transform GetRightToe(Transform[] bodyParts)
        {
            return GetBodyPart(bodyParts, "Avatar_RightToeBase");
        }

        private static Transform GetBodyPart(Transform[] bodyParts, string name)
        {
            for (int i = 0; i < bodyParts.Length; i++) {
                if (bodyParts[i].name == name)
                    return bodyParts[i];
            }
            return null;
        }
    }
}