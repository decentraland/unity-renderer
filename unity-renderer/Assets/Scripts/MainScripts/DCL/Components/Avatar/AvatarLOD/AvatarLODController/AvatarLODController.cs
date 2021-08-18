using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public interface IAvatarLODController : IDisposable
    {
        void SetAvatarState();
        void SetSimpleAvatar();
        void SetImpostorState();
        void UpdateImpostorTint(float distanceToClosestPosition, float maxImpostorTintDistance, float nearestImpostorColorTint, float farestImpostorColorTint, float nearestImpostorAlphaValue, float farestImpostorAlphaValue);
    }

    public class AvatarLODController : IAvatarLODController
    {
        private const float TRANSITION_DURATION = 0.25f;
        private const float MAX_IMPOSTOR_TINT_DISTANCE = 32f;
        private const float NEAREST_IMPOSTOR_COLOR_TINT_VALUE = 0.2f;
        private const float FAREST_IMPOSTOR_COLOR_TINT_VALUE = 0.9f;
        private const float NEAREST_IMPOSTOR_COLOR_ALPHA_VALUE = 1f;
        private const float FAREST_IMPOSTOR_COLOR_ALPHA_VALUE = 0.75f;
        internal Player player;

        internal float avatarFade;
        internal float impostorFade;
        internal float targetAvatarFade;
        internal float targetImpostorFade;

        internal bool SSAOEnabled;
        internal bool facialFeaturesEnabled;

        internal Coroutine currentTransition = null;

        public AvatarLODController(Player player)
        {
            this.player = player;
            avatarFade = 1;
            targetAvatarFade = 1;
            impostorFade = 0;
            targetImpostorFade = 0;
            SSAOEnabled = true;
            facialFeaturesEnabled = true;
            if (player?.renderer == null)
                return;
            player.renderer.SetAvatarFade(avatarFade);
            player.renderer.SetImpostorFade(impostorFade);
        }

        public void SetAvatarState()
        {
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(true, true);
            StartTransition(1, 0);
        }

        public void SetSimpleAvatar()
        {
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(false, false);
            StartTransition(1, 0);
        }

        public void SetImpostorState()
        {
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(false, false);
            StartTransition(0, 1);
        }

        private void StartTransition(float newTargetAvatarFade, float newTargetImpostorFade)
        {
            if (Mathf.Approximately(targetAvatarFade, newTargetAvatarFade) && Mathf.Approximately(targetImpostorFade, newTargetImpostorFade))
                return;

            targetAvatarFade = newTargetAvatarFade;
            targetImpostorFade = newTargetImpostorFade;
            CoroutineStarter.Stop(currentTransition);
            currentTransition = CoroutineStarter.Start(Transition(newTargetAvatarFade, newTargetImpostorFade));
        }

        internal IEnumerator Transition(float targetAvatarFade, float targetImpostorFade, float transitionDuration = TRANSITION_DURATION)
        {
            while (!player.renderer.isReady)
            {
                yield return null;
            }

            player.renderer.SetAvatarFade(avatarFade);
            player.renderer.SetImpostorFade(impostorFade);
            player.renderer.SetVisibility(true);
            player.renderer.SetImpostorVisibility(true);

            while (!Mathf.Approximately(avatarFade, targetAvatarFade) || !Mathf.Approximately(impostorFade, targetImpostorFade))
            {
                avatarFade = Mathf.MoveTowards(avatarFade, targetAvatarFade, 1f / transitionDuration * Time.deltaTime);
                impostorFade = Mathf.MoveTowards(impostorFade, targetImpostorFade, 1f / transitionDuration * Time.deltaTime);
                player.renderer.SetAvatarFade(avatarFade);
                player.renderer.SetImpostorFade(impostorFade);
                yield return null;
            }

            avatarFade = targetAvatarFade;
            impostorFade = targetImpostorFade;

            bool avatarVisibility = !Mathf.Approximately(avatarFade, 0);
            player.renderer.SetVisibility(avatarVisibility);
            bool impostorVisibility = !Mathf.Approximately(impostorFade, 0);
            player.renderer.SetImpostorVisibility(impostorVisibility);
            currentTransition = null;
        }

        private void SetAvatarFeatures(bool newSSAOEnabled, bool newFacialFeaturesEnabled)
        {
            if (SSAOEnabled != newSSAOEnabled)
            {
                player.renderer.SetSSAOEnabled(newSSAOEnabled);
                SSAOEnabled = newSSAOEnabled;
            }

            if (facialFeaturesEnabled != newFacialFeaturesEnabled)
            {
                player.renderer.SetFacialFeaturesVisible(newFacialFeaturesEnabled);
                facialFeaturesEnabled = newFacialFeaturesEnabled;
            }
        }

        public void UpdateImpostorTint(float distanceToClosestPosition, float maxImpostorTintDistance, float nearestImpostorColorTint, float farestImpostorColorTint, float nearestImpostorAlphaValue, float farestImpostorAlphaValue)
        {
            float tintStep = Mathf.InverseLerp(0, maxImpostorTintDistance, distanceToClosestPosition);
            float tintValue = Mathf.Lerp(nearestImpostorColorTint, farestImpostorColorTint, tintStep); // 20% to 90% alpha
            Color newColor = Color.Lerp(Color.white, Color.black, tintValue);
            newColor.a = Mathf.Lerp(nearestImpostorAlphaValue, farestImpostorAlphaValue, tintStep); // 100% to 75% alpha

            player.renderer.SetImpostorColor(newColor);
        }

        public void Dispose() { CoroutineStarter.Stop(currentTransition); }
    }
}