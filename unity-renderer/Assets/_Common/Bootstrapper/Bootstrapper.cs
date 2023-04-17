using UnityEngine;
using UnityEngine.Rendering.Universal;

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
    [SerializeField]private UniversalRendererData rendererData = null;

    private void Awake()
    {
        switch (currentPlatform)
        {
            case Platform.WebGL:
                rendererData.depthPrimingMode = DepthPrimingMode.Auto;
                Instantiate(webGLPrefab);
                break;
            case Platform.Desktop:
                rendererData.depthPrimingMode = DepthPrimingMode.Auto;
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
