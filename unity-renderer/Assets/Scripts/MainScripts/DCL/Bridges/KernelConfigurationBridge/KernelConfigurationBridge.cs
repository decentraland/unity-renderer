using DCL;
using UnityEngine;

public class KernelConfigurationBridge : MonoBehaviour
{
    public Main main;
    public void SetKernelConfiguration(string json) { KernelConfig.i.Set(json); }

    public void SetFeatureFlagConfiguration(string json) { main.SetFeatureFlagConfiguration(json); }
}