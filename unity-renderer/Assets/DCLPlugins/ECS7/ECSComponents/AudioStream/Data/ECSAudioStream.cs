using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.ECSComponents
{
    public class ECSAudioStream
    {
        public string url;
        public bool playing = false;
        public float volume = 1;

        // This is the model of the audio clip
        public string audioClipUrl;
    }
}
