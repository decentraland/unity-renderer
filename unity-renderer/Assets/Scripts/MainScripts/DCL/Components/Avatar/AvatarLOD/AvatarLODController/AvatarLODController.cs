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

        bool IsInvisible { get; }
    }

    public class AvatarLODController : IAvatarLODController
    {
        private const string VISIBILITY_CONSTRAIN = "behind_camera_or_out_of_limits";
        public Player player { get; }

        public bool IsInvisible { get; private set; }

        public AvatarLODController(Player player)
        {
            this.player = player;
            player?.avatar?.SetLODLevel(0);
        }

        public void SetLOD0() =>
            SetLODLevel(0, setPointerColliderEnabled: true);

        public void SetLOD1() =>
            SetLODLevel(1, setPointerColliderEnabled: true);

        public void SetLOD2() =>
            SetLODLevel(2, setPointerColliderEnabled: false);

        private void SetLODLevel(int level, bool setPointerColliderEnabled)
        {
            if (player?.avatar == null)
                return;

            player.onPointerDownCollider.SetColliderEnabled(setPointerColliderEnabled);
            player.avatar.SetLODLevel(level);
            player.avatar.RemoveVisibilityConstrain(VISIBILITY_CONSTRAIN);

            IsInvisible = false;
        }

        public void SetInvisible()
        {
            if (player?.avatar == null)
                return;

            player.avatar.AddVisibilityConstraint(VISIBILITY_CONSTRAIN);
            player.onPointerDownCollider.SetColliderEnabled(false);
            IsInvisible = true;
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
