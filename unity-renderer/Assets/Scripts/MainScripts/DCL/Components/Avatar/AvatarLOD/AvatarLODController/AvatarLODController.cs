using System;
using System.Collections;
using UnityEngine;

namespace DCL
{
    //TODO Rename to IAvatar (not now to avoid conflicts)
    public interface IAvatarLODController : IDisposable
    {
        Player player { get; }
        void SetLOD0();
        void SetLOD1();
        void SetLOD2();
        void SetInvisible();
        void UpdateImpostorTint(float distanceToMainPlayer);
        void SetThrottling(int framesBetweenUpdates);
        void SetNameVisible(bool visible);
    }

    public class AvatarLODController : IAvatarLODController
    {
        public Player player { get; }

        public AvatarLODController(Player player)
        {
            this.player = player;
            if (player?.avatar == null)
                return;
            player.avatar.SetLODLevel(0);
        }

        public void SetLOD0()
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(true);
            player.avatar.SetLODLevel(0);
        }

        public void SetLOD1()
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(true);
            player.avatar.SetLODLevel(1);
        }

        public void SetLOD2()
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(false);
            player.avatar.SetLODLevel(2);
        }

        public void SetInvisible()
        {
            if (player?.avatar == null)
                return;

            player.avatar.SetVisibility(false);
            player.onPointerDownCollider.SetColliderEnabled(false);
        }

        public void SetThrottling(int framesBetweenUpdates)
        {
//            Debug.Log("TODO");
            //player?.renderer?.SetThrottling(framesBetweenUpdates);
        }
        public void SetNameVisible(bool visible)
        {
            if (visible)
                player?.playerName.Show();
            else
                player?.playerName.Hide();
        }

        private void StartTransition(float newTargetAvatarFade, float newTargetImpostorFade, float transitionDuration = TRANSITION_DURATION)
        {
            CoroutineStarter.Stop(currentTransition);
            currentTransition = CoroutineStarter.Start(Transition(newTargetAvatarFade, newTargetImpostorFade, transitionDuration));
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

        public void Dispose() { }
    }
}