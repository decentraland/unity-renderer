using System;
using DCL.Helpers;
using UnityEngine;

namespace DCL.SettingsCommon
{
    public class SettingsModule<T> where T : ICloneable
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
            dataValue = (T) defaultPreset.Clone();
            if (!PlayerPrefsUtils.HasKey(playerPrefsKey))
                return;

            try
            {
                JsonUtility.FromJsonOverwrite(PlayerPrefsUtils.GetString(playerPrefsKey), dataValue);
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

            dataValue = (T) newSettings.Clone();
            OnChanged?.Invoke(dataValue);
        }

        public void Save() { PlayerPrefsUtils.SetString(playerPrefsKey, JsonUtility.ToJson(dataValue)); }
    }
}