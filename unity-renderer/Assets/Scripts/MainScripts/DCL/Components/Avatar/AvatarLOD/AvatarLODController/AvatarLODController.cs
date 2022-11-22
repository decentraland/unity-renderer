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
        public void SetAnimationThrottling(int framesBetweenUpdates);
        void SetInvisible();
        void UpdateImpostorTint(float distanceToMainPlayer);
        void SetNameVisible(bool visible);
    }

    public class AvatarLODController : IAvatarLODController
    {
        private string VISIBILITY_CONSTRAIN = "behind_camera_or_out_of_limits";
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
            player.avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAIN);
        }

        public void SetLOD1()
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(true);
            player.avatar.SetLODLevel(1);
            player.avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAIN);
        }

        public void SetLOD2()
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(false);
            player.avatar.SetLODLevel(2);
            player.avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAIN);
        }

        public void SetInvisible()
        {
            if (player?.avatar == null)
                return;

            player.avatar.AddVisibilityConstraint(VISIBILITY_CONSTRAIN);
            player.onPointerDownCollider.SetColliderEnabled(false);
        }

        public void SetAnimationThrottling(int framesBetweenUpdates)
        {
            if (player?.avatar == null)
                return;
            
            player.avatar.SetAnimationThrottling(framesBetweenUpdates);
        }

        public void SetNameVisible(bool visible)
        {
            if (visible)
                player?.playerName.Show();
            else
                player?.playerName.Hide();
        }
        public void UpdateImpostorTint(float distanceToMainPlayer)
        {
            if (player?.avatar == null)
                return;
            
            player.avatar.SetImpostorTint(AvatarRendererHelpers.CalculateImpostorTint(distanceToMainPlayer));
        }

        public void Dispose() { }
    }
}