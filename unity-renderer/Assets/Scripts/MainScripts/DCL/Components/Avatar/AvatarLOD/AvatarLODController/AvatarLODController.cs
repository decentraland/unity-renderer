using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    public class AvatarLODController : IDisposable
    {
        private enum LODState
        {
            LOD0,
            LOD1,
            LOD2
        }

        private const float TRANSITION_DURATION = 0.25f;
        private Player player;

        private float avatarFade;
        private float impostorFade;
        private float targetAvatarFade;
        private float targetImpostorFade;

        private bool SSAOEnabled;
        private bool facialFeaturesEnabled;
        private LODState currentState = LODState.LOD0;

        private Coroutine currentTransition = null;

        public AvatarLODController(Player player)
        {
            this.player = player;
            avatarFade = 1;
            impostorFade = 0;
            SSAOEnabled = false;
            facialFeaturesEnabled = true;
            if (player?.renderer == null)
                return;
            player.renderer.SetAvatarFade(avatarFade);
            player.renderer.SetImpostorFade(impostorFade);
        }

        public void SetAvatarState()
        {
            if (currentState == LODState.LOD0 || player?.renderer == null)
                return;

            currentState = LODState.LOD0;

            SetAvatarFeatures(true, true);
            StartTransition(1, 0);
        }

        public void SetSimpleAvatar()
        {
            if (currentState == LODState.LOD1 || player?.renderer == null)
                return;

            currentState = LODState.LOD1;

            SetAvatarFeatures(false, false);
            StartTransition(1, 0);
        }

        public void SetImpostorState()
        {
            if (currentState == LODState.LOD2 || player?.renderer == null)
                return;

            currentState = LODState.LOD2;

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

        private IEnumerator Transition(float targetAvatarFade, float targetImpostorFade)
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
                avatarFade = Mathf.MoveTowards(avatarFade, targetAvatarFade, 1f / TRANSITION_DURATION * Time.deltaTime);
                impostorFade = Mathf.MoveTowards(impostorFade, targetImpostorFade, 1f / TRANSITION_DURATION * Time.deltaTime);
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

        public void Dispose() { CoroutineStarter.Stop(currentTransition); }
    }
}