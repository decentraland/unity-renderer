using AvatarSystem;
using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.EmotesService;
using System;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private readonly EmoteVolumeHandler emoteVolumeHandler;

        public EmoteAnimationLoader(IWearableRetriever retriever, AddressableResourceProvider resourceProvider, EmoteVolumeHandler emoteVolumeHandler)
        {
            this.emoteVolumeHandler = emoteVolumeHandler;
            this.retriever = retriever;
            this.resourceProvider = resourceProvider;
        }

        public async UniTask LoadRemoteEmote(GameObject targetContainer, WearableItem emote, string bodyShapeId, CancellationToken ct = default)
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

            GameObject emoteInstance = rendereable.container;

            SetupEmote(emoteInstance, emote.id);

            var contentProvider = emote.GetContentProvider(bodyShapeId);

            foreach (var contentMap in contentProvider.contents)
            {
                if (!IsValidAudioClip(contentMap.file)) continue;

                AudioClip audioClip = null;
                try
                {
                    audioClip = await AsyncLoadAudioClip(contentMap.file, contentProvider);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (audioClip != null)
                    await SetupAudioClip(audioClip, emoteInstance, ct);

                // we only support one audio clip
                break;
            }

        }

        public async UniTask LoadLocalEmote(GameObject targetContainer, ExtendedEmote embeddedEmote, CancellationToken ct)
        {
            GameObject emoteInstance = Object.Instantiate(embeddedEmote.propPrefab, targetContainer.transform, false);
            var renderers = emoteInstance.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
                renderer.enabled = false;

            SetupEmote(emoteInstance, embeddedEmote.id);
            await SetupAudioClip(embeddedEmote.clip, emoteInstance, ct);
        }

        private void SetupEmote(GameObject emoteInstance, string emoteId)
        {
            var animation = emoteInstance.GetComponentInChildren<Animation>();

            if (animation == null)
            {
                Debug.LogError("Animation component not found in the container for emote " + emoteId);
                return;
            }

            mainClip = animation.clip;

            if (animation.GetClipCount() > 1)
            {
                this.container = emoteInstance;

                // we cant use the animation order so we use the naming convention at /creator/emotes/props-and-sounds/
                foreach (AnimationState state in animation)
                {
                    AnimationClip clip = state.clip;

                    // Replace the main clip with the one that's correctly named
                    if (clip.name.Contains("_avatar", StringComparison.OrdinalIgnoreCase) || clip.name == emoteId)
                        mainClip = clip;

                    // There's a bug with the legacy animation where animations start ahead of time the first time
                    // our workaround is to play every animation while we load the audio clip and then disable the animator
                    animation.Play(clip.name);
                }
            }

            if (mainClip == null)
            {
                Debug.LogError("AnimationClip not found in the container for emote " + emoteId);
                return;
            }

            // Clip names should be unique because of the Legacy Animation string based usage.
            // In rare cases some animations might use the same GLB, thus causing this clip to be used by 2 different emotes
            //     so we avoid renaming the clip again witch can cause problems as we use the clip names internally
            if (!mainClip.name.Contains("urn"))
                mainClip.name = emoteId;

            animation.Stop();
            animation.enabled = false;
        }

        private async UniTask SetupAudioClip(AudioClip clip, GameObject audioSourceParent, CancellationToken ct)
        {
            audioClip = clip;
            audioSource = await resourceProvider.Instantiate<AudioSource>(EMOTE_AUDIO_SOURCE, "EmoteAudioSource",
                cancellationToken: ct);
            audioSource.clip = audioClip;
            audioSource.transform.SetParent(audioSourceParent.transform, false);
            audioSource.transform.ResetLocalTRS();

            emoteVolumeHandler.AddAudioSource(audioSource);
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
            emoteVolumeHandler.RemoveAudioSource(audioSource);
            AssetPromiseKeeper_AudioClip.i.Forget(audioClipPromise);
            retriever?.Dispose();
        }
    }
}
