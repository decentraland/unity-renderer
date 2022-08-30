using System;
using DCL.CameraTool;
using DCL.Helpers;
using UnityEngine;

namespace DCL.ECSComponents
{
    public static class ProtoConvertUtils 
    {
        public static RaycastHit ToPBRaycasHit(long entityId, string meshName, Ray ray, HitInfo rawHit)
        {
            var hit = new RaycastHit();
            hit.Length = rawHit.distance;
            hit.Origin = UnityVectorToPBVector(ray.origin);
            hit.EntityId = (int)entityId;
            hit.MeshName = meshName;
            hit.Position = UnityVectorToPBVector(rawHit.point);
            hit.NormalHit = UnityVectorToPBVector(rawHit.normal);
            hit.Direction = UnityVectorToPBVector(ray.direction);

            return hit;
        }

        public static Vector3 UnityVectorToPBVector(UnityEngine.Vector3 original)
        {
            Vector3 vector = new Vector3();
            vector.X = original.x;
            vector.Y = original.y;
            vector.Z = original.z;
            return vector;
        }
        
        public static UnityEngine.Vector3 PBVectorToUnityVector(Vector3 original)
        {
            UnityEngine.Vector3 vector = new UnityEngine.Vector3();
            vector.x = original.X;
            vector.y = original.Y;
            vector.z = original.Z;
            return vector;
        }
        
        public static CameraMode.ModeId PBCameraEnumToUnityEnum(CameraModeValue mode)
        {
            switch (mode)
            {
                case CameraModeValue.FirstPerson:
                    return CameraMode.ModeId.FirstPerson;
                case CameraModeValue.ThirdPerson:
                    return CameraMode.ModeId.ThirdPerson;
                default:
                    return CommonScriptableObjects.cameraMode.Get();
            }
        }

        public static CameraModeValue UnityEnumToPBCameraEnum(CameraMode.ModeId mode)
        {
            switch (mode)
            {
                case CameraMode.ModeId.FirstPerson:
                    return CameraModeValue.FirstPerson;
                case CameraMode.ModeId.ThirdPerson:
                default:
                    return CameraModeValue.ThirdPerson;
            }
        }

        public static Color ToUnityColor(this Color3 color)
        {
            return new Color(color.R, color.G, color.B);
        }
    }
}
