using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Helpers;
using UnityEngine;

namespace DCL.ECSComponents
{
    public static class ProtoConvertUtils 
    {
        public static PBOnPointerUpResult GetPointerUpResultModel(ActionButton buttonId, string meshName, Ray ray, HitInfo hit)
        {
            PBOnPointerUpResult result = new PBOnPointerUpResult();
            result.Button = buttonId;
            result.Direction = UnityVectorToPBVector(ray.direction);
            result.Distance = hit.distance;
            result.Normal = UnityVectorToPBVector(hit.normal);
            result.Origin = UnityVectorToPBVector(ray.origin);
            result.Point = UnityVectorToPBVector(hit.point);
            
            // This null check will disappear when we introduce optionals to the proto
            if(meshName == null)
                meshName = String.Empty;
            result.MeshName = meshName;
            return result;
        }
        
        public static PBOnPointerDownResult GetPointerDownResultModel(ActionButton buttonId, string meshName, Ray ray, HitInfo hit)
        {
            PBOnPointerDownResult result = new PBOnPointerDownResult();
            result.Button = buttonId;
            result.Direction = UnityVectorToPBVector(ray.direction);
            result.Distance = hit.distance;
            result.Normal = UnityVectorToPBVector(hit.normal);
            result.Origin = UnityVectorToPBVector(ray.origin);
            result.Point = UnityVectorToPBVector(hit.point);
            
            // This null check will disappear when we introduce optionals to the proto
            if(meshName == null)
                meshName = String.Empty;
            result.MeshName = meshName;
            return result;
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
        
        public static CameraTool.CameraMode.ModeId PBCameraEnumToUnityEnum(CameraModeValue mode)
        {
            switch (mode)
            {
                case CameraModeValue.FirstPerson:
                    return CameraTool.CameraMode.ModeId.FirstPerson;
                case CameraModeValue.ThirdPerson:
                    return CameraTool.CameraMode.ModeId.ThirdPerson;
                default:
                    return CommonScriptableObjects.cameraMode.Get();
            }
        }

        public static CameraModeValue UnityEnumToPBCameraEnum(CameraTool.CameraMode.ModeId mode)
        {
            switch (mode)
            {
                case CameraTool.CameraMode.ModeId.FirstPerson:
                    return CameraModeValue.FirstPerson;
                case CameraTool.CameraMode.ModeId.ThirdPerson:
                default:
                    return CameraModeValue.ThirdPerson;
            }
        }
    }
}
