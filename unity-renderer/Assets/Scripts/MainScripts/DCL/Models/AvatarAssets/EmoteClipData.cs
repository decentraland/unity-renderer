using JetBrains.Annotations;
using System;
using UnityEngine;

namespace DCL.Emotes
{
    public class EmoteClipData
    {
        public AnimationClip AvatarClip { get; }
        public bool Loop { get; }

        [CanBeNull] private Animation animation;
        [CanBeNull] public GameObject ExtraContent { get; set; }

        [CanBeNull] private Renderer[] renderers;

        [CanBeNull] public AudioSource AudioSource { get; }

        public EmoteClipData(AnimationClip avatarClip, bool loop = false)
        {
            this.AvatarClip = avatarClip;
            this.Loop = loop;
        }

        public EmoteClipData(AnimationClip mainClip, GameObject container, AudioSource audioSource, bool loop = false)
        {
            this.AvatarClip = mainClip;
            this.Loop = loop;
            this.ExtraContent = container;
            this.AudioSource = audioSource;

            if (ExtraContent == null) return;

            animation = ExtraContent.GetComponentInChildren<Animation>();

            if (animation == null)
                Debug.LogError($"Animation {AvatarClip.name} extra content does not have an animation");
            else
            {
                animation.wrapMode = WrapMode.Default;

                foreach (AnimationState state in animation)
                {
                    if (state.clip == AvatarClip) continue;
                    state.wrapMode = Loop ? WrapMode.Loop : WrapMode.Once;
                }
            }

            renderers = ExtraContent.GetComponentsInChildren<Renderer>();
        }

        public void Play(int gameObjectLayer, bool spatial, float volume, bool occlude)
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                    renderer.gameObject.layer = gameObjectLayer;
                    renderer.allowOcclusionWhenDynamic = occlude;
                }
            }

            if (animation != null)
            {
                animation.gameObject.layer = gameObjectLayer;
                animation.cullingType = occlude ? AnimationCullingType.BasedOnRenderers : AnimationCullingType.AlwaysAnimate;
                animation.enabled = true;

                foreach (AnimationState state in animation)
                {
                    if (state.clip == AvatarClip) continue;

                    // this reduntant stop is intended, sometimes when animations are triggered their first frame is not 0
                    animation.Stop(state.clip.name);
                    animation.CrossFade(state.clip.name, 0, PlayMode.StopAll);
                }
            }

            if (AudioSource == null) return;

            AudioSource.spatialBlend = spatial ? 1 : 0;
            AudioSource.volume = volume;
            AudioSource.loop = Loop;
            AudioSource.Play();
        }

        public void Stop()
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers) { renderer.enabled = false; }
            }

            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (state.clip == AvatarClip) continue;
                    animation.Stop(state.clip.name);
                }

                animation.enabled = false;
            }

            if (AudioSource != null)
                AudioSource.Stop();
        }
    }
}
