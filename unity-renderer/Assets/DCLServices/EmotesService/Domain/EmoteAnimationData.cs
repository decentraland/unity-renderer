using JetBrains.Annotations;
using System;
using UnityEngine;

namespace DCLServices.EmotesService.Domain
{
    public enum EmoteState
    {
        PLAYING,
        STOPPING,
        STOPPED,
    }
    public class EmoteAnimationData
    {
        private const float EXPRESSION_EXIT_TRANSITION_TIME = 0.2f;
        private const float EXPRESSION_ENTER_TRANSITION_TIME = 0.1f;

        [CanBeNull] private readonly Animation extraContentAnimation;
        [CanBeNull] private readonly GameObject extraContent;
        [CanBeNull] private readonly Renderer[] renderers;
        [CanBeNull] private readonly AudioSource audioSource;

        private readonly AnimationClip avatarClip;
        private readonly bool loop;
        private AnimationSequence animationSequence;
        private Animation avatarAnimation;
        private bool isSequential;
        private AnimationState currentAvatarAnimation;
        private AnimationState avatarClipState;
        private AnimationState sequenceAvatarLoopState;
        private EmoteState currentState = EmoteState.STOPPED;

        // Constructor used by Embed Emotes
        public EmoteAnimationData(AnimationClip avatarClip, bool loop = false)
        {
            this.avatarClip = avatarClip;
            this.loop = loop;
        }

        // Constructor used by Remote Emotes
        public EmoteAnimationData(AnimationClip mainClip, GameObject container, AudioSource audioSource, bool loop = false)
        {
            this.avatarClip = mainClip;
            this.loop = loop;
            this.extraContent = container;
            this.audioSource = audioSource;

            if (extraContent == null) return;

            extraContentAnimation = extraContent.GetComponentInChildren<Animation>();

            if (extraContentAnimation == null)
                Debug.LogError($"Animation {avatarClip.name} extra content does not have an animation");

            renderers = extraContent.GetComponentsInChildren<Renderer>();
        }

        public bool IsLoop() =>
            loop;

        public bool HasAudio() =>
            audioSource != null;

        public void UnEquip()
        {
            if (extraContent != null)
                extraContent.transform.SetParent(null, false);

            if (isSequential)
            {

                avatarAnimation.RemoveClip(animationSequence.AvatarStart);
                avatarAnimation.RemoveClip(animationSequence.AvatarLoop);
                avatarAnimation.RemoveClip(animationSequence.AvatarEnd);
            }
            else
            {
                avatarAnimation.RemoveClip(avatarClip);
                Debug.Log("UnEquip " + avatarClip);
            }
        }

        public int GetLoopCount() =>
            Mathf.RoundToInt(currentAvatarAnimation.time / currentAvatarAnimation.length);

        public void Equip(Animation animation)
        {
            avatarAnimation = animation;
            if (isSequential)
            {
                avatarAnimation.AddClip(animationSequence.AvatarStart, animationSequence.AvatarStart.name);
                avatarAnimation.AddClip(animationSequence.AvatarLoop, animationSequence.AvatarLoop.name);
                avatarAnimation.AddClip(animationSequence.AvatarEnd, animationSequence.AvatarEnd.name);
                sequenceAvatarLoopState = avatarAnimation[animationSequence.AvatarLoop.name];
            }
            else
            {
                avatarAnimation.AddClip(avatarClip, avatarClip.name);
                avatarClipState = animation[avatarClip.name];
                Debug.Log(avatarClip.name, avatarAnimation);
            }

            // We set the extra content as a child of the avatar gameobject and use its local position to mimick its positioning and correction
            if (extraContent != null)
            {
                Transform animationTransform = animation.transform;
                extraContent.transform.SetParent(animationTransform.parent, false);
                extraContent.transform.localRotation = animationTransform.localRotation;
                extraContent.transform.localScale = animationTransform.localScale;
                extraContent.transform.localPosition = animationTransform.localPosition;
            }
        }

        public void Play(int layer, bool spatial, float volume, bool occlude)
        {
            currentState = EmoteState.PLAYING;

            EnableRenderers(layer, occlude);

            if (isSequential)
                PlaySequential(spatial, volume);
            else
                PlayNormal(spatial, volume);
        }

        private void EnableRenderers(int gameObjectLayer, bool occlude)
        {
            if (renderers == null) return;

            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = true;
                renderer.gameObject.layer = gameObjectLayer;
                renderer.allowOcclusionWhenDynamic = occlude;
            }
        }

        private void PlayNormal(bool spatial, float volume)
        {
            string avatarClipName = avatarClip.name;

            if (avatarAnimation.IsPlaying(avatarClipName))
                avatarAnimation.Rewind(avatarClipName);

            avatarAnimation.CrossFade(avatarClipName, EXPRESSION_ENTER_TRANSITION_TIME, PlayMode.StopAll);
            currentAvatarAnimation = avatarClipState;

            if (extraContentAnimation != null)
            {
                var layer = 0;

                extraContentAnimation.enabled = true;

                foreach (AnimationState state in extraContentAnimation)
                {
                    if (state.clip == avatarClip) continue;
                    state.layer = layer++;
                    state.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;
                    extraContentAnimation.Play(state.clip.name);
                }
            }

            if (audioSource != null)
            {
                audioSource.spatialBlend = spatial ? 1 : 0;
                audioSource.volume = volume;
                audioSource.loop = loop;
                audioSource.Play();
            }
        }

        private void PlaySequential(bool spatial, float volume)
        {
            avatarAnimation.wrapMode = WrapMode.Default;
            avatarAnimation[animationSequence.AvatarStart.name].wrapMode = WrapMode.Once;
            avatarAnimation[animationSequence.AvatarLoop.name].wrapMode = WrapMode.Loop;
            avatarAnimation.Stop();
            avatarAnimation.CrossFadeQueued(animationSequence.AvatarStart.name, EXPRESSION_ENTER_TRANSITION_TIME, QueueMode.PlayNow);
            avatarAnimation.CrossFadeQueued(animationSequence.AvatarLoop.name, 0, QueueMode.CompleteOthers);
            currentAvatarAnimation = sequenceAvatarLoopState;

            if (extraContentAnimation != null)
            {
                extraContentAnimation.enabled = true;
                extraContentAnimation.wrapMode = WrapMode.Default;
                extraContentAnimation[animationSequence.PropStart.name].wrapMode = WrapMode.Once;
                extraContentAnimation[animationSequence.PropLoop.name].wrapMode = WrapMode.Loop;
                extraContentAnimation.Stop();
                extraContentAnimation.CrossFadeQueued(animationSequence.PropStart.name, EXPRESSION_ENTER_TRANSITION_TIME, QueueMode.PlayNow);
                extraContentAnimation.CrossFadeQueued(animationSequence.PropLoop.name, 0 ,QueueMode.CompleteOthers);
            }

            if (audioSource == null) return;

            audioSource.spatialBlend = spatial ? 1 : 0;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.Play();
        }

        public void Stop(bool immediate)
        {
            if (isSequential)
            {
                SequentialStop(immediate);
                currentState = !immediate ? EmoteState.STOPPING : EmoteState.STOPPED;
            }
            else
            {
                NormalStop(immediate);
                currentState = EmoteState.STOPPED;
            }

            if (audioSource != null)
                audioSource.Stop();
        }

        private void SequentialStop(bool immediate)
        {
            avatarAnimation[animationSequence.AvatarEnd.name].wrapMode = WrapMode.Once;
            avatarAnimation.Stop();

            if (!immediate)
                avatarAnimation.CrossFade(animationSequence.AvatarEnd.name, EXPRESSION_EXIT_TRANSITION_TIME);

            currentAvatarAnimation = avatarAnimation[animationSequence.AvatarEnd.name];

            if (extraContentAnimation == null) return;

            extraContentAnimation[animationSequence.PropEnd.name].wrapMode = WrapMode.Once;
            extraContentAnimation.Stop();

            if (!immediate)
                extraContentAnimation.CrossFade(animationSequence.PropEnd.name, EXPRESSION_EXIT_TRANSITION_TIME);
        }

        private void NormalStop(bool immediate)
        {
            avatarAnimation.Blend(avatarClip.name, 0, !immediate ? EXPRESSION_EXIT_TRANSITION_TIME : 0);

            if (renderers != null)
                foreach (Renderer renderer in renderers)
                    renderer.enabled = false;

            if (extraContentAnimation == null) return;

            foreach (AnimationState state in extraContentAnimation)
            {
                if (state.clip == avatarClip) continue;
                extraContentAnimation.Stop(state.clip.name);
            }

            extraContentAnimation.enabled = false;
        }

        public void SetupSequentialAnimation(AnimationSequence sequence)
        {
            isSequential = true;
            animationSequence = sequence;

            if (extraContentAnimation == null) return;

            extraContentAnimation.AddClip(animationSequence.PropStart, animationSequence.PropStart.name);
            extraContentAnimation.AddClip(animationSequence.PropLoop, animationSequence.PropLoop.name);
            extraContentAnimation.AddClip(animationSequence.PropEnd, animationSequence.PropEnd.name);
        }

        public bool CanTransitionOut() =>
            !isSequential || IsFinished();

        public bool IsFinished()
        {
            if (loop && !isSequential) return false;
            float timeTillEnd = currentAvatarAnimation == null ? 0 : currentAvatarAnimation.length - currentAvatarAnimation.time;
            return timeTillEnd < EXPRESSION_EXIT_TRANSITION_TIME;
        }

        public EmoteState GetState() =>
            currentState;
    }
}
