using System;
using DCL.Helpers;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class SettingsModule<T> where T : struct
    {
        public event Action<T> OnChanged;
        
        private readonly string playerPrefsKey;
        private readonly T defaultPreset;

        public T Data => data;
        private T data;

        public SettingsModule(string playerPrefsKey, T defaultPreset)
        {
            this.playerPrefsKey = playerPrefsKey;
            this.defaultPreset = defaultPreset;
            Preload();
        }
        private void Preload()
        {
            bool isQualitySettingsSet = false;
            if (PlayerPrefsUtils.HasKey(playerPrefsKey))
            {
                try
                {
                    data = JsonUtility.FromJson<T>(PlayerPrefsUtils.GetString(playerPrefsKey));
                    isQualitySettingsSet = true;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }

            if (!isQualitySettingsSet)
            {
                data = defaultPreset;
            }
        }

        public void Reset()
        {
            Apply(defaultPreset);
        }

        public void Apply(T newSettings)
        {
            if (data.Equals(newSettings))
                return;

            data = newSettings;
            OnChanged?.Invoke(data);
        }

        public void Save()
        {
            PlayerPrefsUtils.SetString(playerPrefsKey, JsonUtility.ToJson(data));
        }
    }
}