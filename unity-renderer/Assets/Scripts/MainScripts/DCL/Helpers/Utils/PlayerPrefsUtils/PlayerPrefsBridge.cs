using System;
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
            SetPlayerPref(() => PROVIDER.SetBool(key, value), key, "bool");
        }

        public static void SetInt(string key, int value)
        {
            SetPlayerPref(() => PROVIDER.SetInt(key, value), key, "int");
        }

        public static bool HasKey(string key) =>
            PROVIDER.HasKey(key);

        public static string GetString(string key, string defaultValue = null) =>
            PROVIDER.GetString(key, defaultValue);

        public static void SetString(string key, string value)
        {
            SetPlayerPref(() => PROVIDER.SetString(key, value), key, "string");
        }

        public static void Save()
        {
            PROVIDER.Save();
        }

        public static float GetFloat(string key, float defaultValue = 0f) =>
            PROVIDER.GetFloat(key, defaultValue);

        public static void SetFloat(string key, float value)
        {
            SetPlayerPref(() => PROVIDER.SetFloat(key, value), key, "float");
        }

        private static void SetPlayerPref(Action setFunc, string key, string typeName)
        {
            try { setFunc(); }
            catch (PlayerPrefsException e) { Debug.Log($"There was an issue setting {key} PlayerPrefs {typeName}!"); }
        }
    }
}
