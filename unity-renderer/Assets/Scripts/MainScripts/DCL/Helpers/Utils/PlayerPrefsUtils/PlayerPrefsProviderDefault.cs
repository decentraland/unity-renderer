using UnityEngine;

namespace DCL.Helpers
{
    public class PlayerPrefsProviderDefault : IPlayerPrefsProvider
    {
        public void Save()
        {
            PlayerPrefs.Save();
        }

        public bool HasKey(string key) =>
            PlayerPrefs.HasKey(key);

        public int GetInt(string key) =>
            PlayerPrefs.GetInt(key);

        public int GetInt(string key, int defaultValue) =>
            PlayerPrefs.GetInt(key, defaultValue);

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public bool GetBool(string key, bool defaultValue) =>
            PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }

        public string GetString(string key, string defaultValue) =>
            PlayerPrefs.GetString(key, string.IsNullOrEmpty(defaultValue) ? "" : defaultValue);

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public float GetFloat(string key, float defaultValue) =>
            PlayerPrefs.GetFloat(key, defaultValue);

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
    }
}
