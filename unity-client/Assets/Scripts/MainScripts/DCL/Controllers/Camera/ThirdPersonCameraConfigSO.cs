using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ThirdPersonCameraConfig", menuName = "CameraConfig/ThirdPerson")]
public class ThirdPersonCameraConfigSO : BaseVariable<ThirdPersonCameraConfig> { }

[Serializable]
public class ThirdPersonCameraConfig
{
    public Vector3 offset;
}