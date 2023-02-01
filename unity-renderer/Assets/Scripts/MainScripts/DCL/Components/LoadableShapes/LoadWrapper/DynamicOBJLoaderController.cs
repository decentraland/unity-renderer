using DCL;
using UnityEngine;
using UnityEngine.Networking;

public class DynamicOBJLoaderController : MonoBehaviour
{
    public bool loadOnStart = true;
    public string OBJUrl = "";
    public GameObject loadingPlaceholder;

    public event System.Action OnFinishedLoadingAsset;

    [HideInInspector] public bool alreadyLoadedAsset = false;
    [HideInInspector] public GameObject loadedOBJGameObject;

    private UnityWebRequest uwr = null;

    void Awake()
    {
        if (loadOnStart && !string.IsNullOrEmpty(OBJUrl))
        {
            LoadAsset();
        }
    }

    public void LoadAsset(string url = "", bool loadEvenIfAlreadyLoaded = false)
    {
        if (alreadyLoadedAsset && !loadEvenIfAlreadyLoaded)
            return;

        if (!string.IsNullOrEmpty(url))
        {
            OBJUrl = url;
        }

        uwr?.Dispose();

        LoadAsset();
    }

    async void LoadAsset()
    {
        if (!string.IsNullOrEmpty(OBJUrl))
        {
            Destroy(loadedOBJGameObject);

            uwr = await Environment.i.platform.webRequest.GetAsync(
                url: OBJUrl,
                onSuccess: (request) =>
                {
                    loadedOBJGameObject = OBJLoader.LoadOBJFile(request.downloadHandler.text, true);
                    loadedOBJGameObject.name = "LoadedOBJ";
                    loadedOBJGameObject.transform.SetParent(transform);
                    loadedOBJGameObject.transform.localPosition = Vector3.zero;

                    OnFinishedLoadingAsset?.Invoke();
                    alreadyLoadedAsset = true;
                },
                onFail: (request) =>
                {
                    Debug.Log("Couldn't get OBJ, error: " + request.error + " ... " + OBJUrl);
                });
        }
        else
        {
            Debug.Log("couldn't load OBJ because url is empty");
        }

        if (loadingPlaceholder != null)
        {
            loadingPlaceholder.SetActive(false);
        }
    }

    void OnDestroy()
    {
        uwr?.Dispose();

        Destroy(loadingPlaceholder);
        Destroy(loadedOBJGameObject);
    }
}
