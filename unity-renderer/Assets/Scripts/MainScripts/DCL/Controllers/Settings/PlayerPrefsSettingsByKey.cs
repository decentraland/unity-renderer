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
            if (!Enum.TryParse<T>(PlayerPrefsUtils.GetString(GetFieldKey(fieldName), ""), out var result))
                return defaultValue;
            return result;
        }

        public bool GetBool(string fieldName, bool defaultValue)
        {
            return PlayerPrefsUtils.GetBool(GetFieldKey(fieldName), defaultValue);
        }

        public float GetFloat(string fieldName, float defaultValue)
        {
            return PlayerPrefsUtils.GetFloat(GetFieldKey(fieldName), defaultValue);
        }

        public int GetInt(string fieldName, int defaultValue)
        {
            return PlayerPrefsUtils.GetInt(GetFieldKey(fieldName), defaultValue);
        }

        public string GetString(string fieldName, string defaultValue)
        {
            return PlayerPrefsUtils.GetString(GetFieldKey(fieldName), defaultValue);
        }

        public void SetBool(string fieldName, bool value)
        {
            PlayerPrefsUtils.SetBool(GetFieldKey(fieldName), value);
        }

        public void SetFloat(string fieldName, float value)
        {
            PlayerPrefsUtils.SetFloat(GetFieldKey(fieldName), value);
        }

        public void SetEnum<T>(string fieldName, T value) where T : struct
        {
            PlayerPrefsUtils.SetString(GetFieldKey(fieldName), value.ToString());
        }

        public void SetInt(string fieldName, int value)
        {
            PlayerPrefsUtils.SetInt(GetFieldKey(fieldName), value);
        }

        public void SetString(string fieldName, string value)
        {
            PlayerPrefsUtils.SetString(GetFieldKey(fieldName), value);
        }

        private string GetFieldKey(string fieldName)
        {
            return $"{prefixPrefsKey}.{fieldName}";
        }
    }
}
