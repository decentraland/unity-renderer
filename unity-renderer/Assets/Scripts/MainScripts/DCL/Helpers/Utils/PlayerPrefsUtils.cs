using UnityEngine;
using System.Runtime.InteropServices;

namespace DCL.Helpers
{
    public static class PlayerPrefsUtils
    {
        public static int GetInt(string key)
        {
#if UNITY_WEBGL
            string value = LoadStringFromLocalStorage(key: key);
            return int.Parse(value);
#else
            return PlayerPrefs.GetInt(key);
#endif
        }

        public static int GetInt(string key, int defaultValue)
        {
#if UNITY_WEBGL
            string value = LoadStringFromLocalStorage(key: key);
            return int.Parse(value);
#else
            return PlayerPrefs.GetInt(key, defaultValue);
#endif
        }

        public static bool GetBool(string key, bool defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }

        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static void SetInt(string key, int value)
        {
            try
            {
#if UNITY_WEBGL
                SaveStringToLocalStorage(key: key, value: value.ToString());
#else
                PlayerPrefs.SetInt(key, value);
#endif
            }
            catch (PlayerPrefsException e)
            {
                Debug.Log("There was an issue setting PlayerPrefs int!");
            }
        }

        public static bool HasKey(string key)
        {
#if UNITY_WEBGL
            return (HasKeyInLocalStorage(key) == 1);
#else
            return PlayerPrefs.HasKey(key);
#endif
        }

        public static string GetString(string key, string defaultValue = null)
        {
#if UNITY_WEBGL
            return LoadStringFromLocalStorage(key: key);
#else
            return PlayerPrefs.GetString(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue);
#endif
        }

        public static void SetString(string key, string value)
        {
            try
            {
#if UNITY_WEBGL
                SaveStringToLocalStorage(key: key, value: value);
#else
                PlayerPrefs.SetString(key, value);
#endif
            }
            catch (PlayerPrefsException e)
            {
                Debug.Log("There was an issue setting PlayerPrefs string!");
            }
        }

        public static void Save() { PlayerPrefs.Save(); }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
#if UNITY_WEBGL
            string value = LoadStringFromLocalStorage(key: key);
            Debug.Log($"GETTING FLOAT FROM LOCAL STORAGE {float.Parse(value)}");
            return float.Parse(value);
#else
            return PlayerPrefs.GetFloat(key, defaultValue);
#endif
        }


        public static void SetFloat(string key, float value)
        {
#if UNITY_WEBGL
            Debug.Log($"SETTING FLOAT IN LOCAL STORAGE {value.ToString()}");
            SaveStringToLocalStorage(key: key, value: value.ToString());
#else
            PlayerPrefs.SetFloat(key, value);
#endif
        }


#if UNITY_WEBGL
      [DllImport("__Internal")]
      private static extern void SaveStringToLocalStorage(string key, string value);

      [DllImport("__Internal")]
      private static extern string LoadStringFromLocalStorage(string key);

      [DllImport("__Internal")]
      private static extern int HasKeyInLocalStorage(string key);
#endif
    }
}
