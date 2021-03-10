using UnityEngine;

public class KernelConfigurationBridge : MonoBehaviour
{
    public void SetKernelConfiguration(string json)
    {
        KernelConfig.i.Set(json);
    }
}
