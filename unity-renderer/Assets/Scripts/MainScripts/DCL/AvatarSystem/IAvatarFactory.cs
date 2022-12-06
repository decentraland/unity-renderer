using DCL;
using System;
using UnityEngine;

namespace AvatarSystem
{
    public interface IAvatarFactory : IService
    {
        IAvatar CreateAvatar(
            GameObject avatarContainer,
            IAnimator animator,
            ILOD lod,
            IVisibility visibility);

        IAvatar CreateAvatarWithHologram(
            GameObject avatarContainer,
            Transform avatarRevealContainer,
            GameObject armatureContainer,
            IAnimator animator,
            ILOD lod,
            IVisibility visibility
        );

        void IDisposable.Dispose() { }

        void IService.Initialize() { }
    }
}
