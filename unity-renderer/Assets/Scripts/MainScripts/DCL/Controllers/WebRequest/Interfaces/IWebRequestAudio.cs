using UnityEngine;

namespace DCL
{
    /// <summary>
    /// Our custom implementation of the UnityWebRequestMultimedia (Audio Clip).
    /// </summary>
    public interface IWebRequestAudio : IWebRequest
    {
        /// <summary>
        /// Configure the type of the audio that will be requested.
        /// </summary>
        /// <param name="audioType">Audio type.</param>
        void SetAudioType(AudioType audioType);
    }
}