using UnityEngine;

public class ThirdPersonCameraSetup : CameraSetup<ThirdPersonCameraConfig>
{
    public ThirdPersonCameraSetup(Transform cameraTransform, BaseVariable<ThirdPersonCameraConfig> configuration) : base(cameraTransform, configuration) { }

    protected override void SetUp()
    {
        cameraTransform.localPosition = configuration.Get().offset;
    }

    protected override void OnConfigChanged(ThirdPersonCameraConfig newConfig, ThirdPersonCameraConfig oldConfig)
    {
        SetUp();
    }

    protected override void CleanUp() { }
}