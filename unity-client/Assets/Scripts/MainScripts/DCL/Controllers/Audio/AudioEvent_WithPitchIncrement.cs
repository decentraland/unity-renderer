using UnityEngine;

[CreateAssetMenu(fileName = "AudioEventWithPitchIncrement", menuName = "AudioEvents/AudioEvent - With pitch increment")]
public class AudioEvent_WithPitchIncrement : AudioEvent
{
    [SerializeField]
    float pitchIncrement;

    public override void Initialize(AudioContainer audioContainer)
    {
        base.Initialize(audioContainer);
        OnPlay += OnEventPlay;
    }

    void OnEventPlay()
    {
        SetPitch(pitch + pitchIncrement);
    }
}