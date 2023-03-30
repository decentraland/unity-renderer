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
        private const string VISIBILITY_CONSTRAIN = "behind_camera_or_out_of_limits";
        public Player player { get; }

        public AvatarLODController(Player player)
        {
            this.player = player;
            player?.avatar?.SetLODLevel(0);
        }

        public void SetLOD0() =>
            SetLODLevel(0, setColliderEnabled: true);

        public void SetLOD1() =>
            SetLODLevel(1, setColliderEnabled: true);

        public void SetLOD2() =>
            SetLODLevel(2, setColliderEnabled: false);

        private void SetLODLevel(int level, bool setColliderEnabled)
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(setColliderEnabled);
            player.avatar.SetLODLevel(level);
            player.avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAIN);
        }

        public void SetInvisible()
        {
            if (player?.avatar == null)
                return;

            player.avatar.AddVisibilityConstraint(VISIBILITY_CONSTRAIN);
            player.onPointerDownCollider.SetColliderEnabled(false);
        }

        public void SetAnimationThrottling(int framesBetweenUpdates) =>
            player?.avatar?.SetAnimationThrottling(framesBetweenUpdates);

        public void SetNameVisible(bool visible)
        {
            if (visible)
                player?.playerName.Show();
            else
                player?.playerName.Hide();
        }
        public void UpdateImpostorTint(float distanceToMainPlayer) =>
            player?.avatar?.SetImpostorTint(AvatarRendererHelpers.CalculateImpostorTint(distanceToMainPlayer));

        public void Dispose() { }
    }
}
