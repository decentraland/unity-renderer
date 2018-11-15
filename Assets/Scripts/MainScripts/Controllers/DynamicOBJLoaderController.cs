using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DynamicOBJLoaderController : MonoBehaviour {
  public bool loadOnStart = true;
  public string OBJUrl = "";
  public Renderer loadingPlaceholderRenderer;

  [HideInInspector] public bool alreadyLoaded = false;
  [HideInInspector] public Action<float> finishedLoadingModelCallback;

  MeshFilter meshFilter;

  void Awake() {
    meshFilter = GetComponent<MeshFilter>();

    if (loadOnStart) {
      StartCoroutine(LoadRemoteOBJ());
    }
  }

  public IEnumerator LoadRemoteOBJ(Action<float> callbackAction = null) {
    if (OBJUrl != "") {
      float loadingStartTime = Time.time;

      if (callbackAction != null) {
        finishedLoadingModelCallback = callbackAction;
      }

      UnityWebRequest webRequest = UnityWebRequest.Get(OBJUrl);

      yield return webRequest.SendWebRequest();

      if (webRequest.isNetworkError || webRequest.isHttpError) {
        Debug.Log("Couldn't get OBJ, error: " + webRequest.error);
      } else {
        alreadyLoaded = true;

        GameObject loadedOBJGameObject = OBJLoader.LoadOBJFile(webRequest.downloadHandler.text, true);
        loadedOBJGameObject.name = "LoadedOBJ";
        loadedOBJGameObject.transform.SetParent(transform);
        loadedOBJGameObject.transform.localPosition = Vector3.zero;

        if (finishedLoadingModelCallback != null)
          finishedLoadingModelCallback(Time.time - loadingStartTime);
      }
    } else {
      Debug.Log("couldn't load OBJ as url is empty");
    }

    if (loadingPlaceholderRenderer != null) {
      loadingPlaceholderRenderer.enabled = false;
    }

    finishedLoadingModelCallback = null;

    yield return true;
  }
}
