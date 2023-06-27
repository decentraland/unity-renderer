using DCL;
using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private GameObject webGLPrefab;
    [SerializeField] private GameObject desktopPrefab;

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
        else if(!started)
        {
            started = true;
            profService.StartRecordGCAllocatedInFrame();
        }
        else
        {
            Avarage();
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


    void OnGUI()
    {
        if(profService == null) return;

        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 4 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);

        var text = $"{gc:0.0}";
        GUI.Label(rect, text, style);
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
