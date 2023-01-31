using Cysharp.Threading.Tasks;
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

    private UnityWebRequest uwr;
    

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
            OBJUrl = url;

        uwr?.Dispose();
        LoadAsset();
    }

    private async UniTaskVoid LoadAsset()
    {
        if (!string.IsNullOrEmpty(OBJUrl))
        {
            Destroy(loadedOBJGameObject);

            uwr = await Environment.i.platform.webRequest.Get(url: OBJUrl);
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                loadedOBJGameObject = OBJLoader.LoadOBJFile(uwr.downloadHandler.text, true);
                loadedOBJGameObject.name = "LoadedOBJ";
                loadedOBJGameObject.transform.SetParent(transform);
                loadedOBJGameObject.transform.localPosition = Vector3.zero;

                OnFinishedLoadingAsset?.Invoke();
                alreadyLoadedAsset = true;
            }
            else
            {
                Debug.Log("Couldn't get OBJ, error: " + uwr.error + " ... " + OBJUrl);
            }
        }
        else
        {
            Debug.Log("couldn't load OBJ because url is empty");
        }

        if (loadingPlaceholder != null)
            loadingPlaceholder.SetActive(false);
    }

    void OnDestroy()
    {
        uwr?.Dispose();

        Destroy(loadingPlaceholder);
        Destroy(loadedOBJGameObject);
    }
}
