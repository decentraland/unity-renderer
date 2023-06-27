using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using TMPro;
using Unity.Profiling;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject webGLPrefab;
    [SerializeField] private GameObject desktopPrefab;
    [SerializeField] private TMP_Text text;
    [SerializeField] private TMP_Text text1;
    [SerializeField] private TMP_Text text2;

    private ProfilerRecorder gcAllocatedInFrameRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

#if UNITY_EDITOR
    private enum Platform
    {
        WebGL,
        Desktop,
    }

    [SerializeField] private Platform currentPlatform;

    private void Awake()
    {
        switch (currentPlatform)
        {
            case Platform.WebGL:
                Instantiate(webGLPrefab);
                break;
            case Platform.Desktop:
                Instantiate(desktopPrefab);
                break;
        }
    }

    float deltaTime = 0.0f;

    bool serviceFound;
    private IProfilerRecordsService profService;

    private bool started;
    void Update()
    {
        text.text = $"{gcAllocatedInFrameRecorder.LastValue}";
        text1.text = $"{gcAllocatedInFrameRecorder.CurrentValue}";
        text2.text = $"{gcAllocatedInFrameRecorder.IsRunning}";
    }

#else
    private void Awake()
    {
#if UNITY_WEBGL
        Instantiate(webGLPrefab);
#else
        Instantiate(desktopPrefab);
#endif
    }
#endif
}
