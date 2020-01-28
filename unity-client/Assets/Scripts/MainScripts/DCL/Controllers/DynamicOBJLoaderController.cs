using System.Collections;
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

    Coroutine loadingRoutine = null;

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

        if (loadingRoutine != null)
        {
            StopCoroutine(loadingRoutine);
        }

        loadingRoutine = StartCoroutine(LoadAssetCoroutine());
    }

    IEnumerator LoadAssetCoroutine()
    {
        if (!string.IsNullOrEmpty(OBJUrl))
        {
            Destroy(loadedOBJGameObject);

            UnityWebRequest webRequest = UnityWebRequest.Get(OBJUrl);

            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log("Couldn't get OBJ, error: " + webRequest.error + " ... " + OBJUrl);
            }
            else
            {
                loadedOBJGameObject = OBJLoader.LoadOBJFile(webRequest.downloadHandler.text, true);
                loadedOBJGameObject.name = "LoadedOBJ";
                loadedOBJGameObject.transform.SetParent(transform);
                loadedOBJGameObject.transform.localPosition = Vector3.zero;

                OnFinishedLoadingAsset?.Invoke();
                alreadyLoadedAsset = true;
            }
        }
        else
        {
            Debug.Log("couldn't load OBJ because url is empty");
        }

        if (loadingPlaceholder != null)
        {
            loadingPlaceholder.SetActive(false);
        }

        loadingRoutine = null;
    }

    void OnDestroy()
    {
        Destroy(loadingPlaceholder);
        Destroy(loadedOBJGameObject);
    }
}
