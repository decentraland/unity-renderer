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
        [CanBeNull] public GameObject ExtraContent { get; }

        [CanBeNull] private Renderer[] renderers;

        [CanBeNull] private AudioSource audioSource;

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
            this.audioSource = audioSource;

            if (ExtraContent == null) return;

            animation = ExtraContent.GetComponentInChildren<Animation>();

            if (animation == null)
                Debug.LogError($"Animation {AvatarClip.name} extra content does not have an animation");

            renderers = ExtraContent.GetComponentsInChildren<Renderer>();
        }

        public void Play(int gameObjectLayer, bool spatialSound)
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                    renderer.gameObject.layer = gameObjectLayer;
                }
            }

            if (animation != null)
            {
                var layer = 0;

                foreach (AnimationState state in animation)
                {
                    if (state.clip == AvatarClip) continue;
                    state.layer = layer++;
                    animation.Play(state.clip.name);
                }

                animation.enabled = true;
            }

            if (audioSource != null)
            {
                audioSource.spatialBlend = spatialSound ? 1 : 0;
                audioSource.loop = Loop;
                audioSource.Play();
            }
        }

        public void Stop()
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = false;
                }
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

            if (audioSource != null)
                audioSource.Stop();
        }
    }
}
