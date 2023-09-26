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

            if (animation.GetClipCount() > 1)
            {
                this.container = rendereable.container;

                // we cant use the animation order so we use the naming convention at /creator/emotes/props-and-sounds/
                foreach (AnimationState state in animation)
                {
                    AnimationClip clip = state.clip;

                    if (clip.name.Contains("_avatar", StringComparison.OrdinalIgnoreCase) || clip.name == emote.id)
                        mainClip = clip;

                    // There's a bug with the legacy animation where animations start ahead of time the first time
                    // our workaround is to play every animation while we load the audio clip and then disable the animator
                    animation.Play(clip.name);
                }

                // in the case that the animation names are badly named, we just get the first animation that does not contain prop in its name
                if (mainClip == null)
                {
                    foreach (AnimationState animationState in animation)
                    {
                        if (animationState.clip.name.Contains("prop", StringComparison.OrdinalIgnoreCase)) continue;
                        mainClip = animationState.clip;
                        break;
                    }
                }
            }
            else
                mainClip = animation.clip;

            if (mainClip == null)
            {
                Debug.LogError("AnimationClip not found in the container for emote " + emote.id);
                return;
            }

            // Clip names should be unique because of the Legacy Animation string based usage.
            // In rare cases some animations might use the same GLB, thus causing this clip to be used by 2 different emotes
            //     so we avoid renaming the clip again witch can cause problems as we use the clip names internally
            if (!mainClip.name.Contains("urn"))
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

            animation.Stop();
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
