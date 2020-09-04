using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRendererAudioHandler : MonoBehaviour
{
    AudioEvent eventMapParcelHighlight;

    private void Awake()
    {
        AudioContainer ac = GetComponent<AudioContainer>();
        eventMapParcelHighlight = ac.GetEvent("MapParcelHighlight");
        eventMapParcelHighlight.SetPitch(4f);
    }

    public void PlayMapParcelHighlight()
    {
        eventMapParcelHighlight.Play(true);
    }
}
