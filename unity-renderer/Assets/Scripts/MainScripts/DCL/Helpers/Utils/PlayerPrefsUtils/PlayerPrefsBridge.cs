using UnityEngine;

namespace DCL.Helpers
{
    public static class PlayerPrefsBridge
    {
        private static readonly IPlayerPrefsProvider PROVIDER = Application.platform.Equals(RuntimePlatform.WebGLPlayer)
            ? new PlayerPrefsProviderLocalStorage()
            : new PlayerPrefsProviderDefault();

        public static int GetInt(string key) =>
            PROVIDER.GetInt(key);

        public static int GetInt(string key, int defaultValue) =>
            PROVIDER.GetInt(key, defaultValue);

        public static bool GetBool(string key, bool defaultValue) =>
            PROVIDER.GetBool(key, defaultValue);

        public static void SetBool(string key, bool value)
        {
            PROVIDER.SetBool(key, value);
        }

        public static void SetInt(string key, int value)
        {
            try { PROVIDER.SetInt(key, value); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs int!"); }
        }

        public static bool HasKey(string key) =>
            PROVIDER.HasKey(key);

        public static string GetString(string key, string defaultValue = null) =>
            PROVIDER.GetString(key, defaultValue);

        public static void SetString(string key, string value)
        {
            try { PROVIDER.SetString(key, value); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs string!"); }
        }

        public static void Save()
        {
            PROVIDER.Save();
        }

        public static float GetFloat(string key, float defaultValue = 0f) =>
            PROVIDER.GetFloat(key, defaultValue);

        public static void SetFloat(string key, float value)
        {
            try { PROVIDER.SetFloat(key, value); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs float!"); }
        }
    }
}
