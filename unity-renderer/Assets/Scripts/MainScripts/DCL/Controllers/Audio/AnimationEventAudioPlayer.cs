using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventAudioPlayer : MonoBehaviour
{
    public void PlayAudioEvent(AudioEvent audioEvent) { audioEvent.Play(); }
}