using Altom.AltDriver;

namespace Altom.AltTester
{
    public static class AltVectorExtensions
    {
        public static UnityEngine.Vector2 ToUnity(this AltVector2 vector2)
        {
            return new UnityEngine.Vector2(vector2.x, vector2.y);
        }

        public static UnityEngine.Vector3 ToUnity(this AltVector3 vector3)
        {
            return new UnityEngine.Vector3(vector3.x, vector3.y, vector3.z);
        }
    }

}