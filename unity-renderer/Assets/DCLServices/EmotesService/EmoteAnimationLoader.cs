using AvatarSystem;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteAnimationLoader : IEmoteAnimationLoader
    {
        private readonly IWearableRetriever retriever;
        public AnimationClip mainClip { get; internal set; }
        public GameObject container { get; private set; }
        public AudioSource audioSource { get; private set; }

        private AudioClip audioClip;

        public EmoteAnimationLoader(IWearableRetriever retriever)
        {
            this.retriever = retriever;
        }

        public async UniTask LoadEmote(GameObject targetContainer, WearableItem emote, string bodyShapeId, CancellationToken ct = default)
        {
            if (targetContainer == null)
                throw new NullReferenceException("Container cannot be null");

            if (emote == null)
                throw new NullReferenceException("Emote cannot be null");

            if (string.IsNullOrEmpty(bodyShapeId))
                throw new NullReferenceException("bodyShapeId cannot be null or empty");

            ct.ThrowIfCancellationRequested();

            Rendereable rendereable = await retriever.Retrieve(targetContainer, emote, bodyShapeId, ct);

            var animation = rendereable.container.GetComponentInChildren<Animation>();

            if (animation == null)
            {
                Debug.LogError("Animation component not found in the container for emote " + emote.id);
                return;
            }

            if (animation.GetClipCount() > 1) { this.container = rendereable.container; }

            animation.enabled = false;
            var animationClip = animation.clip;

            if (animationClip == null)
            {
                Debug.LogError("AnimationClip not found in the container for emote " + emote.id);
                return;
            }

            //Setting animation name equal to emote id to avoid unity animation clip duplication on Animation.AddClip()
            this.mainClip = animationClip;
            animationClip.name = emote.id;

            var contentProvider = emote.GetContentProvider(bodyShapeId);

            foreach (var contentMap in contentProvider.contents)
            {
                if (!contentMap.file.EndsWith(".mp3")) continue; //do we need to support more of em?

                try
                {
                    await AsyncLoadAudioClip(contentMap.file, contentProvider).ToUniTask(cancellationToken: ct);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (audioClip != null)
                {
                    audioSource = rendereable.container.AddComponent<AudioSource>();
                    audioSource.clip = audioClip;
                    audioSource.rolloffMode = AudioRolloffMode.Linear;
                    audioSource.minDistance = 3;
                    audioSource.maxDistance = 30;
                    audioSource.spatialBlend = 1;
                    audioSource.playOnAwake = false;
                }

                break;
            }
        }

        private IEnumerator AsyncLoadAudioClip(string file, ContentProvider contentProvider)
        {
            var audioClipPromise = new AssetPromise_AudioClip(file, contentProvider);
            audioClipPromise.OnSuccessEvent += asset => audioClip = asset.audioClip;
            audioClipPromise.OnFailEvent += (_, e) => throw e;

            AssetPromiseKeeper_AudioClip.i.Keep(audioClipPromise);

            yield return audioClipPromise;
        }

        public void Dispose()
        {
            retriever?.Dispose();
        }
    }
}
