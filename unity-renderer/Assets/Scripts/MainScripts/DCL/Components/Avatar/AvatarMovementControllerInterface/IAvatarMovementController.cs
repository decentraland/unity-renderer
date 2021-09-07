using System;

namespace DCL
{
    public interface IAvatarMovementController
    {
        public event Action OnMovedAvatar;
        public event Action OnAvatarMovementWait;
    }
}