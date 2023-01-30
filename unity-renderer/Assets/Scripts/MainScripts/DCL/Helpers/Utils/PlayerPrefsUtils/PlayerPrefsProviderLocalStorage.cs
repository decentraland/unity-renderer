using System.Runtime.InteropServices;
using UnityEngine;

namespace DCL.Helpers
{
    public class PlayerPrefsProviderLocalStorage : IPlayerPrefsProvider
    {
        public int GetInt(string key) =>
            int.Parse(GetLocalStorageValue(key, "0"));

        public int GetInt(string key, int defaultValue) =>
            int.Parse(GetLocalStorageValue(key, defaultValue.ToString()));

        public void SetInt(string key, int value)
        {
            SaveStringToLocalStorage(key, value.ToString());
        }

        public bool GetBool(string key, bool defaultValue) =>
            GetInt(key, defaultValue ? 1 : 0) == 1;

        public void SetBool(string key, bool value) =>
            SetInt(key, value ? 1 : 0);

        public string GetString(string key, string defaultValue) =>
            GetLocalStorageValue(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue);

        public void SetString(string key, string value)
        {
            SaveStringToLocalStorage(key, value);
        }

        //Nothing to save when using local storage
        public void Save() { }

        public float GetFloat(string key, float defaultValue)
        {
            Debug.Log($"GETTING FLOAT FROM LOCAL STORAGE {float.Parse(GetLocalStorageValue(key, defaultValue.ToString()))}");
            return float.Parse(GetLocalStorageValue(key, defaultValue.ToString()));
        }

        public void SetFloat(string key, float value)
        {
            Debug.Log($"SETTING FLOAT IN LOCAL STORAGE {value.ToString()}");
            SaveStringToLocalStorage(key, value.ToString());
        }

        public bool HasKey(string key) =>
            HasKeyInLocalStorage(key) == 1;

        private string GetLocalStorageValue(string key, string defaultValue)
        {
            if (HasKey(key))
                return LoadStringFromLocalStorage(key);

            return defaultValue;
        }

#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void SaveStringToLocalStorage(string key, string value);

        [DllImport("__Internal")]
        private static extern string LoadStringFromLocalStorage(string key);

        [DllImport("__Internal")]
        private static extern int HasKeyInLocalStorage(string key);
#else
    private static void SaveStringToLocalStorage(string key, string value) { }

    private static string LoadStringFromLocalStorage(string key) =>
        "";

    private static int HasKeyInLocalStorage(string key) =>
        0;
#endif
    }
}
