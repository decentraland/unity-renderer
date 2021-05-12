using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class WebRequestAudio : IWebRequestAudio
    {
        private AudioType audioType = AudioType.UNKNOWN;

        public void SetAudioType(AudioType audioType) { this.audioType = audioType; }

        public UnityWebRequest CreateWebRequest(string url) { return UnityWebRequestMultimedia.GetAudioClip(url, audioType); }
    }
}