using System;

namespace DCL
{
    public interface IAvatarsLODController : IDisposable
    {
        void Update();
        void RegisterAvatar(AvatarLODController newAvatar);
        void RemoveAvatar(AvatarLODController targetAvatar);
    }
}