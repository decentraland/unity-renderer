using UnityEngine;

namespace DCL
{
    public class Asset_AudioClip : Asset
    {
        public AudioClip audioClip { get; internal set; }

        public override void Cleanup()
        {
            if (audioClip == null)
                return;

            audioClip.UnloadAudioData();
            Object.Destroy(audioClip);
            audioClip = null;
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}