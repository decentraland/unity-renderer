using DCL.Helpers;
using DCL.SettingsCommon;
using System.Collections.Generic;
using UnityEngine;
using AudioSettings = DCL.SettingsCommon.AudioSettings;

namespace DCLServices.EmotesService
{
    public class EmoteVolumeHandler
    {
        private const float BASE_VOLUME = 0.2f;

        private readonly List<AudioSource> audioSources = new ();
        private readonly ISettingsRepository<AudioSettings> audioSettings;

        public EmoteVolumeHandler()
        {
            audioSettings = Settings.i.audioSettings;
            audioSettings.OnChanged += OnVolumeChanged;
            OnVolumeChanged(audioSettings.Data);
        }

        public void AddAudioSource(AudioSource audioSource)
        {
            audioSources.Add(audioSource);
            SetVolume(audioSource, GetVolume(audioSettings.Data));
        }

        public void RemoveAudioSource(AudioSource audioSource)
        {
            audioSources.Remove(audioSource);
        }

        private void SetVolume(AudioSource audioSource, float volume)
        {
            audioSource.volume = volume;
        }

        private void OnVolumeChanged(AudioSettings settings)
        {
            float targetVolume = GetVolume(settings);

            foreach (AudioSource audioSource in audioSources)
                SetVolume(audioSource, targetVolume);
        }

        private static float GetVolume(AudioSettings audioSettings) =>
            BASE_VOLUME * Utils.ToVolumeCurve(audioSettings.avatarSFXVolume * audioSettings.masterVolume);
    }
}
