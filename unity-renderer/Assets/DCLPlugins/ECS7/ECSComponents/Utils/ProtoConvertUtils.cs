using System;

namespace DCL.ECSComponents
{
    public static class ProtoConvertUtils 
    {
        public static CameraMode.ModeId PBCameraEnumToUnityEnum(PBCameraModeArea.Types.CameraMode mode)
        {
            switch (mode)
            {
                case PBCameraModeArea.Types.CameraMode.FirstPerson:
                    return CameraMode.ModeId.FirstPerson;
                case PBCameraModeArea.Types.CameraMode.ThirdPerson:
                    return CameraMode.ModeId.ThirdPerson;
                default:
                    return CommonScriptableObjects.cameraMode.Get();
            }
        }
        
        public static UnityEngine.Vector3 PBVectorToUnityVector(Vector3 original)
        {
            UnityEngine.Vector3 vector = new UnityEngine.Vector3();
            vector.x = original.X;
            vector.y = original.Y;
            vector.z = original.Z;
            return vector;
        }
    }
}