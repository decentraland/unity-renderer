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
    public float volume = 1.0f;
    public float initialPitch = 1f;
    [Range(0f, 1f)]
    public float randomPitch = 0.0f;
    public float cooldownSeconds = 0.0f;
    [Reorderable]
    public AudioClipList clips;

    [HideInInspector]
    public AudioSource source;

    private int clipIndex;
    protected float pitch;
    private float lastPlayed, nextPlayTime; // Used for cooldown

    protected event System.Action OnPlay;

    public virtual void Initialize(AudioContainer audioContainer)
    {
        if (audioContainer == null) return;

        pitch = initialPitch;
        lastPlayed = 0f;
        nextPlayTime = 0f;
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
        
        source.volume = volume;
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
        int newIndex;
        do { newIndex = Random.Range(0, clips.Length); } while (clips.Length > 1 && newIndex == clipIndex);
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

        RandomizeIndex();

        lastPlayed = Time.time;
        nextPlayTime = lastPlayed + cooldownSeconds;

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

        RandomizeIndex();

        lastPlayed = Time.time;
        nextPlayTime = lastPlayed + cooldownSeconds;
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

    public IEnumerator FadeOut(float fadeSeconds)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * (Time.deltaTime / fadeSeconds);
            yield return null;
        }

        source.Stop();
        source.volume = volume;
    }
}