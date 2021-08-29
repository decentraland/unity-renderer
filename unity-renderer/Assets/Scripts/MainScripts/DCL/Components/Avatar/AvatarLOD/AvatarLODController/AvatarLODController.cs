using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    //TODO Rename to IAvatar (not now to avoid conflicts)
    public interface IAvatarLODController : IDisposable
    {
        Player player { get; }
        void SetFullAvatar();
        void SetSimpleAvatar();
        void SetImpostor();
        void SetInvisible();
    }

    public class AvatarLODController : IAvatarLODController
    {
        internal enum State
        {
            Invisible,
            FullAvatar,
            SimpleAvatar,
            Impostor,
        }

        private const float TRANSITION_DURATION = 0.5f;

        public Player player { get; }

        internal float avatarFade;
        internal float impostorFade;

        internal bool SSAOEnabled;
        internal bool facialFeaturesEnabled;

        internal Coroutine currentTransition = null;
        internal State? lastRequestedState = null;

        public AvatarLODController(Player player)
        {
            this.player = player;
            avatarFade = 1;
            impostorFade = 0;
            SSAOEnabled = true;
            facialFeaturesEnabled = true;
            if (player?.renderer == null)
                return;
            player.renderer.SetAvatarFade(avatarFade);
            player.renderer.SetImpostorFade(impostorFade);
        }

        public void SetFullAvatar()
        {
            if (lastRequestedState == State.FullAvatar)
                return;

            lastRequestedState = State.FullAvatar;
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(true, true);
            StartTransition(1, 0);
        }

        public void SetSimpleAvatar()
        {
            if (lastRequestedState == State.SimpleAvatar)
                return;

            lastRequestedState = State.SimpleAvatar;
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(false, false);
            StartTransition(1, 0);
        }

        public void SetImpostor()
        {
            if (lastRequestedState == State.Impostor)
                return;

            lastRequestedState = State.Impostor;
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(false, false);
            StartTransition(0, 1);
        }

        public void SetInvisible()
        {
            if (lastRequestedState == State.Invisible)
                return;

            lastRequestedState = State.Invisible;
            if (player?.renderer == null)
                return;

            SetAvatarFeatures(false, false);
            StartTransition(0, 0);
        }

        private void StartTransition(float newTargetAvatarFade, float newTargetImpostorFade)
        {
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
            player.renderer.SetRendererEnabled(true);
            player.renderer.SetImpostorVisibility(true);

            while (!Mathf.Approximately(avatarFade, targetAvatarFade) || !Mathf.Approximately(impostorFade, targetImpostorFade))
            {
                avatarFade = Mathf.MoveTowards(avatarFade, targetAvatarFade, (1f / transitionDuration) * Time.deltaTime);
                impostorFade = Mathf.MoveTowards(impostorFade, targetImpostorFade, (1f / transitionDuration) * Time.deltaTime);
                player.renderer.SetAvatarFade(avatarFade);
                player.renderer.SetImpostorFade(impostorFade);
                yield return null;
            }

            avatarFade = targetAvatarFade;
            impostorFade = targetImpostorFade;

            bool avatarVisibility = !Mathf.Approximately(avatarFade, 0);
            player.renderer.SetRendererEnabled(avatarVisibility);
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

        public void Dispose()
        {
            lastRequestedState = null;
            CoroutineStarter.Stop(currentTransition);
        }
    }
}