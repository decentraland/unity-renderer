using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRendererAudioHandler : MonoBehaviour
{
    [SerializeField]
    DCL.MapRenderer mapRenderer;

    [SerializeField]
    AudioEvent eventMapParcelHighlight;

    private void Awake() { mapRenderer.OnMovedParcelCursor += OnMovedParcelCursor; }

    public void OnMovedParcelCursor(float x, float y) { eventMapParcelHighlight.Play(true); }
}