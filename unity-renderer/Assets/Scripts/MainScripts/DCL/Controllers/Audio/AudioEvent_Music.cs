using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "AudioEvent - Music", menuName = "AudioEvents/AudioEvent - Music")]
public class AudioEvent_Music : AudioEvent
{
    [SerializeField]
    double loopStart, loopEnd;


}