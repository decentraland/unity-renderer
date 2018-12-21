using UnityEngine;
using DCL.Configuration;
using System.Collections;
using System;
using UnityEngine.Networking;

namespace DCL.Helpers {
  public static class LandHelpers {

    /**
     * Transforms a grid position into a world-relative 3d position
     */
    public static Vector3 GridToWorldPosition(float xGridPosition, float yGridPosition) {
      return new Vector3(
        x: xGridPosition * ParcelSettings.PARCEL_SIZE,
        y: 0f,
        z: yGridPosition * ParcelSettings.PARCEL_SIZE
      );
    }

    /**
     * Transforms a world position into a grid position
     */
    public static Vector2 worldToGrid(Vector3 vector) {
      return new Vector2(
        Mathf.Floor(vector.x / ParcelSettings.PARCEL_SIZE),
        Mathf.Floor(vector.z / ParcelSettings.PARCEL_SIZE)
      );
    }

    //todo: move this
    public static T GetOrCreateComponent<T>(GameObject gameObject) where T : UnityEngine.Component {
      T component = gameObject.GetComponent<T>();
      if (!component) {
        return gameObject.AddComponent<T>();
      }
      return component;
    }

    // todo: move this
    public static IEnumerator FetchTexture(Controllers.ParcelScene scene, string textureURL, Action<Texture> callback) {
      if (!string.IsNullOrEmpty(textureURL)) {
        using (var webRequest = UnityWebRequestTexture.GetTexture(textureURL)) {
          yield return webRequest.SendWebRequest();

          if (webRequest.isNetworkError || webRequest.isHttpError) {
            Debug.Log("Fetching texture failed: " + webRequest.error);
          } else {
            callback(DownloadHandlerTexture.GetContent(webRequest));
          }
        }
      } else {
        Debug.Log("Can't fetch texture as the url is empty");

        yield return null;
      }
    }
  }
}
