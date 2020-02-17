using System;
using System.Collections;
using System.Collections.Generic;
using DCL.Configuration;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

namespace DCL.Helpers
{
    public static class Utils
    {
        public static Dictionary<string, Material> staticMaterials;

        public static Material EnsureResourcesMaterial(string path)
        {
            if (staticMaterials == null)
            {
                staticMaterials = new Dictionary<string, Material>();
            }

            if (!staticMaterials.ContainsKey(path))
            {
                Material material = Resources.Load(path) as Material;

                if (material != null)
                {
                    staticMaterials.Add(path, material);
                }

                return material;
            }

            return staticMaterials[path];
        }

        public static void CleanMaterials(Renderer r)
        {
            if (r != null)
            {
                foreach (Material m in r.materials)
                {
                    if (m != null)
                    {
                        Material.Destroy(m);
                    }
                }
            }
        }

        public static Vector2[] FloatArrayToV2List(float[] uvs)
        {
            Vector2[] uvsResult = new Vector2[uvs.Length / 2];
            int uvsResultIndex = 0;

            for (int i = 0; i < uvs.Length;)
            {
                Vector2 tmpUv = Vector2.zero;
                tmpUv.x = uvs[i++];
                tmpUv.y = uvs[i++];

                uvsResult[uvsResultIndex++] = tmpUv;
            }

            return uvsResult;
        }


        public static void ResetLocalTRS(this Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        public static void SetToMaxStretch(this RectTransform t)
        {
            t.anchorMin = Vector2.zero;
            t.offsetMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.offsetMax = Vector2.one;
            t.sizeDelta = Vector2.zero;
            t.anchoredPosition = Vector2.zero;
            t.ForceUpdateRectTransforms();
        }

        public static void SetToCentered(this RectTransform t)
        {
            t.anchorMin = Vector2.one * 0.5f;
            t.offsetMin = Vector2.one * 0.5f;
            t.anchorMax = Vector2.one * 0.5f;
            t.offsetMax = Vector2.one * 0.5f;
            t.sizeDelta = Vector2.one * 100;
            t.ForceUpdateRectTransforms();
        }

        public static void SetToBottomLeft(this RectTransform t)
        {
            t.anchorMin = Vector2.zero;
            t.offsetMin = Vector2.zero;
            t.anchorMax = Vector2.zero;
            t.offsetMax = Vector2.zero;
            t.sizeDelta = Vector2.one * 100;
            t.ForceUpdateRectTransforms();
        }

        public static void InverseTransformChildTraversal<TComponent>(Action<TComponent> action, Transform startTransform)
            where TComponent : Component
        {
            Assert.IsTrue(startTransform != null, "startTransform must not be null");

            foreach (Transform t in startTransform)
            {
                InverseTransformChildTraversal(action, t);
            }

            var component = startTransform.GetComponent<TComponent>();

            if (component != null)
            {
                action.Invoke(component);
            }
        }

        public static void ForwardTransformChildTraversal<TComponent>(Func<TComponent, bool> action, Transform startTransform)
            where TComponent : Component
        {
            Assert.IsTrue(startTransform != null, "startTransform must not be null");

            var component = startTransform.GetComponent<TComponent>();

            if (component != null)
            {
                if (!action.Invoke(component))
                    return;
            }

            foreach (Transform t in startTransform)
            {
                ForwardTransformChildTraversal(action, t);
            }
        }


        public static T GetOrCreateComponent<T>(this GameObject gameObject) where T : UnityEngine.Component
        {
            T component = gameObject.GetComponent<T>();

            if (!component)
            {
                return gameObject.AddComponent<T>();
            }

            return component;
        }

        public static bool WebRequestSucceded(this UnityWebRequest request)
        {
            return request != null && !request.isNetworkError && !request.isHttpError;
        }

        static IEnumerator FetchAsset(string url, UnityWebRequest request,
            System.Action<UnityWebRequest> OnSuccess = null, System.Action<string> OnFail = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                using (var webRequest = request)
                {
                    yield return webRequest.SendWebRequest();

                    if (!WebRequestSucceded(request))
                    {
                        Debug.LogError(
                            string.Format("Fetching asset failed ({0}): {1} ", request.url, webRequest.error));

                        if (OnFail != null)
                        {
                            OnFail.Invoke(webRequest.error);
                        }
                    }
                    else
                    {
                        if (OnSuccess != null)
                        {
                            OnSuccess.Invoke(webRequest);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError(string.Format("Can't fetch asset as the url is empty!"));
            }
        }

        public static IEnumerator FetchAudioClip(string url, AudioType audioType, Action<AudioClip> OnSuccess,
            Action<string> OnFail)
        {
            //NOTE(Brian): This closure is called when the download is a success.
            Action<UnityWebRequest> OnSuccessInternal =
                (request) =>
                {
                    if (OnSuccess != null)
                    {
                        AudioClip ac = null;
                        try //In Editor we cannot decode MPEG and it's interrupting the flow
                        {
                            ac = DownloadHandlerAudioClip.GetContent(request);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                        OnSuccess.Invoke(ac);
                    }
                };

            Action<string> OnFailInternal =
                (error) =>
                {
                    if (OnFail != null)
                    {
                        OnFail.Invoke(error);
                    }
                };

            yield return FetchAsset(url, UnityWebRequestMultimedia.GetAudioClip(url, audioType), OnSuccessInternal,
                OnFailInternal);
        }

        public static IEnumerator FetchTexture(string textureURL, Action<Texture2D> OnSuccess)
        {
            //NOTE(Brian): This closure is called when the download is a success.
            System.Action<UnityWebRequest> OnSuccessInternal =
                (request) =>
                {
                    var texture = DownloadHandlerTexture.GetContent(request);
                    OnSuccess?.Invoke(texture);
                };

            yield return FetchAsset(textureURL, UnityWebRequestTexture.GetTexture(textureURL), OnSuccessInternal);
        }

        public static AudioType GetAudioTypeFromUrlName(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogError("GetAudioTypeFromUrlName >>> Null url!");
                return AudioType.UNKNOWN;
            }

            string ext = url.Substring(url.Length - 3).ToLower();

            switch (ext)
            {
                case "mp3":
                    return AudioType.MPEG;
                case "wav":
                    return AudioType.WAV;
                case "ogg":
                    return AudioType.OGGVORBIS;
                default:
                    return AudioType.UNKNOWN;
            }
        }

        public static bool SafeFromJsonOverwrite(string json, object objectToOverwrite)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(json, objectToOverwrite);
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError("ArgumentException Fail!... Json = " + json + " " + e.ToString());
                return false;
            }

            return true;
        }

        public static T FromJsonWithNulls<T>(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        }

        public static T SafeFromJson<T>(string json)
        {
            T returningValue = default(T);

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    returningValue = JsonUtility.FromJson<T>(json);
                }
                catch (System.ArgumentException e)
                {
                    Debug.LogError("ArgumentException Fail!... Json = " + json + " " + e.ToString());
                }
            }

            return returningValue;
        }

        public static GameObject AttachPlaceholderRendererGameObject(UnityEngine.Transform targetTransform)
        {
            var placeholderRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();

            placeholderRenderer.material = Resources.Load<Material>("Materials/AssetLoading");
            placeholderRenderer.transform.SetParent(targetTransform);
            placeholderRenderer.transform.localPosition = Vector3.zero;
            placeholderRenderer.name = "PlaceholderRenderer";

            return placeholderRenderer.gameObject;
        }

        public static void SafeDestroy(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj, false);
#else
                UnityEngine.Object.Destroy(obj);
#endif
        }

        /**
         * Transforms a grid position into a world-relative 3d position
         */
        public static Vector3 GridToWorldPosition(float xGridPosition, float yGridPosition)
        {
            return new Vector3(
                x: xGridPosition * ParcelSettings.PARCEL_SIZE,
                y: 0f,
                z: yGridPosition * ParcelSettings.PARCEL_SIZE
            );
        }

        /**
         * Transforms a world position into a grid position
         */
        public static Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            return new Vector2Int(
                (int)Mathf.Floor(worldPosition.x / ParcelSettings.PARCEL_SIZE),
                (int)Mathf.Floor(worldPosition.z / ParcelSettings.PARCEL_SIZE)
            );
        }

        public static string GetTestAssetsPathRaw()
        {
            return Application.dataPath + "/../TestResources";
        }

        public static string GetTestsAssetsPath(bool useWebServerPath = false)
        {
            if (useWebServerPath)
            {
                return "http://127.0.0.1:9991";
            }
            else
            {
                var uri = new System.Uri(GetTestAssetsPathRaw());
                var converted = uri.AbsoluteUri;
                return converted;
            }
        }

        public static bool AproxComparison(this Color color1, Color color2, float tolerance = 0.01f) // tolerance of roughly 1f / 255f
        {
            if (Mathf.Abs(color1.r - color2.r) < tolerance
                && Mathf.Abs(color1.g - color2.g) < tolerance
                && Mathf.Abs(color1.b - color2.b) < tolerance)
            {
                return true;
            }
            return false;
        }

        public static T ParseJsonArray<T>(string jsonArray) where T : IEnumerable => DummyJsonUtilityFromArray<T>.GetFromJsonArray(jsonArray);

        [Serializable]
        private class DummyJsonUtilityFromArray<T> where T : IEnumerable //UnityEngine.JsonUtility is really fast but cannot deserialize json arrays
        {
            [SerializeField] private T value;

            public static T GetFromJsonArray(string jsonArray)
            {
                string newJson = $"{{ \"value\": {jsonArray}}}";
                return JsonUtility.FromJson<DummyJsonUtilityFromArray<T>>(newJson).value;
            }
        }

        public static Bounds BuildMergedBounds(Renderer[] renderers)
        {
            Bounds bounds = new Bounds();

            for (int i = 0; i < renderers.Length; i++)
            {
                if (i == 0)
                    bounds = renderers[i].bounds;
                else
                    bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static int lockedInFrame = -1;
        public static bool LockedThisFrame() => lockedInFrame == Time.frameCount;

        //NOTE(Brian): Made as an independent flag because the CI doesn't work well with the Cursor.lockState check.
        public static bool isCursorLocked = false;

        public static void LockCursor()
        {
            isCursorLocked = true;
            lockedInFrame = Time.frameCount;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            EventSystem.current.SetSelectedGameObject(null);
        }

        public static void UnlockCursor()
        {
            isCursorLocked = false;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public static void DestroyAllChild(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }
    }
}
