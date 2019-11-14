using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("CameraTests")]
public class FirstPersonCameraSetup : CameraSetup<FirstPersonCameraConfig>
{
    public FirstPersonCameraSetup(Transform cameraTransform, BaseVariable<FirstPersonCameraConfig> configuration) : base(cameraTransform, configuration) { }

    protected override void SetUp()
    {
        cameraTransform.localPosition = Vector3.up * configuration.Get().yOffset;
    }

    protected override void OnConfigChanged(FirstPersonCameraConfig newConfig, FirstPersonCameraConfig oldConfig)
    {
        SetUp();
    }

    protected override void CleanUp() { }
}