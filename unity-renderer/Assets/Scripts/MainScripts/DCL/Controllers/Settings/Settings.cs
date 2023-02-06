using System;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine.Audio;

namespace DCL.SettingsCommon
{
    public class Settings
    {
        public static Settings i { get; private set; }

        public event Action OnResetAllSettings;
        
        public QualitySettingsData qualitySettingsPresets => qualitySettingsPreset;

        public readonly ISettingsRepository<QualitySettings> qualitySettings;
        public readonly ISettingsRepository<GeneralSettings> generalSettings;
        public readonly ISettingsRepository<AudioSettings> audioSettings;
        
        private readonly QualitySettingsData qualitySettingsPreset;
        private readonly AudioMixer audioMixer;

        private bool isDisposed;

        public static void CreateSharedInstance(ISettingsFactory settingsFactory)
        {
            if (i != null && !i.isDisposed) return;
            i = settingsFactory.Build();
        }

        public Settings(QualitySettingsData qualitySettingsPreset,
            AudioMixer audioMixer,
            ISettingsRepository<QualitySettings> graphicsQualitySettingsRepository,
            ISettingsRepository<GeneralSettings> generalSettingsRepository,
            ISettingsRepository<AudioSettings> audioSettingsRepository)
        {
            this.qualitySettingsPreset = qualitySettingsPreset;
            this.audioMixer = audioMixer;
            qualitySettings = graphicsQualitySettingsRepository;
            generalSettings = generalSettingsRepository;
            audioSettings = audioSettingsRepository;

            SubscribeToVirtualAudioMixerEvents();
        }

        public void Dispose()
        {
            UnsubscribeFromVirtualAudioMixerEvents();
            isDisposed = true;
        }

        public void LoadDefaultSettings()
        {
            qualitySettings.Reset();
            generalSettings.Reset();
            audioSettings.Reset();
        }

        public void ResetAllSettings()
        {
            LoadDefaultSettings();
            SaveSettings();
            OnResetAllSettings?.Invoke();
        }

        private void SubscribeToVirtualAudioMixerEvents()
        {
            DataStore.i.virtualAudioMixer.voiceChatVolume.OnChange += ApplyVoiceChatSettings;
            DataStore.i.virtualAudioMixer.musicVolume.OnChange += ApplyMusicVolume;
            DataStore.i.virtualAudioMixer.avatarSFXVolume.OnChange += ApplyAvatarSFXVolume;
            DataStore.i.virtualAudioMixer.uiSFXVolume.OnChange += ApplyUISFXVolume;
        }

        private void UnsubscribeFromVirtualAudioMixerEvents()
        {
            DataStore.i.virtualAudioMixer.voiceChatVolume.OnChange -= ApplyVoiceChatSettings;
            DataStore.i.virtualAudioMixer.musicVolume.OnChange -= ApplyMusicVolume;
            DataStore.i.virtualAudioMixer.avatarSFXVolume.OnChange -= ApplyAvatarSFXVolume;
            DataStore.i.virtualAudioMixer.uiSFXVolume.OnChange -= ApplyUISFXVolume;
        }

        public void ApplyMasterVolume()
        {
            // Update the "All" mixer group
            audioMixer.SetFloat("AllBusVolume", Utils.ToAudioMixerGroupVolume(audioSettings.Data.masterVolume));

            // Update voice chat volume, as it does not pass through the AudioMixer
            ApplyVoiceChatSettings();
        }

        public void ApplyVoiceChatSettings(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f)
        {
            AudioSettings audioSettingsData = audioSettings.Data;
            float calculatedVolume = Utils.ToVolumeCurve(DataStore.i.virtualAudioMixer.voiceChatVolume.Get() * audioSettingsData.voiceChatVolume * audioSettingsData.masterVolume);
            WebInterface.ApplySettings(calculatedVolume, (int)generalSettings.Data.voiceChatAllow);
        }

        public void ApplyAvatarSFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("AvatarSFXBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.avatarSFXVolume.Get() * audioSettings.Data.avatarSFXVolume)); }

        public void ApplyUISFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("UIBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * audioSettings.Data.uiSFXVolume)); }

        public void ApplyMusicVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("MusicBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * audioSettings.Data.musicVolume)); }

        public void SaveSettings()
        {
            generalSettings.Save();
            qualitySettings.Save();
            audioSettings.Save();
            PlayerPrefsBridge.Save();
        }
    }
}