using System.Runtime.InteropServices;
using UnityEngine;

namespace DCL.Helpers
{
    /// <summary>
    ///     Provider class to use LocalStorage instead of IndexedDB because it was not reliably saving settings.
    ///     For all the gets, we have to check that the PlayerPref key does exist. We may lose information that was
    ///     succesfully stored in IndexedDB. That check should only go once per user per key.
    /// </summary>
    public class PlayerPrefsProviderLocalStorage : IPlayerPrefsProvider
    {
        //Nothing to save when using local storage
        public void Save() { }

        public bool HasKey(string key) =>
            HasKeyInLocalStorage(key) == 1;

        public int GetInt(string key) =>
            GetInt(key, 0);

        public int GetInt(string key, int defaultValue)
        {
            var valueGotFromLocalStorage = int.Parse(GetLocalStorageValue(key, defaultValue.ToString()));

            if (valueGotFromLocalStorage.Equals(defaultValue) && PlayerPrefs.HasKey(key))
            {
                // We get the saved value in PlayerPrefs and move it to LocalStorage
                valueGotFromLocalStorage = PlayerPrefs.GetInt(key);
                SetInt(key, valueGotFromLocalStorage);
                PlayerPrefs.DeleteKey(key);
            }

            return valueGotFromLocalStorage;
        }

        public void SetInt(string key, int value)
        {
            SaveStringToLocalStorage(key, value.ToString());
        }

        public bool GetBool(string key, bool defaultValue) =>
             GetInt(key, defaultValue ? 1 : 0) == 1;

        public void SetBool(string key, bool value) =>
            SetInt(key, value ? 1 : 0);

        public string GetString(string key, string defaultValue)
        {
            string valueGotFromLocalStorage = GetLocalStorageValue(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue);

            if (!string.IsNullOrEmpty(valueGotFromLocalStorage) && valueGotFromLocalStorage.Equals(defaultValue) && PlayerPrefs.HasKey(key))
            {
                // We get the saved value in PlayerPrefs and move it to LocalStorage
                valueGotFromLocalStorage = PlayerPrefs.GetString(key);
                SetString(key, valueGotFromLocalStorage);
                PlayerPrefs.DeleteKey(key);
            }

            return valueGotFromLocalStorage;
        }

        public void SetString(string key, string value)
        {
            SaveStringToLocalStorage(key, value);
        }

        public float GetFloat(string key, float defaultValue)
        {
            var valueGotFromLocalStorage = float.Parse(GetLocalStorageValue(key, defaultValue.ToString()));

            if (valueGotFromLocalStorage.Equals(defaultValue) && PlayerPrefs.HasKey(key))
            {
                // We get the saved value in PlayerPrefs and move it to LocalStorage
                valueGotFromLocalStorage = PlayerPrefs.GetFloat(key);
                SetFloat(key, valueGotFromLocalStorage);
                PlayerPrefs.DeleteKey(key);
            }

            return valueGotFromLocalStorage;
        }

        public void SetFloat(string key, float value)
        {
            SaveStringToLocalStorage(key, value.ToString());
        }

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
