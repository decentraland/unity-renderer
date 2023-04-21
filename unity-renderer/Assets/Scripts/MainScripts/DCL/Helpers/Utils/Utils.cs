#if UNITY_WEBGL && !UNITY_EDITOR
#define WEB_PLATFORM
#endif

using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DCL.Configuration;
using DG.Tweening;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.Rendering.Universal;

namespace DCL.Helpers
{
    public static class Utils
    {
        public static Dictionary<string, Material> staticMaterials;

        public static Material EnsureResourcesMaterial(string path)
        {
            if (staticMaterials == null) { staticMaterials = new Dictionary<string, Material>(); }

            if (!staticMaterials.ContainsKey(path))
            {
                Material material = Resources.Load(path) as Material;

                if (material != null) { staticMaterials.Add(path, material); }

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
                    if (m != null) { Material.Destroy(m); }
                }
            }
        }

        public static ScriptableRendererFeature ToggleRenderFeature<T>(this UniversalRenderPipelineAsset asset, bool enable) where T: ScriptableRendererFeature
        {
            var type = asset.GetType();
            var propertyInfo = type.GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);

            if (propertyInfo == null) { return null; }

            var scriptableRenderData = (ScriptableRendererData[])propertyInfo.GetValue(asset);

            if (scriptableRenderData != null && scriptableRenderData.Length > 0)
            {
                foreach (var renderData in scriptableRenderData)
                {
                    foreach (var rendererFeature in renderData.rendererFeatures)
                    {
                        if (rendererFeature is T)
                        {
                            rendererFeature.SetActive(enable);

                            return rendererFeature;
                        }
                    }
                }
            }

            return null;
        }

        public static Vector2[] FloatArrayToV2List(RepeatedField<float> uvs)
        {
            Vector2[] uvsResult = new Vector2[uvs.Count / 2];
            int uvsResultIndex = 0;

            for (int i = 0; i < uvs.Count;)
            {
                Vector2 tmpUv = Vector2.zero;
                tmpUv.x = uvs[i++];
                tmpUv.y = uvs[i++];

                uvsResult[uvsResultIndex++] = tmpUv;
            }

            return uvsResult;
        }

        public static Vector2[] FloatArrayToV2List(IList<float> uvs)
        {
            Vector2[] uvsResult = new Vector2[uvs.Count / 2];
            int uvsResultIndex = 0;

            for (int i = 0; i < uvs.Count;) { uvsResult[uvsResultIndex++] = new Vector2(uvs[i++], uvs[i++]); }

            return uvsResult;
        }

        public static Vector2Int StringToVector2Int(string coords)
        {
            string[] coordSplit = coords.Split(',');

            if (coordSplit.Length == 2 && int.TryParse(coordSplit[0], out int x) && int.TryParse(coordSplit[1], out int y)) { return new Vector2Int(x, y); }

            return Vector2Int.zero;
        }

        private const int MAX_TRANSFORM_VALUE = 10000;

        public static void CapGlobalValuesToMax(this Transform transform)
        {
            bool positionOutsideBoundaries = transform.position.sqrMagnitude > MAX_TRANSFORM_VALUE * MAX_TRANSFORM_VALUE;
            bool scaleOutsideBoundaries = transform.lossyScale.sqrMagnitude > MAX_TRANSFORM_VALUE * MAX_TRANSFORM_VALUE;

            if (positionOutsideBoundaries || scaleOutsideBoundaries)
            {
                Vector3 newPosition = transform.position;

                if (positionOutsideBoundaries)
                {
                    if (Mathf.Abs(newPosition.x) > MAX_TRANSFORM_VALUE)
                        newPosition.x = MAX_TRANSFORM_VALUE * Mathf.Sign(newPosition.x);

                    if (Mathf.Abs(newPosition.y) > MAX_TRANSFORM_VALUE)
                        newPosition.y = MAX_TRANSFORM_VALUE * Mathf.Sign(newPosition.y);

                    if (Mathf.Abs(newPosition.z) > MAX_TRANSFORM_VALUE)
                        newPosition.z = MAX_TRANSFORM_VALUE * Mathf.Sign(newPosition.z);
                }

                Vector3 newScale = transform.lossyScale;

                if (scaleOutsideBoundaries)
                {
                    if (Mathf.Abs(newScale.x) > MAX_TRANSFORM_VALUE)
                        newScale.x = MAX_TRANSFORM_VALUE * Mathf.Sign(newScale.x);

                    if (Mathf.Abs(newScale.y) > MAX_TRANSFORM_VALUE)
                        newScale.y = MAX_TRANSFORM_VALUE * Mathf.Sign(newScale.y);

                    if (Mathf.Abs(newScale.z) > MAX_TRANSFORM_VALUE)
                        newScale.z = MAX_TRANSFORM_VALUE * Mathf.Sign(newScale.z);
                }

                SetTransformGlobalValues(transform, newPosition, transform.rotation, newScale, scaleOutsideBoundaries);
            }
        }

        public static void SetTransformGlobalValues(Transform transform, Vector3 newPos, Quaternion newRot, Vector3 newScale, bool setScale = true)
        {
            transform.position = newPos;
            transform.rotation = newRot;

            if (setScale)
            {
                transform.localScale = Vector3.one;
                var m = transform.worldToLocalMatrix;

                m.SetColumn(0, new Vector4(m.GetColumn(0).magnitude, 0f));
                m.SetColumn(1, new Vector4(0f, m.GetColumn(1).magnitude));
                m.SetColumn(2, new Vector4(0f, 0f, m.GetColumn(2).magnitude));
                m.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));

                transform.localScale = m.MultiplyPoint(newScale);
            }
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
                InverseTransformChildTraversal<RectTransform>(
                    (x) => { ForceRebuildLayoutImmediate(x); },
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
            layoutElements.RemoveAll(e => (e is Behaviour && !((Behaviour)e).isActiveAndEnabled) || e is TextMeshProUGUI);

            foreach (var layoutElem in layoutElements)
            {
                (layoutElem as ILayoutElement).CalculateLayoutInputHorizontal();
                (layoutElem as ILayoutElement).CalculateLayoutInputVertical();
            }

            var layoutControllers = rectTransformRoot.GetComponentsInChildren(typeof(ILayoutController), true).ToList();
            layoutControllers.RemoveAll(e => e is Behaviour && !((Behaviour)e).isActiveAndEnabled);

            foreach (var layoutCtrl in layoutControllers)
            {
                (layoutCtrl as ILayoutController).SetLayoutHorizontal();
                (layoutCtrl as ILayoutController).SetLayoutVertical();
            }
        }

        private static IEnumerator ForceUpdateLayoutRoutine(RectTransform rt)
        {
            yield return null;

            InverseTransformChildTraversal<RectTransform>(
                (x) => { ForceRebuildLayoutImmediate(x); },
                rt);
        }

        public static void InverseTransformChildTraversal<TComponent>(Action<TComponent> action, Transform startTransform)
            where TComponent: Component
        {
            if (startTransform == null)
                return;

            foreach (Transform t in startTransform) { InverseTransformChildTraversal(action, t); }

            var component = startTransform.GetComponent<TComponent>();

            if (component != null) { action.Invoke(component); }
        }

        public static void ForwardTransformChildTraversal<TComponent>(Func<TComponent, bool> action, Transform startTransform)
            where TComponent: Component
        {
            Assert.IsTrue(startTransform != null, "startTransform must not be null");

            var component = startTransform.GetComponent<TComponent>();

            if (component != null)
            {
                if (!action.Invoke(component))
                    return;
            }

            foreach (Transform t in startTransform) { ForwardTransformChildTraversal(action, t); }
        }

        public static T GetOrCreateComponent<T>(this GameObject gameObject) where T: Component
        {
            T component = gameObject.GetComponent<T>();

            if (!component) { return gameObject.AddComponent<T>(); }

            return component;
        }

        public static IWebRequestAsyncOperation FetchTexture(string textureURL, bool isReadable, Action<Texture2D> OnSuccess, Action<IWebRequestAsyncOperation> OnFail = null)
        {
            void SuccessInternal(IWebRequestAsyncOperation request) { OnSuccess?.Invoke(DownloadHandlerTexture.GetContent(request.webRequest)); }

            var asyncOp = Environment.i.platform.webRequest.GetTexture(
                url: textureURL,
                OnSuccess: SuccessInternal,
                OnFail: OnFail,
                isReadable: isReadable);

            return asyncOp;
        }

        public static bool SafeFromJsonOverwrite(string json, object objectToOverwrite)
        {
            try { JsonUtility.FromJsonOverwrite(json, objectToOverwrite); }
            catch (ArgumentException e)
            {
                Debug.LogError("ArgumentException Fail!... Json = " + json + " " + e.ToString());
                return false;
            }

            return true;
        }

        public static T FromJsonWithNulls<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static T SafeFromJson<T>(string json)
        {
            ProfilingEvents.OnMessageDecodeStart?.Invoke("Misc");

            T returningValue = default(T);

            if (!string.IsNullOrEmpty(json))
            {
                try { returningValue = JsonUtility.FromJson<T>(json); }
                catch (ArgumentException e) { Debug.LogError("ArgumentException Fail!... Json = " + json + " " + e.ToString()); }
            }

            ProfilingEvents.OnMessageDecodeEnds?.Invoke("Misc");

            return returningValue;
        }

        public static GameObject AttachPlaceholderRendererGameObject(Transform targetTransform)
        {
            var placeholderRenderer = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();

            placeholderRenderer.material = Resources.Load<Material>("Materials/AssetLoading");
            placeholderRenderer.transform.SetParent(targetTransform);
            placeholderRenderer.transform.localPosition = Vector3.zero;
            placeholderRenderer.name = "PlaceholderRenderer";

            return placeholderRenderer.gameObject;
        }

        public static void SafeDestroy(Object obj)
        {
            if (obj is Transform)
                return;

#if UNITY_EDITOR
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj, false);
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

        public static Vector2 WorldToGridPositionUnclamped(Vector3 worldPosition)
        {
            return new Vector2(
                worldPosition.x / ParcelSettings.PARCEL_SIZE,
                worldPosition.z / ParcelSettings.PARCEL_SIZE
            );
        }

        public static bool AproxComparison(this Color color1, Color color2, float tolerance = 0.01f) // tolerance of roughly 1f / 255f
        {
            if (Mathf.Abs(color1.r - color2.r) < tolerance
                && Mathf.Abs(color1.g - color2.g) < tolerance
                && Mathf.Abs(color1.b - color2.b) < tolerance) { return true; }

            return false;
        }

        public static T ParseJsonArray<T>(string jsonArray) where T: IEnumerable =>
            DummyJsonUtilityFromArray<T>.GetFromJsonArray(jsonArray);

        [Serializable]
        private class DummyJsonUtilityFromArray<T> where T: IEnumerable //UnityEngine.JsonUtility is really fast but cannot deserialize json arrays
        {
            [SerializeField]
            private T value;

            public static T GetFromJsonArray(string jsonArray)
            {
                string newJson = $"{{ \"value\": {jsonArray}}}";
                return JsonUtility.FromJson<DummyJsonUtilityFromArray<T>>(newJson).value;
            }
        }

        private static int lockedInFrame = -1;

        public static bool LockedThisFrame() =>
            lockedInFrame == Time.frameCount;

        private static bool isCursorLocked;

        //NOTE(Brian): Made as an independent flag because the CI doesn't work well with the Cursor.lockState check.
        public static bool IsCursorLocked
        {
            get => isCursorLocked;

            private set
            {
                if (isCursorLocked == value) return;
                isCursorLocked = value;
                OnCursorLockChanged?.Invoke(isCursorLocked);
            }
        }

        public static event Action<bool> OnCursorLockChanged;

        public static void LockCursor()
        {
#if WEB_PLATFORM
            //TODO(Brian): Encapsulate all this mechanism to a new MouseLockController and branch
            //             behaviour using strategy pattern instead of this.
            if (IsCursorLocked)
            {
                return;
            }
#endif
            Cursor.visible = false;
            IsCursorLocked = true;
            Cursor.lockState = CursorLockMode.Locked;
            lockedInFrame = Time.frameCount;

            EventSystem.current?.SetSelectedGameObject(null);
        }

        public static void UnlockCursor()
        {
#if WEB_PLATFORM
            //TODO(Brian): Encapsulate all this mechanism to a new MouseLockController and branch
            //             behaviour using strategy pattern instead of this.
            if (!IsCursorLocked)
            {
                return;
            }
#endif
            Cursor.visible = true;
            IsCursorLocked = false;
            Cursor.lockState = CursorLockMode.None;

            EventSystem.current?.SetSelectedGameObject(null);
        }

#region BROWSER_ONLY
        //TODO(Brian): Encapsulate all this mechanism to a new MouseLockController and branch
        //             behaviour using strategy pattern instead of this.
        // NOTE: This should come from browser's pointerlockchange callback
        public static void BrowserSetCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

            IsCursorLocked = locked;
            Cursor.visible = !locked;
        }
#endregion

        public static void DestroyAllChild(this Transform transform)
        {
            foreach (Transform child in transform) { Object.Destroy(child.gameObject); }
        }

        public static List<Vector2Int> GetBottomLeftZoneArray(Vector2Int bottomLeftAnchor, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = bottomLeftAnchor.x; x < bottomLeftAnchor.x + size.x; x++)
            {
                for (int y = bottomLeftAnchor.y; y < bottomLeftAnchor.y + size.y; y++) { coords.Add(new Vector2Int(x, y)); }
            }

            return coords;
        }

        public static List<Vector2Int> GetCenteredZoneArray(Vector2Int center, Vector2Int size)
        {
            List<Vector2Int> coords = new List<Vector2Int>();

            for (int x = center.x - size.x; x < center.x + size.x; x++)
            {
                for (int y = center.y - size.y; y < center.y + size.y; y++) { coords.Add(new Vector2Int(x, y)); }
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

        public static bool CompareFloats(float a, float b, float precision = 0.1f)
        {
            return Mathf.Abs(a - b) < precision;
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        /// <summary>
        /// Set a layer to the given transform and its child
        /// </summary>
        /// <param name="transform"></param>
        public static void SetLayerRecursively(Transform transform, int layer)
        {
            transform.gameObject.layer = layer;

            foreach (Transform child in transform) { SetLayerRecursively(child, layer); }
        }

        /// <summary>
        /// Converts a linear float (between 0 and 1) into an exponential curve fitting for audio volume.
        /// </summary>
        /// <param name="volume">Linear volume float</param>
        /// <returns>Exponential volume curve float</returns>
        public static float ToVolumeCurve(float volume)
        {
            return volume * (2f - volume);
        }

        /// <summary>
        /// Takes a linear volume value between 0 and 1, converts to exponential curve and maps to a value fitting for audio mixer group volume.
        /// </summary>
        /// <param name="volume">Linear volume (0 to 1)</param>
        /// <returns>Value for audio mixer group volume</returns>
        public static float ToAudioMixerGroupVolume(float volume)
        {
            return (ToVolumeCurve(volume) * 80f) - 80f;
        }

        public static IEnumerator Wait(float delay, Action onFinishCallback)
        {
            yield return new WaitForSeconds(delay);
            onFinishCallback.Invoke();
        }

        public static string GetHierarchyPath(this Transform transform)
        {
            if (transform.parent == null)
                return transform.name;

            return $"{transform.parent.GetHierarchyPath()}/{transform.name}";
        }

        public static bool TryFindChildRecursively(this Transform transform, string name, out Transform foundChild)
        {
            foundChild = transform.Find(name);

            if (foundChild != null)
                return true;

            foreach (Transform child in transform)
            {
                if (TryFindChildRecursively(child, name, out foundChild))
                    return true;
            }

            return false;
        }

        public static bool IsPointerOverUIElement(Vector3 mousePosition)
        {
            if (EventSystem.current == null)
                return false;

            var eventData = new PointerEventData(EventSystem.current);
            eventData.position = mousePosition;
            var results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);
            return results.Count > 1;
        }

        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverUIElement(Input.mousePosition);
        }

        public static string UnixTimeStampToLocalTime(ulong unixTimeStampMilliseconds)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStampMilliseconds).ToLocalTime();
            return $"{dtDateTime.Hour}:{dtDateTime.Minute.ToString("D2")}";
        }

        public static DateTime UnixToDateTimeWithTime(ulong unixTimeStampMilliseconds)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTimeStampMilliseconds).ToLocalTime();
            return dtDateTime;
        }

        public static Color GetRandomColorInGradient(Color minColor, Color maxColor)
        {
            Color.RGBToHSV(minColor, out float startH, out float startS, out float startV);
            Color.RGBToHSV(maxColor, out float endH, out float _, out float _);
            return Color.HSVToRGB(UnityEngine.Random.Range(startH, endH), startS, startV);
        }

        public static UniTask ToUniTaskInstantCancelation(this Tween tween, bool completeWhenCancel = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.RegisterWithoutCaptureExecutionContext(() =>
            {
                // We cannot use the built in TweenCancelBehavior because we are manually killing the tween
                if (completeWhenCancel)
                    tween.Complete(false);

                tween.Kill();
            });

            return tween.ToUniTask(tweenCancelBehaviour: completeWhenCancel ? TweenCancelBehaviour.Complete : TweenCancelBehaviour.Kill, cancellationToken: cancellationToken);
        }
    }
}
