using DCL;
using UnityEngine;

public class DynamicOBJLoaderController : MonoBehaviour
{
    public bool loadOnStart = true;
    public string OBJUrl = "";
    public GameObject loadingPlaceholder;

    public event System.Action OnFinishedLoadingAsset;

    [HideInInspector] public bool alreadyLoadedAsset = false;
    [HideInInspector] public GameObject loadedOBJGameObject;

    WebRequestAsyncOperation asyncOp = null;

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

        if (asyncOp != null)
        {
            asyncOp.Dispose();
        }

        LoadAsset();
    }

    void LoadAsset()
    {
        if (!string.IsNullOrEmpty(OBJUrl))
        {
            Destroy(loadedOBJGameObject);

            asyncOp = Environment.i.platform.webRequest.Get(
                url: OBJUrl,
                OnSuccess: (webRequestResult) =>
                {
                    loadedOBJGameObject = OBJLoader.LoadOBJFile(webRequestResult.downloadHandler.text, true);
                    loadedOBJGameObject.name = "LoadedOBJ";
                    loadedOBJGameObject.transform.SetParent(transform);
                    loadedOBJGameObject.transform.localPosition = Vector3.zero;

                    OnFinishedLoadingAsset?.Invoke();
                    alreadyLoadedAsset = true;
                },
                OnFail: (webRequestResult) =>
                {
                    Debug.Log("Couldn't get OBJ, error: " + webRequestResult.error + " ... " + OBJUrl);
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
        if (asyncOp != null)
        {
            asyncOp.Dispose();
        }

        Destroy(loadingPlaceholder);
        Destroy(loadedOBJGameObject);
    }
}