using UnityEngine;
using DCL.Configuration;

internal class MoveVerticalOnVoiceChatDisabled : MonoBehaviour
{
    [SerializeField] float targetPositionY;
    [SerializeField] RectTransform target;

    float defaultPositionY;

    void Awake()
    {
        if (EnvironmentSettings.RUNNING_TESTS)
            return;

        defaultPositionY = target.anchoredPosition.y;

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

        target.anchoredPosition = new Vector3(target.anchoredPosition.x, config.comms.voiceChatEnabled ? defaultPositionY : targetPositionY);
    }
}
