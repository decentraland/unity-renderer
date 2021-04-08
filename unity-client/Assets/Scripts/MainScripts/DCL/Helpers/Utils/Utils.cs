#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_PLATFORM
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DCL.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

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
        }

        public static void SetToCentered(this RectTransform t)
        {
            t.anchorMin = Vector2.one * 0.5f;
            t.offsetMin = Vector2.one * 0.5f;
            t.anchorMax = Vector2.one * 0.5f;
            t.offsetMax = Vector2.one * 0.5f;
            t.sizeDelta = Vector2.one * 100;
        }

        public static void SetToBottomLeft(this RectTransform t)
        {
            t.anchorMin = Vector2.zero;
            t.offsetMin = Vector2.zero;
            t.anchorMax = Vector2.zero;
            t.offsetMax = Vector2.zero;
            t.sizeDelta = Vector2.one * 100;
        }

        public static void ForceUpdateLayout(this RectTransform rt, bool delayed = true)
        {
            if (!rt.gameObject.activeInHierarchy)
                return;

            if (delayed)
                CoroutineStarter.Start(ForceUpdateLayoutRoutine(rt));
            else
            {
                Utils.InverseTransformChildTraversal<RectTransform>(
                    (x) => { Utils.ForceRebuildLayoutImmediate(x); },
                    rt);
            }
        }

        /// <summary>
        /// Reimplementation of the LayoutRebuilder.ForceRebuildLayoutImmediate() function (Unity UI API) for make it more performant.
        /// </summary>
        /// <param name="rectTransformRoot">Root from which to rebuild.</param>
        public static void ForceRebuildLayoutImmediate(RectTransform rectTransformRoot)
        {
            if (rectTransformRoot == null)
                return;

            // NOTE(Santi): It seems to be very much cheaper to execute the next instructions manually than execute directly the function
            //              'LayoutRebuilder.ForceRebuildLayoutImmediate()', that theorically already contains these instructions.
            var layoutElements = rectTransformRoot.GetComponentsInChildren(typeof(ILayoutElement), true).ToList();
            layoutElements.RemoveAll(e => (e is Behaviour && !((Behaviour) e).isActiveAndEnabled) || e is TextMeshProUGUI);
            foreach (var layoutElem in layoutElements)
            {
                (layoutElem as ILayoutElement).CalculateLayoutInputHorizontal();
                (layoutElem as ILayoutElement).CalculateLayoutInputVertical();
            }

            var layoutControllers = rectTransformRoot.GetComponentsInChildren(typeof(ILayoutController), true).ToList();
            layoutControllers.RemoveAll(e => e is Behaviour && !((Behaviour) e).isActiveAndEnabled);
            foreach (var layoutCtrl in layoutControllers)
            {
                (layoutCtrl as ILayoutController).SetLayoutHorizontal();
                (layoutCtrl as ILayoutController).SetLayoutVertical();
            }
        }

        private static IEnumerator ForceUpdateLayoutRoutine(RectTransform rt)
        {
            yield return null;

            Utils.InverseTransformChildTraversal<RectTransform>(
                (x) => { Utils.ForceRebuildLayoutImmediate(x); },
                rt);
        }

        public static void InverseTransformChildTraversal<TComponent>(Action<TComponent> action, Transform startTransform)
            where TComponent : Component
        {
            if (startTransform == null)
                return;

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
            return request != null &&
                   !request.isNetworkError &&
                   !request.isHttpError;
        }

        public static bool WebRequestServerError(this UnityWebRequest request)
        {
            return request != null &&
                   request.responseCode >= 500 &&
                   request.responseCode < 600;
        }

        public static bool WebRequestAborted(this UnityWebRequest request)
        {
            return request != null &&
                   request.isNetworkError &&
                   request.isHttpError &&
                   !string.IsNullOrEmpty(request.error) &&
                   request.error.ToLower().Contains("aborted");
        }

        public static IEnumerator FetchAsset(string url, UnityWebRequest request,
            System.Action<UnityWebRequest> OnSuccess = null, System.Action<string> OnFail = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                using (var webRequest = request)
                {
                    yield return webRequest.SendWebRequest();

                    if (!WebRequestSucceded(request))
                    {
                        Debug.Log(
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
                Debug.Log(string.Format("Can't fetch asset as the url is empty!"));
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
                        bool supported = true;
#if UNITY_EDITOR
                        supported = audioType != AudioType.MPEG;
#endif
                        AudioClip ac = null;

                        if (supported)
                            ac = DownloadHandlerAudioClip.GetContent(request);

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

            var req = UnityWebRequestMultimedia.GetAudioClip(url, audioType);

            yield return FetchAsset(url, req, OnSuccessInternal,
                OnFailInternal);
        }

        public static IEnumerator FetchTexture(string textureURL, Action<Texture2D> OnSuccess, Action<string> OnFail = null)
        {
            //NOTE(Brian): This closure is called when the download is a success.
            void SuccessInternal(UnityWebRequest request)
            {
                var texture = DownloadHandlerTexture.GetContent(request);
                OnSuccess?.Invoke(texture);
            }

            yield return FetchAsset(textureURL, UnityWebRequestTexture.GetTexture(textureURL), SuccessInternal, OnFail);
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

        public static T FromJsonWithNulls<T>(string json) { return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json); }

        public static T SafeFromJson<T>(string json)
        {
            ProfilingEvents.OnMessageDecodeStart?.Invoke("Misc");

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

            ProfilingEvents.OnMessageDecodeEnds?.Invoke("Misc");

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
                (int) Mathf.Floor(worldPosition.x / ParcelSettings.PARCEL_SIZE),
                (int) Mathf.Floor(worldPosition.z / ParcelSettings.PARCEL_SIZE)
            );
        }

        public static Vector2 WorldToGridPositionUnclamped(Vector3 worldPosition)
        {
            return new Vector2(
                worldPosition.x / ParcelSettings.PARCEL_SIZE,
                worldPosition.z / ParcelSettings.PARCEL_SIZE
            );
        }

        public static string GetTestAssetsPathRaw() { return Application.dataPath + "/../TestResources"; }

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
            [SerializeField]
            private T value;

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
                if (renderers[i] == null)
                    continue;

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
        public static bool isCursorLocked { get; private set; } = false;

        public static void LockCursor()
        {
#if WEB_PLATFORM
            //TODO(Brian): Encapsulate all this mechanism to a new MouseLockController and branch
            //             behaviour using strategy pattern instead of this.
            if (isCursorLocked)
            {
                return;
            }
            if (requestedUnlock || requestedLock)
            {
                return;
            }
            requestedLock = true;
#else
            isCursorLocked = true;
            Cursor.visible = false;
#endif
            Cursor.lockState = CursorLockMode.Locked;
            lockedInFrame = Time.frameCount;

            EventSystem.current?.SetSelectedGameObject(null);
        }

        public static void UnlockCursor()
        {
#if WEB_PLATFORM
            //TODO(Brian): Encapsulate all this mechanism to a new MouseLockController and branch
            //             behaviour using strategy pattern instead of this.
            if (!isCursorLocked)
            {
                return;
            }
            if (requestedUnlock || requestedLock)
            {
                return;
            }
            requestedUnlock = true;
#else
            isCursorLocked = false;
            Cursor.visible = true;
#endif
            Cursor.lockState = CursorLockMode.None;

            EventSystem.current?.SetSelectedGameObject(null);
        }

        #region BROWSER_ONLY

        //TODO(Brian): Encapsulate all this mechanism to a new MouseLockController and branch
        //             behaviour using strategy pattern instead of this.
        private static bool requestedUnlock = false;
        private static bool requestedLock = false;

        // NOTE: This should come from browser's pointerlockchange callback
        public static void BrowserSetCursorState(bool locked)
        {
            if (!locked && !requestedUnlock)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            isCursorLocked = locked;
            Cursor.visible = !locked;
            requestedUnlock = false;
            requestedLock = false;
        }

        #endregion

        public static void DestroyAllChild(this Transform transform)
        {
            foreach (Transform child in transform)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        public static List<Vector2Int> GetBottomLeftZoneArray(Vector2Int bottomLeftAnchor, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = bottomLeftAnchor.x; x < bottomLeftAnchor.x + size.x; x++)
            {
                for (int y = bottomLeftAnchor.y; y < bottomLeftAnchor.y + size.y; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            return coords;
        }

        public static List<Vector2Int> GetCenteredZoneArray(Vector2Int center, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = center.x - size.x; x < center.x + size.x; x++)
            {
                for (int y = center.y - size.y; y < center.y + size.y; y++)
                {
                    coords.Add(new Vector2Int(x, y));
                }
            }

            return coords;
        }

        public static void DrawRectGizmo(Rect rect, Color color, float duration)
        {
            Vector3 tl2 = new Vector3(rect.xMin, rect.yMax, 0);
            Vector3 bl2 = new Vector3(rect.xMin, rect.yMin, 0);
            Vector3 tr2 = new Vector3(rect.xMax, rect.yMax, 0);
            Vector3 br2 = new Vector3(rect.xMax, rect.yMin, 0);

            Debug.DrawLine(tl2, bl2, color, duration);
            Debug.DrawLine(tl2, tr2, color, duration);
            Debug.DrawLine(bl2, br2, color, duration);
            Debug.DrawLine(tr2, br2, color, duration);
        }

        public static string ToUpperFirst(this string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var capital = char.ToUpper(value[0]);
                value = capital + value.Substring(1);
            }

            return value;
        }

        public static Vector3 Sanitize(Vector3 value)
        {
            float x = float.IsInfinity(value.x) ? 0 : value.x;
            float y = float.IsInfinity(value.y) ? 0 : value.y;
            float z = float.IsInfinity(value.z) ? 0 : value.z;

            return new Vector3(x, y, z);
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }
    }
}