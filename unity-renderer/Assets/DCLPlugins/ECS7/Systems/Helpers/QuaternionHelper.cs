using UnityEngine;

namespace ECSSystems.Helpers
{
    public static class QuaternionHelper
    {
        public static void Set(ref Quaternion target, ref Quaternion source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
            target.w = source.w;
        }
    }
}