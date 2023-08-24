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
        [CanBeNull] public GameObject ExtraContent { get; private set; }
        [CanBeNull] public AudioClip AudioClip { get; private set; }

        [CanBeNull] private Renderer[] renderers;

        public EmoteClipData(AnimationClip avatarClip, bool loop = false)
        {
            this.AvatarClip = avatarClip;
            this.Loop = loop;
        }

        public void LoadExtraContent(GameObject extraContent, AudioClip audioClip)
        {
            this.ExtraContent = extraContent;
            this.AudioClip = audioClip;

            if (extraContent != null)
            {
                animation = extraContent.GetComponentInChildren<Animation>();

                if (animation == null)
                    Debug.LogError($"Animation {AvatarClip.name} extra content does not have an animation");

                renderers = extraContent.GetComponentsInChildren<Renderer>();
            }
        }

        public void PlayAllAnimations()
        {
            if (renderers != null)
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
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
        }

        public void StopAllAnimations()
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
        }
    }
}
