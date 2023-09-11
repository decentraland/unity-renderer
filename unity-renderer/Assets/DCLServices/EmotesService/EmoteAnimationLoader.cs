using AvatarSystem;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Providers;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteAnimationLoader : IEmoteAnimationLoader
    {
        private const string EMOTE_AUDIO_SOURCE = "EmoteAudioSource";
        private readonly IWearableRetriever retriever;
        private readonly AddressableResourceProvider resourceProvider;
        public AnimationClip mainClip { get; private set; }
        public GameObject container { get; private set; }
        public AudioSource audioSource { get; private set; }

        private AudioClip audioClip;
        private AssetPromise_AudioClip audioClipPromise;

        public EmoteAnimationLoader(IWearableRetriever retriever, AddressableResourceProvider resourceProvider)
        {
            this.retriever = retriever;
            this.resourceProvider = resourceProvider;
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

            this.mainClip = animationClip;

            //Clip names should be unique because of the Legacy Animation string based usage
            animationClip.name = emote.id;

            var contentProvider = emote.GetContentProvider(bodyShapeId);

            foreach (var contentMap in contentProvider.contents)
            {
                if (!IsValidAudioClip(contentMap.file)) continue;

                try { await AsyncLoadAudioClip(contentMap.file, contentProvider); }
                catch (Exception e) { Debug.LogError(e); }

                if (audioClip != null)
                {
                    audioSource = await resourceProvider.Instantiate<AudioSource>(EMOTE_AUDIO_SOURCE, "EmoteAudioSource", rendereable.container.transform, ct);
                    audioSource.clip = audioClip;
                }

                // we only support one audio clip
                break;
            }
        }

        private bool IsValidAudioClip(string fileName) =>
            fileName.EndsWith(".ogg") || fileName.EndsWith(".mp3");

        private async UniTask<AudioClip> AsyncLoadAudioClip(string file, ContentProvider contentProvider)
        {
            audioClipPromise = new AssetPromise_AudioClip(file, contentProvider);
            AssetPromiseKeeper_AudioClip.i.Keep(audioClipPromise);
            await audioClipPromise;
            return audioClipPromise.asset.audioClip;
        }

        public void Dispose()
        {
            AssetPromiseKeeper_AudioClip.i.Forget(audioClipPromise);
            retriever?.Dispose();
        }
    }
}
