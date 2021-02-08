using UnityEngine;
using UnityEngine.Audio;
using ReorderableList;

public class AudioContainer : MonoBehaviour
{
    [System.Serializable]
    public class AudioEventList : ReorderableArray<AudioEvent>
    {
    }

    public AudioMixerGroup audioMixerGroup;
    [Range(0f, 1f)]
    public float spatialBlend = 1f;
    public bool overrideDefaults = false;
    public float dopplerLevel = 1f;
    public float minDistance = 1;
    public float maxDistance = 500;

    [Reorderable]
    public AudioEventList audioEvents;

    void Awake()
    {
        if (!overrideDefaults)
        {
            dopplerLevel = 0.0f;
            minDistance = 1f;
            maxDistance = 400f;
        }

        foreach (AudioEvent e in audioEvents)
        {
            e.Initialize(this);
        }
    }
}