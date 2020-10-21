using UnityEngine;
using UnityEngine.Audio;

public class HUDAudioHandler : MonoBehaviour
{
    public static HUDAudioHandler i { get; private set; }

    [SerializeField]
    AudioMixer audioMixer;
    [SerializeField]
    AudioMixerSnapshot snapshotInWorld, snapshotOutOfWorld;

    [HideInInspector]
    public ulong chatLastCheckedTimestamp;

    private void Awake()
    {
        if (i != null)
        {
            Destroy(this);
            return;
        }

        i = this;

        RefreshChatLastCheckedTimestamp();
        CommonScriptableObjects.rendererState.OnChange += OnRendererStateChange;
        CommonScriptableObjects.allUIHidden.OnChange += OnAllUIHiddenChange;
    }

    private void OnDestroy()
    {
        CommonScriptableObjects.rendererState.OnChange -= OnRendererStateChange;
        CommonScriptableObjects.allUIHidden.OnChange -= OnAllUIHiddenChange;
    }

    public void RefreshChatLastCheckedTimestamp()
    {
        // Get UTC datetime (used to determine whether chat messages are old or new)
        chatLastCheckedTimestamp = (ulong)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalMilliseconds;
    }

    void OnRendererStateChange(bool isReady, bool isReadyPrevious)
    {
        if (isReady)
            snapshotInWorld.TransitionTo(0.1f);
        else
            snapshotOutOfWorld.TransitionTo(0.2f);
    }

    void OnAllUIHiddenChange(bool current, bool previous)
    {
        if (current)
            AudioScriptableObjects.UIHide.Play(true);
        else
            AudioScriptableObjects.UIShow.Play(true);
    }
}