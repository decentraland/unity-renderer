using UnityEngine;

[CreateAssetMenu(fileName = "AudioEventWithBlockingEvent", menuName = "AudioEvents/AudioEvent - With blocking event")]
public class AudioEvent_WithBlockingEvent : AudioEvent
{
    [SerializeField]
    AudioEvent blockingEvent;

    public override void Play(bool oneShot = false)
    {
        if (blockingEvent == null) return;
        if (blockingEvent.source == null) return;

        if (!blockingEvent.source.isPlaying)
            base.Play(oneShot);
    }
}
