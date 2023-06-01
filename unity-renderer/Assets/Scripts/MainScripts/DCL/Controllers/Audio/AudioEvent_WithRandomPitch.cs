using UnityEngine;

[CreateAssetMenu(fileName = "AudioEventWithRandomPitch", menuName = "AudioEvents/AudioEvent - With random pitch")]
public class AudioEvent_WithRandomPitch : AudioEvent
{
    [SerializeField]
    float pitchMaxVariation;

    public override void Initialize(AudioContainer audioContainer)
    {
        base.Initialize(audioContainer);
        OnPlay += OnEventPlay;
    }

    void OnEventPlay() { SetPitch(pitch + Random.Range(-pitchMaxVariation, pitchMaxVariation)); }
}
