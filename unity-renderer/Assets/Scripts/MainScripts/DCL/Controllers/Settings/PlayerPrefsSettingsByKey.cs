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
            if (!Enum.TryParse<T>(PlayerPrefsBridge.GetString(GetFieldKey(fieldName), ""), out var result))
                return defaultValue;
            return result;
        }

        public bool GetBool(string fieldName, bool defaultValue)
        {
            return PlayerPrefsBridge.GetBool(GetFieldKey(fieldName), defaultValue);
        }

        public float GetFloat(string fieldName, float defaultValue)
        {
            return PlayerPrefsBridge.GetFloat(GetFieldKey(fieldName), defaultValue);
        }

        public int GetInt(string fieldName, int defaultValue)
        {
            return PlayerPrefsBridge.GetInt(GetFieldKey(fieldName), defaultValue);
        }

        public string GetString(string fieldName, string defaultValue)
        {
            return PlayerPrefsBridge.GetString(GetFieldKey(fieldName), defaultValue);
        }

        public void SetBool(string fieldName, bool value)
        {
            PlayerPrefsBridge.SetBool(GetFieldKey(fieldName), value);
        }

        public void SetFloat(string fieldName, float value)
        {
            PlayerPrefsBridge.SetFloat(GetFieldKey(fieldName), value);
        }

        public void SetEnum<T>(string fieldName, T value) where T : struct
        {
            PlayerPrefsBridge.SetString(GetFieldKey(fieldName), value.ToString());
        }

        public void SetInt(string fieldName, int value)
        {
            PlayerPrefsBridge.SetInt(GetFieldKey(fieldName), value);
        }

        public void SetString(string fieldName, string value)
        {
            PlayerPrefsBridge.SetString(GetFieldKey(fieldName), value);
        }

        private string GetFieldKey(string fieldName)
        {
            return $"{prefixPrefsKey}.{fieldName}";
        }
    }
}
