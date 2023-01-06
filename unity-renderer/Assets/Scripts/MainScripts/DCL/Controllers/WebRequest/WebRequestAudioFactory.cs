using UnityEngine;
using UnityEngine.Networking;

namespace DCL
{
    public class WebRequestAudioFactory : IWebRequestAudioFactory
    {
        private AudioType audioType = AudioType.UNKNOWN;

        public void SetAudioType(AudioType audioType) =>
            this.audioType = audioType;

        public UnityWebRequest CreateWebRequest(string url) =>
            UnityWebRequestMultimedia.GetAudioClip(url, audioType);
    }
}
