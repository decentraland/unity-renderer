using UnityEngine;
using UnityEngine.Audio;
using ReorderableList;

public class AudioContainer : MonoBehaviour
{
    [System.Serializable]
    public class AudioEventList : ReorderableArray<AudioEvent>
    {
    }

    public string identifier;

    [Header("General")]
    public AudioMixerGroup audioMixerGroup;

    [Header("3D")]
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
            if (e.clips.Length == 0)
            {
                Debug.LogWarning("There are no clips in the audio event '" + e.name + "' (" + name + " -> " + identifier + ")");
                continue;
            }

            e.Initialize();

            // Set up AudioSource component for event
            e.source = gameObject.AddComponent(typeof(AudioSource)) as AudioSource;

            e.source.clip = e.clips[0];
            e.source.outputAudioMixerGroup = audioMixerGroup;

            e.source.volume = e.volume;
            e.source.loop = e.loop;
            e.source.spatialBlend = spatialBlend;

            e.source.dopplerLevel = dopplerLevel;
            e.source.minDistance = minDistance;
            e.source.maxDistance = maxDistance;

            e.source.playOnAwake = false;

            if (e.playOnAwake)
            {
                e.Play();
            }
        }
    }

    public AudioEvent GetEvent(string name)
    {
        AudioEvent e = null;
        foreach (AudioEvent i in audioEvents)
        {
            if (i.name == name)
            {
                e = i;
            }
        }
        if (e == null)
        {
            Debug.LogWarning(this.name + " -> AudioContainer (" + identifier + "): Couldn't find audio event: " + name);
        }
        return e;
    }
}