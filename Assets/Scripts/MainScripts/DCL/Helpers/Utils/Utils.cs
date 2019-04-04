using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

namespace DCL.Helpers
{
    public static class Utils
    {
        public static Dictionary<string, Material> staticMaterials;

        public static Material EnsureResourcesMaterial(string path)
        {
            if (staticMaterials == null)
                staticMaterials = new Dictionary<string, Material>();

            if (!staticMaterials.ContainsKey(path))
            {
                Material material = Resources.Load(path) as Material;

                if (material != null)
                    staticMaterials.Add(path, material);

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
        }

        public static T GetOrCreateComponent<T>(GameObject gameObject) where T : UnityEngine.Component
        {
            T component = gameObject.GetComponent<T>();

            if (!component)
            {
                return gameObject.AddComponent<T>();
            }

            return component;
        }

        public static bool WebRequestSucceded(UnityWebRequest request)
        {
            return request != null && !request.isNetworkError && !request.isHttpError;
        }

        static IEnumerator FetchAsset(string url, UnityWebRequest request, System.Action<UnityWebRequest> OnSuccess = null, System.Action<string> OnFail = null)
        {
            if (!string.IsNullOrEmpty(url))
            {
                using (var webRequest = request)
                {
                    yield return webRequest.SendWebRequest();

                    if (!WebRequestSucceded(request))
                    {
                        Debug.LogError(string.Format("Fetching asset failed ({0}): {1} ", request.url, webRequest.error));

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

        public static IEnumerator FetchAudioClip(string url, AudioType audioType, Action<AudioClip> OnSuccess, Action<string> OnFail)
        {
            //NOTE(Brian): This closure is called when the download is a success.
            Action<UnityWebRequest> OnSuccessInternal =
                (request) =>
                {
                    if (OnSuccess != null)
                    {
                        AudioClip ac = DownloadHandlerAudioClip.GetContent(request);
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

            yield return FetchAsset(url, UnityWebRequestMultimedia.GetAudioClip(url, audioType), OnSuccessInternal, OnFailInternal);
        }

        public static IEnumerator FetchTexture(string textureURL, Action<Texture> OnSuccess)
        {
            //NOTE(Brian): This closure is called when the download is a success.
            System.Action<UnityWebRequest> OnSuccessInternal =
                (request) =>
                {
                    if (OnSuccess != null)
                    {
                        OnSuccess.Invoke(DownloadHandlerTexture.GetContent(request));
                    }
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

        public static T SafeFromJson<T>(string json) where T : new()
        {
            T returningValue;

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    returningValue = JsonUtility.FromJson<T>(json);
                }
                catch (System.ArgumentException e)
                {
                    Debug.LogError("ArgumentException Fail!... Json = " + json + " " + e.ToString());

                    returningValue = new T();
                }
            }
            else
            {
                returningValue = new T();
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
            {
                UnityEngine.Object.Destroy(obj);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
#else
            UnityEngine.Object.Destroy(obj);
#endif
        }
    }
}
