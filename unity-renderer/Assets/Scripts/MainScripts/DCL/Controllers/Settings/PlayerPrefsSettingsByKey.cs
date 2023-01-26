using System;
using DCL.Helpers;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class PlayerPrefsSettingsByKey : IPlayerPrefsSettingsByKey
    {
        private readonly string prefixPrefsKey;

        public PlayerPrefsSettingsByKey(string prefixPrefsKey)
        {
            this.prefixPrefsKey = prefixPrefsKey;
        }

        public T GetEnum<T>(string fieldName, T defaultValue) where T : struct
        {
            if (!Enum.TryParse<T>(PlayerPrefs.GetString(GetFieldKey(fieldName), ""), out var result))
                return defaultValue;
            return result;
        }

        public bool GetBool(string fieldName, bool defaultValue)
        {
            return PlayerPrefsUtils.GetBool(GetFieldKey(fieldName), defaultValue);
        }

        public float GetFloat(string fieldName, float defaultValue)
        {
            if (fieldName.Equals("SCENES_LOAD_RADIUS"))
            {
                Debug.Log($"THE VALUE HAS_KEY FOR SCENE_LOAD_RADIUS IS {PlayerPrefs.HasKey(GetFieldKey(fieldName))}");
                Debug.Log($"THE VALUE FOR SCENE_LOAD_RADIUS IS {PlayerPrefs.GetFloat(GetFieldKey(fieldName))}");

            }
            return PlayerPrefs.GetFloat(GetFieldKey(fieldName), defaultValue);
        }

        public int GetInt(string fieldName, int defaultValue)
        {
            return PlayerPrefs.GetInt(GetFieldKey(fieldName), defaultValue);
        }

        public string GetString(string fieldName, string defaultValue)
        {
            return PlayerPrefs.GetString(GetFieldKey(fieldName), defaultValue);
        }

        public void SetBool(string fieldName, bool value)
        {
            PlayerPrefsUtils.SetBool(GetFieldKey(fieldName), value);
        }

        public void SetFloat(string fieldName, float value)
        {
            PlayerPrefs.SetFloat(GetFieldKey(fieldName), value);
        }

        public void SetEnum<T>(string fieldName, T value) where T : struct
        {
            PlayerPrefs.SetString(GetFieldKey(fieldName), value.ToString());
        }

        public void SetInt(string fieldName, int value)
        {
            PlayerPrefs.SetInt(GetFieldKey(fieldName), value);
        }

        public void SetString(string fieldName, string value)
        {
            PlayerPrefs.SetString(GetFieldKey(fieldName), value);
        }

        private string GetFieldKey(string fieldName)
        {
            return $"{prefixPrefsKey}.{fieldName}";
        }
    }
}
