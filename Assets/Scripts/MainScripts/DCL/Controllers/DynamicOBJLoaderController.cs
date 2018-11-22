using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DynamicOBJLoaderController : MonoBehaviour {
  public bool loadOnStart = true;
  public string OBJUrl = "";
  public GameObject loadingPlaceholder;

  [HideInInspector] public bool alreadyLoadedAsset = false;
  [HideInInspector] public Action<float> finishedLoadingAssetCallback;

  Coroutine loadingRoutine = null;

  void Awake() {
    if (loadOnStart && !string.IsNullOrEmpty(OBJUrl)) {
      LoadAsset();
    }
  }

  public void LoadAsset(string url = "", bool loadEvenIfAlreadyLoaded = false) {
    if (!alreadyLoadedAsset || loadEvenIfAlreadyLoaded) {
      if (!string.IsNullOrEmpty(url)) {
        OBJUrl = url;
      }

      if (loadingRoutine != null) {
        StopCoroutine(loadingRoutine);
      }

      loadingRoutine = StartCoroutine(LoadAssetCoroutine());
    } else {
      finishedLoadingAssetCallback = null;
    }
  }

  IEnumerator LoadAssetCoroutine(Action<float> callbackAction = null) {
    if (!string.IsNullOrEmpty(OBJUrl)) {
      float loadingStartTime = Time.time;

      if (callbackAction != null) {
        finishedLoadingAssetCallback = callbackAction;
      }

      UnityWebRequest webRequest = UnityWebRequest.Get(OBJUrl);

      yield return webRequest.SendWebRequest();

      if (webRequest.isNetworkError || webRequest.isHttpError) {
        Debug.Log("Couldn't get OBJ, error: " + webRequest.error);
      } else {
        alreadyLoadedAsset = true;

        GameObject loadedOBJGameObject = OBJLoader.LoadOBJFile(webRequest.downloadHandler.text, true);
        loadedOBJGameObject.name = "LoadedOBJ";
        loadedOBJGameObject.transform.SetParent(transform);
        loadedOBJGameObject.transform.localPosition = Vector3.zero;

        if (finishedLoadingAssetCallback != null) {
          finishedLoadingAssetCallback(Time.time - loadingStartTime);
        }
      }
    } else {
      Debug.Log("couldn't load OBJ because url is empty");
    }

    if (loadingPlaceholder != null) {
      loadingPlaceholder.SetActive(false);
    }

    finishedLoadingAssetCallback = null;

    loadingRoutine = null;
  }
}
