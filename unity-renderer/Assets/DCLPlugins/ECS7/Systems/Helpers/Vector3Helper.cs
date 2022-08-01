using DCL.Configuration;
using DCL.Controllers;
using UnityEngine;

namespace ECSSystems.Helpers
{
    public static class Vector3Helper
    {
        public static void Set(ref Vector3 target, ref Vector3 source)
        {
            target.x = source.x;
            target.y = source.y;
            target.z = source.z;
        }

        public static void Set(ref Vector3 target, float x, float y, float z)
        {
            target.x = x;
            target.y = y;
            target.z = z;
        }

        public static void SetInSceneOffset(ref Vector3 target, IParcelScene scene, ref Vector3 position, ref Vector3 worldOffset)
        {
            target.x = (position.x + worldOffset.x) - scene.sceneData.basePosition.x * ParcelSettings.PARCEL_SIZE;
            target.y = (position.y + worldOffset.y);
            target.z = (position.z + worldOffset.z) - scene.sceneData.basePosition.y * ParcelSettings.PARCEL_SIZE;
        }
    }
}