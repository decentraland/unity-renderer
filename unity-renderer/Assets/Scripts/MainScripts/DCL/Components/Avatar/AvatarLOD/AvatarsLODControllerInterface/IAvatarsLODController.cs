using System;

namespace DCL
{
    public interface IAvatarsLODController : IDisposable
    {
        void Update();
        void RegisterAvatar(AvatarLODController newAvatarLODController);
        void RemoveAvatar(AvatarLODController targetAvatar);
    }
}