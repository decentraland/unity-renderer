using DCL;
using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using UnityEngine;
using UnityEngine.UI;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject webGLPrefab;
    [SerializeField] private GameObject desktopPrefab;
    [SerializeField] private InputField text;

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
        if( profService == null)
            profService = Environment.i?.serviceLocator?.Get<IProfilerRecordsService>();
        else
        {
            started = true;
            profService.StartRecordGCAllocatedInFrame();
            Avarage();
            text.text = gc.ToString();
        }
    }

    int frameCounter = 0;
    float updateInterval = 0.3f; // Time in seconds between updates
    float nextUpdate = 0.0f;
    float gc = 0.0f;

    void Avarage()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Time.unscaledTime > nextUpdate)
        {
            gc = profService.GcAllocatedInFrame/ 1024f;
            nextUpdate = Time.unscaledTime + updateInterval;
        }
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
