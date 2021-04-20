using UnityEngine;
using UnityEngine.Audio;
using ReorderableList;
using System.Collections.Generic;

public class AudioContainer : MonoBehaviour
{
    [System.Serializable]
    public class AudioEventList : ReorderableArray<AudioEvent>
    {
    }

    public bool instantiateEvents = false;
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

        if (instantiateEvents)
        {
            for(int i = 0; i < audioEvents.Count; i++)
            {
                string str = audioEvents[i].name;
                AudioEvent instance = Instantiate(audioEvents[i]);
                audioEvents[i] = instance;
                instance.name = str;
            }
        }

        foreach (AudioEvent e in audioEvents)
        {
            e.Initialize(this);
        }
    }

    public AudioEvent GetEvent(string eventName)
    {
        for (int i = 0; i < audioEvents.Count; i++)
        {
            if (audioEvents[i].name == eventName)
                return audioEvents[i];
        }

        Debug.Log($"{name}'s AudioContainer couldn't find an event called {eventName}");
        return null;
    }
}