using AvatarSystem;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.EmotesService.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteAnimationLoader : IEmoteAnimationLoader
    {
        private const string AVATAR_START = "avatar_start";
        private const string AVATAR_LOOP = "avatar_loop";
        private const string AVATAR_END = "avatar_end";
        private const string PROP_START = "prop_start";
        private const string PROP_LOOP = "prop_loop";
        private const string PROP_END = "prop_end";
        private const string EMOTE_AUDIO_SOURCE = "EmoteAudioSource";

        private readonly IWearableRetriever retriever;
        private readonly AddressableResourceProvider resourceProvider;
        private AudioClip audioClip;
        private AssetPromise_AudioClip audioClipPromise;

        public AnimationClip mainClip { get; private set; }
        public GameObject container { get; private set; }
        public AudioSource audioSource { get; private set; }
        public bool IsSequential { get; private set; }

        private AnimationSequence animationSequence;

        public EmoteAnimationLoader(IWearableRetriever retriever, AddressableResourceProvider resourceProvider)
        {
            this.retriever = retriever;
            this.resourceProvider = resourceProvider;
        }

        public AnimationSequence GetSequence() =>
            animationSequence;

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

            var requiredAnimationSequence = new HashSet<string>
            {
                AVATAR_END, AVATAR_LOOP, AVATAR_START,
                PROP_END, PROP_LOOP, PROP_START,
            };

            if (animation.GetClipCount() > 1)
            {
                this.container = rendereable.container;

                foreach (AnimationState state in animation)
                {
                    FillAnimationSequence(requiredAnimationSequence, state.clip);

                    if (state.clip.name.Contains("avatar", StringComparison.OrdinalIgnoreCase) ||
                        state.clip.name == emote.id) { this.mainClip = state.clip; }

                    // There's a bug with the legacy animation where animations start ahead of time the first time
                    // our workaround is to play every animation while we load the audio clip and then disable the animator
                    animation.Play(state.clip.name);
                }
            }

            if (requiredAnimationSequence.Count == 0)
                IsSequential = true;

            if (mainClip == null)
            {
                Debug.LogError("AnimationClip not found in the container for emote " + emote.id);
                return;
            }

            //Clip names should be unique because of the Legacy Animation string based usage
            if (IsSequential)
            {
                animationSequence.AvatarStart.name = RenameIfValid(animationSequence.AvatarStart.name, $"{emote.id}:{animationSequence.AvatarStart.name}");
                animationSequence.AvatarLoop.name = RenameIfValid(animationSequence.AvatarLoop.name, $"{emote.id}:{animationSequence.AvatarLoop.name}");
                animationSequence.AvatarEnd.name = RenameIfValid(animationSequence.AvatarEnd.name, $"{emote.id}:{animationSequence.AvatarEnd.name}");
            }
            else
            {
                mainClip.name = RenameIfValid(mainClip.name, emote.id);
            }

            var contentProvider = emote.GetContentProvider(bodyShapeId);

            foreach (var contentMap in contentProvider.contents)
            {
                if (!IsValidAudioClip(contentMap.file)) continue;

                try { audioClip = await AsyncLoadAudioClip(contentMap.file, contentProvider); }
                catch (Exception e) { Debug.LogError(e); }

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

        // Kinerius note: There was a weird case when testing where 2 emotes with different urn ids where using the same GLTF,
        //                when the second one loads the clip gets renamed and that causes the first user to not be able to use the emote.
        //                To avoid this, we rename only when the clip is not already renamed
        private string RenameIfValid(string from, string to) =>
            from.StartsWith("urn") ? from : to;

        private void FillAnimationSequence(HashSet<string> requiredAnimationSequence, AnimationClip clip)
        {
            string name = clip.name.ToLower();

            if (!requiredAnimationSequence.Contains(name)) return;

            switch (name)
            {
                case AVATAR_START:
                    animationSequence.AvatarStart = clip;
                    break;
                case AVATAR_LOOP:
                    animationSequence.AvatarLoop = clip;
                    break;
                case AVATAR_END:
                    animationSequence.AvatarEnd = clip;
                    break;
                case PROP_START:
                    animationSequence.PropStart = clip;
                    break;
                case PROP_LOOP:
                    animationSequence.PropLoop = clip;
                    break;
                case PROP_END:
                    animationSequence.PropEnd = clip;
                    break;
            }

            requiredAnimationSequence.Remove(name);
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
