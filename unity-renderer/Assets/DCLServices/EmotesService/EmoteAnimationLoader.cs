using AvatarSystem;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
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

            foreach (Renderer renderer in rendereable.renderers)
                renderer.enabled = false;

            var animation = rendereable.container.GetComponentInChildren<Animation>();

            if (animation == null)
            {
                Debug.LogError("Animation component not found in the container for emote " + emote.id);
                return;
            }

            this.mainClip = animation.clip;

            if (animation.GetClipCount() > 1)
            {
                this.container = rendereable.container;

                foreach (AnimationState state in animation)
                {
                    if (state.clip.name.Contains("avatar", StringComparison.OrdinalIgnoreCase) ||
                        state.clip.name == emote.id) { this.mainClip = state.clip; }

                    // There's a bug with the legacy animation where animations start ahead of time the first time
                    // our workaround is to play every animation while we load the audio clip and then disable the animator
                    animation.Play(state.clip.name);
                }
            }

            if (mainClip == null)
            {
                Debug.LogError("AnimationClip not found in the container for emote " + emote.id);
                return;
            }

            //Clip names should be unique because of the Legacy Animation string based usage
            mainClip.name = emote.id;

            var contentProvider = emote.GetContentProvider(bodyShapeId);

            foreach (var contentMap in contentProvider.contents)
            {
                if (!IsValidAudioClip(contentMap.file)) continue;

                try { audioClip = await AsyncLoadAudioClip(contentMap.file, contentProvider); }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (audioClip != null)
                {
                    audioSource = await resourceProvider.Instantiate<AudioSource>(EMOTE_AUDIO_SOURCE, "EmoteAudioSource", cancellationToken: ct);
                    audioSource.clip = audioClip;
                    audioSource.transform.SetParent(rendereable.container.transform, false);
                    audioSource.transform.ResetLocalTRS();
                }

                // we only support one audio clip
                break;
            }

            animation.enabled = false;
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
