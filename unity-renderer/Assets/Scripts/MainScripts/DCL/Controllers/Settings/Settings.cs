using System;
using DCL.Helpers;
using DCL.Interface;
using UnityEngine.Audio;

namespace DCL.SettingsCommon
{
    public class Settings
    {
        private readonly AudioMixer audioMixer;
        public readonly ISettingsRepository<AudioSettings> audioSettings;
        public readonly ISettingsRepository<GeneralSettings> generalSettings;

        public readonly ISettingsRepository<QualitySettings> qualitySettings;

        private bool isDisposed;

        public Settings(QualitySettingsData qualitySettingsPreset,
            AudioMixer audioMixer,
            ISettingsRepository<QualitySettings> graphicsQualitySettingsRepository,
            ISettingsRepository<GeneralSettings> generalSettingsRepository,
            ISettingsRepository<AudioSettings> audioSettingsRepository)
        {
            qualitySettingsPresets = qualitySettingsPreset;
            this.audioMixer = audioMixer;
            qualitySettings = graphicsQualitySettingsRepository;
            generalSettings = generalSettingsRepository;
            audioSettings = audioSettingsRepository;

            SubscribeToVirtualAudioMixerEvents();
        }
        public static Settings i { get; private set; }

        public QualitySettingsData qualitySettingsPresets { get ; }

        public event Action OnResetAllSettings;

        public static void CreateSharedInstance(ISettingsFactory settingsFactory)
        {
            if (i != null && !i.isDisposed)
                return;
            i = settingsFactory.Build();
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

        public void ChangeAudioDevicesSettings() =>
            WebInterface.ChangeAudioDevice(audioSettings.Data.outputDevice, audioSettings.Data.inputDevice);

        public void ApplyAvatarSFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("AvatarSFXBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.avatarSFXVolume.Get() * audioSettings.Data.avatarSFXVolume)); }

        public void ApplyUISFXVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("UIBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * audioSettings.Data.uiSFXVolume)); }

        public void ApplyMusicVolume(float currentDataStoreVolume = 0f, float previousDataStoreVolume = 0f) { audioMixer.SetFloat("MusicBusVolume", Utils.ToAudioMixerGroupVolume(DataStore.i.virtualAudioMixer.uiSFXVolume.Get() * audioSettings.Data.musicVolume)); }

        public void SaveSettings()
        {
            generalSettings.Save();
            qualitySettings.Save();
            audioSettings.Save();
            PlayerPrefsUtils.Save();
        }
    }
}