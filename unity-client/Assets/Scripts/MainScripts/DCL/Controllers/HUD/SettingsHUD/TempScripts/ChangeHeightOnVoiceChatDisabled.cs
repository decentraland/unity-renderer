using UnityEngine;
using DCL.Configuration;

internal class ChangeHeightOnVoiceChatDisabled : MonoBehaviour
{
    [SerializeField] float targetHeight;
    [SerializeField] RectTransform target;

    float defaultHeight;

    void Awake()
    {
        if (EnvironmentSettings.RUNNING_TESTS)
            return;

        defaultHeight = target.sizeDelta.y;

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
        if (!target)
            return;

        target.sizeDelta = new Vector3(target.sizeDelta.x, config.comms.voiceChatEnabled ? defaultHeight : targetHeight);
    }
}
