using UnityEngine;
using UnityEngine.Networking;

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

    public class WebRequestAudio : IWebRequestAudio
    {
        private AudioType audioType = AudioType.UNKNOWN;

        public void SetAudioType(AudioType audioType) { this.audioType = audioType; }

        public UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestMultimedia.GetAudioClip(url, audioType); }
    }
}