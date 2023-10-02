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
                animation.cullingType = occlude ? AnimationCullingType.BasedOnRenderers :  AnimationCullingType.AlwaysAnimate;
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

            if (AudioSource != null)
                AudioSource.Stop();
        }

        public void CheckStatus(float timeSincePlay)
        {
            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (state.clip == AvatarClip) continue;

                    if (!(state.time < state.clip.length)) continue;

                    float delta = timeSincePlay - state.time;
                    if (!(delta < 0.05f) && state.enabled) continue;

                    state.time = timeSincePlay;
                    animation.CrossFade(state.clip.name, 0, PlayMode.StopAll);
                }
            }
        }

        public void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10,10,5000,5000));
            GUI.color = Color.magenta;
            GUI.skin.label.fontSize = 20;
            GUILayout.Label("Animator enabled: " + animation.enabled);
            GUILayout.Space(25);

            if (animation != null)
            {
                foreach (AnimationState state in animation)
                {
                    if (state.clip == AvatarClip) continue;
                    GUILayout.Label("Clip: " + state.clip.name);
                    GUILayout.Label(" - Animation State Enabled: " + state.enabled);
                    GUILayout.Label(" - Animation State Time: " + state.time);
                }
            }

            if (ExtraContent != null)
            {
                var pos = ExtraContent.transform.localPosition;
                var pos2 = ExtraContent.transform.position;
                GUILayout.Label($"Local Pos: {pos.x},{pos.y},{pos.z}");
                GUILayout.Label($"Global Pos: {pos2.x},{pos2.y},{pos2.z}");
            }

            GUILayout.Space(25);

            if (renderers != null)
                foreach (Renderer renderer in renderers)
                    GUILayout.Label($"Renderer {renderer.name} enabled: {renderer.enabled} visible: {renderer.isVisible} Layer: {LayerMask.LayerToName(renderer.gameObject.layer)}");

            GUILayout.EndArea();
        }
    }
}
