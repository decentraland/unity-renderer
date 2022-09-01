using System.Collections.Generic;
using UnityEngine;

namespace ECSSystems.PointerInputSystem
{
    internal static class PointerInputHelper
    {
        public static bool IsInputForEntity(Collider pointerEventCollider, IList<Collider> colliders)
        {
            return colliders.Contains(pointerEventCollider);
        }

        public static bool IsValidFistanceForEntity(float pointerEventDistance, float maxDistance)
        {
            return pointerEventDistance <= maxDistance;
        }

        public static bool IsValidInputForEntity(int pointerEventButton, float pointerEventDistance, float maxDistance, ActionButton button)
        {
            if (!IsValidFistanceForEntity(pointerEventDistance, maxDistance))
                return false;

            if (button == ActionButton.Any || (int)button == pointerEventButton)
            {
                return true;
            }
            return false;
        }

        public static string GetName(this ActionButton button)
        {
            const string ACTION_BUTTON_POINTER = "POINTER";
            const string ACTION_BUTTON_PRIMARY = "PRIMARY";
            const string ACTION_BUTTON_SECONDARY = "SECONDARY";

            switch (button)
            {
                case ActionButton.Primary:
                    return ACTION_BUTTON_PRIMARY;
                case ActionButton.Secondary:
                    return ACTION_BUTTON_SECONDARY;
                default:
                    return ACTION_BUTTON_POINTER;
            }
        }
    }
}