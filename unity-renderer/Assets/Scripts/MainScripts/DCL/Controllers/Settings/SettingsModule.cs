using System;
using DCL.Helpers;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class SettingsModule<T> : ISettingsRepository<T> where T : struct
    {
        public event Action<T> OnChanged;

        private readonly string playerPrefsKey;
        private readonly T defaultPreset;

        public T Data => dataValue;
        private T dataValue;

        public SettingsModule(string playerPrefsKey, T defaultPreset)
        {
            this.playerPrefsKey = playerPrefsKey;
            this.defaultPreset = defaultPreset;
            Preload();
        }

        private void Preload()
        {
            dataValue = defaultPreset;
            if (!PlayerPrefsBridge.HasKey(playerPrefsKey))
                return;

            try
            {
                dataValue = JsonUtility.FromJson<T>(PlayerPrefsBridge.GetString(playerPrefsKey));
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public void Reset() { Apply(defaultPreset); }

        public void Apply(T newSettings)
        {
            if (dataValue.Equals(newSettings))
                return;

            dataValue = newSettings;
            OnChanged?.Invoke(dataValue);
        }

        public void Save() { PlayerPrefsBridge.SetString(playerPrefsKey, JsonUtility.ToJson(dataValue)); }

        public bool HasAnyData() => !Data.Equals(defaultPreset);
    }
}