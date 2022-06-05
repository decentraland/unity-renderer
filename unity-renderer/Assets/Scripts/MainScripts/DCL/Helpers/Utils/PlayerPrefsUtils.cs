using UnityEngine;

namespace DCL.Helpers
{
    public static class PlayerPrefsUtils
    {
        public static int GetInt(string key) { return PlayerPrefs.GetInt(key); }

        public static int GetInt(string key, int defaultValue) { return PlayerPrefs.GetInt(key, defaultValue); }
        
        public static bool GetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public static void SetInt(string key, int value)
        {
            try
            {
                PlayerPrefs.SetInt(key, value);
            }
            catch (PlayerPrefsException e)
            {
                Debug.Log("There was an issue setting PlayerPrefs int!");
            }
        }

        public static bool HasKey(string key) { return PlayerPrefs.HasKey(key); }

        public static string GetString(string key, string defaultValue = null) { return PlayerPrefs.GetString(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue); }

        public static void SetString(string key, string value)
        {
            try
            {
                PlayerPrefs.SetString(key, value);
            }
            catch (PlayerPrefsException e)
            {
                Debug.Log("There was an issue setting PlayerPrefs string!");
            }
        }

        public static void Save() { PlayerPrefs.Save(); }

        public static float GetFloat(string key, float defaultValue = 0f) =>
            PlayerPrefs.GetFloat(key, defaultValue);

        public static void SetFloat(string key, float value) =>
            PlayerPrefs.SetFloat(key, value);
    }
}