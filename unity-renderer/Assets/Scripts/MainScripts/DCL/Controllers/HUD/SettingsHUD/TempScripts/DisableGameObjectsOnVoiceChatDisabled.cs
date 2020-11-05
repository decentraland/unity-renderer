using UnityEngine;
using DCL.Configuration;

internal class DisableGameObjectsOnVoiceChatDisabled : MonoBehaviour
{
    [SerializeField] GameObject[] gameObjects;

    void Awake()
    {
        if (EnvironmentSettings.RUNNING_TESTS)
            return;

        KernelConfig.i.EnsureConfigInitialized().Then(config => DoChanges(config));

        KernelConfig.i.OnChange += OnKernelConfigChanged;
    }

    void OnDestroy()
    {
        KernelConfig.i.OnChange -= OnKernelConfigChanged;
    }

    void OnKernelConfigChanged(KernelConfigModel current, KernelConfigModel previous)
    {
        if (current.comms.voiceChatEnabled == previous.comms.voiceChatEnabled)
        {
            return;
        }
        DoChanges(current);
    }

    void DoChanges(KernelConfigModel config)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            gameObjects[i].SetActive(config.comms.voiceChatEnabled);
        }
    }
}
