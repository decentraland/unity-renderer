using System.Collections;
using UnityEngine;
using ReorderableList;

[System.Serializable, CreateAssetMenu(fileName = "AudioEvent", menuName = "AudioEvents/AudioEvent")]
public class AudioEvent : ScriptableObject
{
    [System.Serializable]
    public class AudioClipList : ReorderableArray<AudioClip>
    {
    }

    public bool loop = false;
    [Range(0f, 1f)]
    public float initialVolume = 1.0f;
    public float initialPitch = 1f;
    [Range(0f, 1f)]
    public float randomPitch = 0.0f;
    public float cooldownSeconds = 0.0f;
    [Reorderable]
    public AudioClipList clips;

    [HideInInspector]
    public AudioSource source;

    private int clipIndex, lastPlayedIndex;
    protected float pitch;
    private float lastPlayedTime, nextPlayTime; // Used for cooldown

    protected event System.Action OnPlay;

    public virtual void Initialize(AudioContainer audioContainer)
    {
        if (audioContainer == null) return;

        pitch = initialPitch;
        lastPlayedTime = 0f;
        nextPlayTime = 0f;
        lastPlayedIndex = -1;
        RandomizeIndex();

        // Add AudioSource component for event
        source = audioContainer.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;

        if (clips.Length == 0)
        {
            Debug.LogWarning("There are no clips in the audio event '" + name + "' (" + audioContainer.name + ")");
        }
        else
        {
            source.clip = clips[0];
        }
        
        source.volume = initialVolume;
        source.loop = loop;
        source.playOnAwake = false;

        source.outputAudioMixerGroup = audioContainer.audioMixerGroup;
        source.spatialBlend = audioContainer.spatialBlend;
        source.dopplerLevel = audioContainer.dopplerLevel;
        source.minDistance = audioContainer.minDistance;
        source.maxDistance = audioContainer.maxDistance;
    }

    
    public void RandomizeIndex()
    {
        RandomizeIndex(0, clips.Length);
    }

    // Randomize the index from (inclusive) to y (exclusive)
    public void RandomizeIndex(int from, int to)
    {
        int newIndex;
        do { newIndex = Random.Range(from, to); } while (clips.Length > 1 && newIndex == lastPlayedIndex);
        clipIndex = newIndex;
    }

    public virtual void Play(bool oneShot = false)
    {
        if (source == null) { Debug.Log("AudioEvent: Tried to play " + name + " with source equal to null."); return; }

        // Check if AudioSource is active and check cooldown time
        if (!source.gameObject.activeSelf || Time.time < nextPlayTime) return;

        source.clip = clips[clipIndex];
        source.pitch = pitch + Random.Range(0f, randomPitch) - (randomPitch * 0.5f);

        // Play
        if (oneShot)
            source.PlayOneShot(source.clip);
        else
            source.Play();

        lastPlayedIndex = clipIndex;
        RandomizeIndex();

        lastPlayedTime = Time.time;
        nextPlayTime = lastPlayedTime + cooldownSeconds;

        OnPlay?.Invoke();
    }

    public void PlayScheduled(float delaySeconds)
    {
        if (source == null) return;

        // Check if AudioSource is active and check cooldown time (taking delay into account)
        if (!source.gameObject.activeSelf || Time.time + delaySeconds < nextPlayTime) return;

        source.clip = clips[clipIndex];
        source.pitch = pitch + Random.Range(0f, randomPitch) - (randomPitch * 0.5f);
        source.PlayScheduled(AudioSettings.dspTime + delaySeconds);

        lastPlayedIndex = clipIndex;
        RandomizeIndex();

        lastPlayedTime = Time.time + delaySeconds;
        nextPlayTime = lastPlayedTime + cooldownSeconds;
    }

    public void Stop()
    {
        source.Stop();
    }

    public void SetIndex(int index)
    {
        this.clipIndex = index;
    }

    public void SetPitch(float pitch)
    {
        this.pitch = pitch;
    }

    public IEnumerator FadeIn(float fadeSeconds)
    {
        float startVolume = source.volume;

        while (source.volume < initialVolume)
        {
            source.volume += (initialVolume - startVolume) * (Time.deltaTime / fadeSeconds);
            yield return null;
        }

        source.volume = initialVolume;
    }

    public IEnumerator FadeOut(float fadeSeconds)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * (Time.deltaTime / fadeSeconds);
            yield return null;
        }

        source.Stop();
        source.volume = initialVolume;
    }
}