using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ECSAudioSource 
{
    public bool playing = false;
    public float volume = 1f;
    public bool loop = false;
    public float pitch = 1f;
    public long playedAtTimestamp = 0;

    // This is the model of the audio clip
    public string audioClipUrl;
}
