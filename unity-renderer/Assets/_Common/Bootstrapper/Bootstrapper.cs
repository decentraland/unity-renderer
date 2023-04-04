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
